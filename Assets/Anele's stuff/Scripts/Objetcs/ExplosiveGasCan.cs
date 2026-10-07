using System.Collections;
using UnityEngine;

public class ExplosiveGasCan : DestructibleObject
{
    [Header("Explosion")]
    [SerializeField] float radius = 3f;
    [SerializeField] float damage = 40f;
    [SerializeField] float force = 12f;
    [SerializeField] float terrainRadius = 1.5f;
    [SerializeField] float chainDelay = 0.08f;
    [SerializeField] LayerMask affectedLayers;

    protected override void Die()
    {
        isDead = true;
        StartCoroutine(Explode());
    }

    IEnumerator Explode()
    {
        yield return new WaitForSeconds(chainDelay);   // makes chains ripple

        if (destroyVFX) Instantiate(destroyVFX, transform.position, Quaternion.identity);
        Explosion.Create(transform.position, radius, damage, force, affectedLayers, terrainRadius);
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}