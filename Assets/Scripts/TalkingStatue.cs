using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkingStatue : MonoBehaviour
{
    public AudioSource voice, popSound;
    bool firstTime = true;
    public bool finished = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public GameObject smoke;
    // Update is called once per frame
    void Update()
    {
        LookAtPlayer();
    }

    private void LookAtPlayer()
    {
        Transform playerTransform = MyPlayerController.Instance.transform;
        if (playerTransform != null)
        {
            Vector3 playerPosition = playerTransform.position;
            Vector3 directionToPlayer = playerPosition - transform.position;
            directionToPlayer.y = 0;
            transform.rotation = Quaternion.LookRotation(directionToPlayer, Vector3.up);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "RightHand" && firstTime)
        {

            if (!gameObject.GetComponent<MeshRenderer>().enabled)
            {
                smoke.SetActive(true);
                popSound.Play();
                Invoke("show", 1f);
                Invoke("disappear", voice.clip.length+2f);
            }
           
            Invoke("talk", 1.5f);
            firstTime = false;
        }
    }
    void talk()
    {
        voice.Play();

    }
    void disableSmoke()
    {
        smoke.SetActive(false);
    }
    void smoke_out()
    {
        smoke.SetActive(false);
        gameObject.GetComponent<MeshRenderer>().enabled = false;
    }
    void show()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = true;
        Invoke("disableSmoke",2);
        
        
    }
    void disappear()
    {
        smoke.SetActive(true);
        popSound.Play();
        finished = true;
        Invoke("smoke_out", 1.5f);
    }
}

