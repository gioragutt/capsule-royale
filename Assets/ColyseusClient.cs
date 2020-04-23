using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Colyseus;
using Colyseus.Schema;
using UnityEngine;
using UnityEngine.UI;

public class ColyseusClient : MonoBehaviour
{
    public InputField endpointField;
    public Text idText;
    public Text sessionIdText;

    public Client Client { get; private set; }

    void Start()
    {
        DontDestroyOnLoad(this);
    }

    public void ConnectToServer()
    {
        string endpoint = endpointField.text;

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
        return await Client.Create<T>(roomName, ConnectionOptions("CreateRoom"));
    }

    public async Task<Room<T>> JoinOrCreateRoom<T>(string roomName) where T : Schema
    {
        return await Client.JoinOrCreate<T>(roomName, ConnectionOptions("JoinOrCreateRoom"));
    }

    public async Task<Room<T>> JoinRoom<T>(string roomName) where T : Schema
    {
        return await Client.Join<T>(roomName, ConnectionOptions("JoinRoom"));
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

        return await Client.Reconnect<T>(roomId, sessionId);
    }
}
