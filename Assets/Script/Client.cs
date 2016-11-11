using UnityEngine;
using System.Collections;

public class Client : MonoBehaviour
{

    public Camera cam;              // ссылка на нашу камеру
    public float move;
    public float maxSpeed = 10f;
    public float jumpForce = 700f;
    bool facingRight = true;
    bool grounded = false;
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask whatIsGround;
    private Rigidbody2D rigidbody2D;

    private float lastSynchronizationTime = 0f;
    private float syncDelay = 0f;
    private float syncTime = 0f;
    private Vector3 syncStartPosition = Vector3.zero;
    private Vector3 syncEndPosition = Vector3.zero;
    private Animator animator = new Animator();

    public bool attack = false;
    private int attackA = 0;
    public GameObject HitPrefub;
    private float timeStartAttack;
    private float timeAttack = 100f;

    // При создании объекта со скриптом
    void Awake()
    {
        cam = transform.GetComponentInChildren<Camera>().GetComponent<Camera>();
    }

    void FixedUpdate()
    {
        grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, whatIsGround);
        move = Input.GetAxis("Horizontal");

        if (attack)
        {
            attackA++;
            if (attackA > 30)
            {
                attack = false;
                attackA = 0;
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

    void Start()
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    //void OnGUI()
    //{
    //    if (!GetComponent<NetworkView>().isMine)
    //    {
    //        GUI.Label(new Rect((Screen.width - 100) / 2, Screen.height / 2 - 60, 100, 20), syncEndPosition.ToString());
    //        GUI.Label(new Rect((Screen.width - 100) / 2, Screen.height / 2 - 90, 100, 20), syncStartPosition.ToString());
    //        GUI.Label(new Rect((Screen.width - 100) / 2, Screen.height / 2 - 120, 100, 20), transform.position.ToString());
    //        GUI.Label(new Rect((Screen.width - 100) / 2, Screen.height / 2 - 150, 100, 20), jjj.ToString());
    //    }
    //}
    // на каждый кадр

    void Update()
    {
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
        }
        else
        {
            stream.Serialize(ref syncPosition);
            stream.Serialize(ref syncScale);

            transform.localScale = syncScale;

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
