using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnInteraction : MonoBehaviour
{
    public UnityEvent FocusEnterEvent;
    public UnityEvent FocusLostEvent;
    public UnityEvent ClickEvent;

    public bool isActive;

    public void Enable()
    {
        isActive = true;

        if (FocusLostEvent != null)
        {
            FocusLostEvent.Invoke();
        }
    }

    public void Disable()
    {
        isActive = false;

        if (FocusLostEvent != null)
        {
            FocusLostEvent.Invoke();
        }
    }

    public void OnClicked()
    {
        if (isActive == false) return;
        if (ClickEvent != null)
        {
            ClickEvent.Invoke();
        }
    }
    public void OnFocusEnter()
    {
        if (isActive == false) return;
        if (FocusEnterEvent != null)
        {
            FocusEnterEvent.Invoke();
        }
    }

    public void OnFocusExit()
    {
        if (isActive == false) return;
        if (FocusLostEvent != null)
        {
            FocusLostEvent.Invoke();
        }
    }
}
