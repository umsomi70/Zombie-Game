using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [SerializeField] float lifetime = 2f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}