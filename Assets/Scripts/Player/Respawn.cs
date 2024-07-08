using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawn : MonoBehaviour
{
    public GameObject spawn;
    // Start is called before the first frame update
    void Start()
    {
        transform.position = spawn.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Spawn(Transform spawnTransform)
    {
        transform.position = spawnTransform.position;
        transform.eulerAngles = spawnTransform.eulerAngles;
    }
}
