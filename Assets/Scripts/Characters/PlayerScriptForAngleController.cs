using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScriptForAngleController : MonoBehaviour {

    //The player states
    public enum PlayerState
    {
        Free,
        Dash,
        SuperDash,
        HorizontalJump
    }
    protected PlayerState state = PlayerState.Free;

    //Components
    protected AnimationManager m_anim;
    protected Destructor m_destr;

    //Angles
    public float maxSlopeAngle = 55f;
    public float maxSlopeAngleSuperDash = 100f;

    //Move Speeds
    public float fStandSpeed = 4f;
    public float fCrouchSpeed = 2f;
    public float fDashSpeed = 6f;
    public float fSuperDashSpeed = 8f;
    public float fHorizontalJump = 20f;

    //Ability variables

    //Dashing
    protected IEnumerator DashRoutine;
    public float dashTime = 1.2f;

    //Platforming related
    public float maxJumpHeight = 4;
    public float minJumpHeight = 1;
    public float timeToJumpApex = .4f;
    float accelerationTimeAirborne = 0;
    float accelerationTimeGrounded = 0;
    float moveSpeed = 12;

    Vector2 input;
    private float directionFloat = 1;

    public Vector2 wallJumpClimb;
    public Vector2 wallJumpOff;
    public Vector2 wallLeap;
    bool wallSliding = false;
    int wallDirX;

    public float wallSlideSpeedMax = 3;
    public float wallStickTime = .25f;
    float timeToWallUnstick;

    float gravity;
    float maxJumpVelocity;
    float minJumpVelocity;
    Vector3 velocity;
    float velocityXSmoothing;
    bool defyingGravity = false;

    Controller2D controller;

    void Start()
    {
        controller = GetComponent<Controller2D>();

        m_anim = GetComponent<AnimationManager>();
        m_destr = GetComponent<Destructor>();
        m_destr.enabled = false;

        gravity = 1;
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
        print("Gravity: " + gravity + "  Jump Velocity: " + maxJumpVelocity);

        controller.maxSlopeAngle = maxSlopeAngle;
    }

    void Update()
    {
        //Actions done regardless of states
        CalculateGravity();
        defyingGravity = false;
        controller.Move(velocity * Time.deltaTime, input);
        AnimationUpdate();

        if (controller.collisions.above || controller.collisions.below)
        {
            velocity.y = 0;
        }

        //Do actions based on states
        switch (state)
        {
            case PlayerState.Free:
                Move();
                Jump();
                AttackStart();
                checkDirection();
                CrouchSlope();
                break;

            case PlayerState.Dash:
                Jump();
                Attack();
                CrouchSlope();
                break;

            case PlayerState.SuperDash:
                SuperAttack();
                Jump();
                break;

            case PlayerState.HorizontalJump:
                HorizontalJump();
                break;
        }

    }

    public void Jump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (wallSliding)
            {
                if (wallDirX == input.x)
                {
                    velocity.x = -wallDirX * wallJumpClimb.x;
                    velocity.y = wallJumpClimb.y;
                }
                else if (input.x == 0)
                {
                    velocity.x = -wallDirX * wallJumpOff.x;
                    velocity.y = wallJumpOff.y;
                }
                else
                {
                    velocity.x = -wallDirX * wallLeap.x;
                    velocity.y = wallLeap.y;
                }
            }
            if (defyingGravity)
            {
                directionFloat *= -1;
                velocity.x = Mathf.Sign(controller.collisions.slopeNormal.x) * fHorizontalJump;
                state = PlayerState.HorizontalJump;

            }

            if (controller.collisions.below)
            {
                velocity.y = maxJumpVelocity;
            }


        }

        if (Input.GetButtonUp("Jump"))
        {

            if (velocity.y > minJumpVelocity)
            {
                velocity.y = minJumpVelocity;
            }

        }
    }

    public void Move()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        float targetVelocityX = input.x * fStandSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
    }

    void HandleWallSliding()
    {
        wallDirX = (controller.collisions.left) ? -1 : 1;
        wallSliding = false;
        if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0)
        {
            wallSliding = true;

            if (velocity.y < -wallSlideSpeedMax)
            {
                velocity.y = -wallSlideSpeedMax;
            }

            if (timeToWallUnstick > 0)
            {
                velocityXSmoothing = 0;
                velocity.x = 0;

                if (input.x != wallDirX && input.x != 0)
                {
                    timeToWallUnstick -= Time.deltaTime;
                }
                else
                {
                    timeToWallUnstick = wallStickTime;
                }
            }
            else
            {
                timeToWallUnstick = wallStickTime;
            }

        }

    }

    void CalculateGravity()
    {
        if (!defyingGravity)
        {
            velocity.y += gravity * Time.deltaTime;
        }

    }

    IEnumerator DashAttack()
    {
        yield return new WaitForSeconds(dashTime);
        state = PlayerState.Free;
        controller.maxSlopeAngle = maxSlopeAngle;
    }

    public void AttackStart()
    {
        m_destr.enabled = false;
        if (Input.GetButtonDown("Fire1"))
        {
            DashRoutine = DashAttack();
            state = PlayerState.Dash;
            StartCoroutine(DashRoutine);
        }
    }

    public void HorizontalJump()
    {
        //Check for bump

        if (controller.collisions.bumped)
        {
            state = PlayerState.Free;
        }

        if (controller.collisions.below || controller.collisions.bumped)
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

    public void Attack()
    {
        velocity.x = fDashSpeed * directionFloat;
        m_destr.enabled = true;

        //Check for angle
        if (controller.collisions.below)
        {
            controller.maxSlopeAngle = maxSlopeAngleSuperDash;
        }
        else
        {
            controller.maxSlopeAngle = maxSlopeAngle;
        }
    }

    public void SuperAttack()
    {
        velocity.x = fSuperDashSpeed * directionFloat;
        m_destr.enabled = true;
        Debug.Log(controller.collisions.moveAmountOld.x);
        float Angle = controller.collisions.slopeAngle;
        Debug.Log(Angle);
        if (Angle >= 89)
        {

            defyingGravity = true;
        }

        //Check for angle
        if (controller.collisions.below)
        {
            controller.maxSlopeAngle = maxSlopeAngleSuperDash;
        }
        else
        {
            controller.maxSlopeAngle = maxSlopeAngle;
        }


        //if ((controller.collisions.left || controller.collisions.right) && velocity.y < 0)
        //{
        //    controller.maxSlopeAngle = maxSlopeAngle;

        //    state = PlayerState.Free;
        //}

        if ((controller.collisions.left || controller.collisions.right) && controller.collisions.bumped)
        {
            controller.maxSlopeAngle = maxSlopeAngle;

            state = PlayerState.Free;
        }

    }

    public void CrouchSlope()
    {
        if (input.y < 0 && controller.collisions.below)
        {
            //controller.Move(velocity * Time.deltaTime, input);

            if (directionFloat < 0)
            {
                float temp = velocity.y;
                velocity.y = -1;
                velocity.x = -1;
                controller.Move(velocity * Time.deltaTime, input);
                velocity.y = temp;
                velocity.x = 0;
            }

            if (controller.collisions.slopeAngle != 0)
            {
                Debug.Log("could do a slope dash, angle is" + controller.collisions.slopeAngle);
                Debug.Log("normal vector is" + controller.collisions.slopeNormal);
                Debug.Log("Would make you go to direction:" + Mathf.Sign(controller.collisions.slopeNormal.x));
                directionFloat = Mathf.Sign(controller.collisions.slopeNormal.x);
                state = PlayerState.SuperDash;

            }
        }
    }

    public void AnimationUpdate()
    {
        //set animator variables
        m_anim.getAnimator().SetFloat("Speed", input.x);
        m_anim.getAnimator().SetBool("InAir", !controller.collisions.below);
        m_anim.getAnimator().SetBool("Dash", (state == PlayerState.Dash));
        m_anim.getAnimator().SetBool("SuperDash", (state == PlayerState.SuperDash));
    }

    public void checkDirection()
    {
        if (input.x > 0)
        {
            directionFloat = 1;
        }
        if (input.x < 0)
        {
            directionFloat = -1;
        }

    }
}
