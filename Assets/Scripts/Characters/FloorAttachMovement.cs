using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorAttachMovement : MonoBehaviour
{
    protected Rigidbody2D rigidbody;
    protected CapsuleCollider2D collider;

    public AngleInfo leftAngle;
    public AngleInfo middleAngle;
    public AngleInfo rightAngle;
    public AngleInfo leftTopAngle;
    public AngleInfo topAngle;
    public AngleInfo rightTopAngle;
    public Transform leftAnglePos;
    public Transform middleAnglePos;
    public Transform rightAnglePos;
    public Transform leftTopAnglePos;
    public Transform topAnglePos;
    public Transform rightTopAnglePos;

    protected float raycastDistance = .38f;
    protected float raycastRegularDistance = .38f;
    protected float raycastShortenedDistance = .23f;
    public LayerMask groundLayer;
    public float maxClimbingAngle = 80f;
    public float maxStandingAngle = 55f;
    public float groundedAngle = 0f;
    public float testSpeed = 18f;

    public bool isGrounded = false;
    public bool isSticked = false;
    public bool isBumping = false;
    public bool isRayShortened = false;
    public bool canBumpLeft = true;
    public bool canBumpRight = true;
    public bool canBump = true;
    public bool canBumpTop = true;

    // Use this for initialization
    void Start()
    {
        collider = GetComponent<CapsuleCollider2D>();
        rigidbody = GetComponent<Rigidbody2D>();
        raycastDistance = raycastRegularDistance;
    }

    void Update()
    {
        AngleCalculation();
        GroundedCalculation();
        CheckShortenedRays();
     }

    public void CheckShortenedRays()
    {
        if (isRayShortened)
        {
            raycastDistance = raycastShortenedDistance;
            if (isGrounded)
            {
                isRayShortened = false;
                stickToFloor();
                raycastDistance = raycastRegularDistance;
            }
        }
    }

    public void AngleCalculation()
    {
        canBump = true;
        canBumpLeft = true;
        canBumpRight = true;

        //Check all angles
        leftAngle.reset();
        middleAngle.reset();
        rightAngle.reset();

        middleAngle = RayCastDown(middleAnglePos);
        topAngle = RayCastUp(topAnglePos);

        leftAngle = RayCastSide(leftAnglePos, -1f);
        if (!leftAngle.detect)
            leftAngle = RayCastDown(leftAnglePos);
        if (!leftAngle.detect)
        {
            leftAngle = RayCastSide(leftAnglePos, 1.5f, -raycastDistance);
            canBumpLeft = false;
        }

        rightAngle = RayCastSide(rightAnglePos, 1f);
        if (!rightAngle.detect)
            rightAngle = RayCastDown(rightAnglePos);
        if (!rightAngle.detect)
        {
            rightAngle = RayCastSide(rightAnglePos, -1.5f, -raycastDistance);
            canBumpRight = false;
        }

        Debug.Log("left: " + leftAngle.angle + "middle: " + middleAngle.angle + "right: " + rightAngle.angle);
    }

    public void GroundedCalculation()
    {
        //Initial values on grounded Calculation
        groundedAngle = 0f;
        isGrounded = false;

        if (middleAngle.detect && (Mathf.Abs(middleAngle.angle) < maxStandingAngle || isSticked) )
        {
            isGrounded = true;
            if (middleAngle.angle != 0)
                groundedAngle = middleAngle.angle;
        }

        //Non sticked groundedness
        if (!isSticked)
        {
            if (leftAngle.detect && (Mathf.Abs(leftAngle.angle) < maxStandingAngle))
            {
                isGrounded = true;
                if (leftAngle.angle != 0)
                    groundedAngle = leftAngle.angle;
            }
            if (rightAngle.detect && (Mathf.Abs(rightAngle.angle) < maxStandingAngle))
            {
                isGrounded = true;
                if (rightAngle.angle != 0)
                    groundedAngle = rightAngle.angle;
            }
        }


    }

    public AngleInfo RayCastDown(Transform raycastTransform)
    {
        return RayCastDetection(raycastTransform, -transform.up);
    }

    public AngleInfo RayCastUp(Transform raycastTransform)
    {
        return RayCastDetection(raycastTransform, transform.up);
    }

    public AngleInfo RayCastSide(Transform raycastTransform, float directionX, float offsetY = 0)
    {
        return RayCastDetection(raycastTransform, transform.right, directionX, offsetY);
    }

    public AngleInfo RayCastDetection(Transform raycastTransform, Vector3 raycastDirection, float directionX = 1, float offsetY = 0)
    {
        AngleInfo TemporaryAngle;
        TemporaryAngle.angle = 0;
        TemporaryAngle.detect = false;
        TemporaryAngle.distance = 0;
        TemporaryAngle.normal = new Vector2(0, 0);

        Vector3 raycastOriginPosition = raycastTransform.position;
        raycastOriginPosition += transform.up * offsetY;
        raycastDirection *= directionX;
        RaycastHit2D hitInfo = Physics2D.Raycast(raycastOriginPosition, raycastDirection, raycastDistance, groundLayer);
        if (hitInfo.collider == null)
        {
            Debug.DrawRay(raycastOriginPosition, raycastDirection * raycastDistance, Color.green);
            return TemporaryAngle;
        }

        Debug.DrawRay(raycastOriginPosition, raycastDirection * raycastDistance, Color.red);
        TemporaryAngle.angle = Vector2.Angle(hitInfo.normal, Vector2.up) * Mathf.Sign(hitInfo.normal.x);
        TemporaryAngle.detect = true;
        TemporaryAngle.distance = hitInfo.distance;
        TemporaryAngle.normal = hitInfo.normal;

        return TemporaryAngle;
    }


    public void MoveSideWays(float speedX)
    {
        //Initial Values
        isBumping = false;

        if (speedX == 0)
        {
            return;
        }

        if (isSticked)
        {
           // Debug.Log("we movin bois. X: " + transform.right.x + " Y: " + transform.right.y);
            //transform.Translate((new Vector3(transform.right.x, transform.right.y, 0)) * speedX * Time.deltaTime);
            Vector3 tempDirection = transform.right * speedX * Time.deltaTime;
            transform.Translate(tempDirection, Space.World);
            //rigidbody.velocity = new Vector2(transform.right.x, transform.right.y) * speedX;
        }
        else
        {
            if (isGrounded)
                groundedMovement(speedX);
            else
                aerialMovement(speedX);
        }

        if (speedX > 0)
            MoveRight(speedX);
        else
            MoveLeft(speedX);


    }

    public void groundedMovement(float speedX)
    {

        //Check velocity depending on angle
        if (speedX > 0)
        {
            if(rightTopAnglePos && canBumpTop)
            {
                rightTopAngle = RayCastSide(rightTopAnglePos, 1f);
                if(rightTopAngle.detect)
                {
                    isBumping = true;
                    return;
                }
            }
                
            if (Mathf.Abs(rightAngle.angle) > maxStandingAngle && rightAngle.angle < 0 && canBumpRight)
            {
                return;
            }


                
            Vector3 tempDirection = new Vector3((rightAngle.normal.y != 0) ? rightAngle.normal.y : 1, -rightAngle.normal.x, 0);
            //Debug.Log("temp direction: " + tempDirection);

            transform.Translate(tempDirection * speedX * Time.deltaTime);
        }
        if (speedX < 0)
        {
            if (leftTopAnglePos && canBumpTop)
            {
                leftTopAngle = RayCastSide(leftTopAnglePos, -1f);
                if (leftTopAngle.detect)
                {
                    
                    isBumping = true;
                    return;
                }
            }
                
            if (Mathf.Abs(leftAngle.angle) > maxStandingAngle && leftAngle.angle > 0 && canBumpLeft)
                return;
                
            Vector3 tempDirection = new Vector3((leftAngle.normal.y != 0) ? leftAngle.normal.y : 1, -leftAngle.normal.x, 0);
            //Debug.Log("temp direction: " + tempDirection);
            transform.Translate(tempDirection * speedX * Time.deltaTime);
        }

        stickToFloor();

    }

    public void aerialMovement(float speedX)
    {
        if (speedX > 0 && (Mathf.Abs(rightAngle.angle) > maxStandingAngle) && rightAngle.angle < 0)
            return;
        if (speedX < 0 && (Mathf.Abs(leftAngle.angle) > maxStandingAngle) && leftAngle.angle > 0)
            return;

        transform.Translate(new Vector3(speedX, 0, 0) * Time.deltaTime);
    }

    public void MoveRight(float speedX)
    {
        float forwardAngle = (groundedAngle != 0) ? groundedAngle : middleAngle.angle;

        float angleDifference = Mathf.Abs(Mathf.Abs(rightAngle.angle) - Mathf.Abs(forwardAngle));
        float angleDifferenceSign = Mathf.Sign(rightAngle.angle - groundedAngle);

        if (angleDifference <= maxClimbingAngle)
        {

            //climbing
            if (angleDifferenceSign < 0)
            {
                //Debug.Log("climbing");

            }
            //descending
            else
            {

            }

            if (isSticked && (isGrounded || angleDifference < maxStandingAngle))
            {
                //Change Angle of player
                transform.eulerAngles = new Vector3(0, 0, -rightAngle.angle);
                stickToFloor();
            }

        }
        else
            if (angleDifference > maxClimbingAngle && canBumpRight)
            {
            Debug.Log("I bumped");
            isBumping = true;
            }



    }

    public void MoveLeft(float speedX)
    {
        float forwardAngle = (groundedAngle != 0) ? groundedAngle : middleAngle.angle;

        float angleDifference = Mathf.Abs(Mathf.Abs(leftAngle.angle) - Mathf.Abs(forwardAngle));
        float angleDifferenceSign = Mathf.Sign(forwardAngle - leftAngle.angle);

        if (angleDifference <= maxClimbingAngle)
        {
            //climbing
            if (angleDifferenceSign < 0)
            {
                if (!CompareFloats(leftAngle.angle, forwardAngle))
                {
                    //rigidbody.MovePosition(transform.position + ((new Vector3(-transform.right.x, -transform.right.y, 0)) * leftAngle.distance));

                }
            }

            //descend
            else
            {
                if (!CompareFloats(leftAngle.angle, forwardAngle))
                {
                    //rigidbody.velocity = rigidbody.velocity * ((new Vector2(-transform.up.x, -transform.up.y)) * Mathf.Sign(speedX) * 2);
                }
            }

            if (isSticked && (isGrounded || angleDifference < maxStandingAngle))
            {
                //Change Angle of player
                transform.eulerAngles = new Vector3(0, 0, -leftAngle.angle);
                stickToFloor();
            }
        }
        else
            if (angleDifference > maxClimbingAngle && canBumpLeft)
            isBumping = true;

    }

    public void stickToFloor()
    {
        Vector3 raycastOriginPosition = transform.position;
        RaycastHit2D hitInfo = Physics2D.Raycast(raycastOriginPosition, -transform.up, raycastDistance*1.5f , groundLayer);
        if (hitInfo.collider == null)
        {
            Debug.DrawRay(raycastOriginPosition, -transform.up * raycastDistance * 1.5f, Color.magenta);
            return;
        }
        Debug.DrawRay(raycastOriginPosition, -transform.up * raycastDistance * 2, Color.yellow);
        transform.position = hitInfo.point + new Vector2(transform.up.x, transform.up.y) * 0.02f;
    }

    public bool CompareFloats(float a, float b = 0)
    {
        return (Mathf.Abs(a - b) < 0.3f);
    }


    public struct AngleInfo
    {
        public bool detect;
        public float angle;
        public float distance;
        public Vector2 normal;

        public void reset()
        {
            detect = false;
            angle = 0;
            distance = 0;
        }
    }

}
