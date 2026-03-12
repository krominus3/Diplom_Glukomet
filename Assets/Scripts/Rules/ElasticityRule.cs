using UnityEngine;

public class ElasticityRule : RuleBase
{
    [Header("Elasticity Settings")]
    public float bounciness = 0.95f;

    private Collider objectCollider;
    private PhysicsMaterial originalMaterial;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        objectCollider = GetComponent<Collider>();
    }

    public override void ApplyRule()
    {
        base.ApplyRule();

        if (objectCollider != null)
        {
            // —охран€ем оригинальный материал
            originalMaterial = objectCollider.material;

            // —оздаем новый упругий материал
            PhysicsMaterial physMat = new PhysicsMaterial("Elastic");
            physMat.bounciness = bounciness;
            physMat.bounceCombine = PhysicsMaterialCombine.Maximum;
            objectCollider.material = physMat;
        }

        SetObjectColor(Color.yellow);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isActive && rb != null)
        {
            // ƒополнительна€ упругость при столкновении
            ContactPoint contact = collision.contacts[0];
            rb.linearVelocity = Vector3.Reflect(rb.linearVelocity, contact.normal) * bounciness;
        }
    }

    public override void RemoveRule()
    {
        if (objectCollider != null)
        {
            // ¬озвращаем оригинальный материал
            objectCollider.material = originalMaterial;
        }

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
            // «десь можно вернуть оригинальный материал
        }
    }
}