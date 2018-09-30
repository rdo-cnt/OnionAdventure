using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorAttachMovementUnrefactored : MonoBehaviour {

    public float skateAirTime = 1f;
    protected float skateAirTimer;

    public bool grounded;
    public bool middleGrounded;
    public float rotationCooldown = 0;
    protected bool stick = false;

    public float leftAngleSuper;
    public float rightAngleSuper;
    public AngleInfo leftAngle;
    public AngleInfo middleAngle;
    public AngleInfo rightAngle;

    public float maxDescendingAngle = 80f;
    public float maxClimbingAngle = 80f;

    public Transform leftAnglePos;
    public Transform middleAnglePos;
    public Transform rightAnglePos;

    protected float fRaycastDistance = .38f;
    protected float fRaycastOffset = 0.3f;
    public LayerMask whatIsGround;

    public float speed = 2f;
    public float dir = 1f;
    public float velocityX = 0;

    protected Rigidbody2D m_rb;
    protected CapsuleCollider2D m_col;

    //Platforming related
    public float maxJumpHeight = 4;
    public float minJumpHeight = 1;
    public float timeToJumpApex = .4f;
    protected float gravity;
    float maxJumpVelocity;
    float minJumpVelocity;

    // Use this for initialization
    void Start()
    {
        //getBoxCollider
        m_col = GetComponent<CapsuleCollider2D>();
        m_rb = GetComponent<Rigidbody2D>();

        gravity = -(0.1f * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
    }

    // Update is called once per frame
    void Update()
    {
        //Check all angles
        leftAngle.reset();
        middleAngle.reset();
        rightAngle.reset();

        middleAngle = RayCastDown(middleAnglePos, true);

        //Check front, downwards and inwards for left and right angles
        leftAngle = RayCastSide(leftAnglePos, -1f);
        if (!leftAngle.detect)
            leftAngle = RayCastDown(leftAnglePos);

        rightAngle = RayCastSide(rightAnglePos, 1f);
        if (!rightAngle.detect)
            rightAngle = RayCastDown(rightAnglePos);


        if (!stick)
        {
            Debug.Log("not sticking");
            transform.eulerAngles = new Vector3(0, 0, 0);
        }

        gravityCheck();
        crouchSlope();

        Debug.Log("left: " + leftAngle.angle + "middle: " + middleAngle.angle + "right: " + rightAngle.angle);

        MoveSideWays(velocityX);

        skatingTiming();

    }

    public void crouchSlope()
    {
        if (middleAngle.detect && !stick)
        {
            
            if (middleAngle.angle != 0)
            {
                Debug.Log("stickyTime");
                velocityX = speed * Mathf.Sign(middleAngle.angle);
                stick = true;
            }

        }
    }

    public void gravityCheck()
    {
        if (stick || grounded)
        {
            m_rb.gravityScale = 0;
        }
        else
        {
            Debug.Log("gravity time");
            m_rb.gravityScale = -gravity;
        }
    }

    public void skatingTiming()
    {
        //Extra air while skating
        if (middleAngle.detect)
            skateAirTimer = skateAirTime;
        else
        {
            if (skateAirTimer > 0)
                skateAirTimer -= Time.deltaTime;
            else
                stick = false;
        }
    }

    public AngleInfo RayCastDown(Transform rayPos, bool ground = false)
    {
        AngleInfo TempAngle;
        TempAngle.angle = 0;
        TempAngle.detect = false;

        Vector3 origin = rayPos.position;
        RaycastHit2D hitInfo = Physics2D.Raycast(origin, -transform.up, fRaycastDistance, whatIsGround);

        //If we hit no collider, this means we are NOT grounded (= not on the ground)
        if (hitInfo.collider == null)
        {
            Debug.DrawRay(origin, -transform.up * fRaycastDistance, Color.green);


            return TempAngle;
        }
        Debug.DrawRay(origin, -transform.up * fRaycastDistance, Color.red);
        TempAngle.angle = Vector2.Angle(hitInfo.normal, Vector2.up) * Mathf.Sign(hitInfo.normal.x);
        TempAngle.detect = true;
        if (ground)
        {
            middleGrounded = true;
            if (!grounded)
            {
                Debug.Log("I'm grounded!");

                m_rb.velocity = m_rb.velocity + ((new Vector2(-transform.up.x, -transform.up.y)) * speed * 1);
            }
        }

        return TempAngle;
    }

    public AngleInfo RayCastSide(Transform rayPos, float dirX, float offsetY = 0)
    {
        AngleInfo TempAngle;
        TempAngle.angle = 0;
        TempAngle.detect = false;

        Vector3 origin = rayPos.position;
        origin.y += transform.up.y * offsetY;

        RaycastHit2D hitInfo = Physics2D.Raycast(origin, dirX * transform.right, fRaycastDistance, whatIsGround);

        //If we hit no collider, this means we are NOT grounded (= not on the ground)
        if (hitInfo.collider == null)
        {
            Debug.DrawRay(origin, dirX * transform.right * fRaycastDistance, Color.green);
            return TempAngle;
        }
        Debug.DrawRay(origin, transform.right * fRaycastDistance, Color.red);
        TempAngle.angle = Vector2.Angle(hitInfo.normal, Vector2.up) * Mathf.Sign(hitInfo.normal.x);
        TempAngle.detect = true;

        return TempAngle;

    }

    public void MoveSideWays(float Dir)
    {
        //Non stick actions
        if (Dir == 0)
            return;

        m_rb.velocity = new Vector2(transform.right.x * Dir, transform.right.y * Dir) * speed;

        if (!stick)
        {
            return;
        }

        //right
        if (Dir > 0)
        {
            float angleDif = Mathf.Abs(Mathf.Abs(rightAngle.angle) - Mathf.Abs(middleAngle.angle));
            float angleSign = Mathf.Sign(rightAngle.angle - middleAngle.angle);
            Debug.Log("middleGrounded is " + middleAngle.detect);
            if (angleDif <= maxClimbingAngle && middleAngle.detect)
            {
                Debug.Log("Valid difference at " + angleDif + "with a sign of " + angleSign);
                //Climbing
                if (angleSign < 0)
                {
                    if (!CompareFloats(rightAngle.angle, middleAngle.angle))
                    {
                        m_rb.MovePosition(transform.position + ((new Vector3(transform.right.x, transform.right.y, 0)) * 0.1f));
                    }
                }
                else
                {
                    if (!CompareFloats(rightAngle.angle, middleAngle.angle))
                    {

                        m_rb.velocity = m_rb.velocity + ((new Vector2(-transform.up.x, -transform.up.y)) * speed * 2);
                    }
                }
                transform.eulerAngles = new Vector3(0, 0, -rightAngle.angle);
            }

        }
        //left
        if (Dir < 0)
        {
            float angleDif = Mathf.Abs(Mathf.Abs(leftAngle.angle) - Mathf.Abs(middleAngle.angle));
            float angleSign = Mathf.Sign(middleAngle.angle - leftAngle.angle);
            if (angleDif <= maxClimbingAngle && grounded)
            {
                Debug.Log("Valid difference at " + angleDif + "with a sign of " + angleSign);
                //Climbing
                if (angleSign < 0)
                {
                    if (!CompareFloats(leftAngle.angle, middleAngle.angle))
                    {
                        m_rb.MovePosition(transform.position + ((new Vector3(-transform.right.x, -transform.right.y, 0)) * 0.1f));
                    }
                }
                else
                {
                    if (!CompareFloats(leftAngle.angle, middleAngle.angle))
                    {
                        m_rb.velocity = m_rb.velocity + ((new Vector2(-transform.up.x, -transform.up.y)) * speed * 2);
                    }
                }
                transform.eulerAngles = new Vector3(0, 0, -leftAngle.angle);
            }

        }

    }

    public bool CompareFloats(float a, float b = 0)
    {
        return (Mathf.Abs(a - b) < 0.3f);
    }

    public float AverageFloats(float a, float b)
    {
        return (a + b) / 2;
    }

    public bool isSided(float offsetX)
    {
        //get ends of collision box
        Vector2 min = m_col.bounds.min;
        Vector2 max = m_col.bounds.max;

        //set offset for better precision on raycast positioning
        min.y += 0.1f;
        max.y -= 0.1f;

        //determines on which side to put the raycast
        if (offsetX > 0)
            min.x = m_col.bounds.max.x;
        else
            max.x = m_col.bounds.min.x;

        //determines the width and starting position of the raycast
        Vector2 originLine = max - min;
        Vector2 center = min + originLine * 0.5f;

        Vector2 centerTemp = center;
        centerTemp.y += (max.y - min.y) / 3;

        //center.y += originLine.y * raycastOffsetY;
        //Create line that shoots downwards from the feet of our unit
        RaycastHit2D hitInfo = Physics2D.Raycast(centerTemp, new Vector2(offsetX, 0), fRaycastDistance, whatIsGround);

        Vector2 centerTemp2 = center;
        centerTemp2.y -= (max.y - min.y) / 4;

        RaycastHit2D hitInfo2 = Physics2D.Raycast(centerTemp2, new Vector2(offsetX, 0), fRaycastDistance, whatIsGround);

        //if there was no collider
        if (hitInfo.collider == null || hitInfo2.collider == null)
        {
            //this will draw a line in our screen, similar to the raycast
            Debug.DrawRay(centerTemp, new Vector2(fRaycastDistance * (Mathf.Abs(offsetX) / offsetX), 0), Color.green);
            Debug.DrawRay(centerTemp2, new Vector2(fRaycastDistance * (Mathf.Abs(offsetX) / offsetX), 0), Color.green);
            //transform.parent = null;
            return false;
        }

        Debug.DrawRay(centerTemp, new Vector2(fRaycastDistance * (Mathf.Abs(offsetX) / offsetX), 0), Color.red);
        Debug.DrawRay(centerTemp2, new Vector2(fRaycastDistance * (Mathf.Abs(offsetX) / offsetX), 0), Color.red);


        return true;
    }

    public struct AngleInfo
    {
        public bool detect;
        public float angle;

        public void reset()
        {
            detect = false;
            angle = 0;
        }
    }
}
