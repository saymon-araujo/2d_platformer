using System;
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
    
    [Header("Player Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 14f;
    private bool _isFacingRight = true;
    private FacingDirection _facingDirection = FacingDirection.Right;
    
    [Header("Collision Info")]
    [SerializeField] private float groundCheckDistance = .9f;
    [SerializeField] private LayerMask groundLayer;
    private bool _isGrounded;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        _xInput = Input.GetAxisRaw("Horizontal");
        
        HandleCollisions();
        HandleMovement();
        HandleFlip();
        HandleJump();
        HandleAnimations();
    }

    private void HandleCollisions()
    {
        _isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance,groundLayer);
    }

    private void HandleMovement()
    {
        _rb.linearVelocity = new Vector2(_xInput * moveSpeed, _rb.linearVelocity.y);
    }

    private void HandleAnimations()
    {
        _anim.SetFloat("xVelocity", _rb.linearVelocity.x );
        _anim.SetFloat("yVelocity", _rb.linearVelocity.y );
        _anim.SetBool("isGrounded", _isGrounded);
    }

    private void HandleJump()
    {
        if(Input.GetKeyDown(KeyCode.Space) && _isGrounded)
        {
            // Better for realism jump:
            // _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            
            // Better for precise jump, which is more used in platformers: 
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
        }
    }
    
    private void Flip()
    {
        transform.Rotate(0f, 180f, 0f);
        _isFacingRight = !_isFacingRight;
    }
    
    private void HandleFlip()
    {
        // If the player is moving right and not facing right or moving left and facing right
        if (_xInput > 0 && !_isFacingRight || _xInput < 0 && _isFacingRight)
        {
            _facingDirection = _facingDirection == FacingDirection.Right ? FacingDirection.Left : FacingDirection.Right;
            Flip();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistance));
    }
}
