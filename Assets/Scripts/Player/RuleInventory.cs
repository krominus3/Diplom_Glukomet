using UnityEngine;
using System.Collections.Generic;

public class RuleInventory : MonoBehaviour
{
    [Header("Available Rules")]
    public List<RuleBase> availableRules = new List<RuleBase>();

    [Header("Weapon Reference")]
    public RuleWeapon weapon;


    // Текущее выбранное правило
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
        // При старте выбираем первое правило, если есть
        if (availableRules.Count > 0 && weapon != null)
        {
            weapon.SetCurrentRule(availableRules[0]);
        }
    }

    void Update()
    {
        // Переключение правил колесиком мыши
        if (availableRules.Count != 0)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0 && availableRules.Count > 0)
            {
                if (scroll > 0)
                    NextRule();
                else
                    PreviousRule();
            }

            // Переключение цифрами 1-4
            for (int i = 0; i < availableRules.Count && i < 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    SelectRule(i);
                }
            }
        }

    }

    // Добавить новое правило
    public void AddRule(RuleBase rule)
    {
        if (rule == null) return;

        if (!availableRules.Contains(rule))
        {
            availableRules.Add(rule);
            Debug.Log($"Получено новое правило: {rule.ruleName}");

            // Если это первое правило, автоматически выбираем его
            if (availableRules.Count == 1 && weapon != null)
            {
                weapon.SetCurrentRule(rule);
            }
        }
    }

    // Удалить правило
    public void RemoveRule(RuleBase rule)
    {
        if (availableRules.Contains(rule))
        {
            availableRules.Remove(rule);
            Debug.Log($"Правило {rule.ruleName} удалено");

            // Если удалили текущее правило, выбираем другое
            if (weapon != null && weapon.currentRulePrefab == rule)
            {
                if (availableRules.Count > 0)
                    weapon.SetCurrentRule(availableRules[0]);
                else
                    weapon.SetCurrentRule(null);
            }
        }
    }

    // Удалить правило по индексу
    public void RemoveRule(int index)
    {
        if (index >= 0 && index < availableRules.Count)
        {
            RemoveRule(availableRules[index]);
        }
    }

    // Очистить все правила
    public void ClearAllRules()
    {
        availableRules.Clear();
        if (weapon != null)
        {
            weapon.SetCurrentRule(null);
        }
        Debug.Log("Все правила удалены");
    }

    // Выбрать следующее правило
    public void NextRule()
    {
        if (availableRules.Count == 0) return;

        currentRuleIndex = (currentRuleIndex + 1) % availableRules.Count;
        SelectRule(currentRuleIndex);
    }

    // Выбрать предыдущее правило
    public void PreviousRule()
    {
        if (availableRules.Count == 0) return;

        currentRuleIndex--;
        if (currentRuleIndex < 0)
            currentRuleIndex = availableRules.Count - 1;

        SelectRule(currentRuleIndex);
    }

    // Выбрать правило по индексу
    public void SelectRule(int index)
    {
        if (index >= 0 && index < availableRules.Count && weapon != null)
        {
            currentRuleIndex = index;
            weapon.SetCurrentRule(availableRules[index]);
            Debug.Log($"Выбрано правило: {availableRules[index].ruleName}");

        }
    }

    // Выбрать правило по типу
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

    // Получить все правила
    public List<RuleBase> GetAllRules()
    {
        return new List<RuleBase>(availableRules);
    }

    
}