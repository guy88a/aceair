using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingSceneController : MonoBehaviour
{
    [Header("Visual Roots")]
    [SerializeField] private GameObject bootLoadingRoot;
    [SerializeField] private GameObject runStartLoadingRoot;

    private void Start()
    {
        SetupVisuals();
        StartLoading();
    }

    private void SetupVisuals()
    {
        LoadingVisualType visualType = GameStateManager.Instance.LoadingVisualType;

        bootLoadingRoot.SetActive(visualType == LoadingVisualType.Boot);
        runStartLoadingRoot.SetActive(visualType == LoadingVisualType.RunStart);
    }

    private void StartLoading()
    {
        StartCoroutine(LoadTargetSceneAsync());
    }

    private IEnumerator LoadTargetSceneAsync()
    {
        string targetSceneName = GetTargetSceneName(GameStateManager.Instance.LoadingTargetState);

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);
        loadOperation.allowSceneActivation = false;

        while (loadOperation.progress < 0.9f)
        {
            UpdateProgress(loadOperation.progress / 0.9f);
            yield return null;
        }

        UpdateProgress(1f);

        loadOperation.allowSceneActivation = true;

        while (!loadOperation.isDone)
            yield return null;

        CompleteLoading();
    }

    private void UpdateProgress(float progress)
    {
        Debug.Log($"Loading Progress: {progress:P0}");
    }

    private void CompleteLoading()
    {
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(GetTargetSceneName(GameStateManager.Instance.LoadingTargetState)));
        SceneManager.UnloadSceneAsync("Loading");
        GameStateManager.Instance.SetState(GameStateManager.Instance.LoadingTargetState);
    }

    private string GetTargetSceneName(GameState targetState)
    {
        switch (targetState)
        {
            case GameState.MainMenu:
                return "MainMenu";

            case GameState.Run:
                return "Gameplay";

            default:
                Debug.LogError($"No scene mapped for target state: {targetState}");
                return "MainMenu";
        }
    }
}