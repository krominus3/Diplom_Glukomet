using UnityEngine;
using System.Collections.Generic;

public static class PlayerData
{
    // Ключи для PlayerPrefs
    private const string SAVE_EXISTS = "SaveExists";
    private const string LEVEL_NAME = "SavedLevel";
    private const string POSITION_X = "PlayerPosX";
    private const string POSITION_Y = "PlayerPosY";
    private const string POSITION_Z = "PlayerPosZ";
    private const string ROTATION_X = "PlayerRotX";
    private const string ROTATION_Y = "PlayerRotY";
    private const string ROTATION_Z = "PlayerRotZ";
    private const string ROTATION_W = "PlayerRotW";

    // Текущие данные (для быстрого доступа)
    public static Vector3 lastPosition;
    public static Quaternion lastRotation;
    public static string currentLevel;
    public static bool hasData = false;

    /// <summary>
    /// Сохранить данные игрока (позиция, поворот, уровень)
    /// </summary>
    public static void SavePlayerData(Transform playerTransform, string levelSceneName)
    {
        lastPosition = playerTransform.position;
        lastRotation = playerTransform.rotation;
        currentLevel = levelSceneName;
        hasData = true;

        // Сохраняем в PlayerPrefs
        PlayerPrefs.SetInt(SAVE_EXISTS, 1);
        PlayerPrefs.SetString(LEVEL_NAME, levelSceneName);

        PlayerPrefs.SetFloat(POSITION_X, lastPosition.x);
        PlayerPrefs.SetFloat(POSITION_Y, lastPosition.y);
        PlayerPrefs.SetFloat(POSITION_Z, lastPosition.z);

        PlayerPrefs.SetFloat(ROTATION_X, lastRotation.x);
        PlayerPrefs.SetFloat(ROTATION_Y, lastRotation.y);
        PlayerPrefs.SetFloat(ROTATION_Z, lastRotation.z);
        PlayerPrefs.SetFloat(ROTATION_W, lastRotation.w);

        PlayerPrefs.Save();

        Debug.Log($"Прогресс сохранен: Уровень '{levelSceneName}', Позиция {lastPosition}, Поворот {lastRotation.eulerAngles}");
    }

    /// <summary>
    /// Загрузить сохраненные данные
    /// </summary>
    public static bool LoadSavedData()
    {
        if (PlayerPrefs.GetInt(SAVE_EXISTS, 0) == 0)
        {
            hasData = false;
            Debug.Log("❌ Сохранений не найдено");
            return false;
        }

        // Загружаем уровень
        currentLevel = PlayerPrefs.GetString(LEVEL_NAME, "");

        // Загружаем позицию
        float posX = PlayerPrefs.GetFloat(POSITION_X, 0);
        float posY = PlayerPrefs.GetFloat(POSITION_Y, 0);
        float posZ = PlayerPrefs.GetFloat(POSITION_Z, 0);
        lastPosition = new Vector3(posX, posY, posZ);

        // Загружаем поворот
        float rotX = PlayerPrefs.GetFloat(ROTATION_X, 0);
        float rotY = PlayerPrefs.GetFloat(ROTATION_Y, 0);
        float rotZ = PlayerPrefs.GetFloat(ROTATION_Z, 0);
        float rotW = PlayerPrefs.GetFloat(ROTATION_W, 1);
        lastRotation = new Quaternion(rotX, rotY, rotZ, rotW);

        hasData = true;
        Debug.Log($"Прогресс загружен: Уровень '{currentLevel}', Позиция {lastPosition}");
        return true;
    }

    /// <summary>
    /// Очистить все сохранения
    /// </summary>
    public static void ClearSave()
    {
        PlayerPrefs.DeleteKey(SAVE_EXISTS);
        PlayerPrefs.DeleteKey(LEVEL_NAME);
        PlayerPrefs.DeleteKey(POSITION_X);
        PlayerPrefs.DeleteKey(POSITION_Y);
        PlayerPrefs.DeleteKey(POSITION_Z);
        PlayerPrefs.DeleteKey(ROTATION_X);
        PlayerPrefs.DeleteKey(ROTATION_Y);
        PlayerPrefs.DeleteKey(ROTATION_Z);
        PlayerPrefs.DeleteKey(ROTATION_W);
        PlayerPrefs.Save();

        hasData = false;
        Debug.Log("Все сохранения очищены");
    }

    /// <summary>
    /// Проверить, есть ли сохранение
    /// </summary>
    public static bool HasSave()
    {
        return PlayerPrefs.GetInt(SAVE_EXISTS, 0) == 1;
    }
}