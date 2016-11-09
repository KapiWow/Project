using UnityEngine;
using System.Collections;

public class CheckScript : MonoBehaviour {

    // Use this for initialization

    public GameObject Player;

    public Animator animator;

    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag == "Enemy")
        {
            //animator = Player.GetComponent<Animator>();
            //animator.SetBool("Attack", true);
            Player.SendMessage("Attack");
        }
        //    Player.SendMessage("Attack");

        //animator = Player.GetComponent<Animator>();
        //animator.SetBool("Attack", true);
    }
}
