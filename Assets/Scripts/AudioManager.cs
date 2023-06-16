using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {
    
    public AudioSource audioEffects = null;

    [System.Serializable]
    public struct SoundClip
    {
        public string name;
        public AudioClip clip;
    }

    public SoundClip[] clipArray;

    public float Volume
    {
        get { return AudioListener.volume; }
        set { AudioListener.volume = value; }
    }

    
    IEnumerator fadeout(float duration)
    {
        float timePassed = 0;

        while (timePassed < duration)
        {
            timePassed += Time.deltaTime;
            audioEffects.volume = 1 - (timePassed / duration);
            yield return null;
        }
    }

    public void FadeoutEffects(float duration)
    {
        StartCoroutine("fadeout", duration);
    }

    //Stops any Invokes and all sounds, called at Start of each Scene
    public void ResetSound()
    {
        CancelInvoke();
        StopCoroutine("fadeout");
        //Stop normal clips
        audioEffects.Stop();
        //Stop PlayOneShot clips
        audioEffects.enabled = false;
        audioEffects.enabled = true;
        audioEffects.volume = 1;
    }

    public void playSound(string clipname)
    {
        ResetSound();

        foreach (SoundClip sc in clipArray )
        {
            if (sc.name == clipname && sc.clip != null)
            {
                audioEffects.PlayOneShot(sc.clip);
                break;
            }
        }
    }

    public static AudioManager Instance
    {
        get { return instance; }
    }

    private static AudioManager instance = null;

    void Awake()
    {
        //if (instance)
        //{
        //    DestroyImmediate(gameObject);
        //}
        //else
        //{
            instance = this;
        //    DontDestroyOnLoad(gameObject);
        //}
    }
}
