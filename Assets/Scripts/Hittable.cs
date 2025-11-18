using UnityEngine;

public class Hittable : MonoBehaviour
{
    [SerializeField] float mass = 1f;

    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

        rb.mass = mass;       // apply custom mass
        rb.isKinematic = true;
    }

    void OnCollisionEnter(Collision c)
    {
        rb.isKinematic = false;
    }
}
