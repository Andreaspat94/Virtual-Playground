using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;

public class HandSwitcher : Singleton<HandSwitcher>
{
    public GameObject hands;

	public void ShowHands()
    {
        hands.SetActive(true);
    }
    public void HideHands()
    {
        hands.SetActive(false);
    }
}
