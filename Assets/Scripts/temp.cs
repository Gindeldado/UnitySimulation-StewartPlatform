using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class temp : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Vector3 centerPoint = gameObject.GetComponent<Renderer>().bounds.center;
        Debug.Log(centerPoint);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
