using UnityEngine;

public static class Explosion
{
    public static void Create(Vector2 position, float radius, float damage,
                              float force, LayerMask mask, float terrainRadius = 0f)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, radius, mask);

        foreach (var hit in hits)
        {
            float dist = Vector2.Distance(position, hit.transform.position);
            float falloff = Mathf.Clamp01(1f - dist / radius);

            var dmg = hit.GetComponentInParent<IDamageable>();
            if (dmg != null)
                dmg.TakeDamage(damage * falloff, position);

            Rigidbody2D rb = hit.attachedRigidbody;
            if (rb != null)
            {
                Vector2 dir = ((Vector2)hit.transform.position - position).normalized;
                rb.AddForce((dir + Vector2.up * 0.5f) * force * falloff, ForceMode2D.Impulse);
            }
        }

        // optional: blow holes in the tilemap (Step 10)
        if (terrainRadius > 0f && DestructibleTerrain.Instance != null)
            DestructibleTerrain.Instance.DestroyInRadius(position, terrainRadius);
    }
}