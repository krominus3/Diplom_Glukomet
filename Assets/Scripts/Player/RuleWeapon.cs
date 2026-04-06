using Unity.VisualScripting;
using UnityEngine;

public class RuleWeapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    public Transform shootPoint;
    public float shootForce = 20f;
    public float maxDistance = 100f;

    [Header("Layer Masks")]
    public LayerMask targetLayers = -1; // Какие слои можно поражать
    public string[] ignoredTags = { "Player" }; // Теги, которые игнорируем

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
        if (Physics.Raycast(shootPoint.position, shootPoint.forward, out hit, maxDistance, targetLayers))
        {
            // Проверяем, нужно ли игнорировать этот объект по тегу
            if (ShouldIgnoreObject(hit.collider.gameObject))
            {
                Debug.Log($"Игнорируем {hit.collider.gameObject.name} (тег: {hit.collider.tag})");
                return;
            }

            RuleNode node = hit.collider.GetComponent<RuleNode>();
            if (node != null)
            {
                node.ApplyRuleToObject(currentRulePrefab, hit.collider.gameObject, gameObject);
                return; // Если это нода, не нужно проверять RuleContainer
            }

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
        if (Physics.Raycast(shootPoint.position, shootPoint.forward, out hit, maxDistance, targetLayers))
        {
            // Проверяем, нужно ли игнорировать этот объект по тегу
            if (ShouldIgnoreObject(hit.collider.gameObject))
            {
                Debug.Log($"Игнорируем {hit.collider.gameObject.name} (тег: {hit.collider.tag})");
                return;
            }

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

    // Проверка, нужно ли игнорировать объект
    private bool ShouldIgnoreObject(GameObject obj)
    {
        foreach (string tag in ignoredTags)
        {
            if (obj.CompareTag(tag))
                return true;
        }
        return false;
    }

    // Метод для смены текущего правила
    public void SetCurrentRule(RuleBase newRule)
    {
        currentRulePrefab = newRule;
    }
}