using UnityEngine;

public class ElasticityRule : RuleBase
{
    [Header("Elasticity Settings")]
    public float bounciness = 0.95f;

    private Collider objectCollider;
    private PhysicsMaterial originalPhysicsMaterial;
    private Rigidbody rb;
    //private Material originalMaterial;
    //private Color originalColor;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        objectCollider = GetComponent<Collider>();
    }

    public override void ApplyRule()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (objectCollider == null) objectCollider = GetComponent<Collider>();

        base.ApplyRule();

        isActive = true;
        if (objectCollider != null)
        {
            // —охран€ем оригинальный материал
            originalPhysicsMaterial = objectCollider.material;

            // —оздаем новый упругий материал
            PhysicsMaterial physMat = new PhysicsMaterial("Elastic");
            physMat.bounciness = bounciness;
            physMat.bounceCombine = PhysicsMaterialCombine.Maximum;
            objectCollider.material = physMat;
        }

        SetObjectColor(Color.green);
    }

    //void OnCollisionEnter(Collision collision)
    //{
    //    //if (isActive && rb != null)
    //    //{
    //    //    // ƒополнительна€ упругость при столкновении
    //    //    ContactPoint contact = collision.contacts[0];
    //    //    rb.linearVelocity = Vector3.Reflect(rb.linearVelocity, contact.normal) * bounciness;
    //    //}
    //}

    public override void RemoveRule()
    {
        isActive = false;
        if (objectCollider != null)
        {
            // ¬озвращаем оригинальный материал
            objectCollider.material = originalPhysicsMaterial;
        }

        ResetColor();
        base.RemoveRule();
    }

}