using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet2D : MonoBehaviour
{
    public float lifetime = 3f;
    public float damage = 10f;

    [Header("Tilemap")]
    public float tileHitOffset = 0.1f;

    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        Destroy(gameObject, lifetime);
    }

    // Solid hits: ground, crates, cages, gas cans, barrels, enemies, player, tilemap
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // --------------------------------------------------
        // TILEMAP DESTRUCTION
        // --------------------------------------------------

        DestructibleTilemap2D destructibleTilemap =
            collision.collider.GetComponentInParent<DestructibleTilemap2D>();

        if (destructibleTilemap != null)
        {
            // Get the exact point where the bullet hit
            ContactPoint2D contact = collision.GetContact(0);

            // Move the position slightly back into the tile
            Vector2 correctedHitPoint =
                contact.point - contact.normal * tileHitOffset;

            // Destroy only the tile that was actually hit
            destructibleTilemap.BreakTile(correctedHitPoint);
        }

        // --------------------------------------------------
        // DAMAGEABLE OBJECTS
        // --------------------------------------------------

        var target =
            collision.collider.GetComponentInParent<IDamageable>();

        if (target != null)
        {
            target.TakeDamage(
                damage,
                transform.position
            );
        }

        // --------------------------------------------------
        // DESTROY BULLET
        // --------------------------------------------------

        Destroy(gameObject);
    }

    // Trigger hits: needed for the landmine, whose collider is a trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        var target =
            other.GetComponentInParent<IDamageable>();

        if (target == null)
            return; // Ignore fire zones and other triggers

        target.TakeDamage(
            damage,
            transform.position
        );

        Destroy(gameObject);
    }
}