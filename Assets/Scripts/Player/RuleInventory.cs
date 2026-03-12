using UnityEngine;
using System.Collections.Generic;

public class RuleInventory : MonoBehaviour
{
    [Header("Available Rules")]
    public List<RuleBase> availableRules = new List<RuleBase>();

    [Header("Weapon Reference")]
    public RuleWeapon weapon;

    // Добавить новое правило в инвентарь
    public void AddRule(RuleBase rule)
    {
        if (!availableRules.Contains(rule))
        {
            availableRules.Add(rule);
            Debug.Log($"Получено новое правило: {rule.ruleName}");

            // Автоматически экипируем новое правило
            if (weapon != null)
            {
                weapon.SetCurrentRule(rule);
            }
        }
    }

    // Получить правило по индексу
    public RuleBase GetRule(int index)
    {
        if (index >= 0 && index < availableRules.Count)
            return availableRules[index];
        return null;
    }

    // Получить все правила
    public List<RuleBase> GetAllRules()
    {
        return new List<RuleBase>(availableRules);
    }
}