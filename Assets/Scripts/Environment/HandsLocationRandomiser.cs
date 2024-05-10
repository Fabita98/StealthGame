using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HandsLocationRandomiser : MonoBehaviour
{
    [Header("Hands")]
    public int handsNumber;
    public GameObject handPrefab;
    HashSet<Vector3> positions;

    [Header("Parent Ref Collider")]
    public Bounds boxBounds;
    public GameObject colliderObj;
    public bool isRight = false;

    public static int instanceCount = 0;

    private void Awake()
    {
        if (instanceCount > 2)
        {
            Destroy(this);
            return;
        }

        instanceCount++;

        // Add a BoxCollider to this object
        BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.size = transform.parent.GetComponent<BoxCollider>().size * 0.8f;
        boxCollider.tag = "LeftOrRightWall";

        if (transform.TryGetComponent<Collider>(out var refCollider))
        {
            boxBounds = refCollider.bounds;
            colliderObj = refCollider.gameObject;
        }

        positions = new HashSet<Vector3>();

        while (positions.Count < handsNumber)
        {
            Vector3 newPos;
            do
            {
                newPos = new Vector3(
                    Random.Range(boxBounds.min.x, boxBounds.max.x),
                    Random.Range(boxBounds.min.y, boxBounds.max.y),
                    Random.Range(boxBounds.min.z, boxBounds.max.z)
                );
            }
            while (newPos == boxBounds.center);

            newPos = colliderObj.transform.InverseTransformPoint(newPos);
            positions.Add(newPos);
        }

        #region Set Hand parameters when spawned
        foreach (Vector3 pos in positions)
        {
            // Instantiate the hand at the origin, then set its local position
            GameObject handInstance = Instantiate(handPrefab, Vector3.zero, Quaternion.identity, colliderObj.transform);
            handInstance.transform.localPosition = new Vector3(-0.936f, pos.y, pos.z);
            //when instantiating a prefab in code and set its parent, Unity does not automatically adjust the scale of the instantiated GameObject
            handInstance.transform.localScale = new Vector3(2.28571415f, 0.111111119f, 0.0277777798f);

            Transform armature = handInstance.transform.Find("Armature");
            if (armature != null)
            {
                float randomX = Random.Range(-90, 90);
                armature.Rotate(randomX, 0, 0);
            }
        }
        #endregion
    }

    private void OnDestroy()
    {
        instanceCount--;
    }
}