using UnityEngine;
using UnityEngine.Events;

public class PrisonerCage : DestructibleObject
{
    [SerializeField] GameObject prisonerPrefab;
    [SerializeField] Transform spawnPoint;
    public UnityEvent onRescued;

    protected override void Die()
    {
        if (prisonerPrefab)
            Instantiate(prisonerPrefab, spawnPoint ? spawnPoint.position : transform.position,
                        Quaternion.identity);

        onRescued?.Invoke();
        base.Die();
    }
}