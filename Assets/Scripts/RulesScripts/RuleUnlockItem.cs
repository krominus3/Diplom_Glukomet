using UnityEngine;

public class RuleUnlockItem : MonoBehaviour
{
    public RuleType ruleToUnlock;
    public GameObject pickupEffect;
    public AudioClip pickupSound;

    void OnTriggerEnter(Collider other)
    {
        IRuleProvider ruleProvider = other.GetComponent<IRuleProvider>();

        if (ruleProvider != null)
        {
            ruleProvider.AddRule(ruleToUnlock);

            if (pickupEffect)
                Instantiate(pickupEffect, transform.position, Quaternion.identity);

            if (pickupSound)
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);

            Destroy(gameObject);
        }
    }
}