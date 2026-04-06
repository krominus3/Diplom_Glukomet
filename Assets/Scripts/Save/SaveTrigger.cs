using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveTrigger : MonoBehaviour
{
    [Header("Settings")]
    public string triggerTag = "Player";
    public bool showSaveMessage = true;
    public float messageDuration = 2f;

    [Header("UI (опционально)")]
    public GameObject saveNotificationPanel;
    public TMPro.TextMeshProUGUI saveNotificationText;

    private bool hasSaved = false;

    void OnTriggerEnter(Collider other)
    {
        if (!hasSaved && other.CompareTag(triggerTag))
        {
            SaveGame();
            hasSaved = true;
        }
    }

    void SaveGame()
    {
        // Находим игрока
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Игрок не найден для сохранения!");
            return;
        }

        // Получаем имя текущей сцены
        string currentScene = SceneManager.GetActiveScene().name;

        // Сохраняем данные
        PlayerData.SavePlayerData(player.transform, currentScene);

        // Показываем уведомление
        ShowSaveNotification();

        Debug.Log($"Игра сохранена на уровне '{currentScene}'");
        Destroy(gameObject);
    }

    void ShowSaveNotification()
    {
        if (saveNotificationPanel != null)
        {
            saveNotificationPanel.SetActive(true);
            Invoke(nameof(HideNotification), messageDuration);
        }

        if (saveNotificationText != null)
        {
            saveNotificationText.text = "ИГРА СОХРАНЕНА!";
        }
    }

    void HideNotification()
    {
        if (saveNotificationPanel != null)
        {
            saveNotificationPanel.SetActive(false);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        Collider col = GetComponent<Collider>();
        if (col != null && col.isTrigger)
        {
            if (col is BoxCollider)
                Gizmos.DrawWireCube(transform.position + (col as BoxCollider).center, (col as BoxCollider).size);
            else if (col is SphereCollider)
                Gizmos.DrawWireSphere(transform.position + (col as SphereCollider).center, (col as SphereCollider).radius);
        }
    }
}