using UnityEngine;

public class AttractionRule : RuleBase
{
    [Header("Attraction Settings")]
    public float attractionRadius = 15f;
    public float attractionForce = 10f;
    public LayerMask targetLayers = -1; // Все слои
    public string targetTag = ""; // Если не пусто, притягивать только объекты с этим тегом

    public override void ApplyRule()
    {
        base.ApplyRule();
        SetObjectColor(Color.green);
    }

    public override void FixedUpdateRule()
    {
        if (!isActive) return;

        // Находим все объекты в радиусе
        Collider[] colliders = Physics.OverlapSphere(transform.position, attractionRadius, targetLayers);

        foreach (var col in colliders)
        {
            // Пропускаем себя
            if (col.gameObject == gameObject) continue;

            // Проверяем по тегу
            if (!string.IsNullOrEmpty(targetTag) && !col.CompareTag(targetTag))
                continue;

            // Притягиваем объекты с Rigidbody
            Rigidbody targetRb = col.GetComponent<Rigidbody>();
            if (targetRb != null)
            {
                Vector3 direction = (transform.position - col.transform.position).normalized;
                float distance = Vector3.Distance(transform.position, col.transform.position);
                float force = attractionForce * (1 - distance / attractionRadius);

                targetRb.AddForce(direction * force, ForceMode.Force);
            }
        }
    }

    public override void RemoveRule()
    {
        ResetColor();
        base.RemoveRule();
    }

    void SetObjectColor(Color color)
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            renderer.material = mat;
        }
    }

    void ResetColor()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            // Вернуть оригинальный цвет
        }
    }

    // Визуализация радиуса в редакторе
    void OnDrawGizmosSelected()
    {
        if (isActive)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, attractionRadius);
        }
    }
}