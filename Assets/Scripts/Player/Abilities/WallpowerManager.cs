using UnityEngine;

public class WallpowerManager : MonoBehaviour
{
    public static WallpowerManager Instance { get; private set; }
    [Header("Blue power variables")]
    public static bool isSpiritVisionActive = false;
    public static float eyeClosingRequiredTime = 2f;

    private void OnEnable() {
        Flower_animator_wallpower.OnBlueLotusPowerChanged += HandleBlueLotusPowerActivation;
    }

    private void OnDisable() {
        Flower_animator_wallpower.OnBlueLotusPowerChanged -= HandleBlueLotusPowerActivation;
    }

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

    void Update()
    {

    }

    public bool HandleBlueLotusPowerActivation(bool isActive) => isSpiritVisionActive = isActive;

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
        if (BluePowerActivationCheck()) {
            if (CheckEyeClosed()) { 
                //to add the blue power effective usage: ask Max
            
            
            }
        } else return;
    }

    private bool CheckEyeClosed() {
        float eyeClosedL = 0;
        float eyeClosedR = 0;

        var isValidData = DataTracker.ovrexpr.ValidExpressions;

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
}