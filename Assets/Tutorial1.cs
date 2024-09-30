using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial1 : MonoBehaviour
{
    public AudioSource voice;
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
        if (other.tag == "Player")
        {

            if (!gameObject.GetComponent<MeshRenderer>().enabled)
            {
                smoke.SetActive(true);
                Invoke("show", 1f);
            }
            
            Invoke("cooldown", 1.5f);
            Invoke("talk", 1.5f);
        }
    }
    void talk()
    {
        voice.Play();

    }
    
    void cooldown()
    {
        smoke.SetActive(false);
        
    }
    void show()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = true;
    }
    void disappear()
    {
        smoke.SetActive(true); 
        Invoke("cooldown", 1.5f);
        gameObject.SetActive(false);
    }
}

