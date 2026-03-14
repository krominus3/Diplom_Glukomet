using UnityEngine;

public class GrowthRule : RuleBase
{
    [Header("Growth Settings")]
    public float growthMultiplier = 2f; // Во сколько раз вырасти
    public float growthSpeed = 1f; // Скорость роста
    
    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool isGrowing = false;
    //private Material originalMaterial;
    //private Color originalColor;

    // Для коррекции позиции
    private Vector3 groundCheckOffset = new Vector3(0, 0.1f, 0); // Смещение для проверки земли
    private float groundCheckDistance = 0.2f;
    private LayerMask groundLayer = ~0; // Все слои
    
    public override void ApplyRule()
    {
        base.ApplyRule();
        isActive = true;
        originalScale = transform.localScale;
        targetScale = originalScale * growthMultiplier;
        isGrowing = true;
        
        // Пробуждаем объект, если он спал
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.WakeUp();
        }
        
        // Визуальный эффект
        SetObjectColor(Color.orange);
        
        
        Debug.Log($"{gameObject.name} начинает расти до {growthMultiplier}x");
    }
    
    public override void UpdateRule()
    {
        if (!isGrowing) return;
        
        // Сохраняем текущую позицию Y перед изменением масштаба
        float oldY = transform.position.y;
        
        // Плавно растем
        transform.localScale = Vector3.Lerp(
            transform.localScale, 
            targetScale, 
            growthSpeed * Time.deltaTime
        );
        
        // Корректируем позицию, чтобы объект не проваливался в землю
        CorrectPosition(oldY);
        
        // Проверяем, достигли ли цели
        if (Vector3.Distance(transform.localScale, targetScale) < 0.01f)
        {
            transform.localScale = targetScale;
            isGrowing = false;
            Debug.Log($"{gameObject.name} закончил рост");
        }
    }
    
    void CorrectPosition(float oldY)
    {
        // Проверяем, есть ли земля под объектом
        RaycastHit hit;
        Vector3 rayStart = transform.position + groundCheckOffset;
        
        if (Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckDistance, groundLayer))
        {
            // Если нашли землю, ставим объект на неё
            float newY = hit.point.y + (transform.localScale.y / 2f);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
        else
        {
            // Если земли нет, но объект опустился, возвращаем старую высоту
            if (transform.position.y < oldY - 0.1f)
            {
                transform.position = new Vector3(
                    transform.position.x, 
                    oldY, 
                    transform.position.z
                );
            }
        }
    }
    
    public override void RemoveRule()
    {
        isActive = false;
        // При удалении правила возвращаем оригинальный размер
        transform.localScale = originalScale;
        isGrowing = false;
        
        // Корректируем позицию после возврата размера
        CorrectPosition(transform.position.y);
        
        ResetColor();
        base.RemoveRule();
    }
    
}