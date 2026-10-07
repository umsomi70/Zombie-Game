using UnityEngine;



public class SupplyCrate : DestructibleObject
{
    [SerializeField] AudioClip explosionSound;   // add with the other fields
    [System.Serializable]
    public class Drop
    {
        public GameObject prefab;
        [Range(0f, 1f)] public float chance = 0.5f;
    }

    [SerializeField] Drop[] possibleDrops;

    protected override void Die()
    {
        foreach (var drop in possibleDrops)
        {
            if (drop.prefab && Random.value <= drop.chance)
            {
                var item = Instantiate(drop.prefab, transform.position, Quaternion.identity);
                var rb = item.GetComponent<Rigidbody2D>();
                if (rb) rb.AddForce(new Vector2(Random.Range(-2f, 2f), 4f), ForceMode2D.Impulse);
            }
        }
        base.Die();
        if (explosionSound) AudioSource.PlayClipAtPoint(explosionSound, transform.position);
    }
}