using UnityEngine;

public enum CharacterState
{
    Idle,
    Attack,
    Walk,
    Run,
    Slide,
    Jump,
    Crouch,
    Sneak,
    Roll,
    Swimming
}

public class mainplayer : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;

    public float walkSpeed = 2f;
    public float runSpeed = 6f;
    public float doubleTapTime = 0.2f; // Tempo máximo entre dois toques para considerar um double tap

    private CharacterState currentState = CharacterState.Idle;
    private bool isGrounded;
    private bool isJump = false;
    public LayerMask groundLayerMask;
    private float lastTapTime = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        switch (currentState)
        {
            case CharacterState.Idle:
                if (Input.GetKeyDown(KeyCode.J)) Attack();
                else if (IsInWater()) Swimming();
                else if (Input.GetKeyDown(KeyCode.W) && isJump || Input.GetKeyDown(KeyCode.UpArrow) && isJump) Jump();
                else if (Input.GetKeyDown(KeyCode.LeftControl)) Crouch();
                else if (Input.GetKeyDown(KeyCode.LeftShift) || IsDoubleTap()) Run();
                else if (Input.GetKeyDown(KeyCode.LeftAlt)) Roll();
                else if (Input.GetKeyDown(KeyCode.S)) Slide();
                else if (Mathf.Abs(horizontalInput) > 0) Walk();
                break;
            case CharacterState.Attack:
                // Implementar lógica para o estado Attack
                break;
            case CharacterState.Walk:
                // Implementar lógica para o estado Walk
                break;
                // Adicionar cases para os outros estados conforme necessário
        }

        // Verifica se o personagem está no chão
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.1f, groundLayerMask);
        isGrounded = hit.collider != null;

        // Verifica se o personagem parou de se mover e chama o método Idle()
        if (currentState != CharacterState.Jump && !isJump && isGrounded && Mathf.Abs(horizontalInput) == 0)
        {
            Idle();
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("isGrounded"))
        {
            isJump = true;
        }
    }

    void SetAnimation(CharacterState state)
    {
        animator.SetInteger("State", (int)state);
    }

    void Idle()
    {
        rb.velocity = Vector2.zero;
        SetAnimation(CharacterState.Idle);
    }

    void Attack()
    {
        // Implementar lógica para o estado Attack
    }

    void Walk()
    {
        SetAnimation(CharacterState.Walk);
        rb.velocity = walkSpeed * Input.GetAxisRaw("Horizontal") * Vector2.right;
    }

    void Run()
    {
        // Implementar lógica para o estado Run
    }

    void Slide()
    {
        // Implementar lógica para o estado Slide
    }

    void Jump()
    {
        // Implementar lógica para o estado Jump
    }

    void Crouch()
    {
        // Implementar lógica para o estado Crouch
    }

    void Sneak()
    {
        // Implementar lógica para o estado Sneak
    }

    void Roll()
    {
        // Implementar lógica para o estado Roll
    }

    void Swimming()
    {
        // Implementar lógica para o estado Swimming
    }

    bool IsDoubleTap()
    {
        if (Time.time - lastTapTime < doubleTapTime)
        {
            lastTapTime = 0;
            return true;
        }
        else
        {
            lastTapTime = Time.time;
            return false;
        }
    }

    bool IsInWater()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.5f); // Raio de detecção de água
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Water"))
            {
                return true;
            }
        }
        return false;
    }
}
