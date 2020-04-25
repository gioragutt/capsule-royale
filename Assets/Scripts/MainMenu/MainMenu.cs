using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlaySquad()
    {
        SceneManager.LoadScene("SquadArrangement");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
