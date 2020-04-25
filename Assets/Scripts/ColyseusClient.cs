using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Colyseus;
using Colyseus.Schema;
using UnityEngine;

public class ColyseusClient : MonoBehaviour
{
    public static ColyseusClient Instance { get; private set; }
    public Client Client { get; private set; }

    public string Endpoint
    {
        get
        {
            if (string.IsNullOrWhiteSpace(PlayerPrefs.GetString("serverEndpoint")))
            {
                PlayerPrefs.SetString("serverEndpoint", "ws://localhost:2567");
            }

            return PlayerPrefs.GetString("serverEndpoint");
        }
        set
        {
            PlayerPrefs.SetString("serverEndpoint", value);
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ConnectToServer(Endpoint);
    }

    public void ConnectToServer(string endpoint)
    {
        Debug.Log("Connecting to " + endpoint);

        Client = ColyseusManager.Instance.CreateClient(endpoint);
    }

    private static Dictionary<string, object> ConnectionOptions(string methodName) =>
        new Dictionary<string, object>()
        {
            ["name"] = "From " + methodName,
            ["bla"] = "kapara"
        };

    public async Task<Room<T>> CreateRoom<T>(string roomName) where T : Schema
    {
        return SaveRoom(await Client.Create<T>(roomName, ConnectionOptions("CreateRoom")));
    }

    public async Task<Room<T>> JoinOrCreateRoom<T>(string roomName) where T : Schema
    {
        return SaveRoom(await Client.JoinOrCreate<T>(roomName, ConnectionOptions("JoinOrCreateRoom")));
    }

    public async Task<Room<T>> JoinRoom<T>(string roomId, Dictionary<string, object> options) where T : Schema
    {
        Dictionary<string, object> opts = ConnectionOptions("JoinRoom");
        var combined = opts.Union(options).ToDictionary(k => k.Key, k => k.Value);
        return SaveRoom(await Client.JoinById<T>(roomId, combined));
    }

    public async Task<Room<T>> JoinRoom<T>(string roomId) where T : Schema
    {
        return await JoinRoom<T>(roomId, new Dictionary<string, object>());
    }

    public async Task<Room<T>> ReconnectRoom<T>()
    {
        string roomId = PlayerPrefs.GetString("roomId");
        string sessionId = PlayerPrefs.GetString("sessionId");
        if (string.IsNullOrEmpty(sessionId) || string.IsNullOrEmpty(roomId))
        {
            Debug.Log("Cannot Reconnect without having a roomId and sessionId");
            return null;
        }

        return SaveRoom(await Client.Reconnect<T>(roomId, sessionId));
    }

    private static Room<T> SaveRoom<T>(Room<T> room)
    {
        PlayerPrefs.SetString("roomId", room.Id);
        PlayerPrefs.SetString("sessionId", room.SessionId);
        PlayerPrefs.Save();
        return room;
    }
}
