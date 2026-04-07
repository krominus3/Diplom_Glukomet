using UnityEngine;
using System.Collections.Generic;

public static class PlayerData
{
    private const string SAVE_EXISTS = "SaveExists";
    private const string LEVEL_NAME = "SavedLevel";
    private const string POSITION_X = "PlayerPosX";
    private const string POSITION_Y = "PlayerPosY";
    private const string POSITION_Z = "PlayerPosZ";
    private const string ROTATION_X = "PlayerRotX";
    private const string ROTATION_Y = "PlayerRotY";
    private const string ROTATION_Z = "PlayerRotZ";
    private const string ROTATION_W = "PlayerRotW";

    private const string UNLOCKED_RULES = "UnlockedRules";
    private const string CURRENT_RULE = "CurrentRule";

    public static Vector3 lastPosition;
    public static Quaternion lastRotation;
    public static string currentLevel;
    public static bool hasData = false;

    public static void SavePlayerPosition(Transform playerTransform)
    {
        lastPosition = playerTransform.position;
        lastRotation = playerTransform.rotation;
        hasData = true;

        PlayerPrefs.SetFloat(POSITION_X, lastPosition.x);
        PlayerPrefs.SetFloat(POSITION_Y, lastPosition.y);
        PlayerPrefs.SetFloat(POSITION_Z, lastPosition.z);

        PlayerPrefs.SetFloat(ROTATION_X, lastRotation.x);
        PlayerPrefs.SetFloat(ROTATION_Y, lastRotation.y);
        PlayerPrefs.SetFloat(ROTATION_Z, lastRotation.z);
        PlayerPrefs.SetFloat(ROTATION_W, lastRotation.w);

        PlayerPrefs.Save();
    }

    public static void SaveSpawnerPosition(Vector3 position, Quaternion rotation)
    {
        lastPosition = position;
        lastRotation = rotation;
        hasData = true;

        PlayerPrefs.SetFloat(POSITION_X, position.x);
        PlayerPrefs.SetFloat(POSITION_Y, position.y);
        PlayerPrefs.SetFloat(POSITION_Z, position.z);

        PlayerPrefs.SetFloat(ROTATION_X, rotation.x);
        PlayerPrefs.SetFloat(ROTATION_Y, rotation.y);
        PlayerPrefs.SetFloat(ROTATION_Z, rotation.z);
        PlayerPrefs.SetFloat(ROTATION_W, rotation.w);

        PlayerPrefs.Save();
    }

    public static void SaveOnlyLevel(string levelName)
    {
        currentLevel = levelName;
        PlayerPrefs.SetInt(SAVE_EXISTS, 1);
        PlayerPrefs.SetString(LEVEL_NAME, levelName);
        PlayerPrefs.Save();
    }

    public static void SaveLevel()
    {
        PlayerPrefs.SetInt(SAVE_EXISTS, 1);
        PlayerPrefs.SetString(LEVEL_NAME, currentLevel);
        PlayerPrefs.Save();
    }

    public static void SavePlayerRules(RuleInventory inventory)
    {
        if (inventory == null)
        {
            Debug.LogWarning("RuleInventory равен null");
            return;
        }

        List<string> ruleNames = new List<string>();

        // Получаем правила через availableRules
        var rules = inventory.GetAllRules();
        Debug.Log($"Найдено правил в инвентаре: {rules.Count}");

        foreach (var rule in rules)
        {
            if (rule != null)
            {
                ruleNames.Add(rule.ruleName);
                Debug.Log($"Добавлено правило для сохранения: {rule.ruleName}");
            }
        }

        string rulesString = string.Join(",", ruleNames);
        string currentRuleName = inventory.GetCurrentRuleName;

        PlayerPrefs.SetString(UNLOCKED_RULES, rulesString);
        PlayerPrefs.SetString(CURRENT_RULE, currentRuleName);
        PlayerPrefs.SetInt(SAVE_EXISTS, 1);
        PlayerPrefs.Save();

        Debug.Log($"Сохранено правил: {ruleNames.Count}, текущее: {currentRuleName}");
        Debug.Log($"Строка правил: {rulesString}");
    }

    public static bool LoadSavedData()
    {
        if (PlayerPrefs.GetInt(SAVE_EXISTS, 0) == 0)
        {
            hasData = false;
            return false;
        }

        currentLevel = PlayerPrefs.GetString(LEVEL_NAME, "");

        float posX = PlayerPrefs.GetFloat(POSITION_X, 0);
        float posY = PlayerPrefs.GetFloat(POSITION_Y, 0);
        float posZ = PlayerPrefs.GetFloat(POSITION_Z, 0);
        lastPosition = new Vector3(posX, posY, posZ);

        float rotX = PlayerPrefs.GetFloat(ROTATION_X, 0);
        float rotY = PlayerPrefs.GetFloat(ROTATION_Y, 0);
        float rotZ = PlayerPrefs.GetFloat(ROTATION_Z, 0);
        float rotW = PlayerPrefs.GetFloat(ROTATION_W, 1);
        lastRotation = new Quaternion(rotX, rotY, rotZ, rotW);

        hasData = true;
        return true;
    }

    public static bool HasSave()
    {
        return PlayerPrefs.GetInt(SAVE_EXISTS, 0) == 1;
    }

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
        PlayerPrefs.DeleteKey(UNLOCKED_RULES);
        PlayerPrefs.DeleteKey(CURRENT_RULE);
        PlayerPrefs.Save();

        hasData = false;
    }
}