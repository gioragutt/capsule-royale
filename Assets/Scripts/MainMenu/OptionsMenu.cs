using System;
using UnityEngine;
using UnityEngine.Events;

public class OptionsMenu : MonoBehaviour
{
    public delegate void OnBack();

    public UnityEvent onBack;

    public TMPro.TMP_InputField endpointInput;
    public TMPro.TextMeshProUGUI serverLabel;

    private string initialEndpoint;

    // Start is called before the first frame update
    void Start()
    {
        if (string.IsNullOrWhiteSpace(PlayerPrefs.GetString("serverEndpoint")))
        {
            PlayerPrefs.SetString("serverEndpoint", "ws://localhost:2567");
        }

        initialEndpoint = PlayerPrefs.GetString("serverEndpoint");
        endpointInput.text = initialEndpoint;
        ColyseusClient.Instance.ConnectToServer(initialEndpoint);
    }

    public void Cancel()
    {
        endpointInput.text = initialEndpoint;
        GoBack();
    }

    private void GoBack()
    {
        serverLabel.color = Color.white;
        onBack.Invoke();
    }

    public void Save()
    {
        string newEndpoint = endpointInput.text;
        if (newEndpoint == initialEndpoint)
        {
            return;
        }

        PlayerPrefs.SetString("serverEndpoint", newEndpoint);
        try
        {
            ColyseusClient.Instance.ConnectToServer(newEndpoint);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            serverLabel.color = Color.red;
            return;
        }
        GoBack();
    }
}
