using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cat : BaseEnemy {

    //Direction
    public Transform spriteTransform;
    private float directionFloatX = 1;
    private Vector2 initialScale;

    //Components
    protected FloorAttachMovement floorAttachingMovement;
    protected AnimationManager animationManager;
    public Rigidbody2D rigidbody;

    //Movement related
    public float fWalkSpeed = 4f;
    public float gravityMultiplier = 1;
    protected float gravity;
    public float verticalCalulation = 0;

    //Ienumerators
    protected IEnumerator DieRoutine;
    public float dieJump = 10f;
    public float dieTime = 3f;

    //Attack
    protected IEnumerator AttackRoutine;
    public float attackTime = 1f;
    public float attackJump = 1f;

    //The character states
    public enum EnemyState
    {
        Free,
        Bump,
        Hit,
        Attack
    }
    public EnemyState state = EnemyState.Free;

    // Use this for initialization
    void Start () {
        getComponentReferences();
        initializeVariables();
        setInitialGravity();
    }

    void getComponentReferences()
    {
        floorAttachingMovement = GetComponent<FloorAttachMovement>();
        animationManager = GetComponent<AnimationManager>();
        rigidbody = GetComponent<Rigidbody2D>();
    }

    void initializeVariables()
    {
        initialScale = spriteTransform.localScale;
    }

    void setInitialGravity()
    {
        gravity = -gravityMultiplier;
    }

    // Update is called once per frame
    void Update()
    {
        //Do actions based on states
        switch (state)
        {
            case EnemyState.Free:
                Walk();
                CheckForTurns();
                 break;

            case EnemyState.Hit:

                break;

            case EnemyState.Attack:
                Attack();
                break;

                
        }

        //Actions regardless of state
        CheckForGravity();
        SetAnimatorVariables();
    }
    public void CheckForGravity()
    {
        if (!floorAttachingMovement.isGrounded)
        {
            verticalCalulation += -gravityMultiplier;
            transform.Translate(new Vector3(0, verticalCalulation * Time.deltaTime, 0));
            return;
        }

        if (floorAttachingMovement.isGrounded && verticalCalulation != 0 && (state != EnemyState.Hit))
        {
            floorAttachingMovement.stickToFloor();
            verticalCalulation = 0;
        }
    }

    public void SetAnimatorVariables()
    {
        animationManager.getAnimator().SetFloat("Speed", directionFloatX);
        animationManager.getAnimator().SetBool("InAir", !floorAttachingMovement.isGrounded);
        animationManager.getAnimator().SetBool("Bump", (state == EnemyState.Bump));
        animationManager.getAnimator().SetBool("Hit", (state == EnemyState.Hit));
        animationManager.getAnimator().SetBool("Attack", (state == EnemyState.Attack));
        animationManager.getAnimator().SetFloat("VerticalSpeed", verticalCalulation);
    }

    void Walk()
    {
        floorAttachingMovement.MoveSideWays(directionFloatX * fWalkSpeed);
    }

    void CheckForTurns()
    {
        if(floorAttachingMovement.isBumping)
        {
            directionFloatX *= -1;
            ChangeDirection();
            return;
        }

        if (!floorAttachingMovement.canBumpRight)
        {
            directionFloatX = -1;
            ChangeDirection();
        }
            
        if (!floorAttachingMovement.canBumpLeft)
        {
            directionFloatX = 1;
            ChangeDirection();
        }
            
    }

    public void forceJump(float jumpSpeed)
    {
        transform.Translate(new Vector3(0, 0.4f, 0));
        floorAttachingMovement.isGrounded = false;
        floorAttachingMovement.isRayShortened = true;
        verticalCalulation = jumpSpeed;
    }

    public override void OnPlayerTouch(PlayerScript player) {
        if (state == EnemyState.Hit)
            return;

        
        if(player.invicibilityTimer <= 0)
        {
            player.TakeHit();
            if (transform.position.x > player.transform.position.x)
                directionFloatX = -1;
            else
                directionFloatX = 1;
            ChangeDirection();
            state = EnemyState.Attack;
            AttackRoutine = AttackR();
            StartCoroutine(AttackRoutine);

                
        }
    }

    public override void OnAttacked(PlayerScript player)
    {
        Debug.Log("waaa");
        if(state != EnemyState.Hit)
        {
            floorAttachingMovement.enabled = false;
            forceJump(dieJump);
            state = EnemyState.Hit;
            DieRoutine = WaitToImplode();
            StartCoroutine(DieRoutine);
        }
    }

    public override void OnAttackedByProjectile(Throwable throwable)
    {
        Debug.Log("waaa");
        if (state != EnemyState.Hit)
        {
            floorAttachingMovement.enabled = false;
            forceJump(dieJump);
            state = EnemyState.Hit;
            DieRoutine = WaitToImplode();
            StartCoroutine(DieRoutine);
        }
    }

    IEnumerator WaitToImplode()
    {
        yield return new WaitForSeconds(dieTime);
        Destroy(gameObject);
    }

    IEnumerator AttackR()
    {
        yield return new WaitForSeconds(attackTime);
        if(state != EnemyState.Hit)
            state = EnemyState.Free;
    }

    public void Attack()
    {
        if(floorAttachingMovement.isGrounded)
        {
            forceJump(attackJump);
        }
    }

    public void ChangeDirection()
    {
        spriteTransform.localScale = new Vector2(initialScale.x * directionFloatX, initialScale.y);
    }

}
