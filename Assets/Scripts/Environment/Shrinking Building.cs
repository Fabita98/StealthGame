using System.Collections.Generic;
using UnityEngine;

public class ShrinkingBuilding : MonoBehaviour
{
    [Header("Building properties")]
    public GameObject sideWall;
    public GameObject LeftOrRightWall;
    private Vector3[] initialSideWallsPositions;
    //public GameObject door;
    public LayerMask assignedLayer;
    public bool isRight;

    // All shared data between more elements in the game are static
    private static float stressLevel = 0.5f;
    private static GameObject[] sideWalls;
    public static float shrinkSpeed = 0;
    public static bool gameOver;

    public static int instanceCount = 0;
    public static ShrinkingBuilding LocalInstance;

    private void Awake()
    {
        if (instanceCount >= 2)
        {
            Destroy(this);
            return;
        }
        else
        {
            LocalInstance = this;
            instanceCount++;
        }

        sideWalls ??= new GameObject[2];
        gameObject.layer = LayerMask.GetMask("House");
        assignedLayer = gameObject.layer;
    }

    private void Start()
    {
        SetWallsSettings();
    }

    private void FixedUpdate()
    {
        stressLevel += Time.fixedDeltaTime / 100;
        shrinkSpeed = stressLevel/100;
        shrinkSpeed = Mathf.Clamp(shrinkSpeed, 0, 1);
        stressLevel = Mathf.Clamp(stressLevel, 0, 1);
        if (stressLevel > 0)
        {
            ShrinkSideWalls(sideWall);
        }
    }

    // Try to find the child object with the given tag through BFS traversal
    public bool TryToFindChildWithTag(GameObject parent, string tag, out GameObject result)
    {
        Queue<Transform> queue = new();
        queue.Enqueue(parent.transform);

        while (queue.Count > 0)
        {
            Transform current = queue.Dequeue();
            if (current.CompareTag(tag))
            {
                result = current.gameObject;
                return true;
            }

            foreach (Transform child in current)
            {
                queue.Enqueue(child);
            }
        }

        result = null;
        return false;
    }

    private void ShrinkSideWalls(GameObject obj)
    {
        if (obj.CompareTag("SideWalls"))
        {
            obj.transform.position += isRight ? new Vector3(-shrinkSpeed / 2, 0, 0) : new Vector3(shrinkSpeed / 2, 0, 0);
            if (gameOver)
            {
                GameObject[] sideWallsObjects = GameObject.FindGameObjectsWithTag("SideWalls");
                for (int i = 0; i < sideWallsObjects.Length; i++)
                {
                    sideWallsObjects[i].transform.position = LocalInstance.initialSideWallsPositions[i];
                }
                gameOver = false;
            }
        } else {
            Debug.LogError("SideWalls group not found");
            return;
        }
    }

    private void SetWallsSettings() {
        if (TryToFindChildWithTag(gameObject, "SideWalls", out GameObject foundWall))
        {
            sideWall = foundWall;
            sideWalls[isRight ? 0 : 1] = sideWall;
        }
        if (TryToFindChildWithTag(gameObject, "LeftOrRightWall", out GameObject foundLRWall)) LeftOrRightWall = foundLRWall;
        //door = FindChildWithTag(gameObject, "Door");

        // Save the initial position of side walls
        if (sideWall != null)
        {
            LocalInstance.initialSideWallsPositions ??= new Vector3[2];
            LocalInstance.initialSideWallsPositions[isRight ? 0 : 1] = sideWall.transform.position;
        }
    }
}