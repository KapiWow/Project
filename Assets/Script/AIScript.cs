using UnityEngine;
using System.Collections.Generic;

public class AIScript : MonoBehaviour
{
    public float Speed = 5f;
    public float Hp = 2f;
    public float Damage = 1f;
    public float ScoreForDeath = 10;

    public GameObject Fireball;

    private GameObject player;
    private List<int> damageBy = new List<int>();
    bool facingRight = true;
    private Rigidbody2D rb2d;

    private GameObject[] players;
    private float lastAttack;
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        lastAttack = Time.time;
    }

    void FixedUpdate()
    {
        rb2d.velocity = new Vector2(Speed * (facingRight ? 1f : -1f), rb2d.velocity.y);
        if (Network.isServer)
        {
            lastAttack += 1;
            if (Mathf.Abs(Time.time - lastAttack) > 120)
            {
                GetComponent<NetworkView>().RPC("CreateEnemy", RPCMode.All);
                lastAttack = Time.time;
            }
        }
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
        if (col.tag == "Hit")
        {
            bool damaged = true;
            if (damageBy.Capacity != 0)
                foreach (int i in damageBy)  //оптимизировать потом не удаляются все полученные удары
                {
                    if (i == col.gameObject.GetInstanceID())
                        damaged = false;
                }
            if (damaged)
            {
                Hp -= col.GetComponent<HitScript>().Damage;
                damageBy.Add(col.gameObject.GetInstanceID());
                if (Hp <= 0)
                {
                    GetComponent<NetworkView>().RPC("Die", RPCMode.Others);
                    Die();
                }
            }
        }
    }
    [RPC]
    void Die()
    {
        player = GameObject.Find("PlayerNetWork(Clone)");
        player.GetComponent<Client>().addScore(ScoreForDeath);
        Destroy(gameObject);
    }

    [RPC]
    public void Attack()
    {
        Debug.Log("123");
        players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length > 0)
        {
            GameObject Aim = players[0];
            foreach (GameObject player in players)
            {
                if (Vector3.Distance(player.transform.position, transform.position) <
                    Vector3.Distance(Aim.transform.position, transform.position))
                {
                    Aim = player;
                }
            }
            GameObject ball = (GameObject)Instantiate(Fireball, transform.position, transform.rotation);
            Vector3 vec = new Vector3();
            vec = Aim.transform.position - transform.position;
            vec.Normalize();
            ball.GetComponent<Rigidbody2D>().velocity = new Vector2(vec.x * 3, vec.y * 3);
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        //if (col.collider.tag == "Player")
        //{

        //    Destroy(col.gameObject);
        //}
        if (col.collider.tag == "Enemy")
        {
            Flip();
        }
    }
    void OnGUI()
    {
        if (Network.isClient)
            GUI.Label(new Rect((Screen.width - 100) / 2, Screen.height / 2 - 150, 100, 20), Hp.ToString());
    }
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        Vector3 syncPosition = Vector3.zero;
        Vector3 syncScale = Vector3.zero;
        float synchHp = 0;
        if (stream.isWriting)
        {
            if (Network.isServer)
            {
                syncPosition = transform.position;
                syncScale = transform.localScale;
                synchHp = Hp;

                stream.Serialize(ref syncPosition);
                stream.Serialize(ref syncScale);
                stream.Serialize(ref synchHp);
            }
            else
            {

            }
        }
        else
        {
            if (Network.isServer)
            {

            }
            else
            {
                stream.Serialize(ref syncPosition);
                stream.Serialize(ref syncScale);
                stream.Serialize(ref synchHp);

                Hp = synchHp;
                transform.localScale = syncScale;
                transform.position = syncPosition;
            }
        }
    }

}