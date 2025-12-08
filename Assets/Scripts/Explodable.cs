using UnityEngine;

public class ForceExplosion : MonoBehaviour
{
    public float forceThreshold = 10f;
    public float explosionForce = 300f;
    public float explosionRadius = 5f;
    public float upwardsModifier = 0.5f;

    public GameObject explosionPrefab;
    public GameObject miniMap;

    private bool exploded = false;

    public void Explode()
    {
        if (exploded) return;
        exploded = true;

        miniMap.SetActive(false);

        Vector3 explosionPoint = transform.position;

        // spawn main explosion prefab
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, explosionPoint, Quaternion.identity);
        }

        // save momentum
        Rigidbody rootRb = GetComponent<Rigidbody>();
        Vector3 inheritedVel = rootRb ? rootRb.linearVelocity : Vector3.zero;
        Vector3 inheritedAngular = rootRb ? rootRb.angularVelocity : Vector3.zero;

        // detach & blast children
        Transform[] allTransforms = GetComponentsInChildren<Transform>(true);

        foreach (Transform t in allTransforms)
        {
            if (t == transform) continue;

            t.SetParent(null);

            Rigidbody rb = t.GetComponent<Rigidbody>();
            if (rb == null)
                rb = t.gameObject.AddComponent<Rigidbody>();

            rb.isKinematic = false;

            // ensure collider exists
            Collider col = t.GetComponent<Collider>();
            if (col == null)
            {
                MeshFilter mf = t.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    MeshCollider mc = t.gameObject.AddComponent<MeshCollider>();
                    mc.convex = true;
                    col = mc;
                }
                else
                {
                    col = t.gameObject.AddComponent<BoxCollider>();
                }
            }

            rb.linearVelocity = inheritedVel;
            rb.angularVelocity = inheritedAngular;

            rb.AddExplosionForce(
                explosionForce,
                explosionPoint,
                explosionRadius,
                upwardsModifier,
                ForceMode.Impulse
            );
        }

        Destroy(gameObject);
    }
}
