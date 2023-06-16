using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpDownMover : MonoBehaviour
{
    public Vector3 offset;
    public float duration = 1;

    Vector3 initPos;
    Vector3 target;
    float curTime = 0;
    bool ToOffset = true;
    Vector3 velocity = Vector3.zero;

    // Use this for initialization
    void Start ()
    {
        initPos = transform.position;
        target = transform.position + offset;
    }
	
	// Update is called once per frame
	void Update ()
    {
        curTime += Time.deltaTime;
        transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, duration);

        if (curTime > duration)
        {
            curTime = 0;

            if (ToOffset)
            {
                target = initPos;
                ToOffset = false;
            }
            else
            {
                target = initPos + offset;
                ToOffset = true;
            }

        }
    }
}
