using System.Collections.Generic;
using UnityEngine;

[System.Serializable] // Добавьте этот атрибут
public abstract class RuleBase : MonoBehaviour
{
    [Header("Rule Settings")]
    public string ruleName = "New Rule";
    public Sprite ruleIcon;
    public bool isActive = false;
    private List<Material> originalMaterials = new();
    private List<Color> originalColors = new();


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

    protected void SetObjectColor(Color color)
    {
        Renderer renderer = GetComponent<Renderer>();
        originalMaterials.Add(renderer.material);
        originalColors.Add(renderer.material.color);
        


        if (renderer != null)
        {
            //Material mat = new Material(Shader.Find("Standard"));
            //mat.color = color;
            //renderer.material.color *= color;
            renderer.material.color = MixColorsLikePaint(renderer.material.color, color, 0.6f);
        }
    }

    Color MixColorsLikePaint(Color colorA, Color colorB, float t)
    {
        return new Color(
            (colorA.r + colorB.r) * t,
            (colorA.g + colorB.g) * t,
            (colorA.b + colorB.b) * t
        );
    }

    protected void ResetColor()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = originalMaterials[^1];
            renderer.material.color = originalColors[^1];



            // Здесь можно вернуть оригинальный материал
        }
    }

}