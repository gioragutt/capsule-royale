using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections.Generic;

using System.Threading.Tasks;

using Colyseus;
using CapsuleRoyale.SquadArrangement;

using GameDevWare.Serialization;

public class ColyseusClient : MonoBehaviour
{

    // UI Buttons are attached through Unity Inspector
    public Button
        m_ConnectButton,
        m_CreateButton,
        m_JoinOrCreateButton,
        m_JoinButton,
        m_ReconnectButton,
        m_SendMessageButton,
        m_LeaveButton,
        m_GetAvailableRoomsButton;
    public InputField m_EndpointField;
    public Text m_IdText, m_SessionIdText;

    public string roomName = "squad_arrangement";

    protected Client client;
    protected Room<SquadArrangementState> room;

    protected IndexedDictionary<SquadMember, GameObject> members = new IndexedDictionary<SquadMember, GameObject>();

    // Use this for initialization
    void Start()
    {
        /* Demo UI */
        m_ConnectButton.onClick.AddListener(ConnectToServer);

        m_CreateButton.onClick.AddListener(CreateRoom);
        m_JoinOrCreateButton.onClick.AddListener(JoinOrCreateRoom);
        m_JoinButton.onClick.AddListener(JoinRoom);
        m_ReconnectButton.onClick.AddListener(ReconnectRoom);
        m_SendMessageButton.onClick.AddListener(SendMessage);
        m_LeaveButton.onClick.AddListener(LeaveRoom);
        m_GetAvailableRoomsButton.onClick.AddListener(GetAvailableRooms);
    }

    void ConnectToServer()
    {
        /*
		 * Get Colyseus endpoint from InputField
		 */
        string endpoint = m_EndpointField.text;

        Debug.Log("Connecting to " + endpoint);

        /*
		 * Connect into Colyeus Server
		 */
        client = ColyseusManager.Instance.CreateClient(endpoint);
    }

    private static Dictionary<string, object> ConnectionOptions(string methodName)
    {
        return new Dictionary<string, object>()
        {
            ["name"] = "From " + methodName,
        };
    }

    public async void CreateRoom()
    {
        await CleanUpAndLeaveRoom();
        room = await client.Create<SquadArrangementState>(roomName, ConnectionOptions("CreateRoom"));
        RegisterRoomHandlers();
    }

    public async void JoinOrCreateRoom()
    {
        await CleanUpAndLeaveRoom();
        room = await client.JoinOrCreate<SquadArrangementState>(roomName, ConnectionOptions("JoinOrCreateRoom"));
        RegisterRoomHandlers();
    }

    public async void JoinRoom()
    {
        await CleanUpAndLeaveRoom();
        room = await client.Join<SquadArrangementState>(roomName, ConnectionOptions("JoinRoom"));
        RegisterRoomHandlers();
    }

    async void ReconnectRoom()
    {
        string roomId = PlayerPrefs.GetString("roomId");
        string sessionId = PlayerPrefs.GetString("sessionId");
        if (string.IsNullOrEmpty(sessionId) || string.IsNullOrEmpty(roomId))
        {
            Debug.Log("Cannot Reconnect without having a roomId and sessionId");
            return;
        }

        room = await client.Reconnect<SquadArrangementState>(roomId, sessionId);

        Debug.Log("Reconnected into room successfully.");
        RegisterRoomHandlers();
    }

    public void RegisterRoomHandlers()
    {
        m_SessionIdText.text = "sessionId: " + room.SessionId;

        room.State.members.OnAdd += OnEntityAdd;
        room.State.members.OnRemove += OnEntityRemove;
        room.State.members.OnChange += OnEntityMove;
        room.State.TriggerAll();

        PlayerPrefs.SetString("roomId", room.Id);
        PlayerPrefs.SetString("sessionId", room.SessionId);
        PlayerPrefs.Save();

        room.OnLeave += (code) => Debug.Log("ROOM: ON LEAVE");
        room.OnError += (code, message) => Debug.LogError("ERROR, code =>" + code + ", message => " + message);
        room.OnStateChange += OnStateChangeHandler;
    }

    async void LeaveRoom()
    {
        await CleanUpAndLeaveRoom();
    }

    private async Task CleanUpAndLeaveRoom()
    {
        if (room == null)
        {
            return;
        }

        await room.Leave(false);

        // Destroy player entities
        foreach (GameObject entry in members.Values)
        {
            Destroy(entry);
        }

        members.Clear();
    }

    async void GetAvailableRooms()
    {
        var roomsAvailable = await client.GetAvailableRooms(roomName);

        Debug.Log("Available rooms (" + roomsAvailable.Length + ")");
        for (var i = 0; i < roomsAvailable.Length; i++)
        {
            Debug.Log("roomId: " + roomsAvailable[i].roomId);
            Debug.Log("maxClients: " + roomsAvailable[i].maxClients);
            Debug.Log("clients: " + roomsAvailable[i].clients);
        }
    }

    void SendMessage()
    {
        if (room != null)
        {
            _ = room.Send("move_right", new ReadyMessage()
            {
                ready = true,
            });
        }
        else
        {
            Debug.Log("Room is not connected!");
        }
    }

    void OnStateChangeHandler(SquadArrangementState state, bool isFirstState)
    {
        // Setup room first state
        Debug.LogFormat("State has been updated! {0}", state);
    }

    void OnEntityAdd(SquadMember entity, string key)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

        cube.transform.position = new Vector3(0, 0, 0);

        // add "player" to map of players
        members.Add(entity, cube);
    }

    void OnEntityRemove(SquadMember entity, string key)
    {
        if (members.TryGetValue(entity, out GameObject cube) && cube != null)
        {
            Destroy(cube);
        }

        members.Remove(entity);
    }


    void OnEntityMove(SquadMember entity, string key)
    {
        Debug.LogFormat("Member changed {0}", entity);
    }

    void OnApplicationQuit()
    {
    }
}
