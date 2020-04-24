using UnityEngine;

public class ToggleReady : MonoBehaviour
{
    public TMPro.TextMeshProUGUI readyText;
    public SquadArrangementRoom room;

    public async void Toggle()
    {
        if (await room.SendReadyMessage())
        {
            readyText.text = "Unready";
        }
        else
        {
            readyText.text = "Ready";
        }
    }
}
