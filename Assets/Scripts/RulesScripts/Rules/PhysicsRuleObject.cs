using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PhysicsRuleObject : RuleObject
{
    [Header("Physics Components")]
    private Rigidbody rb;
    private Collider objectCollider;
    private Renderer objectRenderer;
    private Material originalMaterial;

    [Header("Object State")]
    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    // Параметры правил
    private float gravityMultiplier = 1f;
    private float elasticity = 0.5f;
    private float growthMultiplier = 1f;
    private float attractionRadius = 10f;
    private float attractionForce = 5f;

    // Для хаотичного роста
    private float growthTimer = 1f;
    private float scaleMultiplayer = 2f;
    private Vector3 targetScale;

    // Для притяжения
    private LayerMask attractionLayer;
    private string attractionTag;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody>();
        objectCollider = GetComponent<Collider>();
        objectRenderer = GetComponent<Renderer>();

        originalScale = transform.localScale;
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        if (objectRenderer)
            originalMaterial = objectRenderer.material;

        // По умолчанию притягиваем все, что имеет Rigidbody
        attractionLayer = ~0; // Все слои
    }

    protected override void InitializeRuleActions()
    {
        ruleActions[RuleType.GravityInversion] = ApplyGravityInversion;
        ruleActions[RuleType.Elasticity] = ApplyElasticity;
        ruleActions[RuleType.ChaoticGrowth] = ApplyChaoticGrowth;
        ruleActions[RuleType.SelectiveAttraction] = ApplySelectiveAttraction;
    }

    #region Rule Implementations

    void ApplyGravityInversion()
    {
        if (rb)
        {
            rb.useGravity = false;
            gravityMultiplier = -0.8f; // Инвертированная гравитация (вверх)

            // Добавляем импульс вверх для начала движения
            rb.AddForce(Vector3.up * 8f, ForceMode.Impulse);
        }

        // Визуальный эффект - голубое свечение
        SetObjectColor(Color.cyan);

        Debug.Log($"[{gameObject.name}] Применена инверсия гравитации");
    }

    void ApplyElasticity()
    {
        elasticity = 0.95f; // Почти полная упругость

        if (objectCollider)
        {
            // Можно добавить PhysicMaterial для большей реалистичности
            PhysicsMaterial physMat = new PhysicsMaterial("Elastic");
            physMat.bounciness = elasticity;
            physMat.bounceCombine = PhysicsMaterialCombine.Maximum;
            objectCollider.material = physMat;
        }

        // Визуальный эффект - желтое свечение
        SetObjectColor(Color.yellow);

        Debug.Log($"[{gameObject.name}] Применена упругость");
    }

    void ApplyChaoticGrowth()
    {
        growthMultiplier = 1f;
        growthTimer = 0f;
        targetScale = originalScale;

        // Визуальный эффект - фиолетовое свечение
        SetObjectColor(Color.magenta);

        Debug.Log($"[{gameObject.name}] Применен хаотичный рост");
    }

    void ApplySelectiveAttraction()
    {
        // Можно настроить через инспектор или передавать параметры
        attractionRadius = 15f;
        attractionForce = 10f;

        // Пример: притягивать только объекты с тегом "Attractable"
        //attractionTag = "Attractable";

        // Визуальный эффект - зеленое свечение
        SetObjectColor(Color.green);

        Debug.Log($"[{gameObject.name}] Применено избирательное притяжение");
    }

    #endregion

    #region Update Logic

    void FixedUpdate()
    {
        if (activeRule == RuleType.None) return;

        switch (activeRule)
        {
            case RuleType.GravityInversion:
                UpdateGravityInversion();
                break;

            case RuleType.SelectiveAttraction:
                UpdateSelectiveAttraction();
                break;
        }
    }

    void Update()
    {
        if (activeRule == RuleType.None) return;

        switch (activeRule)
        {
            case RuleType.ChaoticGrowth:
                UpdateChaoticGrowth();
                break;
        }
    }

    void UpdateGravityInversion()
    {
        if (rb)
        {
            // Применяем инвертированную гравитацию
            Vector3 invertedGravity = Physics.gravity * gravityMultiplier;
            rb.AddForce(invertedGravity, ForceMode.Acceleration);

            // Ограничиваем скорость чтобы объект не улетел бесконечно
            if (rb.linearVelocity.magnitude > 10f)
                rb.linearVelocity = rb.linearVelocity.normalized * 10f;
        }
    }

    void UpdateChaoticGrowth()
    {
 
        StartCoroutine(ScaleOverTime(scaleMultiplayer, growthTimer));
              
    }

    IEnumerator ScaleOverTime(float targetSize, float time)
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScaleVector = new Vector3(targetSize, targetSize, targetSize);
        float elapsedTime = 0f;

        while (elapsedTime < time)
        {
            // Плавно переходим от оригинального размера к целевому
            transform.localScale = Vector3.Lerp(originalScale, targetScaleVector, elapsedTime / time);
            elapsedTime += Time.deltaTime; // Увеличиваем счетчик времени
            yield return null; // Ждем следующего кадра
        }

        // На всякий случай фиксируем точный конечный размер
        transform.localScale = targetScaleVector;
    }

    void UpdateSelectiveAttraction()
    {
        // Находим все объекты в радиусе
        Collider[] colliders = Physics.OverlapSphere(transform.position, attractionRadius);

        foreach (var col in colliders)
        {
            // Пропускаем себя
            if (col.gameObject == gameObject) continue;

            // Проверяем по тегу (если указан)
            if (!string.IsNullOrEmpty(attractionTag) && !col.CompareTag(attractionTag))
                continue;

            // Притягиваем только объекты с Rigidbody
            Rigidbody targetRb = col.GetComponent<Rigidbody>();
            if (targetRb != null)
            {
                // Вектор к этому объекту
                Vector3 direction = (transform.position - col.transform.position).normalized;
                float distance = Vector3.Distance(transform.position, col.transform.position);

                // Сила притяжения обратно пропорциональна расстоянию
                float force = attractionForce * (1 - distance / attractionRadius);

                // Применяем силу
                targetRb.AddForce(direction * force, ForceMode.Force);
            }
        }
    }

    #endregion

    #region Collision Events

    void OnCollisionEnter(Collision collision)
    {
        // Для правила упругости
        if (activeRule == RuleType.Elasticity && rb)
        {
            // Дополнительная логика для упругих столкновений
            ContactPoint contact = collision.contacts[0];
            rb.linearVelocity = Vector3.Reflect(rb.linearVelocity, contact.normal) * elasticity;
        }
    }

    #endregion

    #region Helper Methods

    void SetObjectColor(Color color)
    {
        if (objectRenderer)
        {
            // Создаем временный материал с эффектом свечения
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 0.5f);
            objectRenderer.material = mat;
        }
    }

    protected override void ResetToDefault()
    {
        

        // Сброс физики
        if (rb)
        {
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Сброс параметров
        gravityMultiplier = 1f;
        elasticity = 0.5f;
        growthMultiplier = 1f;

        // Сброс размера
        transform.localScale = originalScale;

        // Сброс позиции (опционально - можно вернуть в исходную)
        // transform.position = originalPosition;
        // transform.rotation = originalRotation;

        // Сброс материала
        if (objectRenderer && originalMaterial)
            objectRenderer.material = originalMaterial;

        // Сброс коллайдера
        if (objectCollider && objectCollider.material)
            objectCollider.material = null;

        Debug.Log($"[{gameObject.name}] Правила сброшены");
    }

    protected override void OnRuleApplied(RuleType rule)
    {
        base.OnRuleApplied(rule);

        // Можно добавить партиклы или звуки
        // Instantiate(effectPrefab, transform.position, Quaternion.identity);
    }

    protected override void OnRulesCleared()
    {
        base.OnRulesCleared();

        // Можно добавить эффект очистки
        // Instantiate(clearEffectPrefab, transform.position, Quaternion.identity);
    }

    #endregion

    // Визуализация радиуса притяжения в редакторе
    void OnDrawGizmosSelected()
    {
        if (activeRule == RuleType.SelectiveAttraction)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, attractionRadius);
        }
    }
}