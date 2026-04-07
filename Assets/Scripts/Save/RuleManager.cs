using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RuleManager : MonoBehaviour
{
    public static RuleManager Instance;

    [Header("All Rule Prefabs")]
    public List<RuleBase> allRulePrefabs;

    private Dictionary<string, RuleBase> ruleDictionary = new Dictionary<string, RuleBase>();
    private RuleInventory currentInventory;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        foreach (var rule in allRulePrefabs)
        {
            if (rule != null && !ruleDictionary.ContainsKey(rule.ruleName))
            {
                ruleDictionary.Add(rule.ruleName, rule);
            }
        }
    }

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentInventory = FindFirstObjectByType<RuleInventory>();

        if (currentInventory != null && PlayerData.HasSave())
        {
            LoadRules();
        }
    }

    public RuleBase GetRuleByName(string ruleName)
    {
        if (ruleDictionary.ContainsKey(ruleName))
        {
            return ruleDictionary[ruleName];
        }
        Debug.LogWarning($"Правило {ruleName} не найдено");
        return null;
    }

    public void SaveRules()
    {
        if (currentInventory == null)
        {
            currentInventory = FindFirstObjectByType<RuleInventory>();
        }

        if (currentInventory != null)
        {
            PlayerData.SavePlayerRules(currentInventory);
        }
    }

    public void LoadRules()
    {
        if (currentInventory == null)
        {
            currentInventory = FindFirstObjectByType<RuleInventory>();
        }

        if (currentInventory == null) return;

        string savedRules = PlayerPrefs.GetString("UnlockedRules", "");
        string savedCurrentRule = PlayerPrefs.GetString("CurrentRule", "");

        if (string.IsNullOrEmpty(savedRules)) return;

        string[] ruleNames = savedRules.Split(',');

        currentInventory.ClearAllRules();

        foreach (string ruleName in ruleNames)
        {
            if (string.IsNullOrEmpty(ruleName)) continue;

            RuleBase rule = GetRuleByName(ruleName);
            if (rule != null)
            {
                currentInventory.AddRule(rule);
                Debug.Log($"Правило {ruleName} восстановлено");
            }
        }

        if (!string.IsNullOrEmpty(savedCurrentRule))
        {
            RuleBase currentRule = GetRuleByName(savedCurrentRule);
            if (currentRule != null)
            {
                currentInventory.SetCurrentRule(currentRule);
                RuleWeapon weapon = FindFirstObjectByType<RuleWeapon>();
                if (weapon != null)
                {
                    weapon.SetCurrentRule(currentRule);
                }
            }
        }
    }

    public void ClearAllPlayerRules()
    {
        if (currentInventory != null)
        {
            currentInventory.ClearAllRules();
        }
        PlayerPrefs.DeleteKey("UnlockedRules");
        PlayerPrefs.DeleteKey("CurrentRule");
        PlayerPrefs.Save();
    }
}