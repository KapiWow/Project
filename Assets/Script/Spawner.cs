using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour
{

    public GameObject EnemyPrefub;
    private float time = 0f;
    // Use this for initialization
    void Start()
    {

    }

    void FixedUpdate()
    {
        if (Network.isServer)
        {
            time += 1;
            if (Mathf.Abs(Time.time - time) > 120)
            {
                GetComponent<NetworkView>().RPC("CreateEnemy", RPCMode.All);
                time = Time.time;
            }
        }
    }

    [RPC]
    void CreateEnemy()
    {
        var enemy = (GameObject)Instantiate(EnemyPrefub, transform.position, transform.rotation);
    }
    // Update is called once per frame
    void Update()
    {

    }
}
