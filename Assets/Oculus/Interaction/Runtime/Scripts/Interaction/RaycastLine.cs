using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastLine : MonoBehaviour
{
    int rayLength = 20;
    public Material lineMaterial;
    public float delay = 0.1f;

    void Start()
    {
       
    }

    void Update()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit, rayLength * 10))
        {
            GameObject myLine = new GameObject();
            myLine.transform.position = transform.position;
            myLine.AddComponent<LineRenderer>();

            LineRenderer lr = myLine.GetComponent<LineRenderer>();
            lr.material = lineMaterial;

            lr.startWidth = 0.01f;
            lr.endWidth = 0.01f;
            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, hit.point);
            GameObject.Destroy(myLine,delay);
        }
    }
}
