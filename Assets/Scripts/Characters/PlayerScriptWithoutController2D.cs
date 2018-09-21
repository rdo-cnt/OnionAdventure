using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScriptWithoutController2D : MonoBehaviour
{

    //The player states
    public enum PlayerState
    {
        Free,
        Dash,
        SuperDash
    }
    protected PlayerState state = PlayerState.Free;

    //Get Inputs
    private float HorizontalInput;
    private float VerticalInput;

    //Get Grounded
    public float fGroundCheckRadius = 0.1f;
    public LayerMask whatIsGround;
    protected float fRaycastDistance = 0.18f;
    protected float fRaycastOffset = 0.28f;

    //Get movement Axis
    public Vector2 velocityAxis;
    private Vector2 velocityAxisNoSpeed;
    private float directionFloat = 1;

    //Speeds
    public float fStandSpeed = 2f;
    public float fCrouchSpeed = 1f;
    public float fDashSpeed = 2.5f;
    public float jumpForce = 5f;

    //Get Collision Classes
    protected Rigidbody2D m_rb;
    protected CapsuleCollider2D m_col;
    protected AnimationManager m_anim;
    protected Destructor m_destr;
    public Vector2 groundDirection;

    //Dashing
    protected IEnumerator DashRoutine;
    public float dashTime = 1.2f;


    //Getters
    public bool isGrounded { get { return (IsGrounded(fRaycastOffset) || IsGrounded(-fRaycastOffset)); } }

    // Use this for initialization
    void Start()
    {

        //Get components
        m_col = GetComponent<CapsuleCollider2D>();
        m_rb = GetComponent<Rigidbody2D>();
        m_anim = GetComponent<AnimationManager>();
        m_destr = GetComponent<Destructor>();
        m_destr.enabled = false;

    }

    // Update is called once per frame
    void Update()
    {
        //Functions done independent of states
        AnimationUpdate();

        //Do actions based on states
        switch (state)
        {
            case PlayerState.Free:
                Movement();
                Jump();
                AttackStart();
                checkDirection();
                break;

            case PlayerState.Dash:
                Jump();
                Attack();
                break;
        }
    }

    void FixedUpdate()
    {

        //Get movement Vector
        HorizontalInput = Input.GetAxis("Horizontal");
        VerticalInput = Input.GetAxis("Vertical");
        velocityAxisNoSpeed = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

    }

    public void checkDirection()
    {
        if (HorizontalInput > 0)
        {
            directionFloat = 1;
        }
        if (HorizontalInput < 0)
        {
            directionFloat = -1;
        }

    }


    public void AnimationUpdate()
    {
        //set animator variables
        m_anim.getAnimator().SetFloat("Speed", Mathf.Abs(HorizontalInput));
        m_anim.getAnimator().SetBool("InAir", !isGrounded);
        m_anim.getAnimator().SetBool("Dash", (state == PlayerState.Dash));
    }

    public void Movement()
    {
        m_rb.velocity = groundDirection * new Vector2(HorizontalInput * fStandSpeed, m_rb.velocity.y);
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

    public void Attack()
    {
        m_rb.velocity = new Vector2(fDashSpeed * directionFloat, m_rb.velocity.y);
        m_destr.enabled = true;
    }


    public void Jump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            m_rb.velocity = new Vector2(m_rb.velocity.x, 0);
            m_rb.velocity = new Vector2(m_rb.velocity.x, jumpForce);
        }

    }

    IEnumerator DashAttack()
    {
        yield return new WaitForSeconds(dashTime);
        state = PlayerState.Free;
    }

    public bool IsGrounded(float offsetX)
    {
        Vector3 origin = transform.position;
        origin.x += offsetX;
        origin.y = m_col.bounds.min.y;

        Vector3 origin2 = transform.position;
        origin.x -= offsetX;

        RaycastHit2D hitInfo = Physics2D.Raycast(origin, Vector2.down, fRaycastDistance, whatIsGround);
        RaycastHit2D hitInfo2 = Physics2D.Raycast(origin2, Vector2.down, fRaycastDistance, whatIsGround);

        //if (Mathf.Abs(-m_rb.velocity.y) > 1)
        // return false;

        //If we hit no collider, this means we are NOT grounded (= not on the ground)
        if (hitInfo.collider == null && hitInfo.collider == null)
        {
            Debug.DrawRay(origin, Vector2.down * fRaycastDistance, Color.green);
            return false;
        }
        Debug.DrawRay(origin, Vector2.down * fRaycastDistance, Color.red);

        //Detect Ground Z angle
        groundDirection = new Vector2(hitInfo.normal.y, hitInfo.normal.x);

        return true;
    }

    void checkGroundNormal()
    {

    }

    void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.layer == whatIsGround && isGrounded)
        {

        }

    }
}
