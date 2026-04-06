using UnityEngine;
using System.Collections;

// Статический класс для хранения данных между сценами
public static class PlayerData
{
    public static Vector3 lastPosition;
    public static Quaternion lastRotation;
    public static bool hasData = false;

    public static void SavePlayerData(Transform playerTransform)
    {
        lastPosition = playerTransform.position;
        lastRotation = playerTransform.rotation;
        hasData = true;
        Debug.Log($"Сохранены данные игрока: Позиция {lastPosition}, Поворот {lastRotation.eulerAngles}");
    }

    public static void ClearData()
    {
        hasData = false;
        Debug.Log("Данные игрока очищены");
    }
}