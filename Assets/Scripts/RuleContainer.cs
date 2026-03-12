using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RuleContainer : MonoBehaviour
{
    [Header("Rule Settings")]
    public bool canAcceptRules = true; // Можно ли применять правила к этому объекту
    
    // Список всех правил на объекте
    private List<RuleBase> activeRules = new List<RuleBase>();
    private Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>();
    private Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>();

    void Start()
    {
        // Сохраняем оригинальные цвета при старте
        SaveOriginalColors();
    }

    private void SaveOriginalColors()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            if (!originalColors.ContainsKey(renderer))
            {
                originalColors[renderer] = renderer.material.color;
                originalMaterials[renderer] = renderer.material;
            }
        }
    }

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
    
    // Применить правило к объекту
    public bool ApplyRule(RuleBase rulePrefab)
    {
        if (!canAcceptRules)
        {
            Debug.Log($"{gameObject.name} не может принимать правила");
            return false;
        }
        
        // Проверяем, есть ли уже такое правило
        RuleBase existingRule = GetRule(rulePrefab.GetType());
        
        if (existingRule != null)
        {
            Debug.Log($"Правило {rulePrefab.ruleName} уже есть на объекте");
            return false;
        }
        
        // СОЗДАЕМ НОВЫЙ КОМПОНЕНТ И КОПИРУЕМ НАСТРОЙКИ ИЗ ПРЕФАБА
        RuleBase newRule = gameObject.AddComponent(rulePrefab.GetType()) as RuleBase;
        if (newRule != null)
        {
            // Копируем все публичные поля из префаба в новый компонент
            CopyRuleProperties(rulePrefab, newRule);
            
            // Применяем правило
            newRule.ApplyRule();
            activeRules.Add(newRule);
            
            Debug.Log($"Правило {rulePrefab.ruleName} добавлено на {gameObject.name}");
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
        foreach (var rule in activeRules.ToList())
        {
            rule.RemoveRule();
            Destroy(rule);
        }
        activeRules.Clear();

        // Возвращаем оригинальные цвета
        RestoreOriginalColors();

        Debug.Log($"Все правила очищены с {gameObject.name}");
    }

    private void RestoreOriginalColors()
    {
        foreach (var kvp in originalColors)
        {
            Renderer renderer = kvp.Key;
            if (renderer != null)
            {
                renderer.material = originalMaterials[renderer];
                renderer.material.color = kvp.Value;
                renderer.material.DisableKeyword("_EMISSION");
            }
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