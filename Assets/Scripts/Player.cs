using System;
using System.Collections;
using UnityEngine;

public enum FacingDirection
    {
    Right,
    Left
    }

public class Player : MonoBehaviour
{
    #region Variables and Properties
    private Rigidbody2D _rb;
    private Animator _anim;
    private float _xInput;
    private float _yInput;

    [Header("Player Movement")]
    [Tooltip("How fast the player moves horizontally.")]
    [SerializeField] private float moveSpeed = 6f;
    [Tooltip("The force applied when the player jumps.")]
    [SerializeField] private float jumpForce = 14f;
    [Tooltip("The force applied when the player performs a double jump.")]
    [SerializeField] private float doubleJumpForce = 11f;
    [Tooltip("The time window to buffer jump inputs before landing.")]
    [SerializeField] private float bufferJumpWindow = 0.25f;
    [Tooltip("The time window for allowing a coyote jump after leaving a platform.")]
    [SerializeField] private float coyoteJumpWindow = 0.25f;
    private float _timeWhenBufferJumpPressed = -1f;
    private float _timeWhenCoyoteJumpPressed = -1f;
    private bool _canDoubleJump;
    private bool _isFacingRight = true;
    private FacingDirection _facingDirection = FacingDirection.Right;

    [Header("Collision Info")]
    [Tooltip("Layers considered as ground for collision detection.")]
    [SerializeField] private LayerMask groundLayer;
    [Tooltip("Distance to check for ground below the player.")]
    [SerializeField] private float groundCheckDistance = .9f;
    [Tooltip("Distance to check for walls in front of the player.")]
    [SerializeField] private float wallCheckDistance = .75f;
    private bool _isGrounded;
    private bool _isAirborne;
    private bool _isWallDetected;

    [Header("Wall Interactions")]
    [Tooltip("Base speed for sliding down a wall.")]
    [SerializeField] private float baseWallSlideSpeed = 0.05f;
    [Tooltip("Increased speed when the player presses against the wall while sliding.")]
    [SerializeField] private float fasterWallSlideSpeed = 1f;
    [Tooltip("Duration of the wall jump effect.")]
    [SerializeField] private float wallJumpDuration = .45f;
    [Tooltip("Force applied when performing a wall jump (x: horizontal, y: vertical).")]
    [SerializeField] private Vector2 wallJumpForce = new Vector2(7f, 14f);
    private bool _isWallJumping;

    [Header("Knock Back")]
    [Tooltip("Duration of the knock back effect.")]
    [SerializeField] private float knockBackDuration = .65f;
    [Tooltip("Force applied when the player is knocked back (x: horizontal, y: vertical).")]
    [SerializeField] private Vector2 knockBackPower = new Vector2(5f, 7f);
    private bool _isKnocked;
    #endregion

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponentInChildren<Animator>();
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            KnockBack();
        }

        HandleInput();
        HandleCollisions();
        HandleMovement();
        HandleFlip();
        HandleJump();
        HandleWallSlide();
        HandleAnimations();
    }

    #region Input Handling
    private void HandleInput()
    {
        _xInput = Input.GetAxisRaw("Horizontal");
        _yInput = Input.GetAxisRaw("Vertical");
    }
    #endregion

    #region Wall Slide
    private void HandleWallSlide()
    {
        var canWallSlide = _isWallDetected && _rb.linearVelocity.y < 0;
        var isPressingAgainstWall = canWallSlide && _yInput < 0;
        var wallSlideSpeed = isPressingAgainstWall ? fasterWallSlideSpeed : baseWallSlideSpeed;

        // Apply sliding behavior only if the player is moving downward on a wall
        if (!canWallSlide)
        {
            return;
        }

        _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y * wallSlideSpeed);
    }
    #endregion

    #region Collision Handling
    private void HandleCollisions()
    {
        var direction = _isFacingRight ? Vector2.right : Vector2.left;

        _isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        _isWallDetected = Physics2D.Raycast(transform.position, direction, wallCheckDistance, groundLayer);
    }
    #endregion

    #region Movement Handling
    private void HandleMovement()
    {
        if (_isWallJumping || _isKnocked) return;

        // Allow movement if not on a wall or if attempting to move away from the wall
        if (_isWallDetected)
        {
            return; // Block movement into the wall
        }

        _rb.linearVelocity = new Vector2(_xInput * moveSpeed, _rb.linearVelocity.y);
    }
    #endregion

    #region Animation Handling
    private void HandleAnimations()
    {
        _anim.SetFloat("xVelocity", _rb.linearVelocity.x);
        _anim.SetFloat("yVelocity", _rb.linearVelocity.y);
        _anim.SetBool("isGrounded", _isGrounded);
        _anim.SetBool("isWallDetected", _isWallDetected);
    }
    #endregion

    #region Jump Handling
    private void HandleJump()
    {
        UpdateAirborneStatus();

        if (!Input.GetKeyDown(KeyCode.Space))
        {
            return;
        }

        RequestBufferJump();

        var isCoyoteJumpAvailable = Time.time < _timeWhenCoyoteJumpPressed + coyoteJumpWindow;

        if (_isGrounded || isCoyoteJumpAvailable)
        {
            Jump();
        }

        if (_isWallDetected && !_isGrounded)
        {
            WallJump();
        }

        if (_canDoubleJump && !_isGrounded && !_isWallDetected)
        {
            DoubleJump();
        }

        CancelCoyoteJump();
    }

    private void DoubleJump()
    {
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, doubleJumpForce);
        _isWallJumping = false;
        _canDoubleJump = false;
    }

    private void Jump()
    {
        // Better for realism jump:
        // _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        // Better for precise jump, which is more used in platformers: 
        _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
    }

    private void WallJump()
    {
        // If facing right, we want to jump left, so use -1; if facing left, we want to jump right, so use 1.
        var horizontalDirection = _isFacingRight ? -1 : 1;

        // Apply the wall jump force in the opposite horizontal direction of the wall
        _rb.linearVelocity = new Vector2(wallJumpForce.x * horizontalDirection, wallJumpForce.y);
        _canDoubleJump = true;

        Flip();
        StopCoroutine(WallJumpRoutine());
        StartCoroutine(WallJumpRoutine());
    }

    private void RequestBufferJump()
    {
        if (_isAirborne)
        {
            _timeWhenBufferJumpPressed = Time.time;
        }
    }

    private void AttemptBufferJump()
    {
        if (Time.time < _timeWhenBufferJumpPressed + bufferJumpWindow)
        {
            Jump();
        }
    }
    #endregion

    #region Knock Back and Handling
    public void KnockBack()
    {
        if (_isKnocked) return;

        _anim.SetTrigger("KnockBack");
        StartCoroutine(KnockBackRoutine());

        var horizontalDirection = _isFacingRight ? -1 : 1;

        _rb.linearVelocity = new Vector2(knockBackPower.x * horizontalDirection, knockBackPower.y);
    }

    private IEnumerator KnockBackRoutine()
    {
        _isKnocked = true;
        yield return new WaitForSeconds(knockBackDuration);
        _isKnocked = false;
    }
    #endregion

    #region Coroutines
    private IEnumerator WallJumpRoutine()
    {
        _isWallJumping = true;
        yield return new WaitForSeconds(wallJumpDuration);
        _isWallJumping = false;
    }
    #endregion

    #region Airborne and Landing
    private void UpdateAirborneStatus()
    {
        if (_isAirborne && _isGrounded)
        {
            HandleLanding();
        }

        if (!_isAirborne && !_isGrounded)
        {
            BecomeAirborne();
        }
    }

    private void BecomeAirborne()
    {
        _isAirborne = true;

        var isFalling = _rb.linearVelocity.y < 0;

        if (isFalling)
        {
            ActivateCoyoteJump();
        }
    }

    private void ActivateCoyoteJump()
    {
        _timeWhenCoyoteJumpPressed = Time.time;
    }

    private void CancelCoyoteJump()
    {
        _timeWhenCoyoteJumpPressed = -Time.time;
    }

    private void HandleLanding()
    {
        _isAirborne = false;
        _canDoubleJump = true;

        AttemptBufferJump();
    }
    #endregion

    #region Flip Handling
    private void Flip()
    {
        transform.Rotate(0f, 180f, 0f);
        _isFacingRight = !_isFacingRight;
    }

    private void HandleFlip()
    {
        // Determine if the player is moving away from the wall while it is detected
        var movingAwayFromWall = false;

        if (_isWallDetected)
        {
            // If facing right and moving left, or facing left and moving right
            movingAwayFromWall = (_isFacingRight && _xInput < 0) || (!_isFacingRight && _xInput > 0);
        }

        // Allow flipping if not on a wall or moving away from the wall
        if (!_isWallDetected || movingAwayFromWall)
        {
            if (_xInput > 0 && !_isFacingRight || _xInput < 0 && _isFacingRight)
            {
                _facingDirection = _facingDirection == FacingDirection.Right ? FacingDirection.Left : FacingDirection.Right;
                Flip();
            }
        }
    }
    #endregion
}