using UnityEngine;

public class RuleProjectile : MonoBehaviour
{
    private RuleType ruleType;
    private float lifeTime = 5f;
    private Rigidbody rb;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    public void Initialize(RuleType rule, float force)
    {
        ruleType = rule;
        
        if (rb)
            rb.AddForce(transform.forward * force, ForceMode.Impulse);
            
        Destroy(gameObject, lifeTime);
    }
    
    void OnCollisionEnter(Collision collision)
    {
        IRuleApplicable ruleApplicable = collision.collider.GetComponent<IRuleApplicable>();
        
        if (ruleApplicable != null && ruleApplicable.CanApplyRule(ruleType))
        {
            ruleApplicable.ApplyRule(ruleType);
            
            // Эффект попадания
            ContactPoint contact = collision.contacts[0];
            CreateHitEffect(contact.point);
            
            Destroy(gameObject);
        }
        //else
        //{
        //    // Рикошет или другой эффект
        //    if (rb)
        //    {
        //        rb.linearVelocity = Vector3.Reflect(rb.linearVelocity, collision.contacts[0].normal);
        //    }
        //}
    }
    
    void CreateHitEffect(Vector3 position)
    {
        // Создать эффект попадания
        // Можно добавить партиклы, звук и т.д.
    }
}