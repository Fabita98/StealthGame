using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class closeDoor : MonoBehaviour
{
    public Animator an1, an2;
    public AudioSource DoorSound1, DoorSound2;
    bool open = true;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnTriggerEnter(Collider other)
    {
        if (open)
        {
            if (other.tag == "LeftHand" || other.tag == "RightHand")
                {
                    an1.SetTrigger("open");
                    an2.SetTrigger("open");
                    Invoke("playsound", 0.5f);
                    open = false;
                }
        }
        
    }
    public void Finish()
    {
        //an.;
    }
    void playsound()
    {
        DoorSound1.Play();
        DoorSound2.Play();
        an1.SetTrigger("close");
        an2.SetTrigger("close");
    }
}
