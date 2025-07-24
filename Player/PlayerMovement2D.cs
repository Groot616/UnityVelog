using UnityEngine;

public class PlayerMovement2D : MonoBehaviour
{
    public float moveSpeed = 5f;
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    [SerializeField]
    private Animator animator;
    private Rigidbody2D rb;
    private float moveInput;

    private bool isMoving = false;
    public bool isAttacking = false;

    [SerializeField]
    private float jumpSpeed = 5f;
    public bool isJumping = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!isAttacking)
        {
            moveInput = Input.GetAxisRaw("Horizontal");
        }
        else
        {
            moveInput = 0f;
        }
        isMoving = (moveInput != 0);
        if (isMoving)
        {
            spriteRenderer.flipX = moveInput < 0;
        }
        animator.SetBool("isMoving", isMoving);

        if (Input.GetKeyDown(KeyCode.C) && !isAttacking)
        {
            Attack();
        }

        if (Input.GetKeyDown(KeyCode.X) && !isAttacking && !isJumping)
        {
            Debug.Log("Jump Key Pressed");
            Jump();
        }

        if(!isJumping && rb.velocity.y < -0.1f)
        {
            isJumping = true;
            animator.SetBool("isJumping", isJumping);
            animator.SetTrigger("Jump");
            Debug.Log("isFalling");
        }
    }

    void FixedUpdate()
    {
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    }

    private void Attack()
    {
        if (isAttacking) return;

        isAttacking = true;
        animator.SetTrigger("Attack");
    }

    private void Jump()
    {
        if (isJumping) return;

        rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
        isJumping = true;
        Debug.Log("isJumping is true");
        animator.SetBool("isJumping", isJumping);
        animator.SetTrigger("Jump");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Ground"))
        {
            isJumping = false;
            Debug.Log("IsJumping is false");
            animator.SetBool("isJumping", false);
        }
    }

}
