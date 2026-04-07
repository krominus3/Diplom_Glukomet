using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public bool useSavedData = true;
    public Transform defaultSpawnPoint;

    [Header("Fallback")]
    public Vector3 fallbackPosition = Vector3.zero;
    public Vector3 fallbackRotation = Vector3.zero;

    private GameObject player;

    void Start()
    {
        SpawnPlayer();

        // После спавна очищаем сохраненную позицию, чтобы при следующем переходе
        // использовалась позиция спавнера нового уровня
        if (useSavedData && PlayerData.HasSave() && player != null)
        {
            // Проверяем, не загрузили ли мы старую позицию
            if (defaultSpawnPoint != null)
            {
                // Сравниваем с точкой спавна по умолчанию
                if (Vector3.Distance(PlayerData.lastPosition, defaultSpawnPoint.position) > 50f)
                {
                    Debug.Log("PlayerSpawner: Сохраненная позиция слишком далеко, используем спавн по умолчанию");
                    player.transform.position = defaultSpawnPoint.position;
                    player.transform.rotation = defaultSpawnPoint.rotation;
                }
            }
        }
    }

    void SpawnPlayer()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("PlayerSpawner: Игрок не найден");
            return;
        }

        // Проверяем, есть ли сохранение и не переходим ли мы на новый уровень
        if (useSavedData && PlayerData.HasSave())
        {
            PlayerData.LoadSavedData();

            // Если сохраненный уровень отличается от текущего, используем спавн по умолчанию
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (PlayerData.currentLevel != currentScene)
            {
                Debug.Log($"PlayerSpawner: Переход на новый уровень ({currentScene}), используем спавн по умолчанию");
                UseDefaultSpawn();
                return;
            }

            // Тот же уровень - восстанавливаем позицию
            player.transform.position = PlayerData.lastPosition;
            player.transform.rotation = PlayerData.lastRotation;
            Debug.Log($"PlayerSpawner: Игрок восстановлен на позиции {PlayerData.lastPosition}");
        }
        else
        {
            UseDefaultSpawn();
        }
    }

    void UseDefaultSpawn()
    {
        if (defaultSpawnPoint != null)
        {
            player.transform.position = defaultSpawnPoint.position;
            player.transform.rotation = defaultSpawnPoint.rotation;
            Debug.Log($"PlayerSpawner: Игрок заспавнен на точке {defaultSpawnPoint.name} (позиция {defaultSpawnPoint.position})");
        }
        else
        {
            player.transform.position = fallbackPosition;
            player.transform.rotation = Quaternion.Euler(fallbackRotation);
            Debug.Log($"PlayerSpawner: Игрок заспавнен на запасной позиции {fallbackPosition}");
        }
    }
}