using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;

using Colyseus;
using Colyseus.Schema;

using GameDevWare.Serialization;

[Serializable]
class Metadata
{
	public string str;
	public int number;
}

[Serializable]
class CustomRoomAvailable : RoomAvailable
{
	public Metadata metadata;
}

class CustomData
{
	public int integer;
	public string str;
}

public class ColyseusClient : MonoBehaviour {

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

	public string roomName = "demo";

	protected Client client;
	protected Room<CapsuleRoyale.Demo.State> room;

	protected IndexedDictionary<CapsuleRoyale.Demo.Entity, GameObject> entities = new IndexedDictionary<CapsuleRoyale.Demo.Entity, GameObject>();

	// Use this for initialization
	void Start () {
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

	async void ConnectToServer ()
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

		//await client.Auth.Login();

		//var friends = await client.Auth.GetFriends();

		//// Update username
		//client.Auth.Username = "Jake";
		//await client.Auth.Save();
	}

	public async void CreateRoom()
	{
		room = await client.Create<CapsuleRoyale.Demo.State>(roomName, new Dictionary<string, object>() { });

		m_SessionIdText.text = "sessionId: " + room.SessionId;

		room.State.entities.OnAdd += OnEntityAdd;
		room.State.entities.OnRemove += OnEntityRemove;
		room.State.entities.OnChange += OnEntityMove;

		PlayerPrefs.SetString("roomId", room.Id);
		PlayerPrefs.SetString("sessionId", room.SessionId);
		PlayerPrefs.Save();

		room.OnLeave += (code) => Debug.Log("ROOM: ON LEAVE");
		room.OnError += (message) => Debug.LogError(message);
		room.OnStateChange += OnStateChangeHandler;
		room.OnMessage += OnMessage;
	}

	public async void JoinOrCreateRoom()
	{
		room = await client.JoinOrCreate<CapsuleRoyale.Demo.State>(roomName, new Dictionary<string, object>() { });

		m_SessionIdText.text = "sessionId: " + room.SessionId;

		room.State.entities.OnAdd += OnEntityAdd;
		room.State.entities.OnRemove += OnEntityRemove;
		room.State.entities.OnChange += OnEntityMove;

		PlayerPrefs.SetString("roomId", room.Id);
		PlayerPrefs.SetString("sessionId", room.SessionId);
		PlayerPrefs.Save();

		room.OnLeave += (code) => Debug.Log("ROOM: ON LEAVE");
		room.OnError += (message) => Debug.LogError(message);
		room.OnStateChange += OnStateChangeHandler;
		room.OnMessage += OnMessage;
	}

	public async void JoinRoom ()
	{
		room = await client.Join<CapsuleRoyale.Demo.State>(roomName, new Dictionary<string, object>() {});

		m_SessionIdText.text = "sessionId: " + room.SessionId;

		room.State.entities.OnAdd += OnEntityAdd;
		room.State.entities.OnRemove += OnEntityRemove;
		room.State.entities.OnChange += OnEntityMove;

		PlayerPrefs.SetString("roomId", room.Id);
		PlayerPrefs.SetString("sessionId", room.SessionId);
		PlayerPrefs.Save();

		room.OnLeave += (code) => Debug.Log("ROOM: ON LEAVE");
		room.OnError += (message) => Debug.LogError(message);
		room.OnStateChange += OnStateChangeHandler;
		room.OnMessage += OnMessage;
	}

	async void ReconnectRoom ()
	{
		string roomId = PlayerPrefs.GetString("roomId");
		string sessionId = PlayerPrefs.GetString("sessionId");
		if (string.IsNullOrEmpty(sessionId) || string.IsNullOrEmpty(roomId))
		{
			Debug.Log("Cannot Reconnect without having a roomId and sessionId");
			return;
		}

		room = await client.Reconnect<CapsuleRoyale.Demo.State>(roomId, sessionId);
		Debug.Log("Reconnected into room successfully.");
		m_SessionIdText.text = "sessionId: " + room.SessionId;

		room.State.entities.OnAdd += OnEntityAdd;
		room.State.entities.OnRemove += OnEntityRemove;
		room.State.entities.OnChange += OnEntityMove;

		room.OnError += (message) => Debug.LogError(message);

		room.OnStateChange += OnStateChangeHandler;
		room.OnMessage += OnMessage;
	}

	async void LeaveRoom()
	{
		await room.Leave(false);

		// Destroy player entities
		foreach (KeyValuePair<CapsuleRoyale.Demo.Entity, GameObject> entry in entities)
		{
			Destroy(entry.Value);
		}

		entities.Clear();
	}

	async void GetAvailableRooms()
	{
		var roomsAvailable = await client.GetAvailableRooms<CustomRoomAvailable>(roomName);

		Debug.Log("Available rooms (" + roomsAvailable.Length + ")");
		for (var i = 0; i < roomsAvailable.Length; i++)
		{
			Debug.Log("roomId: " + roomsAvailable[i].roomId);
			Debug.Log("maxClients: " + roomsAvailable[i].maxClients);
			Debug.Log("clients: " + roomsAvailable[i].clients);
			Debug.Log("metadata.str: " + roomsAvailable[i].metadata.str);
			Debug.Log("metadata.number: " + roomsAvailable[i].metadata.number);
		}
	}

	void SendMessage()
	{
		if (room != null)
		{
			room.Send("move_right");

			// Sending typed data to the server
			room.Send(new CustomData() {
				integer = 100,
				str = "Hello world!"
			});
		}
		else
		{
			Debug.Log("Room is not connected!");
		}
	}

	void OnMessage (object msg)
	{
		if (msg is CapsuleRoyale.Demo.Message)
		{
			var message = (CapsuleRoyale.Demo.Message)msg;
			Debug.Log("Received schema-encoded message:");
			Debug.Log("message.num => " + message.num + ", message.str => " + message.str);
		}
		else
		{
			// msgpack-encoded message
			var message = (IndexedDictionary<string, object>)msg;
			Debug.Log("Received msgpack-encoded message:");
			Debug.Log(message["hello"]);
		}
	}

	void OnStateChangeHandler (CapsuleRoyale.Demo.State state, bool isFirstState)
	{
		// Setup room first state
		Debug.Log("State has been updated!");
	}

	void OnEntityAdd(CapsuleRoyale.Demo.Entity entity, string key)
	{
		GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

		Debug.Log("Player add! x => " + entity.x + ", y => " + entity.y);

		cube.transform.position = new Vector3(entity.x, entity.y, 0);

		// add "player" to map of players
		entities.Add(entity, cube);
	}

	void OnEntityRemove(CapsuleRoyale.Demo.Entity entity, string key)
	{
		GameObject cube;
		entities.TryGetValue(entity, out cube);
		Destroy(cube);

		entities.Remove(entity);
	}


	void OnEntityMove(CapsuleRoyale.Demo.Entity entity, string key)
	{
		if (entities.TryGetValue(entity, out GameObject cube) && cube != null)
		{
			Debug.Log(entity);
			cube.transform.position = new Vector3(entity.x, entity.y, 0);
		}
	}

	void OnApplicationQuit()
	{
	}
}
