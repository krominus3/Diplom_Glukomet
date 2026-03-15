using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RuleContainer : MonoBehaviour
{
    [Header("Rule Settings")]
    public bool canAcceptRules = true; // Можно ли применять правила к этому объекту
    
    // Список всех правил на объекте
    private List<RuleBase> activeRules = new List<RuleBase>();

    void Update()
    {
        // Обновляем все активные правила
        foreach (var rule in activeRules)
        {
            if (rule.isActive)
                rule.UpdateRule();
        }
    }
    
    void FixedUpdate()
    {
        // FixedUpdate для всех активных правил
        foreach (var rule in activeRules)
        {
            if (rule.isActive)
                rule.FixedUpdateRule();
        }
    }

    // Передать правило через ближайшие ноды
    public void TransferRuleThroughNodes(RuleBase rule)
    {
        // Находим ближайшие ноды
        Collider[] colliders = Physics.OverlapSphere(transform.position, 10f);

        foreach (var col in colliders)
        {
            RuleNode node = col.GetComponent<RuleNode>();
            if (node != null)
            {
                // Передаем правило в ноду
                node.TransferRule(rule, gameObject);
                Debug.Log($"Правило {rule.ruleName} передано в ноду {node.gameObject.name}");
            }
        }
    }

    
    public bool ApplyRule(RuleBase rulePrefab, bool transferThroughNodes = true)
    {
        if (!canAcceptRules) return false;

        RuleBase existingRule = GetRule(rulePrefab.GetType());
        if (existingRule != null) return false;

        RuleBase newRule = gameObject.AddComponent(rulePrefab.GetType()) as RuleBase;
        if (newRule != null)
        {
            CopyRuleProperties(rulePrefab, newRule);
            newRule.ApplyRule();
            activeRules.Add(newRule);

            Debug.Log($"Правило {rulePrefab.ruleName} добавлено на {gameObject.name}");

            // Передаем правило через ноды
            if (transferThroughNodes)
            {
                TransferRuleThroughNodes(newRule);
            }

            return true;
        }

        return false;
    }

    // Метод для копирования свойств из префаба
    private void CopyRuleProperties(RuleBase source, RuleBase destination)
    {
        // Копируем базовые поля
        destination.ruleName = source.ruleName;
        destination.ruleIcon = source.ruleIcon;
        
        // Копируем все публичные поля через рефлексию
        System.Type type = source.GetType();
        var fields = type.GetFields(System.Reflection.BindingFlags.Public | 
                                    System.Reflection.BindingFlags.Instance);
        
        foreach (var field in fields)
        {
            // Пропускаем поля, помеченные атрибутом NonSerialized
            if (System.Attribute.IsDefined(field, typeof(System.NonSerializedAttribute)))
                continue;
                
            object value = field.GetValue(source);
            field.SetValue(destination, value);
        }
        
        Debug.Log($"Скопированы настройки для правила {source.ruleName}");
    }
    
    // Удалить конкретное правило
    public bool RemoveRule<T>() where T : RuleBase
    {
        RuleBase rule = GetRule<T>();
        if (rule != null)
        {
            rule.RemoveRule();
            activeRules.Remove(rule);
            Destroy(rule);

            Debug.Log($"Правило {typeof(T).Name} удалено с {gameObject.name}");
            return true;
        }
        return false;
    }

    // Очистить все правила
    public void ClearAllRules()
    {
        for (int i = activeRules.Count - 1; i >= 0; i--)
        {
            RuleBase rule = activeRules[i];
            rule.RemoveRule();
            Destroy(rule);

        }
        
        activeRules.Clear();

        Debug.Log($"Все правила очищены с {gameObject.name}");
    }

    public void RemoveLastRule()
    {
        if (activeRules.Count != 0)
        {
            RuleBase rule = activeRules[^1];
            rule.RemoveRule();
            Destroy(rule);
            activeRules.Remove(rule);
        }
    }


    // Получить правило по типу
    public RuleBase GetRule<T>() where T : RuleBase
    {
        return activeRules.FirstOrDefault(r => r is T);
    }
    
    public RuleBase GetRule(System.Type ruleType)
    {
        return activeRules.FirstOrDefault(r => r.GetType() == ruleType);
    }
    
    // Проверить, есть ли правило
    public bool HasRule<T>() where T : RuleBase
    {
        return GetRule<T>() != null;
    }
    
    // Получить все активные правила
    public List<RuleBase> GetAllRules()
    {
        return new List<RuleBase>(activeRules);
    }
}