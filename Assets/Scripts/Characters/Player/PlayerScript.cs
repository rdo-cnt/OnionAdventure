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
        Bump,
        Lifting,
        Throwing,
        Hit
    }
    public PlayerState state = PlayerState.Free;

    //Input
    Vector2 input;
    private float directionFloatY = 0;
    public float directionFloatX = 1;
    private Vector2 initialScale;
    public Transform spriteTransform;

    //Platforming related

    //Move Speeds
    public float fStandSpeed = 8f;
    public float fCrouchSpeed = 4f;
    public float fDashSpeed = 12f;
    public float fSuperDashSpeedMin = 8f;
    public float fSuperDashSpeedInitial = 16f;
    public float fSuperDashSpeed = 16f;
    public float fSuperDashSpeedMax = 26f;
    public float fStickJump = 20f;


    //Platforming related
    public float coyoteTime = 0.2f;
    private float coyoteTimer = 0;
    public float maxJumpHeight = 4;
    public float minJumpHeight = 1;
    public float bumpJump = 10;
    public float maxFallSpeed = 10;
    public float gravityMultiplier = 1;
    protected float gravity;
    float maxJumpVelocity;
    float minJumpVelocity;
    public float verticalCalulation = 0;

    //Angles
    public float maxSlopeAngle = 55f;
    public float maxSlopeAngleSuperDash = 100f;

    //Collision boxes
    public PlayerHurtBox hurtBox;
    public PlayerAttackBox attackBox;
    public PlayerPickUpBox pickUpBox;
    public Destructor destructor;
    public Transform colliderTransform;
    private float regularCollisionHeight;
    public float crouchCollisionHeight = 0.4f;
    

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
    public float dashTime = 0.6f;
    public float stickingTime = 1f;
    protected float stickingTimer;
    public float dashSlidePower = 0;
    public float BumpPreventionTime = 0.1f;
    protected float BumpPreventionTimer;
    public Transform AttackEffect;

    //Holding
    public Transform holdPos;
    public bool isHolding = false;
    public Throwable heldObject;


    //Hit and invincibility
    public float invicibilityTime = 1.5f;
    public float invicibilityTimer;
    public float fHitSpeed = 2f;
    public float hurtJump = 5;
    public float bounces = 3;
    protected float bounceCounter = 0;

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
        initialScale = spriteTransform.localScale;
        regularCollisionHeight = colliderTransform.localScale.y;
        AttackEffect.gameObject.SetActive(false);
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
                WallBumpingCheck();
                break;

            case PlayerState.SuperDash:
                SuperAttack();
                WallBumpingCheck();
                CheckForSpeedGain();
                Jump();
                
                break;

            case PlayerState.StickyJump:
                StickJump();
                break;

            case PlayerState.Bump:
                Bump();
                break;

            case PlayerState.Lifting:
                
                break;

            case PlayerState.Throwing:

                break;

            case PlayerState.Hit:
                Hit();
                break;
        }

        //Actions regardless of state

        CheckForCoyote();
        CheckForInvincibility();
        CheckForGravity();
        CheckForStickiness();
        CheckForCrouch();
        CheckForCrouchSlide();
        CheckForBumpPrevention();
        CheckForHold();
        SetAnimatorVariables();
    }

    public void CheckForHold()
    {
        pickUpBox.enabled = false;
        if (state == PlayerState.Free)
        {
            pickUpBox.enabled = true;
            if (directionFloatY < 0 || !floorAttachingMovement.isGrounded)
                pickUpBox.enabled = false;
            if (isHolding)
            {
                if (directionFloatY < 0)
                {
                    releaseObject();
                }
            }
        } 
    }

    public void CheckForCoyote()
    {
        if(coyoteTimer > 0)
        {
            coyoteTimer -= Time.deltaTime;
        }
    }

    public void StartThrow()
    {
        if (isHolding)
        {
            state = PlayerState.Throwing;
        }
    }

    public void EndThrow()
    {
        if (isHolding)
        {
            if(directionFloatY>0)
            {
                heldObject.OnThrownUpwards(this);
            }
            else
            {
                if (directionFloatY < 0)
                {
                    heldObject.OnReleased(this);
                }
                else
                {
                    heldObject.OnThrown(this);
                }
                
            }
            
            heldObject = null;
            isHolding = false;
            state = PlayerState.Free;
        }
    }

    public void releaseObject()
    {
        if(isHolding)
        {
            heldObject.OnReleased(this);
            heldObject = null;
            isHolding = false;
        }
    }

    public void HoldObject(Throwable throwable)
    {
        if(!isHolding)
        {
            heldObject = throwable;
            throwable.transform.parent = holdPos;
            isHolding = true;
            state = PlayerState.Lifting;
        }
       
    }

    public void endLifting()
    {
        state = PlayerState.Free;
    }

    public void Walk()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        floorAttachingMovement.MoveSideWays(input.x * fStandSpeed);
        transform.eulerAngles = new Vector3(0, 0, 0);
    }

    public void startLifting()
    {
        state = PlayerState.Lifting;
        
    }

    public void stopLifting()
    {
        state = PlayerState.Free;
    }

    public void CheckForGravity()
    {

        if (verticalCalulation > 0 && floorAttachingMovement.topAngle.detect)
            verticalCalulation = 0;

        if ((!floorAttachingMovement.isSticked || Mathf.Abs(transform.eulerAngles.z) < floorAttachingMovement.maxStandingAngle) && !floorAttachingMovement.isGrounded)
        {
            verticalCalulation += -gravityMultiplier;

            if (verticalCalulation < -maxFallSpeed)
                verticalCalulation = -maxFallSpeed;
            transform.Translate(new Vector3(0, verticalCalulation * Time.deltaTime, 0));
            return;

        }

        if (floorAttachingMovement.isGrounded && verticalCalulation != 0)
        { 
            floorAttachingMovement.stickToFloor();
            verticalCalulation = 0;
            coyoteTimer = coyoteTime;
        }

        //transform.Translate(new Vector3(0, verticalCalulation * Time.deltaTime, 0));


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
        //Extra air while skating in uneven terrain
        if (floorAttachingMovement.middleAngle.detect || floorAttachingMovement.isRayShortened)
            stickingTimer = stickingTime;
        else
        {
            if (stickingTimer > 0)
                stickingTimer -= Time.deltaTime;
            else
            {
                floorAttachingMovement.isSticked = false;
                state = PlayerState.Free;
                disableAttackBoxes();
            }
                
        }
    }

    public void CheckForBumpPrevention()
    {
            if (BumpPreventionTimer > 0)
                BumpPreventionTimer -= Time.deltaTime;

    }

    public void CheckForCrouchSlide()
    {
        if (dashSlidePower <= 0)
            return;

        bool forceCancel = false;

        //Cancel crouch slide if conditions are met
        if (!floorAttachingMovement.isGrounded)
            forceCancel = true;
        if (state != PlayerState.Free)
            forceCancel = true;
        if (directionFloatY >= 0)
            forceCancel = true;

        if (forceCancel)
        {
            dashSlidePower = 0;
            return;
        }
        dashSlidePower = dashSlidePower - (dashSlidePower / 40.0f - Time.deltaTime);
        if (dashSlidePower < 1)
            dashSlidePower = 0f;
        floorAttachingMovement.MoveSideWays(dashSlidePower * directionFloatX);
        

    }

    public void Jump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (floorAttachingMovement.isSticked && Mathf.Abs(floorAttachingMovement.groundedAngle) > floorAttachingMovement.maxStandingAngle)
            {
                floorAttachingMovement.isSticked = false;
                verticalCalulation = maxJumpVelocity * transform.up.y;
                if (transform.up.y == -1)
                {
                    input = new Vector2(transform.up.x * directionFloatX, transform.up.y);
                    Debug.Log("epic");
                }
                   
                else
                    input = transform.up;
                checkDirection();
                state = PlayerState.StickyJump;
                transform.eulerAngles = Vector3.zero;
                BumpPreventionTimer = BumpPreventionTime;
                floorAttachingMovement.CheckShortenedRays();
                return;
            }

            if (floorAttachingMovement.isGrounded || coyoteTimer > 0)
            {
                if(directionFloatY<0)
                    forceJump(maxJumpHeight/1.2f);
                else
                    forceJump(maxJumpHeight);
                transform.eulerAngles = Vector3.zero;
                floorAttachingMovement.CheckShortenedRays();

            }

        }

        if (Input.GetButtonUp("Jump"))
        {

            if (verticalCalulation > minJumpVelocity && !floorAttachingMovement.isSticked)
            {
                verticalCalulation = minJumpVelocity;
            }

        }
    }

    public void CheckForSlope()
    {
        if (input.y < 0 && floorAttachingMovement.isGrounded && floorAttachingMovement.groundedAngle != 0)
        {
            directionFloatX = Mathf.Sign(floorAttachingMovement.groundedAngle);
            input.x = Mathf.Sign(floorAttachingMovement.groundedAngle);
            checkDirection();
            state = PlayerState.SuperDash;
            fSuperDashSpeed = fSuperDashSpeedInitial;
            releaseObject();
        }
    }

    public void CheckForSpeedGain()
    {
        if(floorAttachingMovement.isGrounded)
        {
            if (floorAttachingMovement.isAscending)
            {
                fSuperDashSpeed -= Time.deltaTime * 8 * (Mathf.Abs(floorAttachingMovement.groundedAngle)/90);
                if (fSuperDashSpeed < fSuperDashSpeedMin)
                    fSuperDashSpeed = fSuperDashSpeedMin;
            }

            if (floorAttachingMovement.isDescending)
            {
                fSuperDashSpeed += Time.deltaTime * 12 * (Mathf.Abs(floorAttachingMovement.groundedAngle) / 90);
                if (fSuperDashSpeed > fSuperDashSpeedMax)
                    fSuperDashSpeed = fSuperDashSpeedMax;
            }
        }
        
    }

    IEnumerator DashAttack()
    {
        yield return new WaitForSeconds(dashTime);
        EndAttack();
    }

    public void EndAttack()
    {
        state = PlayerState.Free;
        floorAttachingMovement.maxClimbingAngle = maxSlopeAngle;
        disableAttackBoxes();
    }

    public void AttackStart()
    {
        if (Input.GetButtonDown("Fire1") && directionFloatY >= 0)
        {
            blockDestructor.enabled = false;
            if (isHolding)
            {
                StartThrow();
                return;
            }
            DashRoutine = DashAttack();
            state = PlayerState.Dash;
            StartCoroutine(DashRoutine);
        }
    }

    public void Attack()
    {
        floorAttachingMovement.MoveSideWays(fDashSpeed * directionFloatX);
        enableAttackBoxes();

        //Change angle depended on groundedness
        if (floorAttachingMovement.isGrounded)
        {
            floorAttachingMovement.maxClimbingAngle = maxSlopeAngleSuperDash;
        }
        else
        {
            floorAttachingMovement.maxClimbingAngle = maxSlopeAngle;
        }

        //Cancel on crouch
        if(directionFloatY < 0)
        {
            dashSlidePower = fDashSpeed*1.5f;
            if (DashRoutine != null)
                StopCoroutine(DashRoutine);
            EndAttack();
        }
        

    }

    public void enableAttackBoxes()
    {
        attackBox.enabled = true;
        destructor.enabled = true;
        AttackEffect.gameObject.SetActive(true);
    }

    public void disableAttackBoxes()
    {
        attackBox.enabled = false;
        destructor.enabled = false;
        AttackEffect.gameObject.SetActive(false);
    }

    public void SuperAttack()
    {
        floorAttachingMovement.MoveSideWays(fSuperDashSpeed * directionFloatX);
        enableAttackBoxes();

        //Change angle depended on groundedness
        if (floorAttachingMovement.isGrounded)
        {
            floorAttachingMovement.isSticked = true;
            floorAttachingMovement.maxClimbingAngle = maxSlopeAngleSuperDash;
        }
        else
        {
            floorAttachingMovement.maxClimbingAngle = maxSlopeAngle;
            //if (Mathf.Abs(transform.eulerAngles.z) < floorAttachingMovement.maxStandingAngle)
            //{
            //    Debug.Log("straigthern up");
            //    transform.eulerAngles = Vector3.zero;
            //}
               
        }

    }

    public void WallBumpingCheck()
    {
        if (floorAttachingMovement.isBumping)
        {
            Debug.Log("I bumped ok");
            if(DashRoutine != null)
            StopCoroutine(DashRoutine);
            floorAttachingMovement.maxClimbingAngle = maxSlopeAngle;
            state = PlayerState.Bump;
            forceJump(bumpJump);
            transform.eulerAngles = new Vector3(0, 0, 0);
            disableAttackBoxes();
        }
    }

    public void StickJump()
    {
        //Move
        transform.Translate(new Vector2(Mathf.Sign(input.x),input.y )* fStickJump * Time.deltaTime,Space.World);
        
  
        if (floorAttachingMovement.isBumping)
        {
            state = PlayerState.Bump;
            disableAttackBoxes();
        }

        if (floorAttachingMovement.isGrounded && BumpPreventionTimer <= 0)
        {
            if (Input.GetAxisRaw("Vertical") < 0)
            {
                state = PlayerState.SuperDash;
                fSuperDashSpeed = fStickJump;
            }
            else
            {
                state = PlayerState.Free;
                forceJump(bumpJump);
                disableAttackBoxes();
            }
        }
    }

    public void Bump()
    {
        if (floorAttachingMovement.isGrounded)
        {
            floorAttachingMovement.stickToFloor();
            state = PlayerState.Free;
            disableAttackBoxes();
        }

    }

    public void TakeHit()
    {
        if(invicibilityTimer <= 0)
        {
            Debug.Log("wwerr");
            state = PlayerState.Hit;
            forceJump(hurtJump);
            invicibilityTimer = invicibilityTime;
            bounceCounter = bounces;
        }
        
    }

    public void Hit()
    {
        floorAttachingMovement.MoveSideWays(fHitSpeed * -directionFloatX);
        invicibilityTimer = invicibilityTime;
        if (floorAttachingMovement.isGrounded )
        {
            if(bounceCounter <= 0)
            {
                state = PlayerState.Free;
                return;
            }
            bounceCounter -= 1;
            forceJump(hurtJump*(bounceCounter/bounces));

        }

    }

    public void CheckForInvincibility()
    {
            if (invicibilityTimer > 0)
                invicibilityTimer -= Time.deltaTime;

    }

    public void forceJump(float jumpSpeed)
    {
        transform.Translate(new Vector3(0, 0.4f, 0));
        floorAttachingMovement.isGrounded = false;
        floorAttachingMovement.isSticked = false;
        floorAttachingMovement.isRayShortened = true;
        verticalCalulation = jumpSpeed;
    }

    public void pickUp()
    {
        switch(state)
        {
            case PlayerState.Free:
                isHolding = true;
                break;
            default:
                break;
        }
    }

    public void SetAnimatorVariables()
    {
        animationManager.getAnimator().SetFloat("Speed", input.x);
        animationManager.getAnimator().SetFloat("VerticalInput", directionFloatY);
        animationManager.getAnimator().SetBool("InAir", !floorAttachingMovement.isGrounded);
        animationManager.getAnimator().SetBool("Dash", (state == PlayerState.Dash));
        animationManager.getAnimator().SetBool("SuperDash", (state == PlayerState.SuperDash));
        animationManager.getAnimator().SetBool("Bump", (state == PlayerState.Bump));
        animationManager.getAnimator().SetBool("Hit", (state == PlayerState.Hit));
        animationManager.getAnimator().SetBool("StickyJump", (state == PlayerState.StickyJump));
        animationManager.getAnimator().SetBool("Lifting", (state == PlayerState.Lifting));
        animationManager.getAnimator().SetBool("Throwing", (state == PlayerState.Throwing));
        animationManager.getAnimator().SetFloat("VerticalSpeed", verticalCalulation);
    }


    public void checkDirection()
    {
        if (input.x > 0)
            directionFloatX = 1;
        if (input.x < 0)
            directionFloatX = -1;

        spriteTransform.localScale = new Vector2(initialScale.x * directionFloatX, initialScale.y);
    }

    public void CheckForCrouch()
    {
        float oldDirectionFloatY = directionFloatY;
        directionFloatY = Input.GetAxisRaw("Vertical");
        if (oldDirectionFloatY < 0 && floorAttachingMovement.topAngle.detect)
            directionFloatY = -1;

        bool forceCrouch = false;
        if (directionFloatY < 0)
            forceCrouch = true;
        if (state == PlayerState.SuperDash)
            forceCrouch = true;
        if (state == PlayerState.StickyJump)
            forceCrouch = true;

        if (forceCrouch)
        {
            colliderTransform.localScale = new Vector3(colliderTransform.localScale.x, crouchCollisionHeight , colliderTransform.localScale.z);
            floorAttachingMovement.canBumpTop = false;
        }
        else
        {
            colliderTransform.localScale = new Vector3(colliderTransform.localScale.x, regularCollisionHeight, colliderTransform.localScale.z);
            floorAttachingMovement.canBumpTop = true;
        }
    }



}