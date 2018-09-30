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
    public Transform leftAnglePos;
    public Transform middleAnglePos;
    public Transform rightAnglePos;

    protected float raycastDistance = .40f;
    protected float raycastRegularDistance = .40f;
    protected float raycastShortenedDistance = .23f;
    public LayerMask groundLayer;
    public float maxClimbingAngle = 80f;
    public float maxStandingAngle = 55f;
    public float groundedAngle = 0f;

    public bool isGrounded = false;
    public bool isSticked = false;
    public bool isBumping = false;
    public bool isRayShortened = false;
    public bool canBump = true;

    // Use this for initialization
    void Start()
    {
        collider = GetComponent<CapsuleCollider2D>();
        rigidbody = GetComponent<Rigidbody2D>();
        raycastDistance = raycastRegularDistance;
    }

    void Update()
    {
        GroundedCalculation();
        AngleCalculation();
        CheckShortenedRays();
        MoveSideWays(15f);
    }

    public void CheckShortenedRays()
    {
        if (isRayShortened)
        {
            raycastDistance = raycastShortenedDistance;
            if (isGrounded)
            {
                isRayShortened = false;
                raycastDistance = raycastRegularDistance;
            }
        }
    }

    public void AngleCalculation()
    {
        canBump = true;

        //Check all angles
        leftAngle.reset();
        middleAngle.reset();
        rightAngle.reset();

        middleAngle = RayCastDown(middleAnglePos);

        leftAngle = RayCastSide(leftAnglePos, -1f);
        if (!leftAngle.detect)
            leftAngle = RayCastDown(leftAnglePos);
        if (!leftAngle.detect)
        {
            leftAngle = RayCastSide(leftAnglePos, 1f, -raycastDistance);
            canBump = false;
        } 

        rightAngle = RayCastSide(rightAnglePos, 1f);
        if (!rightAngle.detect)
            rightAngle = RayCastDown(rightAnglePos);
        if (!rightAngle.detect)
        {
            Debug.Log("ok r");
            rightAngle = RayCastSide(rightAnglePos, -1f, -raycastDistance);
            canBump = false;
        }

        //Debug.Log("left: " + leftAngle.angle + "middle: " + middleAngle.angle + "right: " + rightAngle.angle);
    }

    public void GroundedCalculation()
    {
        //Initial values on grounded Calculation
        groundedAngle = 0f;
        isGrounded = false;

        if (middleAngle.detect)
        {
            isGrounded = true;
            if(middleAngle.angle != 0)
             groundedAngle = middleAngle.angle;
        }
        //Non sticked groundedness
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

        if (!isGrounded)
        { //isRayShortened = true; 
        }

    }

    public AngleInfo RayCastDown(Transform raycastTransform)
    {
        return RayCastDetection(raycastTransform, -transform.up);
    }

    public AngleInfo RayCastSide(Transform raycastTransform, float directionX , float offsetY = 0)
    {
        return RayCastDetection(raycastTransform, transform.right, directionX, offsetY);
    }

    public AngleInfo RayCastDetection(Transform raycastTransform, Vector3 raycastDirection, float directionX = 1, float offsetY = 0)
    {
        AngleInfo TemporaryAngle;
        TemporaryAngle.angle = 0;
        TemporaryAngle.detect = false;
        TemporaryAngle.distance = 0;
        TemporaryAngle.normal = new Vector2(0,0);

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
            rigidbody.velocity = new Vector2(0, rigidbody.velocity.y);
            return;
        }
            

        if (isSticked)
        {
            rigidbody.velocity = new Vector2(transform.right.x * speedX, transform.right.y * speedX); //simply push the player forward
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
            Vector2 tempDirection = new Vector2(rightAngle.normal.y != 0 ? rightAngle.normal.y : 1, -rightAngle.normal.x);
            rigidbody.velocity = tempDirection * speedX;
        }
        if (speedX < 0)
        {
            Vector2 tempDirection = new Vector2(leftAngle.normal.y != 0 ? leftAngle.normal.y : 1, -leftAngle.normal.x);
            rigidbody.velocity = tempDirection * speedX;
        }
            

    }

    public void aerialMovement(float speedX)
    {
        rigidbody.velocity = new Vector2(speedX, rigidbody.velocity.y);
    }

    public void MoveRight(float speedX)
    {
        float angleDifference = Mathf.Abs(Mathf.Abs(rightAngle.angle) - Mathf.Abs(middleAngle.angle));
        float angleDifferenceSign = Mathf.Sign(rightAngle.angle - middleAngle.angle);

        if (angleDifference <= maxClimbingAngle && middleAngle.detect)
        {
                 //climbing
                if (angleDifferenceSign < 0)
                {
                    //Debug.Log("climbing");
                    if (!CompareFloats(rightAngle.angle, middleAngle.angle))
                    {
                        //rigidbody.MovePosition(transform.position + ((new Vector3(transform.right.x, transform.right.y, 0)) * rightAngle.distance));

                    }
                }
                //descending
                else
                    if (!CompareFloats(rightAngle.angle, middleAngle.angle))
                    {
                    //rigidbody.velocity = rigidbody.velocity * ((new Vector2(-transform.up.x, -transform.up.y)) * Mathf.Abs(speedX) * 2);
                    }

                if(isSticked)
                {
                    //Change Angle of player
                    transform.eulerAngles = new Vector3(0, 0, -rightAngle.angle);
                    stickToFloor();
                }
 
        }
        else
            if (angleDifference > maxClimbingAngle && canBump)
                isBumping = true;

    }

    public void MoveLeft(float speedX)
    {
        float angleDifference = Mathf.Abs(Mathf.Abs(leftAngle.angle) - Mathf.Abs(middleAngle.angle));
        float angleDifferenceSign = Mathf.Sign(middleAngle.angle - leftAngle.angle);

        if (angleDifference <= maxClimbingAngle && middleAngle.detect)
        {
            //climbing
            if (angleDifferenceSign < 0)
            {
                if (!CompareFloats(leftAngle.angle, middleAngle.angle))
                    rigidbody.MovePosition(transform.position + ((new Vector3(-transform.right.x, -transform.right.y, 0)) * leftAngle.distance));
            }

            //descend
            else
            {
                if (!CompareFloats(leftAngle.angle, middleAngle.angle))
                {
                    //rigidbody.velocity = rigidbody.velocity * ((new Vector2(-transform.up.x, -transform.up.y)) * Mathf.Sign(speedX) * 2);
                }
            }      

            if (isSticked)
            {
                //Change Angle of player
                transform.eulerAngles = new Vector3(0, 0, -leftAngle.angle);
                stickToFloor();
            }
        }
        else
            if (angleDifference > maxClimbingAngle && canBump)
            isBumping = true;

    }

    public void stickToFloor()
    {
        Vector3 raycastOriginPosition = transform.position;
        RaycastHit2D hitInfo = Physics2D.Raycast(raycastOriginPosition, -transform.up, raycastDistance*2, groundLayer);
        if(hitInfo.collider == null)
        {
            Debug.DrawRay(raycastOriginPosition, -transform.up * raycastDistance * 2, Color.magenta);
            return; 
        }
        Debug.DrawRay(raycastOriginPosition, -transform.up * raycastDistance * 2, Color.yellow);
        transform.position = hitInfo.point + new Vector2(transform.up.x, transform.up.y) * 0.01f;
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
