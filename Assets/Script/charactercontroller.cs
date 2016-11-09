using UnityEngine;
using System.Collections;
using System.Timers;
using System;

public class charactercontroller : MonoBehaviour
{
    public float maxSpeed = 10f;
    public float jumpForce = 700f;
    bool facingRight = true;
    bool grounded = false;
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask whatIsGround;
    private Rigidbody2D rigidbody2D;
    public float move;
    public bool attack = false;
    private int attackA = 0;
    public GameObject HitPrefub;
    private float timeStartAttack;
    private float timeAttack = 100f;


    private Animator animator;

    // Use this for initialization
    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, whatIsGround);
        move = Input.GetAxis("Horizontal");
    }

    void Update()
    {
        //if (attack == true)
        //{
        //    attackA = true;
        //    timeStartAttack = Time.time;
        //}


        //if (attackA)
        //    if (Time.time - timeStartAttack < timeAttack)
        //    {
        //        attack = false;
        //        attackA = false;
        //    }

        //if ((animator.GetBool("Attack")) && (attack == false))
        //{
        //    Attack();
        //}

        if (attack)
        {
            attackA++;
            if (attackA>5)
            {
                attack = false;
                attackA = 0;
            }
        }

        //if (attackA)
        //    attack = false;


        if (grounded && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)))
        {
            rigidbody2D.AddForce(new Vector2(0f, jumpForce));
        }
        rigidbody2D.velocity = new Vector2(move * maxSpeed, rigidbody2D.velocity.y);

        if (move > 0 && !facingRight)
            Flip();
        else if (move < 0 && facingRight)
            Flip();

        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetKey(KeyCode.R))
        {
            Application.LoadLevel(Application.loadedLevel);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            //animator.SetBool("Attack", true);
            Attack();
        }

        animator.SetFloat("Speed", Mathf.Abs(move));
        animator.SetBool("Jump", !grounded);
        animator.SetBool("Attack", attack);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.tag == "Enemy")
            Application.LoadLevel(Application.loadedLevel);
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    public void Attack()
    {
        if (attack == false)
        {
            attack = true;
            //timeStartAttack = Time.time;
            //animator.SetBool("Attack", true);
            var hit = (GameObject)Instantiate(HitPrefub, transform.position, transform.rotation);
            Vector3 scale = hit.transform.localScale;
            scale.x *= Mathf.Sign(transform.localScale.x);
            hit.transform.localScale = scale;
        }
    }
}