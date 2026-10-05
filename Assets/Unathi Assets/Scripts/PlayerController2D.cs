using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f;

    [Header("Jump")]
    public float jumpForce = 12f;
    public int maxJumps = 2;

    [Header("Wall Grab")]
    public Transform wallCheckLeft;
    public Transform wallCheckRight;
    public float wallCheckRadius = 0.2f;
    public LayerMask wallLayer;

    [Tooltip("How fast the player slides down a wall when not grabbing.")]
    public float wallSlideSpeed = 3f;

    [Tooltip("Horizontal force when jumping away from a wall.")]
    public float wallJumpHorizontalForce = 8f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Visuals")]
    public Transform playerGFX;

    private Rigidbody2D rb;

    private float moveInput;
    private int jumpsRemaining;

    private bool touchingLeftWall;
    private bool touchingRightWall;

    private bool isWallGrabbing;

    public bool IsWallGrabbing => isWallGrabbing;

    public int WallGrabSide { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        jumpsRemaining = maxJumps;
    }

    private void Update()
    {
        GetMovementInput();

        CheckWalls();

        HandleWallGrab();

        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Jump();
        }

        FlipPlayerGFX();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleWallSlide();

        // Restore jumps when touching the ground
        if (IsGrounded())
        {
            jumpsRemaining = maxJumps;
        }
    }

    private void GetMovementInput()
    {
        moveInput = 0f;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.aKey.isPressed ||
            Keyboard.current.leftArrowKey.isPressed)
        {
            moveInput = -1f;
        }

        if (Keyboard.current.dKey.isPressed ||
            Keyboard.current.rightArrowKey.isPressed)
        {
            moveInput = 1f;
        }
    }

    private void HandleMovement()
    {
        // Don't allow normal horizontal movement
        // while actively grabbing a wall.
        if (isWallGrabbing)
            return;

        rb.linearVelocity = new Vector2(
            moveInput * moveSpeed,
            rb.linearVelocity.y
        );
    }

    private void CheckWalls()
    {
        touchingLeftWall = Physics2D.OverlapCircle(
            wallCheckLeft.position,
            wallCheckRadius,
            wallLayer
        );

        touchingRightWall = Physics2D.OverlapCircle(
            wallCheckRight.position,
            wallCheckRadius,
            wallLayer
        );
    }

    private void HandleWallGrab()
    {
        bool wasWallGrabbing = isWallGrabbing;

        isWallGrabbing = false;
        WallGrabSide = 0;

        // Don't grab while standing on the ground
        if (IsGrounded())
            return;

        // LEFT WALL
        if (touchingLeftWall && moveInput < 0)
        {
            isWallGrabbing = true;
            WallGrabSide = -1;
        }

        // RIGHT WALL
        if (touchingRightWall && moveInput > 0)
        {
            isWallGrabbing = true;
            WallGrabSide = 1;
        }

        if (isWallGrabbing)
        {
            // Reset jumps when we FIRST grab the wall
            if (!wasWallGrabbing)
            {
                jumpsRemaining = maxJumps;
            }

            // Stop falling/movement while grabbing
            rb.linearVelocity = new Vector2(
                0f,
                0f
            );
        }
    }

    private void HandleWallSlide()
    {
        if (IsGrounded())
            return;

        if (isWallGrabbing)
            return;

        // If touching a wall, limit falling speed.
        if (touchingLeftWall || touchingRightWall)
        {
            if (rb.linearVelocity.y < -wallSlideSpeed)
            {
                rb.linearVelocity = new Vector2(
                    rb.linearVelocity.x,
                    -wallSlideSpeed
                );
            }
        }
    }

    private void Jump()
    {
        if (jumpsRemaining <= 0)
            return;

        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            jumpForce
        );

        jumpsRemaining--;
    }

    private void WallJump()
    {
        float jumpDirection;

        if (touchingLeftWall)
        {
            jumpDirection = 1f;
        }
        else
        {
            jumpDirection = -1f;
        }

        rb.linearVelocity = new Vector2(
            jumpDirection * wallJumpHorizontalForce,
            jumpForce
        );

        jumpsRemaining = maxJumps;

        isWallGrabbing = false;
        WallGrabSide = 0;
    }

    private void FlipPlayerGFX()
    {
        if (playerGFX == null)
            return;

        if (moveInput > 0)
        {
            playerGFX.localScale = new Vector3(
                Mathf.Abs(playerGFX.localScale.x),
                playerGFX.localScale.y,
                playerGFX.localScale.z
            );
        }
        else if (moveInput < 0)
        {
            playerGFX.localScale = new Vector3(
                -Mathf.Abs(playerGFX.localScale.x),
                playerGFX.localScale.y,
                playerGFX.localScale.z
            );
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;

            Gizmos.DrawWireSphere(
                groundCheck.position,
                groundCheckRadius
            );
        }

        if (wallCheckLeft != null)
        {
            Gizmos.color = Color.blue;

            Gizmos.DrawWireSphere(
                wallCheckLeft.position,
                wallCheckRadius
            );
        }

        if (wallCheckRight != null)
        {
            Gizmos.color = Color.blue;

            Gizmos.DrawWireSphere(
                wallCheckRight.position,
                wallCheckRadius
            );
        }
    }
}