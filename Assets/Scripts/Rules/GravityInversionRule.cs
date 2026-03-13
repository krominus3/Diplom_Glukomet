using UnityEngine;

public class GravityInversionRule : RuleBase
{
    [Header("Gravity Settings")]
    public float gravityMultiplier = -0.8f;
    public float upwardImpulse = 8f; // Увеличил для теста
    public float maxSpeed = 10f;

    private Rigidbody rb;
    private Vector3 originalGravity;
    private bool hasAppliedImpulse = false; // Флаг для одноразового импульса
    private Material originalMaterial;
    private Color originalColor;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void ApplyRule()
    {
        base.ApplyRule();
        isActive = true;

        if (rb == null) rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            originalGravity = Physics.gravity;
            rb.useGravity = false; // Отключаем гравитацию

            // Применяем импульс только один раз при применении правила
            rb.AddForce(Vector3.up * upwardImpulse, ForceMode.Impulse);
            hasAppliedImpulse = true;

            Debug.Log($"Применена гравитация к {gameObject.name}, импульс: {upwardImpulse}");
        }

        // Визуальный эффект
        SetObjectColor(Color.cyan);
    }

    public override void FixedUpdateRule()
    {
        if (rb != null && isActive)
        {
            // Применяем инвертированную гравитацию как постоянную силу (Acceleration)
            Vector3 invertedGravity = originalGravity * gravityMultiplier;
            rb.AddForce(invertedGravity, ForceMode.Acceleration);


            // Ограничение скорости
            if (rb.linearVelocity.magnitude > maxSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
            }
        }
    }

    public override void RemoveRule()
    {
        if (rb != null)
        {
            rb.useGravity = true; // Включаем гравитацию обратно
            rb.linearVelocity = Vector3.zero;
        }

        hasAppliedImpulse = false;
        ResetColor();
        base.RemoveRule();
    }

    void SetObjectColor(Color color)
    {
        Renderer renderer = GetComponent<Renderer>();
        originalMaterial = renderer.material;
        originalColor = renderer.material.color;

        if (renderer != null)
        {
            // Не создаем новый материал каждый раз, используем существующий
            Material mat = renderer.material;
            mat.color = color;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 0.5f);
        }
    }

    void ResetColor()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            renderer.material.DisableKeyword("_EMISSION");
            renderer.material = originalMaterial;
            renderer.material.color = originalColor;
            // Здесь можно вернуть оригинальный цвет, если он был сохранен
        }
    }
}