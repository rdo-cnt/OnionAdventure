using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hardan : Throwable {

    //Direction
    public Transform spriteTransform;
    private float directionFloatX = 1;
    private Vector2 initialScale;

    //Components
    protected FloorAttachMovement floorAttachingMovement;
    protected AnimationManager animationManager;
    public Rigidbody2D rigidbody;
    public ThrowableAttackBox attackBox;
    public Destructor destructor;
    private PlayerScript carrier;

    //Movement related
    public float throwMomentum;
    public float throwForce = 10f;
    public float gravityMultiplier = 1;
    protected float gravity;
    public float verticalCalulation = 0;

    //RecoveryTimer
    protected IEnumerator RecoveryRoutine;
    public float recoveryTime = 2.5f;
    public bool isRecovering = false;
    public float recoveryHop = 1f;


    //The character states
    public enum ThrowableState
    {
        Free,
        Carried,
        Thrown,
        Recovering
    }
    public ThrowableState state = ThrowableState.Free;

    
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
        disableAttackBoxes();
    }

    void setInitialGravity()
    {
        gravity = -gravityMultiplier;
    }

    // Update is called once per frame
    void Update () {
        //Do actions based on states
        switch (state)
        {
            case ThrowableState.Free:
                
                break;

            case ThrowableState.Carried:
                beingCarried();
                CheckTop();
                break;

            case ThrowableState.Thrown:
                ThrownMovement();
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

        if (floorAttachingMovement.isGrounded && verticalCalulation != 0 && (state != ThrowableState.Thrown))
        {
            floorAttachingMovement.stickToFloor();
            verticalCalulation = 0;
        }
    }

    public void CheckTop()
    {
        if (floorAttachingMovement.topAngle.detect)
        {
            forceRelease();
        }

    }

    public void SetAnimatorVariables()
    {
        animationManager.getAnimator().SetFloat("Speed", directionFloatX);
        animationManager.getAnimator().SetBool("InAir", !floorAttachingMovement.isGrounded);
        animationManager.getAnimator().SetBool("Carried", (state == ThrowableState.Carried));
        animationManager.getAnimator().SetBool("Thrown", (state == ThrowableState.Thrown));
        animationManager.getAnimator().SetBool("Recovering", (state == ThrowableState.Recovering));

    }

    public void ChangeDirection()
    {
        spriteTransform.localScale = new Vector2(initialScale.x * directionFloatX, initialScale.y);
    }

    void ThrownMovement()
    {

        floorAttachingMovement.MoveSideWays(throwMomentum);

        if(throwMomentum == 0 && floorAttachingMovement.isGrounded)
        {
            state = ThrowableState.Recovering;
            StartRecovery();

            return;
        }

        if (throwMomentum < 0.5f && throwMomentum > -0.5f)
            throwMomentum = 0;

        if (floorAttachingMovement.isGrounded)
            bounceDown();

        if (floorAttachingMovement.topAngle.detect)
            bounceUp();

        if (floorAttachingMovement.isBumping)
            bounceSide();

        directionFloatX = Mathf.Sign(throwMomentum);
        ChangeDirection();

    }

    void bounceSide()
    {
        throwMomentum *= -0.8f;
    }

    void bounceDown()
    {
        if (throwMomentum < 0.1f)
        {
            throwMomentum = 0;
            floorAttachingMovement.stickToFloor();
            return;
        }
            
        forceJump(Mathf.Abs(throwMomentum));
        throwMomentum *= 0.4f;

    }

    void bounceUp()
    {
        if (throwMomentum < 0.1f)
        {
            throwMomentum = 0;
            floorAttachingMovement.stickToFloor();
            return;
        }

        verticalCalulation = 0;
        
        throwMomentum *= 0.4f;
    }

    void StartRecovery()
    {
        disableAttackBoxes();
        state = ThrowableState.Recovering;
        RecoveryRoutine = Recovering();
        StartCoroutine(RecoveryRoutine);
    }

    IEnumerator Recovering()
    {
        yield return new WaitForSeconds(recoveryTime);
        state = ThrowableState.Free;
    }

    public override void OnPlayerTouch(PlayerScript player)
    {
        if(state == ThrowableState.Free || state == ThrowableState.Recovering)
        {
            player.HoldObject(this);
            state = ThrowableState.Carried;
            carrier = player;
            verticalCalulation = 0;
        }
    }

    public void beingCarried()
    {
        
        verticalCalulation = 0;
        transform.localPosition = Vector3.zero;
        if (RecoveryRoutine != null)
            StopCoroutine(RecoveryRoutine);
    }


    public override void OnAttacked(PlayerScript player)
    {
           forceJump(throwForce);
            getThrown(player.directionFloatX);
    }

    public override void OnThrown(PlayerScript player)
    {
        if(player)
            getThrown(player.directionFloatX);
    }

    public override void OnThrownUpwards(PlayerScript player)
    {
        if (player)
            forceJump(throwForce*2);
            getThrown(player.directionFloatX/3);
    }

    public override void OnReleased(PlayerScript player)
    {
        getThrown(player.directionFloatX/3);
    }

    public void forceRelease()
    {
        carrier.releaseObject();
    }

    public void getThrown(float DirectionX)
    {
        transform.parent = null;
        carrier = null;
         if (RecoveryRoutine != null)
            StopCoroutine(RecoveryRoutine);
        state = ThrowableState.Thrown;
        enableAttackBoxes();
        throwMomentum = DirectionX * throwForce;
    }

    public void enableAttackBoxes()
    {
        attackBox.enabled = true;
        destructor.enabled = true;
    }

    public void disableAttackBoxes()
    {
        attackBox.enabled = false;
        destructor.enabled = false;
    }

    public void forceJump(float jumpSpeed)
    {
        transform.Translate(new Vector3(0, 0.4f, 0));
        floorAttachingMovement.isGrounded = false;
        floorAttachingMovement.isRayShortened = true;
        verticalCalulation = jumpSpeed;
    }


}
