using System.Collections.Generic;
using UnityEngine;

public class WallpowerManager : MonoBehaviour {
    public static WallpowerManager Instance { get; private set; }

    [Header("Blue power related variables")]
    public static bool isSpiritVisionActive = false;
    public const byte eyeClosingRequiredTime = 2;
    public static bool isEnabled = false;
    public static readonly List<GameObject> enemyinrange = new();
    public static readonly List<GameObject> enemyHighlighted = new();
    private float lastPowerEnabledTime;
    private const byte bluePowerCooldown = 8;

    private void OnEnable() {
        Flower_animator_wallpower.OnBlueLotusPowerChanged += HandleBlueLotusPowerActivation;
    }

    private void OnDisable() {
        Flower_animator_wallpower.OnBlueLotusPowerChanged -= HandleBlueLotusPowerActivation;
    }
    public bool HandleBlueLotusPowerActivation(bool isActive) => isSpiritVisionActive = isActive;

    private void Awake() { 
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else if (Instance != this) {
            Destroy(this);
        }
    }

    private void Start() {
        int currentBlueLotusCounterValue = PlayerPrefsManager.GetInt(PlayerPrefsKeys.BlueLotus, 0);
        if (currentBlueLotusCounterValue > 0) {
            Flower_animator_wallpower.TriggerOnBlueLotusPowerChangeEvent(true);
        }
        else Flower_animator_wallpower.TriggerOnBlueLotusPowerChangeEvent(false);
    }

    void Update() {
        if (BluePowerActivationCheck()) {
            BluePowerUsage();
        }
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

    private bool BluePowerActivationCheck() {
        return isSpiritVisionActive;
    }

    private void BluePowerUsage() {
        if (CheckEyeClosed()) {
            //if ability is used decrease mana and invoke outline; disable outline when mana is over
            if (isSpiritVisionActive && SpiritVision.mana > 0) {
                SpiritVision.mana -= Time.deltaTime * SpiritVision.manaSpeed;
                SpiritVision.mana = Mathf.Clamp(SpiritVision.mana, -0.1f, 30);
                if (!isEnabled) {
                    Invoke("enableVision", 1);
                    isEnabled = true;
                    DecreaseBluePowerCounter();
                }
            }
            else if (isEnabled) DisableVision();
            if (isSpiritVisionActive) {
                lastPowerEnabledTime = Time.time;
                if (Time.time - lastPowerEnabledTime > bluePowerCooldown) {
                    isEnabled = false;
                    isSpiritVisionActive = false;
                }
            }
        }
    }

    private bool CheckEyeClosed() {
        float eyeClosedL = 0;
        float eyeClosedR = 0;

        bool isValidData = DataTracker.ovrexpr.ValidExpressions;

        if (DataTracker.ovrexpr.ValidExpressions) {
            eyeClosedL = DataTracker.ovrexpr[OVRFaceExpressions.FaceExpression.EyesClosedL];
            eyeClosedR = DataTracker.ovrexpr[OVRFaceExpressions.FaceExpression.EyesClosedR];

            //If blendshape is active, sum blendshape offset to recover true eyeclosed value
            if (DataTracker.ovrexpr.EyeFollowingBlendshapesValid) {
                var blendShapeOffset = Mathf.Min(DataTracker.ovrexpr[OVRFaceExpressions.FaceExpression.EyesLookDownL], DataTracker.ovrexpr[OVRFaceExpressions.FaceExpression.EyesLookDownR]);
                eyeClosedL += blendShapeOffset;
                eyeClosedR += blendShapeOffset;
            }
        }
        return eyeClosedL > eyeClosingRequiredTime && eyeClosedR > eyeClosingRequiredTime;
    }

    private void EnableVision() {
        foreach (GameObject enemy in enemyinrange) {
            enemy.GetComponent<Outline>().enabled = true;
            enemyHighlighted.Add(enemy);
        }
    }

    private void DisableVision() {
        isEnabled = false;
        foreach (GameObject enemy in enemyHighlighted) {
            enemy.GetComponent<Outline>().enabled = false;
        }
    }
}