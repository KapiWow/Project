using UnityEngine;
using System.Collections;

public class NewBehaviourScript : MonoBehaviour 
{
	public Transform player;

	// Update is called once per frame
	void Update () 
	{
		transform.position = player.position - new Vector3 (0f, 0f, 10f);

	}
}