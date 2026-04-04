using UnityEngine;

public class LoadingSceneController : MonoBehaviour
{
    [Header("Visual Roots")]
    [SerializeField] private GameObject bootLoadingRoot;
    [SerializeField] private GameObject runStartLoadingRoot;

    private void Awake()
    {

    }

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

    }

    private void UpdateProgress(float progress)
    {

    }

    private void CompleteLoading()
    {

    }
}