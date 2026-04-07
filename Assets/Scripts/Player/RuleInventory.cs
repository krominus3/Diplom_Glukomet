using UnityEngine;
using System.Collections.Generic;

public class RuleInventory : MonoBehaviour
{
    [Header("Available Rules")]
    public List<RuleBase> availableRules = new List<RuleBase>();

    [Header("Weapon Reference")]
    public RuleWeapon weapon;

    private int currentRuleIndex = 0;

    public string GetCurrentRuleName
    {
        get
        {
            return availableRules.Count != 0 ? availableRules[currentRuleIndex].ruleName : "Нет правил";
        }
    }

    void Start()
    {
        if (availableRules.Count > 0 && weapon != null)
        {
            weapon.SetCurrentRule(availableRules[0]);
        }
    }

    void Update()
    {
        if (availableRules.Count != 0)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                if (scroll > 0)
                    NextRule();
                else
                    PreviousRule();
            }

            for (int i = 0; i < availableRules.Count && i < 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    SelectRule(i);
                }
            }
        }
    }

    public void AddRule(RuleBase rule)
    {
        if (rule == null) return;

        if (!availableRules.Contains(rule))
        {
            availableRules.Add(rule);
            Debug.Log($"Получено новое правило: {rule.ruleName}");

            if (availableRules.Count == 1 && weapon != null)
            {
                weapon.SetCurrentRule(rule);
            }
        }
    }

    public void RemoveRule(RuleBase rule)
    {
        if (availableRules.Contains(rule))
        {
            availableRules.Remove(rule);
            Debug.Log($"Правило {rule.ruleName} удалено");

            if (weapon != null && weapon.currentRulePrefab == rule)
            {
                if (availableRules.Count > 0)
                    weapon.SetCurrentRule(availableRules[0]);
                else
                    weapon.SetCurrentRule(null);
            }
        }
    }

    public void RemoveRule(int index)
    {
        if (index >= 0 && index < availableRules.Count)
        {
            RemoveRule(availableRules[index]);
        }
    }

    public void ClearAllRules()
    {
        availableRules.Clear();
        if (weapon != null)
        {
            weapon.SetCurrentRule(null);
        }
        Debug.Log("Все правила удалены");
    }

    public void NextRule()
    {
        if (availableRules.Count == 0) return;

        currentRuleIndex = (currentRuleIndex + 1) % availableRules.Count;
        SelectRule(currentRuleIndex);
    }

    public void PreviousRule()
    {
        if (availableRules.Count == 0) return;

        currentRuleIndex--;
        if (currentRuleIndex < 0)
            currentRuleIndex = availableRules.Count - 1;

        SelectRule(currentRuleIndex);
    }

    public void SelectRule(int index)
    {
        if (index >= 0 && index < availableRules.Count && weapon != null)
        {
            currentRuleIndex = index;
            weapon.SetCurrentRule(availableRules[index]);
            Debug.Log($"Выбрано правило: {availableRules[index].ruleName}");
        }
    }

    public void SelectRule<T>() where T : RuleBase
    {
        for (int i = 0; i < availableRules.Count; i++)
        {
            if (availableRules[i] is T)
            {
                SelectRule(i);
                return;
            }
        }
    }

    public void SetCurrentRule(RuleBase rule)
    {
        for (int i = 0; i < availableRules.Count; i++)
        {
            if (availableRules[i] == rule)
            {
                SelectRule(i);
                return;
            }
        }

        if (availableRules.Count > 0)
        {
            SelectRule(0);
        }
    }

    public List<RuleBase> GetAllRules()
    {
        return new List<RuleBase>(availableRules);
    }
}