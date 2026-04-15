using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Despawn : MonoBehaviour
{

    private void Awake()
    {
        StartCoroutine(Delete());
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator Delete()
    {
        yield return new WaitForSecondsRealtime(3);
        Destroy(gameObject);
    }
}
