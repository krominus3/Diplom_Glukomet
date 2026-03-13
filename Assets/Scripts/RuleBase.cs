using UnityEngine;

[System.Serializable] // Добавьте этот атрибут
public abstract class RuleBase : MonoBehaviour
{
    [Header("Rule Settings")]
    public string ruleName = "New Rule";
    public Sprite ruleIcon;
    public bool isActive = false;

    
    // Вызывается при применении правила
    public virtual void ApplyRule()
    {
        Debug.Log($"Правило {ruleName} применено к {gameObject.name}");
    }

    // Вызывается при очистке правила
    public virtual void RemoveRule()
    {
        Debug.Log($"Правило {ruleName} удалено с {gameObject.name}");
    }

    // Для обновления логики правила каждый кадр
    public virtual void UpdateRule() { }

    // Для FixedUpdate если нужно
    public virtual void FixedUpdateRule() { }
}