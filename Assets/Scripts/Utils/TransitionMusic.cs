using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransitionMusic : MonoBehaviour
{
    public AudioSource audio1, audio2;
    private float duration;
    private float target;
    bool trigger = false;
    public bool initial;

    // Start is called before the first frame update
    void Start()
    {
        if (initial)
        {
            audio1.Play();
            target = 0.13f;
            duration = 3f;
            StartCoroutine(FadeAudioSource.StartFade(audio1, duration, target));
        }
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {

        if (!trigger&&other.tag=="RightHand") { fade1(); trigger = true; Debug.Log("cambia canzone"); }    
    }
   
    public void fade1()
    {
        duration = 4;
        target = 0;
        StartCoroutine(FadeAudioSource.StartFade(audio1, duration, target));
        audio2.PlayDelayed(4);
        duration = 4;
        target = 0.1f;
        StartCoroutine(FadeAudioSource.StartFade(audio2, duration, target));
    }

}
