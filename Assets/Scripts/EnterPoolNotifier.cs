using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterPoolNotifier : MonoBehaviour
{
    [Header("Notifies CheckerManager for TriggerEnter/Exit")]
    public CheckerManager mngr;

    private void OnTriggerEnter(Collider other)
    {
       // if (other.tag == "Player")
       //     CheckerManager.Instance.playerIsInPool = true;
    }
    private void OnTriggerExit(Collider other)
    {
       // if (other.tag == "Player")
       //     CheckerManager.Instance.playerIsInPool = false;
    }
}
