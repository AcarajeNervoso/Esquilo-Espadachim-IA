using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class Player : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float jumpYVelocity = 15f;
    [SerializeField] float initialVelocity = 4f;

    Animator animator;
    Rigidbody2D physics;
    SpriteRenderer sprite;

    enum State { Idle, Walk, Jump, Fall, Attack, Crouch, Climb, Slide, Hang, Dash }

    State state = State.Idle;

    bool isGrounded = false;
    bool jumpInput = false;
    bool attackInput = false;
    bool isAttacking = false;
    bool crouchInput = false;
    bool climbInput = false;
    bool isWalled = false;
    bool canWallJump = true;


    float horizontalInput = 0f;

    void Awake()
    {
        animator = GetComponent<Animator>();
        physics = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();        
    }

    private void Update()
    {
        jumpInput = Input.GetKey(KeyCode.Space);
        attackInput = Input.GetKey(KeyCode.K);
        crouchInput = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        climbInput = Input.GetKey(KeyCode.W);        
        horizontalInput = Input.GetAxisRaw("Horizontal");
    }

    void FixedUpdate()
    {

   
        if (horizontalInput < 0f && (state != State.Slide && state != State.Climb && state != State.Hang))
        {
            sprite.flipX = false;
        }
        else if (horizontalInput > 0f && (state != State.Slide && state != State.Climb && state != State.Hang))
        {
            sprite.flipX = true;
        }

        switch (state)
        {
            case State.Idle: IdleState(); break;
            case State.Walk: WalkState(); break;
            case State.Jump: JumpState(); break;
            case State.Fall: FallState(); break;
            case State.Attack: AttackState(); break;
            case State.Crouch: CrouchState(); break;
            case State.Climb: ClimbState(); break;
            case State.Slide: SlideState(); break;
            case State.Hang: HangState(); break;
            
        }
    }


    void IdleState()
    {
       
        animator.Play("Idle");

        
        if (isGrounded)
        {
            if (jumpInput)
            {
                state = State.Jump;
            }
            else if (horizontalInput != 0f)
            {
                state = State.Walk;
            }
            else if (attackInput)
            {
                isAttacking = true;
                state = State.Attack;
            }
            else if (crouchInput)
            {
                state = State.Crouch;
            }
            else if (climbInput && isWalled)
            {
                state = State.Climb;
            }
        }
        else 
        {
            state = State.Fall;
        }
    }

    void WalkState()
    {
        

        if (!crouchInput && !Input.GetKey(KeyCode.J))
        {
            animator.Play("Walk");
            physics.velocity = initialVelocity * horizontalInput * Vector2.right;
        }

        else if (Input.GetKey(KeyCode.J))
        {
            animator.Play("Run");
            physics.velocity = (initialVelocity + 2) * horizontalInput * Vector2.right;
        }

        else if (crouchInput) 
        {
            
            animator.Play("Sneak");
            physics.velocity = initialVelocity * horizontalInput * Vector2.right;
        }
       
           
        if (isGrounded)
        {
            if (jumpInput)
            {                
                state = State.Jump;
            }
            else if (horizontalInput == 0f)
            {                
                state = State.Idle;
            }
            else if (crouchInput && horizontalInput == 0)
            {               
                state = State.Crouch;
            }
            else if (climbInput && isWalled)
            {                
                state = State.Climb;
            }
        }
        else
        {            
            state = State.Fall;
        }

    }

    void JumpState()
    {
        
        animator.Play("Jump");

        physics.velocity = (initialVelocity * horizontalInput * Vector2.right) + (jumpYVelocity * Vector2.up);

        if (climbInput && isWalled)
        {
            state = State.Climb;
        }
        else
        {
            state = State.Fall;
        }
    }

    IEnumerator WallJumpDelay()
    {
        yield return new WaitForSeconds(2);
        canWallJump = true;
    }


    void FallState()
    {
        
        physics.velocity = (physics.velocity.y * Vector2.up) + (initialVelocity * horizontalInput * Vector2.right);

        if (physics.velocity.y > 0f)
        {
            animator.Play("Jump");
        }
        else
        {
            animator.Play("Fall");
        }
                       
        if (isGrounded)
        {
            if (horizontalInput != 0f && physics.velocity.y == 0f)
            {
                state = State.Walk;
            }
            else
            {
                state = State.Idle;
            }
        }
        else if(isWalled && this.physics.velocity.y < 0)
        {
            state = State.Slide;
        }
       
    }

    void CrouchState()
    {
        
        physics.velocity = new Vector2(0, physics.velocity.y);
        animator.Play("Crouch");
                
        if (isGrounded)
        {
            if (jumpInput)
            {
                state = State.Jump;
            }
            else if (horizontalInput != 0f)
            {
                state = State.Walk;
            }
            else if (horizontalInput == 0f && !crouchInput)
            {
                state = State.Idle;
            }
        }
        else 
        {
            state = State.Fall;
        }
    }
        
    void AttackState()
    {
        
        if(isAttacking && Input.GetKey(KeyCode.S)) 
        {
            animator.Play("Attack C");
        }
        else if(isAttacking && Input.GetKey(KeyCode.W))
        {
            animator.Play("Attack B");
        }
        else
        {
            animator.Play("Attack A");
        }
                
        if (!isAttacking)
        {
            if (isGrounded)
            {
                if (jumpInput)
                {
                    state = State.Jump;
                }
                else if (horizontalInput != 0f)
                {
                    state = State.Walk;
                }
                else if (horizontalInput == 0f)
                {
                    state = State.Idle;
                }
            }
            else
            {
                state = State.Fall;
            }
        }
    }

    public void EndOfAttack()
    {
        isAttacking = false;
    }
      
    void ClimbState()
    {
        animator.Play("Climb");
        physics.velocity = initialVelocity * Vector2.up;

        if (!climbInput) 
        {
            state = State.Slide;
        }
        else if (jumpInput && canWallJump == true)
        {
            canWallJump = false;
            StartCoroutine(WallJumpDelay());
            state = State.Jump;
        }
        else if (isGrounded)
        {
            state = State.Idle;
        }
        else if (!isWalled || (horizontalInput > 0 && sprite.flipX == false || horizontalInput < 0 && sprite.flipX == true))
        {
            state = State.Fall;
        }
    }

    void SlideState()
    {
        
        if(this.physics.velocity.y < 0) 
        {
            animator.Play("Wall Slide");
        }
        
        if(climbInput)
        {
            state = State.Climb;
        }
        else if(horizontalInput > 0 && sprite.flipX == true || horizontalInput < 0 && sprite.flipX == false)
        {
            state = State.Hang;
        }
        else if (jumpInput && canWallJump == true)
        {
            canWallJump = false;
            StartCoroutine(WallJumpDelay());
            state = State.Jump;
        }
        else if (isGrounded)
        {
            state = State.Idle;
        }
        else if (!isWalled || (horizontalInput > 0 && sprite.flipX == false || horizontalInput < 0 && sprite.flipX == true))
        {
            state = State.Fall;
        }
    }

    void HangState()
    {        
        physics.velocity = (physics.velocity.y * Vector2.zero);
        animator.Play("Hanging");
                
        if (climbInput)
        {
            state = State.Climb;
        }
        else if (horizontalInput == 0) 
        {
            state = State.Slide;
        }
        else if (jumpInput && canWallJump == true)
        {
            canWallJump = false;
            StartCoroutine(WallJumpDelay());
            state = State.Jump;
        }
        else if (isGrounded)
        {
            state = State.Idle;
        }
        else if (!isWalled || (horizontalInput > 0 && sprite.flipX == false || horizontalInput < 0 && sprite.flipX == true))
        {
            state = State.Fall;
        }
    }
        
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            isWalled = true;
        }
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }

    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            isWalled = false;
        }
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
       
}
