using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiritVision : MonoBehaviour
{
    // Start is called before the first frame update
    public bool eyeClosed;
    public float mana=10, manaSpeed=0.5f;
    List<GameObject> enemyinrange = new List<GameObject>();
    List<GameObject> enemyHighlighted = new List<GameObject>();
    bool isenable = false;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (eyeClosed && mana > 0)              //if ability is used decrease mana and invoke outline; disable outline when mana is over
        {
            mana -= Time.deltaTime * manaSpeed;
            mana = Mathf.Clamp(mana, -0.1f, 10);
            if (!isenable)
            {
                Invoke("enableVision", 1);
                isenable = true;
            }
        }
        else if (isenable) disableVision();
    }

    public void enableVision()
    {
        foreach (GameObject enemy in enemyinrange)
        {
            enemy.GetComponent<Outline>().enabled = true;
            enemyHighlighted.Add(enemy.gameObject);
        }
    }
    public void disableVision()
    {
        isenable = false;
        foreach (GameObject enemy in enemyHighlighted)
        {
            enemy.GetComponent<Outline>().enabled = false;
        }
    }

    private void OnTriggerStay(Collider other)      //add every enemy in collider to list
    {
        bool match = false;
        if (other.tag == "enemy")
        {
            foreach (GameObject enemy in enemyinrange)
            {
                if (other.gameObject.Equals(enemy)) { 
                    match = true;
                    
                }
            }

            if (!match)
            {
                enemyinrange.Add(other.gameObject);
                if (isenable && !other.gameObject.GetComponent<Outline>().enabled)   //exception handler
                    {
                        enemyHighlighted.Add(other.gameObject);
                        other.gameObject.GetComponent<Outline>().enabled = true;
                    }
            }
        }
    }
    private void OnTriggerExit(Collider other)      //remove enemy from list
    {
        bool match = false;
        if (other.tag == "enemy")
        {
            foreach (GameObject enemy in enemyinrange)
            {
                if (other.gameObject.Equals(enemy)) match = true;
            }

            if (match)
            {
                enemyinrange.Remove(other.gameObject);
                other.gameObject.GetComponent<Outline>().enabled = false;
                enemyHighlighted.Remove(other.gameObject);
            }
        }
    }
}
