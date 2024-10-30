using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum AudioTypes
{
    Soundtrack1,
    Soundtrack2,
    Soundtrack3,
    ChaseSound
}
public class TransitionMusic : MonoBehaviour
{
    public AudioSource audio1, audio2, chaseMusic;
    private float duration;
    private float target;
    bool trigger = false;
    public bool initial;
    bool audio1on, audio2on;
    private AudioTypes currentAudio;

    // Start is called before the first frame update
    void Start()
    {
        if (initial)
        {
            if (GameController.Instance.LevelsController.GetCurrentLevel().LevelNum == 1)
            {
                audio1.Play();
                currentAudio = AudioTypes.Soundtrack1;
                target = 0.13f;
                duration = 3f;
                StartCoroutine(FadeAudioSource.StartFade(audio1, duration, target));   
            }
            else if (GameController.Instance.LevelsController.GetCurrentLevel().LevelNum == 2 ||
                     GameController.Instance.LevelsController.GetCurrentLevel().LevelNum == 3)
            {
                audio2.Play();
                currentAudio = AudioTypes.Soundtrack2;
                duration = 4;
                target = 0.1f;
                StartCoroutine(FadeAudioSource.StartFade(audio2, duration, target));
            }
        }
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (!trigger && other.tag == "RightHand")
        {
            fade(); 
            trigger = true; 
            Debug.Log("cambia canzone");
        }    
    }
   
    public void fade()
    {
        duration = 4;
        target = 0;
        StartCoroutine(FadeAudioSource.StartFade(audio1, duration, target));
        audio2.PlayDelayed(4);
        currentAudio = AudioTypes.Soundtrack2;
        duration = 4;
        target = 0.1f;
        StartCoroutine(FadeAudioSource.StartFade(audio2, duration, target));
    }

    public void startChase()
    {
        if(currentAudio == AudioTypes.ChaseSound)
            return;
        duration = .4f;
        target = 0;
        if (currentAudio == AudioTypes.Soundtrack1)
        {          
            StartCoroutine(FadeAudioSource.StartFade(audio1, duration, target));
            audio1on = true;
            audio2on = false;
 
        } 
        else if (currentAudio == AudioTypes.Soundtrack2) {

            StartCoroutine(FadeAudioSource.StartFade(audio2, duration, target));
            audio2on = true;
            audio1on = false;

        } 
        else
        {
            StartCoroutine(FadeAudioSource.StartFade(audio2, duration, target));
            audio2on = true;
            audio1on = false;
        }
        duration = .4f;
        target = .6f;
        chaseMusic.Play();
        currentAudio = AudioTypes.ChaseSound;
        StartCoroutine(FadeAudioSource.StartFade(chaseMusic, duration, target));
    }

    public void stopChase()
    {
        if (MyPlayerController.Instance.NumberOfEnemiesChasing == 0 && currentAudio == AudioTypes.ChaseSound)
        {
            duration = .4f;
            target = 0;
            StartCoroutine(FadeAudioSource.StartFade(chaseMusic, duration, target));
            duration = 2f;
            target = 0.1f;
            if (audio1on)
            {
                currentAudio = AudioTypes.Soundtrack1;
                StartCoroutine(FadeAudioSource.StartFade(audio1, duration, target));
            }

            if (audio2on)
            {
                currentAudio = AudioTypes.Soundtrack2;
                StartCoroutine(FadeAudioSource.StartFade(audio2, duration, target));
            }   
        }
    }
}


