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
    private Rigidbody2D _rb;
    private Animator _anim;
    private float _xInput;
    private float _yInput;

    [Header("Player Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float doubleJumpForce = 11f;
    private bool _canDoubleJump;
    private bool _isFacingRight = true;
    private FacingDirection _facingDirection = FacingDirection.Right;

    [Header("Collision Info")] 
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = .9f;
    [SerializeField] private float wallCheckDistance = .75f;
    private bool _isGrounded;
    private bool _isAirborne;
    private bool _isWallDetected;

    [Header("Wall Interactions")]
    [SerializeField] private float baseWallSlideSpeed =  0.05f;
    [SerializeField] private float fasterWallSlideSpeed = 1f;
    [SerializeField] private float wallJumpDuration = .45f;
    [SerializeField] private Vector2 wallJumpForce = new Vector2(7f, 14f);
    private bool _isWallJumping;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        HandleInput();
        HandleCollisions();
        HandleMovement();
        HandleFlip();
        HandleJump();
        HandleWallSlide();
        HandleAnimations();
    }

    private void HandleInput()
    {
        _xInput = Input.GetAxisRaw("Horizontal");
        _yInput = Input.GetAxisRaw("Vertical");
    }

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

    private void HandleCollisions()
    {
        Vector2 direction = _isFacingRight ? Vector2.right : Vector2.left;

        _isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        _isWallDetected = Physics2D.Raycast(transform.position, direction, wallCheckDistance, groundLayer);
    }

    private void HandleMovement()
    {
        
        if (_isWallJumping) return;
        
        // Allow movement if not on a wall or if attempting to move away from the wall
        if (_isWallDetected)
        {
            return; // Block movement into the wall
        }

        _rb.linearVelocity = new Vector2(_xInput * moveSpeed, _rb.linearVelocity.y);
    }

    private void HandleAnimations()
    {
        _anim.SetFloat("xVelocity", _rb.linearVelocity.x);
        _anim.SetFloat("yVelocity", _rb.linearVelocity.y);
        _anim.SetBool("isGrounded", _isGrounded);
        _anim.SetBool("isWallDetected", _isWallDetected);
    }

    private void HandleJump()
    {
        UpdateAirborneStatus();

        if (!Input.GetKeyDown(KeyCode.Space))
        {
            return;
        }

        if (_isGrounded)
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

    private IEnumerator WallJumpRoutine()
    {
        _isWallJumping = true;
        yield return new WaitForSeconds(wallJumpDuration);
        _isWallJumping = false;
    }

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
    }

    private void HandleLanding()
    {
        _isAirborne = false;
        _canDoubleJump = true;
    }

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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistance));
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x + wallCheckDistance, transform.position.y));
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x - wallCheckDistance, transform.position.y));
    }
}