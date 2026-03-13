using UnityEngine;
using System.Collections.Generic;

public class AttractionRule : RuleBase
{
    [Header("Attraction Settings")]
    public float attractionRadius = 15f;
    public float attractionForce = 10f;
    public LayerMask targetLayers = -1; // Все слои
    public string targetTag = ""; // Если не пусто, притягивать только объекты с этим тегом

    [Header("Interaction Settings")]
    public bool attractSelf = false; // Притягивать ли самого себя
    //public bool attractObjectsWithRules = true; // Притягивать ли объекты с другими правилами

    [Header("Visual Settings")]
    public Color activeColor = Color.green;
    public bool showDebugLines = true;

    private List<Rigidbody> affectedBodies = new List<Rigidbody>();
    private Renderer objectRenderer;
    private Color originalColor;
    private Material originalMaterial;

    //void Start()
    //{
    //    objectRenderer = GetComponent<Renderer>();
    //    if (objectRenderer != null)
    //    {
    //        originalMaterial = objectRenderer.material;
    //        originalColor = objectRenderer.material.color;
    //    }
    //}

    public override void ApplyRule()
    {
        base.ApplyRule();
        isActive = true;
        SetObjectColor(activeColor);
    }

    public override void FixedUpdateRule()
    {
        if (!isActive) return;

        FindAffectedObjects();

        foreach (var targetRb in affectedBodies)
        {
            if (targetRb == null) continue;

            // Пропускаем себя, если не хотим притягивать
            if (!attractSelf && targetRb.gameObject == gameObject)
                continue;

            Vector3 direction = (transform.position - targetRb.transform.position).normalized;
            float distance = Vector3.Distance(transform.position, targetRb.transform.position);

            // Сила уменьшается с расстоянием
            float forceMultiplier = 1 - Mathf.Clamp01(distance / attractionRadius);
            float force = attractionForce * forceMultiplier;

            // Применяем силу
            targetRb.AddForce(direction * force, ForceMode.Force);
        }
    }

    void FindAffectedObjects()
    {
        affectedBodies.Clear();

        // Находим все коллайдеры в радиусе
        Collider[] colliders = Physics.OverlapSphere(transform.position, attractionRadius, targetLayers);

        foreach (var col in colliders)
        {
            // Проверяем по тегу
            if (!string.IsNullOrEmpty(targetTag) && !col.CompareTag(targetTag))
                continue;

            // Получаем Rigidbody
            Rigidbody targetRb = col.GetComponent<Rigidbody>();
            if (targetRb == null) continue;

            //// Проверяем, нужно ли притягивать объекты с правилами
            //if (!attractObjectsWithRules)
            //{
            //    RuleContainer container = col.GetComponent<RuleContainer>();
            //    if (container != null && container.HasAnyRule())
            //    {
            //        continue;
            //    }
            //}

            affectedBodies.Add(targetRb);
        }
    }

    public override void RemoveRule()
    {
        affectedBodies.Clear();
        ResetColor();
        base.RemoveRule();
    }

    void SetObjectColor(Color color)
    {
        if (objectRenderer == null) objectRenderer = GetComponent<Renderer>();
        originalMaterial = objectRenderer.material;
        originalColor = objectRenderer.material.color;

        if (objectRenderer != null)
        {
            if (objectRenderer.material != null)
            {
                objectRenderer.material.color = color;
                objectRenderer.material.EnableKeyword("_EMISSION");
                objectRenderer.material.SetColor("_EmissionColor", color * 0.3f);
            }
        }
    }

    void ResetColor()
    {
        if (objectRenderer != null)
        {
            objectRenderer.material.DisableKeyword("_EMISSION");
            objectRenderer.material = originalMaterial;
            objectRenderer.material.color = originalColor;
        }
    }

    // Визуализация в редакторе
    void OnDrawGizmosSelected()
    {
        if (!isActive) return;

        // Рисуем радиус притяжения
        Gizmos.color = activeColor;
        Gizmos.DrawWireSphere(transform.position, attractionRadius);

        // Рисуем линии к притягиваемым объектам
        if (showDebugLines && affectedBodies != null)
        {
            Gizmos.color = Color.yellow;
            foreach (var rb in affectedBodies)
            {
                if (rb != null && rb.gameObject != gameObject)
                {
                    Gizmos.DrawLine(transform.position, rb.transform.position);
                }
            }
        }
    }

    // Для отладки - показываем информацию в консоли
    void OnDrawGizmos()
    {
        if (isActive && showDebugLines)
        {
            Debug.Log($"[AttractionRule] {gameObject.name} притягивает {affectedBodies.Count} объектов");
        }
    }
}