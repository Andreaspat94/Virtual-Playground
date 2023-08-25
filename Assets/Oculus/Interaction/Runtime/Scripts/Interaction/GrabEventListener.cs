using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//This is attached to the checker Manager
public class GrabEventListener : MonoBehaviour
{
    public GrabEvent grabEvent;
    public UnityEvent onEventTrigger;

    void OnEnable()
    {
        grabEvent.AddListener(this);
    }

    void OnDisable()
    {
        grabEvent.RemoveListener(this);
    }

    public void OnEventTriggered()
    {
        onEventTrigger.Invoke();
    }
}
