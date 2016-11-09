/// <summary>
/// Client v2.
/// Разработанно командой Sky Games
/// sgteam.ru
/// </summary>
using UnityEngine;
using System.Collections;

public class Client : MonoBehaviour
{

    public Camera cam;              // ссылка на нашу камеру
    private Vector3 moveDirection;  // вектор передвижения

    private float speed = 6.0F;     // скорость для внутренних расчетов
    public float speedStep = 6.0f;  // скорость ходьбы
    public float speedShift = 9.0f;	// скорость бега
    public float gravity = 20.0F;   // скорость падения
    public float speedRotate = 4;   // скорость поворота
    public float jumpSpeed = 8;     // высота прыжка

    public float move;
    public float maxSpeed = 10f;
    public float jumpForce = 700f;
    bool facingRight = true;
    bool grounded = false;
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask whatIsGround;
    private Rigidbody2D rigidbody2D;

    // Анимации
    public AnimationClip a_Idle;
    public float a_IdleSpeed = 1;

    public AnimationClip a_Walk;
    public float a_WalkSpeed = 1;

    public AnimationClip a_Run;
    public float a_RunSpeed = 1;

    public AnimationClip a_Jump;
    public float a_JumpSpeed = 1;

    private string s_anim;

   // private CharacterController controller; // ссылка на контроллер

    private float lastSynchronizationTime = 0f;
    private float syncDelay = 0f;
    private float syncTime = 0f;
    private Vector3 syncStartPosition = Vector3.zero;
    private Vector3 syncEndPosition = Vector3.zero;
    private Quaternion rot;                 // поворот 
    private int numCurAnim;                 // номер анимации для сереализации 0 ожидание 1 ходьба 2 бег 3 прыжок 

    // При создании объекта со скриптом
    void Awake()
    {
        cam = transform.GetComponentInChildren<Camera>().GetComponent<Camera>();

      //  controller = GetComponent<CharacterController>();

        //GetComponent<Animation>()[a_Idle.name].speed = a_IdleSpeed;
        //GetComponent<Animation>()[a_Walk.name].speed = a_WalkSpeed;
        //GetComponent<Animation>()[a_Run.name].speed = a_RunSpeed;
        //GetComponent<Animation>()[a_Jump.name].speed = a_JumpSpeed;

        //GetComponent<Animation>()[a_Idle.name].wrapMode = WrapMode.Loop;
        //GetComponent<Animation>()[a_Walk.name].wrapMode = WrapMode.Loop;
        //GetComponent<Animation>()[a_Run.name].wrapMode = WrapMode.Loop;
        //GetComponent<Animation>()[a_Jump.name].wrapMode = WrapMode.ClampForever;

        s_anim = a_Idle.name;
        numCurAnim = 0;
    }

    void FixedUpdate()
    {
        grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, whatIsGround);
        move = Input.GetAxis("Horizontal");
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
    }
    // на каждый кадр
    void Update()
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
        if (GetComponent<NetworkView>().isMine)
        {
            //if (grounded && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)))
            //{
            //    rigidbody2D.AddForce(new Vector2(0f, jumpForce));
            //}

            //rigidbody2D.velocity = new Vector2(move * maxSpeed, rigidbody2D.velocity.y);

            //if (move > 0 && !facingRight)
            //    Flip();
            //else if (move < 0 && facingRight)
            //    Flip();

            //GetComponent<Animation>().CrossFade(s_anim);

            //if (controller.isGrounded)
            //{
            //    moveDirection = new Vector3(0, 0, Input.GetAxis("Vertical"));
            //    moveDirection = transform.TransformDirection(moveDirection);
            //    moveDirection *= speed;

            //    if (Input.GetKey(KeyCode.LeftShift))
            //        speed = speedShift;
            //    else speed = speedStep;

            //    // Анимация ходьбы
            //    if (Input.GetAxis("Vertical") > 0)
            //    {
            //        if (speed == speedShift)
            //        {
            //            s_anim = a_Run.name;
            //            GetComponent<Animation>()[a_Run.name].speed = a_RunSpeed;
            //            numCurAnim = 2;
            //        }
            //        else
            //        {
            //            s_anim = a_Walk.name;
            //            GetComponent<Animation>()[a_Walk.name].speed = a_WalkSpeed;
            //            numCurAnim = 1;
            //        }
            //    }
            //    else
            //    if (Input.GetAxis("Vertical") < 0)
            //    {
            //        if (speed == speedShift)
            //        {
            //            s_anim = a_Run.name;
            //            GetComponent<Animation>()[a_Run.name].speed = a_RunSpeed * -1;
            //            numCurAnim = 2;
            //        }
            //        else
            //        {
            //            s_anim = a_Walk.name;
            //            GetComponent<Animation>()[a_Walk.name].speed = a_WalkSpeed * -1;
            //            numCurAnim = 1;
            //        }
            //    }
            //    else
            //    if (Input.GetAxis("Vertical") == 0)
            //    {
            //        s_anim = a_Idle.name;
            //        numCurAnim = 0;
            //    }

            //    if (Input.GetKeyDown(KeyCode.Space))
            //    {
            //        moveDirection.y = jumpSpeed;
            //        s_anim = a_Jump.name;
            //        numCurAnim = 3;
            //    }
            //}

            //moveDirection.y -= gravity * Time.deltaTime;
            //controller.Move(moveDirection * Time.deltaTime);
            //transform.Rotate(Vector3.down * speedRotate * Input.GetAxis("Horizontal") * -1, Space.World);
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
        if (stream.isWriting)
        {
            rot = transform.rotation;
            syncPosition = transform.position;

            stream.Serialize(ref syncPosition);
            stream.Serialize(ref rot);
            stream.Serialize(ref numCurAnim);
        }
        else
        {
            stream.Serialize(ref syncPosition);
            stream.Serialize(ref rot);
            stream.Serialize(ref numCurAnim);

            //PlayNameAnim();
            GetComponent<Animation>().CrossFade(s_anim);

            transform.rotation = rot;

            // Расчеты для интерполяции

            // Находим время между текущим моментом и последней интерполяцией

            syncTime = 0f;
            syncDelay = Time.time - lastSynchronizationTime;
            lastSynchronizationTime = Time.time;

            syncStartPosition = transform.position;
            syncEndPosition = syncPosition;
            Debug.Log(GetComponent<NetworkView>().viewID + " " + syncStartPosition + " " + syncEndPosition);
        }
    }

    // Интерполяция
    private void SyncedMovement()
    {
        syncTime += Time.deltaTime;
        transform.position = Vector3.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
    }

    // Определение анимации по номеру
    public void PlayNameAnim()
    {
        switch (numCurAnim)
        {
            case 0:
                s_anim = a_Idle.name;
                break;

            case 1:
                s_anim = a_Walk.name;
                break;

            case 2:
                s_anim = a_Run.name;
                break;

            case 3:
                s_anim = a_Jump.name;
                GetComponent<Animation>()[a_Jump.name].wrapMode = WrapMode.ClampForever;
                break;
        }
    }
}
