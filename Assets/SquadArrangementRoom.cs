using System.Collections.Generic;
using CapsuleRoyale.SquadArrangement;
using Colyseus;
using GameDevWare.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class SquadArrangementRoom : MonoBehaviour
{
    private const string RoomName = "squad_arrangement";

    public Color disconnectedColor;
    public Color notReadyColor;
    public Color readyColor;

    public ColyseusClient colyseusClient;
    public Transform cam;
    public Text inviteIdText;

    protected IndexedDictionary<string, SquadMember> members =
        new IndexedDictionary<string, SquadMember>();
    protected IndexedDictionary<string, GameObject> memberObjects =
        new IndexedDictionary<string, GameObject>();

    public PlayerController playerController;
    public GameObject playerPrefab;

    private Room<SquadArrangementState> room;

    private SquadMember Me => members[room.SessionId];

    public async void CreateNewRoom()
    {
        room = await colyseusClient.CreateRoom<SquadArrangementState>(RoomName);
        RegisterRoomHandlers();
    }

    public async void Reconnect()
    {
        room = await colyseusClient.ReconnectRoom<SquadArrangementState>();
        RegisterRoomHandlers();
    }

    public async void Leave()
    {
        if (room == null)
        {
            return;
        }

        CleanUp();
        await room.Leave(false);
        room = null;
    }

    private void RegisterRoomHandlers()
    {
        room.State.members.OnAdd += OnEntityAdd;
        room.State.members.OnRemove += OnEntityRemove;
        room.State.members.OnChange += OnEntityMove;
        room.State.TriggerAll();

        PlayerPrefs.SetString("roomId", room.Id);
        PlayerPrefs.SetString("sessionId", room.SessionId);
        PlayerPrefs.Save();

        room.OnMessage((InviteIdForOwnerMessage invite) =>
        {
            inviteIdText.text = "inviteId: " + invite.inviteId;
        });
    }

    private void CleanUp()
    {
        foreach (GameObject entry in memberObjects.Values)
        {
            Destroy(entry);
        }

        memberObjects.Clear();
        members.Clear();

        playerController.player = null;
        playerController.enabled = false;

        room.State.members.OnAdd -= OnEntityAdd;
        room.State.members.OnRemove -= OnEntityRemove;
        room.State.members.OnChange -= OnEntityMove;
    }

    public void MovePlayer(Vector3 newPosition)
    {
        _ = room?.Send("move", new Position
        {
            x = newPosition.x,
            y = newPosition.y
        });
    }

    public async void SendReadyMessage()
    {
        if (room == null)
        {
            Debug.Log("Room is not connected!");
            return;
        }

        bool ready = Me.state != SquadMemberState.Ready;
        Debug.Log("Sending ready = " + ready);
        await room.Send("ready", new ReadyMessage
        {
            ready = ready,
        });
    }

    void OnEntityAdd(SquadMember entity, string key)
    {
        GameObject cube = Instantiate(playerPrefab, new Vector3(entity.pos.x, entity.pos.y, 0), Quaternion.identity);

        cube.GetComponentInChildren<Billboard>().cam = cam.transform;
        cube.GetComponentInChildren<Text>().text = entity.name;

        members.Add(entity.id, entity);
        memberObjects.Add(entity.id, cube);

        if (entity.id == room.SessionId)
        {
            playerController.player = cube.transform;
            playerController.enabled = true;
        }

        entity.OnChange += (List<Colyseus.Schema.DataChange> changes) => changes.ForEach(change =>
        {
            Debug.LogFormat("Change to member={0} field={1} value={2} previousValue={3}",
                entity.id, change.Field, change.Value, change.PreviousValue);

            if (change.Field == "state")
            {
                MeshRenderer meshRenderer = cube.GetComponent<MeshRenderer>();
                switch (change.Value as string)
                {
                    case SquadMemberState.Disconnected:
                        meshRenderer.material.color = disconnectedColor;
                        break;
                    case SquadMemberState.Ready:
                        meshRenderer.material.color = readyColor;
                        break;
                    case SquadMemberState.NotReady:
                        meshRenderer.material.color = notReadyColor;
                        break;
                }
            }
        });
    }

    void OnEntityRemove(SquadMember entity, string key)
    {
        if (memberObjects.TryGetValue(entity.id, out GameObject cube) && cube != null)
        {
            Destroy(cube);
        }

        memberObjects.Remove(entity.id);
    }

    void OnEntityMove(SquadMember entity, string key)
    {
        if (memberObjects.TryGetValue(entity.id, out GameObject cube) && cube != null)
        {
            cube.transform.position = new Vector3(entity.pos.x, entity.pos.y, 0);
        }
    }
}
