using System.Collections;
using UnityEngine;

public class CollapsingPlatform : MonoBehaviour
{
    [SerializeField] float shakeTime = 0.6f;
    [SerializeField] float shakeAmount = 0.05f;

    Rigidbody2D rb;
    Collider2D col;
    Vector3 startPos;
    bool triggered;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        startPos = transform.position;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        if (triggered || !c.collider.CompareTag("Player")) return;

        if (c.GetContact(0).normal.y < -0.5f)   // player landed on top
            StartCoroutine(Collapse());
    }

    IEnumerator Collapse()
    {
        triggered = true;

        float t = 0f;
        while (t < shakeTime)
        {
            transform.position = startPos + (Vector3)Random.insideUnitCircle * shakeAmount;
            t += Time.deltaTime;
            yield return null;
        }

        transform.position = startPos;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 2f;
        col.enabled = false;

        Destroy(gameObject, 3f);
    }
}