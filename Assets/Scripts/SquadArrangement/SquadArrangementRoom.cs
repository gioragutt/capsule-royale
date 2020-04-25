using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CapsuleRoyale.SquadArrangement;
using Cinemachine;
using Colyseus;
using GameDevWare.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SquadArrangementRoom : MonoBehaviour
{
    private const string RoomName = "squad_arrangement";

    public Color disconnectedColor;
    public Color notReadyColor;
    public Color readyColor;

    public TextMeshProUGUI inviteIdText;

    protected IndexedDictionary<string, SquadMember> members =
        new IndexedDictionary<string, SquadMember>();
    protected IndexedDictionary<string, GameObject> memberObjects =
        new IndexedDictionary<string, GameObject>();

    public UnityEvent onReadyToStart;
    public UnityEvent onNotReadyToStart;

    public PlayerController playerController;
    public GameObject playerPrefab;
    public CinemachineVirtualCamera cam;

    private Room<SquadArrangementState> room;

    private SquadMember Me => members[room.SessionId];
    private bool ImOwner => room.SessionId == room.State.owner;

    private Color? StateToColor(string state)
    {
        switch (state)
        {
            case SquadMemberState.Disconnected: return disconnectedColor;
            case SquadMemberState.Ready: return readyColor;
            case SquadMemberState.NotReady: return notReadyColor;
            default: return null;
        }
    }

    public async void CreateNewRoom()
    {
        room = await ColyseusClient.Instance.CreateRoom<SquadArrangementState>(RoomName);
        RegisterRoomHandlers();
    }

    public async Task<bool> JoinExisting(string roomId)
    {
        try
        {
            room = await ColyseusClient.Instance.JoinRoom<SquadArrangementState>(roomId);
            RegisterRoomHandlers();
            return true;
        }
        catch (MatchMakeException e)
        {
            Debug.LogError(e);
            return false;
        }
    }

    public async void Reconnect()
    {
        room = await ColyseusClient.Instance.ReconnectRoom<SquadArrangementState>();
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
        inviteIdText.text = room.Id;
        room.State.members.OnAdd += OnMemberAdd;
        room.State.members.OnRemove += OnMemberRemove;
        room.State.members.OnChange += OnMemberMove;
        room.State.OnChange += OnRoomStateChanged;
        room.State.TriggerAll();
    }

    private void OnRoomStateChanged(List<Colyseus.Schema.DataChange> changes)
    {
        changes.ForEach((Colyseus.Schema.DataChange change) =>
        {
            Debug.LogFormat("Change to room field={0} value={1} previousValue={2}",
                change.Field, change.Value, change.PreviousValue);

            switch (change.Field)
            {
                case "readyToStart": ChangeReadyToStart(change.Value as bool? ?? false); break;
            }
        });
    }

    private void ChangeReadyToStart(bool ready)
    {
        if (!ImOwner)
        {
            return;
        }

        if (ready)
        {
            onReadyToStart.Invoke();
        }
        else
        {
            onNotReadyToStart.Invoke();
        }
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

        room.State.members.OnAdd -= OnMemberAdd;
        room.State.members.OnRemove -= OnMemberRemove;
        room.State.members.OnChange -= OnMemberMove;
    }

    public void MovePlayer(Vector3 newPosition)
    {
        _ = room?.Send("move", new Position
        {
            x = newPosition.x,
            y = newPosition.y
        });
    }

    public async void StartGame()
    {
        if (room == null)
        {
            Debug.Log("Room is not connected!");
            return;
        }

        await room.Send("start");
    }

    public async Task<bool> SendReadyMessage()
    {
        if (room == null)
        {
            Debug.Log("Room is not connected!");
            return false;
        }

        bool ready = Me.state != SquadMemberState.Ready;
        Debug.Log("Sending ready = " + ready);
        await room.Send("ready", new ReadyMessage
        {
            ready = ready,
        });
        return ready;
    }

    void OnMemberAdd(SquadMember squadMember, string key)
    {
        GameObject playerGameObject = Instantiate(
            playerPrefab,
            new Vector3(squadMember.pos.x, squadMember.pos.y, 0),
            Quaternion.identity);

        playerGameObject.GetComponentInChildren<Text>().text = squadMember.name;
        playerGameObject.name = $"Player ({squadMember.name} - {squadMember.id})";

        members.Add(squadMember.id, squadMember);
        memberObjects.Add(squadMember.id, playerGameObject);

        if (squadMember.id == room.SessionId)
        {
            playerController.player = playerGameObject.GetComponent<Rigidbody2D>();
            playerController.enabled = true;
            cam.Follow = playerGameObject.transform;
        }
        else
        {
            Destroy(playerGameObject.GetComponent<Rigidbody2D>());
        }

        squadMember.OnDataChange((Colyseus.Schema.DataChange change) =>
        {
            Debug.LogFormat("Change to member={0} field={1} value={2} previousValue={3}",
                squadMember.id, change.Field, change.Value, change.PreviousValue);

            switch (change.Field)
            {
                case "state": HandlePlayerStateFieldChange(change, playerGameObject); break;
            }
        });
    }

    private void HandlePlayerStateFieldChange(Colyseus.Schema.DataChange change, GameObject go)
    {
        Material material = go.GetComponent<SpriteRenderer>().material;
        var color = StateToColor(change.Value as string);
        if (color != null)
        {
            material.color = color.Value;
        }
    }

    void OnMemberRemove(SquadMember squadMember, string key)
    {
        if (memberObjects.TryGetValue(squadMember.id, out GameObject cube) && cube != null)
        {
            Destroy(cube);
        }

        memberObjects.Remove(squadMember.id);
    }

    void OnMemberMove(SquadMember squadMember, string key)
    {
        if (squadMember.id == room.SessionId)
        {
            return;
        }

        if (memberObjects.TryGetValue(squadMember.id, out GameObject cube) && cube != null)
        {
            cube.transform.position = new Vector3(squadMember.pos.x, squadMember.pos.y, 0);
        }
    }
}
