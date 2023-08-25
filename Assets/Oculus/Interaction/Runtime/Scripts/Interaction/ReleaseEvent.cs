using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ReleaseEvent", menuName = "Virtual-Playground/ReleaseEvent", order = 1)]
public class ReleaseEvent : ScriptableObject 
{
    List<ReleaseEventListener> listeners = new List<ReleaseEventListener>()    ;

    public void TriggerEvent()
    {
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            listeners[i].OnEventTriggered();
        }
    }

    public void AddListener(ReleaseEventListener listener)
    {
        listeners.Add(listener);
    }

    public void RemoveListener(ReleaseEventListener listener)
    {
        listeners.Remove(listener);
    }
}

