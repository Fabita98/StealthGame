using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class open_door : MonoBehaviour    
{
    public Animator an;
    public AudioSource DoorSound;
    bool play = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (DoorSound.isPlaying == false && play)  Invoke("playsound", 0.4f); 
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag=="LeftHand"|| other.tag == "RightHand")
        {
            play = true;
            an.SetTrigger("open");

        }
    }
    public void Finish()
    {
        //an.;
    }
    void playsound()
    {
        DoorSound.Play(); play = false;
    }
}
