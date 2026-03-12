using UnityEngine;

public class PlayerPusher : MonoBehaviour
{
    [Header("Push Settings")]
    public float pushForce = 10f; // Сила толчка
    public float pushRadius = 1f; // Радиус толкания (опционально)
    public LayerMask pushableLayers = -1; // Какие слои можно толкать (-1 = все)

    [Header("Advanced Settings")]
    public bool useContinuousPush = true; // Постоянное толкание или по удару
    public float maxPushDistance = 2f; // Максимальная дистанция толкания

    private CharacterController characterController;

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        if (characterController == null)
        {
            Debug.LogError("PlayerPusher требует CharacterController на игроке!");
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Это специальный метод для CharacterController
        // Вызывается когда CharacterController сталкивается с коллайдером

        // Получаем Rigidbody объекта, в который врезались
        Rigidbody rb = hit.collider.attachedRigidbody;

        // Проверяем, можно ли толкать этот объект
        if (rb != null && !rb.isKinematic)
        {
            // Проверяем слой
            if (((1 << hit.gameObject.layer) & pushableLayers) != 0)
            {
                // Рассчитываем направление толчка
                Vector3 pushDirection = hit.moveDirection;

                // Применяем силу
                rb.AddForce(pushDirection * pushForce, ForceMode.Impulse);

                // Визуальная отладка
                Debug.Log($"Толкаем: {hit.gameObject.name}", hit.gameObject);
            }
        }
    }

    // Альтернативный вариант: толкание через триггер (более контролируемое)
    void OnTriggerStay(Collider other)
    {
        if (!useContinuousPush) return;

        Rigidbody rb = other.attachedRigidbody;

        if (rb != null && !rb.isKinematic)
        {
            if (((1 << other.gameObject.layer) & pushableLayers) != 0)
            {
                // Направление от игрока к объекту
                Vector3 directionToObject = other.transform.position - transform.position;
                directionToObject.Normalize();

                // Проверяем дистанцию
                float distance = Vector3.Distance(transform.position, other.transform.position);
                if (distance <= maxPushDistance)
                {
                    // Применяем постоянную силу
                    rb.AddForce(directionToObject * pushForce * Time.fixedDeltaTime, ForceMode.Force);

                    // Будим Rigidbody если уснул
                    if (rb.IsSleeping())
                    {
                        rb.WakeUp();
                    }
                }
            }
        }
    }

    // Визуализация радиуса в редакторе
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxPushDistance);
    }
}