using UnityEngine;

public class FreedPrisoner : MonoBehaviour
{
    [SerializeField] float runSpeed = 4f;
    [SerializeField] float jumpForce = 6f;
    [SerializeField] float lifetime = 4f;

    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);   // celebratory hop
        Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(runSpeed, rb.linearVelocity.y);
    }
}