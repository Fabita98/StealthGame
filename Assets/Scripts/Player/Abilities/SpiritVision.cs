using UnityEngine;

public class SpiritVision : MonoBehaviour
{
    public static float mana = 10, manaSpeed = 0.5f;

    private void OnTriggerStay(Collider other)      
    {
        bool match = false;
        if (other.CompareTag("Monk") || other.CompareTag("Shadow"))
        {
            //add every enemy in collider to list
            foreach (GameObject enemy in WallpowerManager.enemyinrange)
            {
                if (other.gameObject.Equals(enemy)) { 
                    match = true;                    
                }
            }

            if (!match)
            {
                WallpowerManager.enemyinrange.Add(other.gameObject);
                //exception handler
                if (WallpowerManager.isEnabled && !other.gameObject.GetComponent<Outline>().enabled) {
                    WallpowerManager.enemyHighlighted.Add(other.gameObject);
                    other.gameObject.GetComponent<Outline>().enabled = true;
                }
            }
        }
    }

    //remove enemy from list
    private void OnTriggerExit(Collider other)      
    {
        bool match = false;
        if (other.CompareTag("Monk") || other.CompareTag("Shadow"))
        {
            foreach (GameObject enemy in WallpowerManager.enemyinrange)
            {
                if (other.gameObject.Equals(enemy)) match = true;
            }

            if (match)
            {
                WallpowerManager.enemyinrange.Remove(other.gameObject);
                other.gameObject.GetComponent<Outline>().enabled = false;
                WallpowerManager.enemyHighlighted.Remove(other.gameObject);
            }
        }
    }
}