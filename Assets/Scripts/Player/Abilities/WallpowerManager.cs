using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallpowerManager : MonoBehaviour {
    public static WallpowerManager Instance { get; private set; }

    [Header("Blue power related variables")]
    public static bool isSpiritVisionAvailable = false;
    public float eyeClosedTreshold = .7f;
    public float eyeClosedTimer = 1f;
    public static bool isEnabled = false;
    public static List<GameObject> enemyinrange = new();
    public static List<GameObject> enemyHighlighted= new();
    bool firstInstant = true;
    bool status;
    private float eyeClosedFirstInstant;

    private void OnEnable() {
        Flower_animator_wallpower.OnBlueLotusPowerChanged += HandleBlueLotusPowerActivation;
    }

    private void OnDisable() {
        Flower_animator_wallpower.OnBlueLotusPowerChanged -= HandleBlueLotusPowerActivation;
    }
    public bool HandleBlueLotusPowerActivation(bool isActive) => isSpiritVisionAvailable = isActive;

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
        return isSpiritVisionAvailable;
    }

    private void BluePowerUsage() {
        if (CheckEyeClosed()) {
            DecreaseBluePowerCounter();
            StartCoroutine(EnableVisionCoroutine());
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

        if (eyeClosedL > eyeClosedTreshold || eyeClosedR > eyeClosedTreshold) {
            Debug.Log("Eyes closed coglione! ");
            if (firstInstant) {
                eyeClosedFirstInstant = Time.time;
                firstInstant = false;
            }

            if ((Time.time - eyeClosedFirstInstant > eyeClosedTimer)&&!firstInstant)
            {
                status = true;
            }
        } else {
            firstInstant = true;
            status = false;
        }

        return status;
    }

    private IEnumerator EnableVisionCoroutine() {

        if (!isEnabled) {
            enabled = true;
        }
        foreach (GameObject enemy in enemyinrange) {
            enemy.GetComponent<Outline>().enabled = true;
            enemyHighlighted.Add(enemy);
        }
        yield return new WaitForSeconds(SpiritVision.bluePowerTimer);
        
        DisableVision();
    }

    private void DisableVision() {
        SpiritVision.bluePowerTimer = 5;
        isEnabled = false;
        foreach (GameObject enemy in enemyHighlighted) {
            enemy.GetComponent<Outline>().enabled = false;
        }
    }
}