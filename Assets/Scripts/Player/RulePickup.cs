using UnityEngine;

public class RulePickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    public RuleBase rulePrefab; // Правило, которое дает этот предмет
    public GameObject pickupEffect;
    public AudioClip pickupSound;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Ищем у игрока компонент для хранения правил
            RuleInventory inventory = other.GetComponent<RuleInventory>();

            if (inventory != null && rulePrefab != null)
            {
                // Добавляем правило в инвентарь
                inventory.AddRule(rulePrefab);

                // Эффекты
                if (pickupEffect != null)
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);

                if (pickupSound != null)
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);

                // Уничтожаем предмет
                Destroy(gameObject);
            }

        }
    }
}