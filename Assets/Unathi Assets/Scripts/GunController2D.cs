using UnityEngine;
using UnityEngine.InputSystem;

public class GunController2D : MonoBehaviour
{
    [Header("References")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public PlayerController2D playerController;

    [Header("Shooting")]
    public float bulletSpeed = 15f;
    public float fireRate = 0.15f;

    [Header("Aim")]
    [Tooltip("8 = 45 degree increments")]
    public int numberOfDirections = 8;

    private Camera mainCamera;
    private float nextFireTime;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        AimGun();

        if (Mouse.current != null &&
            Mouse.current.leftButton.isPressed)
        {
            Shoot();
        }
    }

    private void AimGun()
    {
        if (Mouse.current == null || mainCamera == null)
            return;

        Vector2 mouseScreenPosition =
            Mouse.current.position.ReadValue();

        Vector3 mouseWorldPosition =
            mainCamera.ScreenToWorldPoint(
                new Vector3(
                    mouseScreenPosition.x,
                    mouseScreenPosition.y,
                    -mainCamera.transform.position.z
                )
            );

        Vector2 direction =
            mouseWorldPosition - transform.position;

        if (direction.sqrMagnitude < 0.01f)
            return;

        float angle =
            Mathf.Atan2(direction.y, direction.x) *
            Mathf.Rad2Deg;

        // Convert -180/+180 into 0-360
        if (angle < 0)
            angle += 360f;

        // Snap to 8 directions
        float angleStep =
            360f / numberOfDirections;

        float snappedAngle =
            Mathf.Round(angle / angleStep) *
            angleStep;

        // Limit aim while wall grabbing
        snappedAngle = LimitWallAim(snappedAngle);

        // Apply rotation
        transform.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                snappedAngle
            );
    }

    private float LimitWallAim(float angle)
    {
        // Not grabbing a wall = full aiming freedom
        if (playerController == null ||
            !playerController.IsWallGrabbing)
        {
            return angle;
        }

        // LEFT WALL
        // Only allow angles from 0° to 90°
        // or 270° to 360°
        if (playerController.WallGrabSide == -1)
        {
            if (angle > 90f && angle < 270f)
            {
                // Decide which boundary is closest
                float distanceTo90 =
                    Mathf.Abs(Mathf.DeltaAngle(angle, 90f));

                float distanceTo270 =
                    Mathf.Abs(Mathf.DeltaAngle(angle, 270f));

                if (distanceTo90 < distanceTo270)
                    return 90f;
                else
                    return 270f;
            }
        }

        // RIGHT WALL
        // Only allow angles from 90° to 270°
        if (playerController.WallGrabSide == 1)
        {
            if (angle < 90f || angle > 270f)
            {
                float distanceTo90 =
                    Mathf.Abs(Mathf.DeltaAngle(angle, 90f));

                float distanceTo270 =
                    Mathf.Abs(Mathf.DeltaAngle(angle, 270f));

                if (distanceTo90 < distanceTo270)
                    return 90f;
                else
                    return 270f;
            }
        }

        return angle;
    }

    private void Shoot()
    {
        if (Time.time < nextFireTime)
            return;

        nextFireTime =
            Time.time + fireRate;

        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning(
                "Gun is missing Bullet Prefab or Fire Point."
            );

            return;
        }

        GameObject bullet =
            Instantiate(
                bulletPrefab,
                firePoint.position,
                firePoint.rotation
            );

        Rigidbody2D bulletRb =
            bullet.GetComponent<Rigidbody2D>();

        if (bulletRb != null)
        {
            bulletRb.linearVelocity =
                firePoint.right * bulletSpeed;
        }
    }
}