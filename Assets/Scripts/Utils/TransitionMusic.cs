using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransitionMusic : MonoBehaviour
{
    public AudioSource audio1, audio2, chaseMusic;
    private float duration;
    private float target;
    bool trigger = false;
    public bool initial;
    bool audio1on, audio2on;

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

        if (!trigger&&other.tag=="RightHand") { fade(); trigger = true; Debug.Log("cambia canzone"); }    
    }
   
    public void fade()
    {
        duration = 4;
        target = 0;
        StartCoroutine(FadeAudioSource.StartFade(audio1, duration, target));
        audio2.PlayDelayed(4);
        duration = 4;
        target = 0.1f;
        StartCoroutine(FadeAudioSource.StartFade(audio2, duration, target));
    }

    public void startChase()
    {
        
        duration = .4f;
        target = 0;
        if (audio1.volume>0.01f)
        {          
            StartCoroutine(FadeAudioSource.StartFade(audio1, duration, target));
            audio1on = true;
            audio2on = false;
 
        } else if (audio2.volume > 0.01f) {

            StartCoroutine(FadeAudioSource.StartFade(audio2, duration, target));
            audio2on = true;
            audio1on = false;

        }
        duration = .4f;
        target = .6f;
        chaseMusic.Play();
        StartCoroutine(FadeAudioSource.StartFade(chaseMusic, duration, target));
    }

    public void stopChase()
    {
        duration = .4f;
        target = 0;
        StartCoroutine(FadeAudioSource.StartFade(chaseMusic, duration, target));
        duration = 2f;
        target = 0.1f;       
        if (audio1on) StartCoroutine(FadeAudioSource.StartFade(audio1, duration, target));
        if (audio2on) StartCoroutine(FadeAudioSource.StartFade(audio2, duration, target));
    }
}


