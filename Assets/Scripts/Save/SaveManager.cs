using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    [Header("Settings")]
    public bool showDebugMessages = true;

    private GameObject player;
    private bool isRestarting = false;
    private string sceneToLoad = "";

    void Awake()
    {
        // Синглтон, который не уничтожается при загрузке сцены
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("SaveManager создан и не будет уничтожен");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        // Подписываемся на событие загрузки сцены
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (showDebugMessages)
            Debug.Log($"Сцена {scene.name} загружена. Восстанавливаем игрока...");

        // Восстанавливаем позицию игрока после загрузки сцены
        RestorePlayerPosition();
    }

    /// <summary>
    /// Перезапустить уровень с сохранением позиции
    /// </summary>
    public void RestartLevel()
    {
        if (isRestarting) return;

        isRestarting = true;

        // Сохраняем позицию игрока ПЕРЕД перезагрузкой
        SaveCurrentPlayerPosition();

        // Перезагружаем текущую сцену
        string currentScene = SceneManager.GetActiveScene().name;
        sceneToLoad = currentScene;

        if (showDebugMessages)
            Debug.Log($"Перезапуск уровня {currentScene}...");

        SceneManager.LoadScene(currentScene);
    }

    /// <summary>
    /// Сохранить текущую позицию игрока
    /// </summary>
    void SaveCurrentPlayerPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            string currentScene = SceneManager.GetActiveScene().name;
            PlayerData.SavePlayerData(player.transform, currentScene);

            if (showDebugMessages)
                Debug.Log($"Сохранена позиция: {player.transform.position}");
        }
        else
        {
            Debug.LogWarning("Игрок не найден для сохранения!");
        }
    }

    /// <summary>
    /// Восстановить позицию игрока после загрузки сцены
    /// </summary>
    void RestorePlayerPosition()
    {
        // Ищем игрока
        player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogWarning("Игрок не найден в сцене, ждем...");
            // Пробуем еще раз через кадр
            Invoke(nameof(RestorePlayerPosition), 0.1f);
            return;
        }

        // Проверяем, есть ли сохраненные данные
        if (PlayerData.hasData || PlayerData.HasSave())
        {
            // Загружаем данные если их нет в памяти
            if (!PlayerData.hasData)
                PlayerData.LoadSavedData();

            // Восстанавливаем позицию
            player.transform.position = PlayerData.lastPosition;
            player.transform.rotation = PlayerData.lastRotation;

            if (showDebugMessages)
                Debug.Log($"Игрок восстановлен на позиции: {PlayerData.lastPosition}");

            // Очищаем флаг рестарта
            isRestarting = false;
        }
        else
        {
            if (showDebugMessages)
                Debug.Log("Нет сохраненных данных, игрок остается на стартовой позиции");

            isRestarting = false;
        }
    }

    /// <summary>
    /// Загрузить уровень с сохранением позиции
    /// </summary>
    public void LoadLevelWithSave(string sceneName)
    {
        if (isRestarting) return;

        isRestarting = true;

        // Сохраняем позицию
        SaveCurrentPlayerPosition();

        // Сохраняем имя сцены в PlayerData
        PlayerData.currentLevel = sceneName;
        PlayerData.SavePlayerData(player.transform, sceneName);

        // Загружаем новую сцену
        sceneToLoad = sceneName;

        if (showDebugMessages)
            Debug.Log($"Загрузка уровня {sceneName}...");

        SceneManager.LoadScene(sceneName);
    }
}