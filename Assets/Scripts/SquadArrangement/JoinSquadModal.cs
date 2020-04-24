using UnityEngine;

public class JoinSquadModal : MonoBehaviour
{
    public SquadArrangementRoom room;
    public TMPro.TMP_InputField inviteIdInput;
    public TMPro.TextMeshProUGUI errorMessageText;
    public GameObject inSquadGui;

    public async void Join()
    {
        if (await room.JoinExisting(inviteIdInput.text))
        {
            errorMessageText.text = "";
            gameObject.SetActive(false);
            inSquadGui.SetActive(true);
        }
        else
        {
            errorMessageText.text = "Error: Invalid Invite ID";
        }
    }
}
