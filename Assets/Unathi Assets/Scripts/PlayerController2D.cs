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

    [Header("Ladder")]
    public float climbSpeed = 5f;

    [Header("Visuals")]
    public Transform playerGFX;

    private Rigidbody2D rb;

    private float moveInput;
    private float climbInput;

    private int jumpsRemaining;

    private bool touchingLeftWall;
    private bool touchingRightWall;

    private bool isWallGrabbing;

    private bool isOnLadder;
    private Ladder2D currentLadder;

    // Remember the player's gravity before entering the ladder
    private float normalGravityScale;

    public bool IsWallGrabbing => isWallGrabbing;

    public int WallGrabSide { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        jumpsRemaining = maxJumps;

        // Store the original gravity setting
        normalGravityScale = rb.gravityScale;
    }

    private void Update()
    {
        GetMovementInput();
        GetClimbInput();

        CheckWalls();

        // If we are climbing and press left/right,
        // exit the ladder.
        if (isOnLadder && Mathf.Abs(moveInput) > 0)
        {
            ExitLadder();
        }

        // Handle ladder
        if (isOnLadder)
        {
            HandleLadder();
        }
        else
        {
            HandleWallGrab();
        }

        // Jump
        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Jump();
        }

        FlipPlayerGFX();
    }

    private void FixedUpdate()
    {
        // Ladder movement replaces normal movement.
        if (isOnLadder)
        {
            HandleLadderMovement();
            return;
        }

        HandleMovement();
        HandleWallSlide();

        // Restore jumps when touching the ground
        if (IsGrounded())
        {
            jumpsRemaining = maxJumps;
        }
    }

    // =========================================================
    // INPUT
    // =========================================================

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

    private void GetClimbInput()
    {
        climbInput = 0f;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.wKey.isPressed ||
            Keyboard.current.upArrowKey.isPressed)
        {
            climbInput = 1f;
        }

        if (Keyboard.current.sKey.isPressed ||
            Keyboard.current.downArrowKey.isPressed)
        {
            climbInput = -1f;
        }
    }

    // =========================================================
    // NORMAL MOVEMENT
    // =========================================================

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

    // =========================================================
    // WALL CHECKING
    // =========================================================

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

    // =========================================================
    // WALL GRAB
    // =========================================================

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

    // =========================================================
    // WALL SLIDE
    // =========================================================

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

    // =========================================================
    // JUMP
    // =========================================================

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

    // =========================================================
    // WALL JUMP
    // =========================================================

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

    // =========================================================
    // LADDER
    // =========================================================

    private void HandleLadder()
    {
        if (!isOnLadder)
            return;

        // While on the ladder, stop wall grabbing.
        isWallGrabbing = false;
        WallGrabSide = 0;

        // Disable gravity while climbing.
        rb.gravityScale = 0f;
    }

    private void HandleLadderMovement()
    {
        if (!isOnLadder)
            return;

        // Move vertically on the ladder.
        rb.linearVelocity = new Vector2(
            0f,
            climbInput * climbSpeed
        );

        // Keep the player centered on the ladder.
        if (currentLadder != null)
        {
            Vector2 position = rb.position;

            position.x = currentLadder.transform.position.x;

            rb.position = position;
        }
    }

    // =========================================================
    // ENTER LADDER
    // =========================================================

    private void EnterLadder(Ladder2D ladder)
    {
        currentLadder = ladder;
        isOnLadder = true;

        // Store gravity before changing it
        normalGravityScale = rb.gravityScale;

        // Disable gravity
        rb.gravityScale = 0f;

        // Stop current movement
        rb.linearVelocity = Vector2.zero;

        // Stop wall grabbing
        isWallGrabbing = false;
        WallGrabSide = 0;
    }

    // =========================================================
    // EXIT LADDER
    // =========================================================

    private void ExitLadder()
    {
        if (!isOnLadder)
            return;

        isOnLadder = false;
        currentLadder = null;

        // Restore the original gravity
        rb.gravityScale = normalGravityScale;

        // Allow normal movement immediately
        rb.linearVelocity = new Vector2(
            moveInput * moveSpeed,
            rb.linearVelocity.y
        );
    }

    // =========================================================
    // LADDER TRIGGER
    // =========================================================

    private void OnTriggerEnter2D(Collider2D other)
    {
        Ladder2D ladder = other.GetComponent<Ladder2D>();

        if (ladder == null)
            return;

        // Don't enter another ladder while already climbing
        if (isOnLadder)
            return;

        EnterLadder(ladder);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Ladder2D ladder = other.GetComponent<Ladder2D>();

        if (ladder == null)
            return;

        if (ladder != currentLadder)
            return;

        ExitLadder();
    }

    // =========================================================
    // PLAYER FLIP
    // =========================================================

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

    // =========================================================
    // GROUND CHECK
    // =========================================================

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }

    // =========================================================
    // GIZMOS
    // =========================================================

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