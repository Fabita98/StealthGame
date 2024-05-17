using System.Collections.Generic;
using UnityEngine;

public class HandsLocationRandomiser : MonoBehaviour
{
    [Header("Hands")]
    public int handsNumber;
    public GameObject handPrefab;

    [Header("Hands PosX for R/L walls")]
    public readonly float handPosXRight = -2.499f;
    public readonly float handPosXLeft = -0.936f;

    [Header("Ref Collider")]
    private Bounds boxBounds;
    public float resizeFactor = 0.8f;
    public readonly float deltaX = 0.7f;
    public GameObject colliderObj;    
    public bool isRight;
    BoxCollider newBoxCollider;

    public static int instanceCount = 0;

    private void Awake()
    {
        if (instanceCount >= 2)
        {
            Destroy(this);
            return;
        }

        instanceCount++;

        CreateNewRefBoxCollider();
        #region New Collider creation explained
        //If the new collider is successfully created, then proceed instantiating hands and
        //enlarging the box collider size to cover the hands for the game over detection
        #endregion
        if (transform.TryGetComponent<BoxCollider>(out var refBoxCollider))
        {
            boxBounds = refBoxCollider.bounds;
            colliderObj = refBoxCollider.gameObject;
            InstantiateHands();
            EnlargeBoxColliderXSize();
        }
    }

    private void CreateNewRefBoxCollider()
    {
        if (transform.parent.TryGetComponent<BoxCollider>(out var parentBoxCollider))
        {
            newBoxCollider = gameObject.AddComponent<BoxCollider>();
            newBoxCollider.size = parentBoxCollider.size * resizeFactor;
            newBoxCollider.tag = "LeftOrRightWall";
            // Required to detect player collision for game over
            newBoxCollider.isTrigger = true;
        }
    }

    private void InstantiateHands()
    {
        HashSet<Vector3> positions = new();

        while (positions.Count < handsNumber)
        {
            Vector3 newPos;
            do
            {
                newPos = new Vector3(
                    //Random.Range(boxBounds.min.x, boxBounds.max.x),
                    0f,
                    Random.Range(boxBounds.min.y, boxBounds.max.y),
                    Random.Range(boxBounds.min.z, boxBounds.max.z)
                );
            }
            while (newPos == boxBounds.center);

            newPos = colliderObj.transform.InverseTransformPoint(newPos);
            positions.Add(newPos);
        }

        foreach (Vector3 pos in positions)
        {
            #region Hands spawn explained
            // Hands on left/right wall need different position and rotation to be set manually:
            // Unity doesn't automatically set them correctly when instanting them
            #endregion
            GameObject handInstance = Instantiate(handPrefab, Vector3.zero, Quaternion.identity, colliderObj.transform);
            handInstance.transform.localPosition = isRight ? new Vector3(handPosXRight, pos.y, pos.z) : new Vector3(handPosXLeft, pos.y, pos.z);
            handInstance.transform.localScale = new Vector3(2.28571415f, 0.111111119f, 0.0277777798f);

            Transform armature = handInstance.transform.Find("Armature");
            if (armature != null)
            {
                if (isRight)
                {
                    armature.transform.localPosition = new Vector2(1.52456f, 0f);
                    armature.Rotate(0, -179.363f, 0);
                }
                float randomX = Random.Range(-90, 90);
                armature.Rotate(randomX, 0, 0);
            }
        }
    }

    #region L/R Walls Collider resize 
    private void EnlargeBoxColliderXSize()
    {
        Vector3 newSize = newBoxCollider.size;
        newSize.x += deltaX;
        newBoxCollider.size = newSize;

        Vector3 newCenter = newBoxCollider.center;
        newCenter.x += isRight ? -deltaX / 2 : deltaX / 2;
        newBoxCollider.center = newCenter;
    }
    #endregion

    private void OnDestroy()
    {
        instanceCount--;
    }
}