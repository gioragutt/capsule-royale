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

    void Start()
    {
        initialEndpoint = ColyseusClient.Instance.Endpoint;
        endpointInput.text = initialEndpoint;
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

        ColyseusClient.Instance.Endpoint = newEndpoint;
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
