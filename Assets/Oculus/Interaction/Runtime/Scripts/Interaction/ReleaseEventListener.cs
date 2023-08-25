using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ReleaseEventListener : MonoBehaviour
{
  public ReleaseEvent releaseEvent;
  public UnityEvent onEventTrigger;

  void OnEnable()
  {
    releaseEvent.AddListener(this);
  }

  void OnDisable()
  {
    releaseEvent.RemoveListener(this);
  }

  public void OnEventTriggered()
  {
    onEventTrigger.Invoke();
  }
}
