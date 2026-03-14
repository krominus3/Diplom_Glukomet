using UnityEngine;

public class RuleWeapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    public Transform shootPoint;
    public float shootForce = 20f;
    public float maxDistance = 100f;

    [Header("Current Rule")]
    public RuleBase currentRulePrefab; // Префаб правила, которым стреляем

    [Header("Effects")]
    public GameObject shootEffect;
    public AudioClip shootSound;

    void Update()
    {
        if (Input.GetButtonDown("Fire1") && currentRulePrefab != null)
        {
            Shoot();
        }

        if (Input.GetButtonDown("Fire2") && currentRulePrefab != null)
        {
            ShootSecond();
        }
    }

    void Shoot()
    {
        RaycastHit hit;
        if (Physics.Raycast(shootPoint.position, shootPoint.forward, out hit, maxDistance))
        {
            // Проверяем, есть ли на объекте RuleContainer
            RuleContainer container = hit.collider.GetComponent<RuleContainer>();

            if (container != null)
            {
                // Применяем правило
                container.ApplyRule(currentRulePrefab);

                // Эффект попадания
                if (shootEffect != null)
                {
                    Instantiate(shootEffect, hit.point, Quaternion.identity);
                }

                // Звук
                if (shootSound != null)
                {
                    AudioSource.PlayClipAtPoint(shootSound, hit.point);
                }
            }
            else
            {
                Debug.Log("Объект не может принимать правила");
            }
        }
    }

    void ShootSecond()
    {
        RaycastHit hit;
        if (Physics.Raycast(shootPoint.position, shootPoint.forward, out hit, maxDistance))
        {
            // Проверяем, есть ли на объекте RuleContainer
            RuleContainer container = hit.collider.GetComponent<RuleContainer>();

            if (container != null)
            {
                container.RemoveLastRule();
            }
            else
            {
                Debug.Log("Объект не может принимать правила");
            }
        }
    }


    // Метод для смены текущего правила
    public void SetCurrentRule(RuleBase newRule)
    {
        currentRulePrefab = newRule;
    }
}