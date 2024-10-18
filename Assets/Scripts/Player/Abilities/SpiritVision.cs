using System.Collections.Generic;
using UnityEngine;

public class SpiritVision : MonoBehaviour
{
    // Start is called before the first frame update
    public bool activate = WallpowerManager.isSpiritVisionActive;
    public float mana=10, manaSpeed=0.5f;
    List<GameObject> enemyinrange = new List<GameObject>();
    List<GameObject> enemyHighlighted = new List<GameObject>();
    bool isEnabled = false;
    private float lastActivateTime;

    // Update is called once per frame
    void Update()
    {
        //if ability is used decrease mana and invoke outline; disable outline when mana is over
        if (activate && mana > 0)              
        {
            mana -= Time.deltaTime * manaSpeed;
            mana = Mathf.Clamp(mana, -0.1f, 30);
            if (!isEnabled)
            {
                Invoke("enableVision", 1);
                isEnabled = true;
            }
        }
        else if (isEnabled) disableVision();
        if (activate)
        {
            lastActivateTime = Time.time;
            if (Time.time - lastActivateTime > 8f)
            {
                isEnabled = false;
                activate = false;
            }
        }
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
        isEnabled = false;
        foreach (GameObject enemy in enemyHighlighted)
        {
            enemy.GetComponent<Outline>().enabled = false;
        }
    }

    private void OnTriggerStay(Collider other)      
    {
        bool match = false;
        if (other.tag == "Monk" || other.tag =="Shadow")
        {
            //add every enemy in collider to list
            foreach (GameObject enemy in enemyinrange)
            {
                if (other.gameObject.Equals(enemy)) { 
                    match = true;
                    
                }
            }

            if (!match)
            {
                enemyinrange.Add(other.gameObject);
                if (isEnabled && !other.gameObject.GetComponent<Outline>().enabled)   //exception handler
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
        if (other.tag == "Monk" || other.tag == "Shadow")
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
