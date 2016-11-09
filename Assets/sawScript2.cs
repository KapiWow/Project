using UnityEngine;
using System.Collections;

public class sawScript2 : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		transform.Rotate (new Vector3 (0f, 0f, 1f));
	}
	void onCollisionEnter2D (Collision2D col) {
		if (col.gameObject.name == "Buzz_Saw")
			Destroy (gameObject);
		

		}
}

	


