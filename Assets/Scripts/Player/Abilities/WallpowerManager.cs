using UnityEngine;

public class WallpowerManager : MonoBehaviour
{
    public static WallpowerManager Instance { get; private set; }
    public static bool isSpiritVisionActive = false;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else if (Instance != this) {
            Destroy(this);
        }
    }

    public bool HandlePinkLotusPowerActivation(bool isActive) => isSpiritVisionActive = isActive;

    void Update()
    {
        
    }

    private void DecreaseBluePowerCounter() {
        int currentBlueLotusCounterValue = PlayerPrefsManager.GetInt(PlayerPrefsKeys.BlueLotus, 0);
        if (currentBlueLotusCounterValue > 0) {
            PlayerPrefsManager.SetInt(PlayerPrefsKeys.BlueLotus, currentBlueLotusCounterValue - 1);
            Debug.Log("PinkLotus counter value: " + currentBlueLotusCounterValue);
            UIController.Instance.AbilitiesUI.SetAbilitiesCount();
            Flower_animator_wallpower.TriggerOnBlueLotusPowerChangeEvent(true);
        }
        else Flower_animator_wallpower.TriggerOnBlueLotusPowerChangeEvent(false);
    }
}