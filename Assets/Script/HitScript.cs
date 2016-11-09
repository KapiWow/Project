using UnityEngine;
using System.Collections;

public class HitScript : MonoBehaviour
{

    public float Damage;
    // Use this for initializations
    void Start()
    {
        Destroy(gameObject, 0.2f);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
