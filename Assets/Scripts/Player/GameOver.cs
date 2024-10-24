using UnityEngine;

public class GameOver : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        if (other.transform.CompareTag("LeftOrRightWall") || other.transform.CompareTag("SideWalls"))
        {
            ShrinkingBuilding.gameOver = true;
            Debug.Log("Game Over");
        }
    }
}