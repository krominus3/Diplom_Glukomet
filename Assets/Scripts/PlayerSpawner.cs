using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public bool useSavedData = true;
    public Transform defaultSpawnPoint;

    [Header("Fallback")]
    public Vector3 fallbackPosition = Vector3.zero;
    public Vector3 fallbackRotation = Vector3.zero;

    [Header("Debug")]
    public bool showSpawnMessage = true;

    private GameObject player;
    private bool hasSpawned = false;

    void Start()
    {
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogError("❌ Игрок не найден!");
            return;
        }

        // ВАЖНО: Сначала загружаем сохраненные данные, если их нет
        if (useSavedData && !PlayerData.hasData)
        {
            // Пытаемся загрузить из PlayerPrefs
            PlayerData.LoadSavedData();
        }

        if (useSavedData && PlayerData.hasData)
        {
            // Восстанавливаем позицию
            player.transform.position = PlayerData.lastPosition;
            Debug.Log($"📍 Позиция игрока восстановлена: {PlayerData.lastPosition}");

            // Восстанавливаем поворот
            player.transform.rotation = PlayerData.lastRotation;
            Debug.Log($"🔄 Поворот игрока восстановлен: {PlayerData.lastRotation.eulerAngles}");

            if (showSpawnMessage)
                Debug.Log($"✅ Игрок заспавнен на сохраненной позиции");
        }
        else if (defaultSpawnPoint != null)
        {
            player.transform.position = defaultSpawnPoint.position;
            player.transform.rotation = defaultSpawnPoint.rotation;
            Debug.Log($"📍 Игрок заспавнен на точке: {defaultSpawnPoint.name}");
        }
        else
        {
            player.transform.position = fallbackPosition;
            player.transform.rotation = Quaternion.Euler(fallbackRotation);
            Debug.Log($"📍 Игрок заспавнен на запасной позиции: {fallbackPosition}");
        }

        hasSpawned = true;
    }

    // Метод для принудительного спавна на сохраненной позиции
    public void RespawnAtSavedPosition()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        if (player != null && PlayerData.hasData)
        {
            player.transform.position = PlayerData.lastPosition;
            player.transform.rotation = PlayerData.lastRotation;
            Debug.Log("🔄 Игрок телепортирован на сохраненную позицию");
        }
    }

    // Метод для ручного обновления позиции
    public void UpdatePlayerPosition(Vector3 newPosition)
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            player.transform.position = newPosition;
        }
    }

    void OnDrawGizmos()
    {
        if (defaultSpawnPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(defaultSpawnPoint.position, 0.5f);
            Gizmos.DrawRay(defaultSpawnPoint.position, defaultSpawnPoint.forward * 2f);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(fallbackPosition, 0.5f);
            Gizmos.DrawRay(fallbackPosition, Quaternion.Euler(fallbackRotation) * Vector3.forward * 2f);
        }
    }
}