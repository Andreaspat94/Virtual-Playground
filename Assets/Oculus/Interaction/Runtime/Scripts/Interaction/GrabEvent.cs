using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GrabEvent", menuName = "Virtual-Playground/GrabEvent", order = 0)]
public class GrabEvent : ScriptableObject {
    
    List<GrabEventListener> listeners = new List<GrabEventListener>();

    public void TriggerEvent()
    {
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            listeners[i].OnEventTriggered();
        }
    }

    public void AddListener(GrabEventListener listener)
    {
        listeners.Add(listener);
    } 

    public void RemoveListener(GrabEventListener listener)
    {
        listeners.Remove(listener);
    }
}

