using UnityEngine;
using System.Collections;

public class AIScript : MonoBehaviour
{
    public float Speed = 5f;
    public float Hp = 2f;

    private int damageBy;


    bool facingRight = true;

    private Rigidbody2D rb2d;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    void Update()
    {

    }

    void FixedUpdate()
    {
        rb2d.velocity = new Vector2(Speed * (facingRight ? 1f : -1f), rb2d.velocity.y);
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Flip")
            Flip();
        if ((col.tag == "Hit") && (damageBy != col.gameObject.GetInstanceID()))
        {
            Hp -= col.GetComponent<HitScript>().Damage;
            damageBy = col.gameObject.GetInstanceID();
            if (Hp <= 0)
            {
                Destroy(gameObject);
            }
            Destroy(col.gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.tag == "Player")
            Application.LoadLevel(Application.loadedLevel);
        if (col.collider.tag == "Enemy")
            Flip();
    }
}