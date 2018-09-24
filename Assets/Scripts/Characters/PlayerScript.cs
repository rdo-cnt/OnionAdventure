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
        StickyJump
    }
    protected PlayerState state = PlayerState.Free;

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
    public float timeToJumpApex = .4f;
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

    //Dashing
    protected IEnumerator DashRoutine;
    public float dashTime = 1.2f;
    public float stickingTime = 1f;
    protected float stickingTimer;
   


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
    }

    void initializeVariables()
    {
        blockDestructor.enabled = false;
        floorAttachingMovement.maxClimbingAngle = maxSlopeAngle;
    }

    void setInitialGravity()
    {
        gravity = -(0.1f * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
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
                Walk();
                Jump();
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
        }
    }

    public void Walk()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        floorAttachingMovement.MoveSideWays(input.x * fStandSpeed);
    }

    public void CheckForGravity()
    {
        if (floorAttachingMovement.isGrounded && floorAttachingMovement.isSticked)
            rigidbody.gravityScale = 0f;
        else
            rigidbody.gravityScale = -gravity;
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
                floorAttachingMovement.isSticked = false;
        }
    }

    public void Jump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (floorAttachingMovement.isSticked)
            {
                directionFloatX *= -1;
                floorAttachingMovement.isSticked = false;
                rigidbody.velocity = new Vector2(Mathf.Sign(floorAttachingMovement.middleAngle.angle)*fStickJump, rigidbody.velocity.y);
                state = PlayerState.StickyJump;

            }

            if (floorAttachingMovement.isGrounded)
            {
                rigidbody.velocity = new Vector2(rigidbody.velocity.x, maxJumpVelocity);
            }


        }

        if (Input.GetButtonUp("Jump"))
        {

            if (!floorAttachingMovement.isGrounded && rigidbody.velocity.y > minJumpVelocity)
            {
                rigidbody.velocity = new Vector2(rigidbody.velocity.x,minJumpVelocity);
            }

        }
    }

    public void CheckForSlope()
    {
        if (input.y < 0 && floorAttachingMovement.isGrounded && floorAttachingMovement.middleAngle.angle != 0)
        {
            directionFloatX = Mathf.Sign(floorAttachingMovement.middleAngle.angle);
            input.x = Mathf.Sign(floorAttachingMovement.middleAngle.angle);
            Debug.Log("could do a slope dash, angle is" + floorAttachingMovement.middleAngle.angle);
            Debug.Log("Would make you go to direction:" + Mathf.Sign(floorAttachingMovement.middleAngle.angle));
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
            floorAttachingMovement.isSticked = false;
            floorAttachingMovement.maxClimbingAngle = maxSlopeAngle;
        }

        WallBumpingCheck();
    }

    public void WallBumpingCheck()
    {
        if (floorAttachingMovement.isBumping)
        {
            floorAttachingMovement.maxClimbingAngle = maxSlopeAngle;
            state = PlayerState.Free;
            floorAttachingMovement.isSticked = false;
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