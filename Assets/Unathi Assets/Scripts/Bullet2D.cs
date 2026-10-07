using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet2D : MonoBehaviour
{
    public float lifetime = 3f;
    public float damage = 10f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    // Solid hits: ground, crates, cages, gas cans, barrels, enemies, player
    private void OnCollisionEnter2D(Collision2D collision)
    {
        var target = collision.collider.GetComponentInParent<IDamageable>();
        if (target != null)
            target.TakeDamage(damage, transform.position);

        Destroy(gameObject);
    }

    // Trigger hits: needed for the landmine, whose collider is a trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        var target = other.GetComponentInParent<IDamageable>();
        if (target == null) return;   // ignore fire zones and other triggers

        target.TakeDamage(damage, transform.position);
        Destroy(gameObject);
    }
}