using UnityEngine;
using UnityEngine.Serialization;

public class LevelConfig : MonoBehaviour
{
    public int levelId = 0;
    public GameObject levelGameObject;
    public int numberOfEnemies = 0;
    public Transform playerSpawnPoint;
    public bool hasTimeLimit = false;
    public float timeLimit = 0;
    public GameObject pathBlockingGameObject;
    public string objectiveDescription;
    public Sound[] sounds;
    public GameObject[] otherObjects;
}
