using UnityEngine;

public class DestructibleObject : MonoBehaviour, IDamageable
{
    [SerializeField] protected float health = 10f;
    [SerializeField] protected GameObject destroyVFX;
    protected bool isDead;

    public virtual void TakeDamage(float amount, Vector2 hitPoint)
    {
        if (isDead) return;
        health -= amount;
        if (health <= 0f) Die();
    }

    protected virtual void Die()
    {
        isDead = true;
        if (destroyVFX) Instantiate(destroyVFX, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}