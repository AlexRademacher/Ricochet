using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetHit : MonoBehaviour
{
    [SerializeField]
    private GameObject BrokenVersion;

    // Start is called before the first frame update
    void Start()
    {
        //HitTheTarget();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HitTheTarget()
    {
        TargetInteraction();
    }

    private void TargetInteraction()
    {
        Instantiate(BrokenVersion, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Bullet")
        {
            HitTheTarget();
            Debug.Log("WE HIT HARD");
        }

        if (collision.gameObject.layer == 7)
        {
            HitTheTarget();
            Debug.Log("WE HIT HARD 7");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Bullet")
        {
            HitTheTarget();
            Debug.Log("WE Struck");
        }

        if (collision.gameObject.layer == 7)
        {
            HitTheTarget();
            Debug.Log("WE Struck HARD 7");
        }
    }
}
