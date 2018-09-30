using UnityEngine;
using System.Collections;


public class PlayerScript : MonoBehaviour
{
    //The player states
    public enum PlayerState
    {
        Free,
        Dash,
        SuperDash,
        StickyJump,
        Bump
    }
    public PlayerState state = PlayerState.Free;

    //Input
    Vector2 input;
    private float directionFloatX = 1;

    //Platforming related

    //Move Speeds
    public float fStandSpeed = 4f;
    public float fCrouchSpeed = 2f;
    public float fDashSpeed = 6f;
    public float fSuperDashSpeed = 8f;
    public float fStickJump = 20f;

    //Platforming related
    public float maxJumpHeight = 4;
    public float minJumpHeight = 1;
    public float gravityMultiplier = 1;
    protected float gravity;
    float maxJumpVelocity;
    float minJumpVelocity;

    //Angles
    public float maxSlopeAngle = 55f;
    public float maxSlopeAngleSuperDash = 100f;

    //Components
    protected FloorAttachMovement floorAttachingMovement;
    protected AnimationManager animationManager;
    protected Destructor blockDestructor;
    public Rigidbody2D rigidbody;
    protected CapsuleCollider2D collider;
    public PhysicsMaterial2D airMaterial;
    public PhysicsMaterial2D groundMaterial;

    //Dashing
    protected IEnumerator DashRoutine;
    public float dashTime = 1.2f;
    public float stickingTime = 1f;
    public float stickingTimer;
   


    // Use this for initialization
    void Start()
    {
        getComponentReferences();
        initializeVariables();
        setInitialGravity();
    }

    void getComponentReferences()
    {
        floorAttachingMovement = GetComponent<FloorAttachMovement>();
        animationManager = GetComponent<AnimationManager>();
        blockDestructor = GetComponent<Destructor>();
        rigidbody = GetComponent<Rigidbody2D>();
        collider = GetComponent<CapsuleCollider2D>();
    }

    void initializeVariables()
    {
        blockDestructor.enabled = false;
        floorAttachingMovement.maxClimbingAngle = maxSlopeAngle;
    }

    void setInitialGravity()
    {
        gravity = -gravityMultiplier;
        maxJumpVelocity = maxJumpHeight;
        minJumpVelocity = minJumpHeight;
    }

    // Update is called once per frame
    void Update()
    {
        //Actions regardless of state
        CheckForGravity();
        CheckForStickiness();

        //Do actions based on states
        switch (state)
        {
            case PlayerState.Free:
                Jump();
                Walk();
                AttackStart();
                checkDirection();
                CheckForSlope();

                break;

            case PlayerState.Dash:
                Jump();
                Attack();

                break;

            case PlayerState.SuperDash:
                SuperAttack();
                Jump();
                break;

            case PlayerState.StickyJump:
                StickJump();
                break;

            case PlayerState.Bump:
                Bump();
                break;
        }
    }

    public void Walk()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        floorAttachingMovement.MoveSideWays(input.x * fStandSpeed);
        transform.eulerAngles = new Vector3(0, 0, 0);
    }

    public void CheckForGravity()
    {
        if (floorAttachingMovement.isSticked)
            rigidbody.gravityScale = 0f;
        else
            rigidbody.gravityScale = -gravity;
    }

    public void CheckForFriction()
    {
        if (floorAttachingMovement.isGrounded && !floorAttachingMovement.isSticked)
            collider.sharedMaterial = groundMaterial;
        else
            collider.sharedMaterial = airMaterial;
    }

    public void CheckForStickiness()
    {
        //Extra air while skating
        if (floorAttachingMovement.middleAngle.detect)
            stickingTimer = stickingTime;
        else
        {
            if (stickingTimer > 0)
                stickingTimer -= Time.deltaTime;
            else
            {
                floorAttachingMovement.isSticked = false;
                state = PlayerState.Free;
            }
                
        }
    }

    public void Jump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (floorAttachingMovement.isSticked && Mathf.Abs(floorAttachingMovement.groundedAngle) > floorAttachingMovement.maxStandingAngle)
            {
                directionFloatX = Mathf.Sign(floorAttachingMovement.groundedAngle);
                Debug.Log(floorAttachingMovement.groundedAngle);
                floorAttachingMovement.isSticked = false;
                rigidbody.velocity = new Vector2(directionFloatX*fStickJump, fStickJump);
                state = PlayerState.StickyJump;

            }

            if (floorAttachingMovement.isGrounded)
            {
                rigidbody.velocity = new Vector2(rigidbody.velocity.x, maxJumpVelocity);
                floorAttachingMovement.isGrounded = false;
                floorAttachingMovement.isRayShortened = true;
                floorAttachingMovement.CheckShortenedRays();

            }

        }

        if (Input.GetButtonUp("Jump"))
        {

            if (rigidbody.velocity.y > minJumpVelocity && !floorAttachingMovement.isSticked)
            {
                rigidbody.velocity = new Vector2(rigidbody.velocity.x,minJumpVelocity);
            }

        }
    }

    public void CheckForSlope()
    {
        if (input.y < 0 && floorAttachingMovement.isGrounded && floorAttachingMovement.groundedAngle != 0)
        {
            directionFloatX = Mathf.Sign(floorAttachingMovement.groundedAngle);
            input.x = Mathf.Sign(floorAttachingMovement.groundedAngle);
            state = PlayerState.SuperDash;
        }
    }

    IEnumerator DashAttack()
    {
        yield return new WaitForSeconds(dashTime);
        state = PlayerState.Free;
        floorAttachingMovement.maxClimbingAngle = maxSlopeAngle;
    }

    public void AttackStart()
    {
        blockDestructor.enabled = false;
        if (Input.GetButtonDown("Fire1"))
        {
            DashRoutine = DashAttack();
            state = PlayerState.Dash;
            StartCoroutine(DashRoutine);
        }
    }

    public void Attack()
    {
        floorAttachingMovement.MoveSideWays(fDashSpeed * directionFloatX);
        blockDestructor.enabled = true;

        //Change angle depended on groundedness
        if (floorAttachingMovement.isGrounded)
        {
            floorAttachingMovement.maxClimbingAngle = maxSlopeAngleSuperDash;
        }
        else
        {
            floorAttachingMovement.maxClimbingAngle = maxSlopeAngle;
        }

        WallBumpingCheck();
    }

    public void SuperAttack()
    {
        floorAttachingMovement.MoveSideWays(fSuperDashSpeed * directionFloatX);
        blockDestructor.enabled = true;

        //Change angle depended on groundedness
        if (floorAttachingMovement.isGrounded)
        {
            floorAttachingMovement.isSticked = true;
            floorAttachingMovement.maxClimbingAngle = maxSlopeAngleSuperDash;
        }
        else
        {
            floorAttachingMovement.maxClimbingAngle = maxSlopeAngle;
        }

        WallBumpingCheck();
    }

    public void WallBumpingCheck()
    {
        if (floorAttachingMovement.isBumping)
        {
            floorAttachingMovement.maxClimbingAngle = maxSlopeAngle;
            state = PlayerState.Bump;
            floorAttachingMovement.isSticked = false;
            floorAttachingMovement.isRayShortened = true;
            rigidbody.velocity = new Vector2(transform.right.x * -directionFloatX, transform.right.y) * fDashSpeed;
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
    }

    public void StickJump()
    {
        if (floorAttachingMovement.isBumping)
        {
            state = PlayerState.Free;
        }

        if (floorAttachingMovement.isBumping || floorAttachingMovement.isGrounded)
        {
            if (Input.GetAxisRaw("Vertical") < 0)
            {
                state = PlayerState.SuperDash;
            }
            else
            {
                state = PlayerState.Free;
            }
        }
    }

    public void Bump()
    {
        if (floorAttachingMovement.isGrounded)
        {
            state = PlayerState.Free;
        }

    }

    public void SetAnimatorVariables()
    {
        animationManager.getAnimator().SetFloat("Speed", input.x);
        animationManager.getAnimator().SetBool("InAir", !floorAttachingMovement.isGrounded);
        animationManager.getAnimator().SetBool("Dash", (state == PlayerState.Dash));
        animationManager.getAnimator().SetBool("SuperDash", (state == PlayerState.SuperDash));
    }

    public void checkDirection()
    {
        if (input.x > 0)
            directionFloatX = 1;
        if (input.x < 0)
            directionFloatX = -1;
     }

    
}