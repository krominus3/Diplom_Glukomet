using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    [Header("Settings")]
    public bool showDebugMessages = true;

    private bool isRestarting = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("SaveManager создан");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (showDebugMessages)
            Debug.Log($"Сцена {scene.name} загружена");

        RestorePlayerPosition();
    }

    public void RestartLevel()
    {
        if (isRestarting) return;
        isRestarting = true;

        SaveAllData();

        string currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentScene);

        isRestarting = false;
    }

    public void HardRestart()
    {
        if (isRestarting) return;
        isRestarting = true;

        string currentScene = SceneManager.GetActiveScene().name;

        PlayerSpawner spawner = FindFirstObjectByType<PlayerSpawner>();

        if (spawner != null)
        {
            PlayerData.SaveSpawnerPosition(spawner.transform.position, spawner.transform.rotation);
        }

        PlayerData.SaveOnlyLevel(currentScene);

        SceneManager.LoadScene(currentScene);

        isRestarting = false;
    }

    private void SaveAllData()
    {
        //GameObject player = GameObject.FindGameObjectWithTag("Player");
        //if (player != null)
        //{
        //    PlayerData.SavePlayerPosition(player.transform);
        //}

        if (RuleManager.Instance != null)
        {
            RuleManager.Instance.SaveRules();
        }

        PlayerData.currentLevel = SceneManager.GetActiveScene().name;
        PlayerData.SaveLevel();

        if (showDebugMessages)
            Debug.Log("Все данные сохранены");
    }

    private void RestorePlayerPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Invoke(nameof(RestorePlayerPosition), 0.1f);
            return;
        }

        if (PlayerData.LoadSavedData())
        {
            player.transform.position = PlayerData.lastPosition;
            player.transform.rotation = PlayerData.lastRotation;

            if (showDebugMessages)
                Debug.Log($"Позиция игрока восстановлена: {PlayerData.lastPosition}");
        }
        else
        {
            if (showDebugMessages)
                Debug.Log("Нет сохраненных данных");
        }
    }

    public void LoadLevel(string sceneName)
    {
        if (isRestarting) return;
        isRestarting = true;

        SaveAllData();
        PlayerData.currentLevel = sceneName;
        PlayerData.SaveLevel();

        SceneManager.LoadScene(sceneName);

        isRestarting = false;
    }
}