using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public void OnPlayPressed()
    {
        GameStateManager.Instance.StartLoading(GameState.Run);
    }

    public void OnSettingsPressed()
    {
        Debug.Log("Settings pressed.");
    }

    public void OnQuitPressed()
    {
        Debug.Log("Quit pressed.");

        Application.Quit();
    }
}