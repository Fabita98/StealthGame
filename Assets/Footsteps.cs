using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footsteps : MonoBehaviour
{
    float playerHorizontalInput;
    float playerVerticalInput;
    bool is_walking, is_playing = false;
    public AudioSource footsteps;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {


        if (OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).magnitude < 0.01f)
        {
            footsteps.Pause();
            is_playing = false;
        }
        else if (!is_playing)
        {
            footsteps.Play();
            Debug.Log("passi passi passi");
            is_playing = true;
        }
    }
}
