using UnityEngine;

public class GameOver : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("LeftOrRightWall") || other.gameObject.CompareTag("SideWalls"))
        {
            Debug.Log("Game Over");
        }
    }
}