using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class Client : MonoBehaviour
{
    public Camera cam;              // ссылка на нашу камеру
    public float move;
    public float jumpForce = 700f;
    bool facingRight = true;
    bool grounded = false;
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask whatIsGround;
    public float Hp = 0f;
    private Rigidbody2D rigidbody2D;

    private float lastSynchronizationTime = 0f;
    private float syncDelay = 0f;
    private float syncTime = 0f;
    private Vector3 syncStartPosition = Vector3.zero;
    private Vector3 syncEndPosition = Vector3.zero;
    private Animator animator = new Animator();

    public bool attack = false;
    //private bool attacking = false;
    private int attackA = 0;
    public GameObject HitPrefub;
    private float timeStartAttack;
    private float timeAttack = 100f;

    private float score = 0;
    private float maxHp = 10;
    private float damage = 1;
    private float maxSpeed = 10f;

    // При создании объекта со скриптом
    void Awake()
    {
        cam = transform.GetComponentInChildren<Camera>().GetComponent<Camera>();
    }

    void OnGUI()
    {
        if (GetComponent<NetworkView>().isMine)
        {
            GUI.Label(new Rect(30, 10, 120, 20), "Score: " + score);
            GUI.Label(new Rect(30, 30, 120, 20), "MaxHp: " + maxHp);
            GUI.Label(new Rect(30, 50, 120, 20), "Damage: " + damage);
            GUI.Label(new Rect(30, 70, 120, 20), "MaxSpeed: " + maxSpeed);
        }
    }
    void FixedUpdate()
    {
        grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, whatIsGround);
        move = Input.GetAxis("Horizontal");

        if (attack)
        {
            attackA++;
            if (attackA > 5)
            {
                attack = false;
                attackA = 0;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.tag == "Enemy")
        {
            Hp -= col.gameObject.GetComponent<AIScript>().Damage;
            rigidbody2D.velocity = (new Vector2(-Mathf.Sign(col.transform.position.x - transform.position.x) * 500f, 50f));
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        List<float> saveIt = new List<float>();
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(Application.persistentDataPath + "/savedGames.gd", FileMode.Open);
        //score = (float)bf.Deserialize(file);
        saveIt = (List<float>)bf.Deserialize(file);
        score = saveIt[0];
        if (saveIt[1] > 10)
            maxHp = saveIt[1];
        if (saveIt[2] > 10)
            damage = saveIt[2];
        if (saveIt[1] > 10)
            maxSpeed = saveIt[3];
        file.Close();
    }

    public void addScore(float scr)
    {
        if (GetComponent<NetworkView>().isMine)
        {
            score += scr / (Network.connections.Length + 1);
        }
    }

    void Update()
    {
        if (Hp <= 0)
        {
            Destroy(gameObject);
        }
        if (GetComponent<NetworkView>().isMine)
        {
            if (grounded && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)))
            {
                rigidbody2D.AddForce(new Vector2(0f, jumpForce));
            }

            rigidbody2D.velocity = new Vector2(move * maxSpeed, rigidbody2D.velocity.y);

            if (move > 0 && !facingRight)
                Flip();
            else if (move < 0 && facingRight)
                Flip();


            if (Input.GetKeyDown(KeyCode.Q))
            {
                Attack();
                GetComponent<NetworkView>().RPC("Attack", RPCMode.Others);
            }

            if (Input.GetKeyDown(KeyCode.F5))
            {
                List<float> saveIt = new List<float>();
                saveIt.Add(score);
                saveIt.Add(maxHp);
                saveIt.Add(damage);
                saveIt.Add(maxSpeed);
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Create(Application.persistentDataPath + "/savedGames.gd");
                bf.Serialize(file, saveIt);
                file.Close();
            }

            animator.SetFloat("Speed", Mathf.Abs(move));
            animator.SetBool("Jump", !grounded);
            animator.SetBool("Attack", attack);
        }
        else
        {
            if (cam.enabled)
            {
                cam.enabled = false;
                cam.gameObject.GetComponent<AudioListener>().enabled = false;
            }
            SyncedMovement();
        }
    }

    // Вызывается с определенной частотой. Отвечает за сереализицию переменных
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        Vector3 syncPosition = Vector3.zero;
        Vector3 syncScale = Vector3.zero;
        if (stream.isWriting)
        {
            syncPosition = transform.position;
            syncScale = transform.localScale;

            stream.Serialize(ref syncPosition);
            stream.Serialize(ref syncScale);
            stream.Serialize(ref move);
            stream.Serialize(ref attack);
            stream.Serialize(ref grounded);
        }
        else
        {
            stream.Serialize(ref syncPosition);
            stream.Serialize(ref syncScale);
            stream.Serialize(ref move);
            stream.Serialize(ref attack);
            stream.Serialize(ref grounded);

            transform.localScale = syncScale;


            animator.SetFloat("Speed", Mathf.Abs(move));
            animator.SetBool("Jump", !grounded);
            animator.SetBool("Attack", attack);

            // Расчеты для интерполяции

            // Находим время между текущим моментом и последней интерполяцией
            syncTime = 0f;
            syncDelay = Time.time - lastSynchronizationTime;
            lastSynchronizationTime = Time.time;

            syncStartPosition = transform.position;
            syncEndPosition = syncPosition;
        }
    }

    // Интерполяция
    private void SyncedMovement()
    {
        syncTime += Time.deltaTime;
        transform.position = Vector3.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
    }
    [RPC]
    public void Attack()
    {
        if (attack == false)
        {
            attack = true;
            var hit = (GameObject)Instantiate(HitPrefub, transform.position, transform.rotation);
            Vector3 scale = hit.transform.localScale;
            scale.x *= Mathf.Sign(transform.localScale.x);
            hit.transform.localScale = scale;
        }
    }
}
