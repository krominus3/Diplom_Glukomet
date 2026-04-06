using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public bool useSavedData = true; // Использовать сохраненные данные
    public Transform defaultSpawnPoint; // Точка спавна по умолчанию

    [Header("Fallback Settings")]
    public Vector3 fallbackPosition = Vector3.zero;
    public Vector3 fallbackRotation = Vector3.zero;

    void Start()
    {
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogError("Игрок не найден в сцене!");
            return;
        }

        if (useSavedData && PlayerData.hasData)
        {
            // Восстанавливаем позицию
            player.transform.position = PlayerData.lastPosition;
            Debug.Log($"Позиция игрока восстановлена: {PlayerData.lastPosition}");

            // Восстанавливаем поворот
            player.transform.rotation = PlayerData.lastRotation;
            Debug.Log($"Поворот игрока восстановлен: {PlayerData.lastRotation.eulerAngles}");

            // Очищаем данные, чтобы не использовать их повторно
            PlayerData.ClearData();
        }
        else if (defaultSpawnPoint != null)
        {
            // Используем точку спавна по умолчанию
            player.transform.position = defaultSpawnPoint.position;
            player.transform.rotation = defaultSpawnPoint.rotation;
            Debug.Log($"Игрок заспавнен на точке: {defaultSpawnPoint.name}");
        }
        else
        {
            // Используем запасные координаты
            player.transform.position = fallbackPosition;
            player.transform.rotation = Quaternion.Euler(fallbackRotation);
            Debug.Log($"Игрок заспавнен на запасной позиции: {fallbackPosition}");
        }
    }

    // Метод для ручного обновления позиции игрока
    public void UpdatePlayerPosition(Vector3 newPosition)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = newPosition;
        }
    }

    // Метод для ручного обновления поворота игрока
    public void UpdatePlayerRotation(Quaternion newRotation)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.rotation = newRotation;
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