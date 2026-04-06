using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    public Button newGameButton;
    public Button continueButton;
    public Button quitButton;

    [Header("UI")]
    public GameObject mainMenuPanel;
    public GameObject loadingPanel;
    public TMPro.TextMeshProUGUI loadingText;
    public Slider loadingProgressBar;

    [Header("Loading Settings")]
    public string firstLevelName = "Level1";

    private bool isLoading = false;

    void Start()
    {
        if (newGameButton != null)
            newGameButton.onClick.AddListener(NewGame);

        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueGame);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        UpdateContinueButton();

        if (loadingPanel != null)
            loadingPanel.SetActive(false);
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }

    void UpdateContinueButton()
    {
        if (continueButton != null)
        {
            continueButton.interactable = PlayerData.HasSave();
        }
    }

    public void NewGame()
    {
        if (isLoading) return;
        PlayerData.ClearSave();
        StartCoroutine(LoadLevel(firstLevelName));
    }

    public void ContinueGame()
    {
        if (isLoading) return;

        if (PlayerData.LoadSavedData())
        {
            StartCoroutine(LoadLevel(PlayerData.currentLevel));
        }
        else
        {
            NewGame();
        }
    }

    IEnumerator LoadLevel(string levelName)
    {
        isLoading = true;

        // Показываем загрузку
        if (loadingPanel != null)
            loadingPanel.SetActive(true);
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        // Начинаем асинхронную загрузку
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(levelName);
        asyncLoad.allowSceneActivation = false;

        // Обновляем прогресс
        while (asyncLoad.progress < 0.9f)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            if (loadingText != null)
                loadingText.text = $"ЗАГРУЗКА... {(progress * 100):F0}%";

            if (loadingProgressBar != null)
                loadingProgressBar.value = progress;

            yield return null;
        }

        // Загрузка завершена, активируем сцену
        asyncLoad.allowSceneActivation = true;
        isLoading = false;
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}