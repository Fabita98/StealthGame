using System.Collections.Generic;
using UnityEngine;

public class ShrinkingBuilding : MonoBehaviour
{
    [Header("Building properties")]
    public GameObject sideWalls;
    public GameObject LeftOrRightWall;
    public GameObject door;
    public LayerMask assignedLayer;
    public float shrinkSpeed = 0;

    [Header("Player Data")]
    public float stressLevel = 0;

    private void Awake()
    {
        assignedLayer = gameObject.layer;
        gameObject.layer = LayerMask.GetMask("House");
        sideWalls = FindChildWithTag(gameObject, "SideWalls");
        LeftOrRightWall = FindChildWithTag(gameObject, "LeftOrRightWall");
        door = FindChildWithTag(gameObject, "Door");
    }

    private void FixedUpdate()
    {
        stressLevel += Time.fixedDeltaTime / 100;
        shrinkSpeed = stressLevel/100;
        shrinkSpeed = Mathf.Clamp(shrinkSpeed, 0, 1);
        stressLevel = Mathf.Clamp(stressLevel, 0, 1);
        if (stressLevel > 0)
        {
            ShrinkSideWalls(sideWalls);
        }
    }

    public GameObject FindChildWithTag(GameObject parent, string tag)
    {
        Queue<Transform> queue = new();
        queue.Enqueue(parent.transform);

        while (queue.Count > 0)
        {
            Transform current = queue.Dequeue();
            if (current.CompareTag(tag))
            {
                return current.gameObject;
            }

            foreach (Transform child in current)
            {
                queue.Enqueue(child);
            }
        }

        return null;
    }

    private void ShrinkSideWalls(GameObject obj)
    {
        if (obj.CompareTag("SideWalls"))
        {
            foreach (Transform child in obj.transform)
            {
                if (child.CompareTag("LeftOrRightWall"))
                {
                    if (child.localPosition.z > 0f)
                    {
                        child.localPosition -= new Vector3(0, 0, shrinkSpeed/2);
                    }
                    else
                    {
                        child.localPosition += new Vector3(0, 0, shrinkSpeed/2);
                    }
                }
            }            
        } else {
            Debug.LogError("SideWalls group not found");
            return;
        }
    }
}