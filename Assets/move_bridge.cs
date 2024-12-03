using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class move_bridge : MonoBehaviour
{

    public Animator an;
    public AudioSource BridgeSound;
    bool play, isLowered = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (BridgeSound.isPlaying == false && play && !isLowered) Invoke("playsound", 0.2f);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "LeftHand" || other.tag == "RightHand")
        {
            play = true;
            an.SetBool("isLowered", true);

        }
    }
    public void Finish()
    {
        //an.;
    }
    void playsound()
    {
        BridgeSound.Play();
        isLowered = true;
    }
}
