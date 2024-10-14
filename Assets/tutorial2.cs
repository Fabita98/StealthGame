using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tutorial2 : MonoBehaviour
{
    public AudioSource voice;
    bool talk = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    
    // Update is called once per frame
    void Update()
    {
        LookAtPlayer();
        if (gameObject.GetComponent<MeshRenderer>().enabled && !talk)
        {
            
            Invoke("StartTalk",1);
            
        }
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
    void StartTalk()
    {
        voice.Play();
        talk = true;
    }
}
