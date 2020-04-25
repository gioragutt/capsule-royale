using CapsuleRoyale.BattleRoyaleMatchmaking;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlaySquad()
    {
        SceneManager.LoadScene("SquadArrangement");
    }

    public async void PlaySolo()
    {
        Debug.Log("Starting solo game");
        var room = await ColyseusClient.Instance.JoinOrCreateRoom<BattleRoyaleMatchmakingState>("battle_royale_matchmaking");
        ColyseusClient.Instance.StoreRoom(room);
        SceneManager.LoadScene("BattleRoyaleMatchmaking");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
