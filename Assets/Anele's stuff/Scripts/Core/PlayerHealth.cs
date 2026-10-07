using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] float maxHealth = 100f;
    [SerializeField] float invincibleTime = 0.15f;   // short window so one blast can't hit twice

    [Header("Feedback")]
    [SerializeField] SpriteRenderer sprite;           // flashes red when hit
    [SerializeField] GameObject deathVFX;

    [Header("On Death")]
    [SerializeField] MonoBehaviour[] disableOnDeath;  // drag your movement and shooting scripts here
    [SerializeField] bool reloadSceneOnDeath = true;
    [SerializeField] float reloadDelay = 1.5f;

    [Header("Events")]
    public UnityEvent onDamaged;
    public UnityEvent onDied;

    public float CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }

    float invincibleUntil;

    void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(float amount, Vector2 hitPoint)
    {
        if (IsDead) return;
        if (Time.time < invincibleUntil) return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        invincibleUntil = Time.time + invincibleTime;

        onDamaged?.Invoke();
        if (sprite) StartCoroutine(Flash());

        if (CurrentHealth <= 0f) Die();
    }

    // call this from health pickups
    public void Heal(float amount)
    {
        if (IsDead) return;
        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
    }

    void Die()
    {
        IsDead = true;

        foreach (var script in disableOnDeath)
            if (script) script.enabled = false;

        var rb = GetComponent<Rigidbody2D>();
        if (rb) rb.linearVelocity = Vector2.zero;

        if (deathVFX) Instantiate(deathVFX, transform.position, Quaternion.identity);
        if (sprite) sprite.enabled = false;

        onDied?.Invoke();

        if (reloadSceneOnDeath)
            StartCoroutine(ReloadAfterDelay());
    }

    IEnumerator Flash()
    {
        sprite.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        if (!IsDead) sprite.color = Color.white;
    }

    IEnumerator ReloadAfterDelay()
    {
        yield return new WaitForSeconds(reloadDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}