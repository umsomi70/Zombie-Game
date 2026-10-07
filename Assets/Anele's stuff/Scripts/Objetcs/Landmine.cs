using System.Collections;
using UnityEngine;

public class Landmine : MonoBehaviour, IDamageable
{
    [SerializeField] float armDelay = 0.4f;
    [SerializeField] float radius = 2.5f;
    [SerializeField] float damage = 35f;
    [SerializeField] float force = 10f;
    [SerializeField] float terrainRadius = 1f;
    [SerializeField] LayerMask affectedLayers;
    [SerializeField] GameObject explosionVFX;
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] AudioClip explosionSound;   // add with the other fields

    bool triggered;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (other.CompareTag("Player") || other.CompareTag("Enemy"))
            StartCoroutine(ArmAndExplode());
    }

    public void TakeDamage(float amount, Vector2 hitPoint)
    {
        if (!triggered) StartCoroutine(ArmAndExplode());
    }

    IEnumerator ArmAndExplode()
    {
        triggered = true;

        float t = 0f;
        while (t < armDelay)
        {
            if (sprite) sprite.color = Mathf.PingPong(t * 20f, 1f) > 0.5f ? Color.red : Color.white;
            t += Time.deltaTime;
            yield return null;
        }

        if (explosionVFX) Instantiate(explosionVFX, transform.position, Quaternion.identity);
        Explosion.Create(transform.position, radius, damage, force, affectedLayers, terrainRadius);
        Destroy(gameObject);
        if (explosionSound) AudioSource.PlayClipAtPoint(explosionSound, transform.position);
    }
}