using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelRestarter : MonoBehaviour
{
    [Header("Restart Settings")]
    public KeyCode restartKey = KeyCode.R;
    public float restartDelay = 0.1f;
    public bool showRestartMessage = true;

    [Header("UI")]
    public GameObject restartNotificationPanel;
    public TMPro.TextMeshProUGUI restartNotificationText;

    [Header("Player Scripts (отключаются при рестарте)")]
    public MonoBehaviour[] playerScriptsToDisable;

    private bool isRestarting = false;

    void Update()
    {
        if (!isRestarting && Input.GetKeyDown(restartKey))
        {
            StartCoroutine(RestartLevel());
        }
    }

    IEnumerator RestartLevel()
    {
        isRestarting = true;

        // ПРОВЕРЯЕМ: есть ли сохраненные данные
        if (!PlayerData.HasSave())
        {
            Debug.LogWarning("Нет сохраненных данных! Создаем сохранение текущей позиции.");

            // Если нет сохранения, создаем его с текущей позицией
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                string currentSceneTemp = SceneManager.GetActiveScene().name;
                PlayerData.SavePlayerData(player.transform, currentSceneTemp);
            }
        }
        else
        {
            Debug.Log("Найдены сохраненные данные для восстановления");
        }

        // Показываем уведомление
        if (showRestartMessage && restartNotificationPanel != null)
        {
            restartNotificationPanel.SetActive(true);
            if (restartNotificationText != null)
                restartNotificationText.text = "ПЕРЕЗАПУСК...";
        }

        // Отключаем скрипты игрока
        foreach (var script in playerScriptsToDisable)
        {
            if (script != null)
                script.enabled = false;
        }

        // Небольшая задержка
        yield return new WaitForSeconds(restartDelay);

        // Перезагружаем текущую сцену
        string currentScene = SceneManager.GetActiveScene().name;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(currentScene);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Сцена перезагружена
        // PlayerSpawner на сцене сам восстановит позицию из PlayerData
        Debug.Log("Уровень перезагружен, позиция будет восстановлена из сохранения");

        isRestarting = false;
    }

    public void RestartNow()
    {
        if (!isRestarting)
        {
            StartCoroutine(RestartLevel());
        }
    }
}