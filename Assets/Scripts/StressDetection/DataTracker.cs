using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Labs.SuperScience;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
// using Utils;

[RequireComponent(typeof(Timestamp))]
// [RequireComponent(typeof(OVRFaceExpressions))]
public class DataTracker : MonoBehaviour {
	//Region to manage Editor behaviour

	#region GUI

	[CustomEditor(typeof(DataTracker))]
	public class DataTrackerGUI : Editor {
		private DataTracker trackerClass;
		private OVRFaceExpressions ovrexpr;

		public void OnEnable() {
			trackerClass = (DataTracker) target;
			ovrexpr = trackerClass.GetComponent<OVRFaceExpressions>();
		}

		public override void OnInspectorGUI() {
			//base.OnInspectorGUI();

			if (trackerClass == null) {
				return;
			}

			trackerClass.collectDataOnStart = EditorGUILayout.Toggle("Collect Data On Start", trackerClass.collectDataOnStart);

			trackerClass.takeTime = EditorGUILayout.Toggle("Take Time", trackerClass.takeTime);

			if (trackerClass.takeTime) {
				EditorGUI.indentLevel++;
				trackerClass.takeStartCycleTime = EditorGUILayout.Toggle("Take Start Cycle Time", trackerClass.takeStartCycleTime);
				trackerClass.takeStartFixedUpdateTime = EditorGUILayout.Toggle("Take Start FixedUpdate Time", trackerClass.takeStartFixedUpdateTime);
				trackerClass.takeTimeDiff = EditorGUILayout.Toggle("Take Fixed Update Time Diff", trackerClass.takeTimeDiff);
				trackerClass.takeUnityTime = EditorGUILayout.Toggle("Take Unity Time", trackerClass.takeUnityTime);
				trackerClass.takeUnityTimeDiff = EditorGUILayout.Toggle("Take Unity Time Diff", trackerClass.takeUnityTimeDiff);
				trackerClass.takeEndFixedUpdateTime = EditorGUILayout.Toggle("Take End FixedUpdate Time", trackerClass.takeEndFixedUpdateTime);
				EditorGUI.indentLevel--;
			} else {
				trackerClass.takeStartCycleTime = false;
				trackerClass.takeStartFixedUpdateTime = false;
				trackerClass.takeTimeDiff = false;
				trackerClass.takeUnityTime = false;
				trackerClass.takeUnityTimeDiff = false;
				trackerClass.takeEndFixedUpdateTime = false;
			}


			if (!FindObjectOfType<OVRCameraRig>()) {
				EditorGUILayout.HelpBox("Missing OVRCameraRig in the scene", MessageType.Warning);

				trackerClass.takeEyeData = false;
				trackerClass.takeFaceData = false;
				trackerClass.takeMovementData = false;

				if (trackerClass.takeEyeHead || trackerClass.takeEyeTracking || trackerClass.takeEyeWorld) {
					trackerClass.takeEyeHead = false;
					trackerClass.takeEyeTracking = false;
					trackerClass.takeEyeWorld = false;
					trackerClass.RemoveUnusedEyes();
				}

				ovrexpr.enabled = false;
			} else {
				trackerClass.takeEyeData = EditorGUILayout.Toggle("Take Eye Data", trackerClass.takeEyeData);

				if (trackerClass.takeEyeData) {
					EditorGUI.indentLevel++;
					trackerClass.takeEyeHead = EditorGUILayout.Toggle("Take Eye Head", trackerClass.takeEyeHead);

					trackerClass.takeEyeTracking = EditorGUILayout.Toggle("Take Eye Tracking", trackerClass.takeEyeTracking);

					trackerClass.takeEyeWorld = EditorGUILayout.Toggle("Take Eye World", trackerClass.takeEyeWorld);
					if (trackerClass.takeEyeWorld) {
						EditorGUI.indentLevel++;
						trackerClass.takeEyeSemanticWorld = EditorGUILayout.Toggle("Take Semantic", trackerClass.takeEyeSemanticWorld);

						if (trackerClass.takeEyeSemanticWorld) {
							EditorGUI.indentLevel++;

							trackerClass.toggleDebugEye = EditorGUILayout.Toggle("Debug mode eyes", trackerClass.toggleDebugEye);

							LayerMask tempMask = EditorGUILayout.MaskField(InternalEditorUtility.LayerMaskToConcatenatedLayersMask(trackerClass.eyeCastLayer), InternalEditorUtility.layers);

							trackerClass.eyeCastLayer = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask);

							EditorGUI.indentLevel--;
						}
						EditorGUI.indentLevel--;
					} else {
						trackerClass.takeEyeSemanticWorld = false;
					}
					EditorGUI.indentLevel--;


					var eyeTracking = trackerClass.transform.Find("rightEyeTracking");
					var eyeHead = trackerClass.transform.Find("rightEyeHead");
					var eyeWorld = trackerClass.transform.Find("rightEyeWorld");

					if (trackerClass.takeEyeTracking && eyeTracking == null) {
						trackerClass.CreateEyes(OVREyeGaze.EyeTrackingMode.TrackingSpace);
						trackerClass.ConnectEyesToCameraRig(OVREyeGaze.EyeTrackingMode.TrackingSpace);
					}

					if (trackerClass.takeEyeHead && eyeHead == null) {
						trackerClass.CreateEyes(OVREyeGaze.EyeTrackingMode.HeadSpace);
					}

					if (trackerClass.takeEyeWorld && eyeWorld == null) {
						trackerClass.CreateEyes(OVREyeGaze.EyeTrackingMode.WorldSpace);
						trackerClass.ConnectEyesToCameraRig(OVREyeGaze.EyeTrackingMode.WorldSpace);
					}

					if (!trackerClass.takeEyeHead && !trackerClass.takeEyeTracking && !trackerClass.takeEyeWorld) {
						EditorGUILayout.HelpBox("At least one type of eye must be selected to collect eye data", MessageType.Warning);
					}

					trackerClass.RemoveUnusedEyes();
				} else if (trackerClass.takeEyeHead || trackerClass.takeEyeTracking || trackerClass.takeEyeWorld) {
					trackerClass.takeEyeHead = false;
					trackerClass.takeEyeTracking = false;
					trackerClass.takeEyeWorld = false;
					trackerClass.RemoveUnusedEyes();
				}

				trackerClass.takeFaceData = EditorGUILayout.Toggle("Take Face Data", trackerClass.takeFaceData);

				ovrexpr.enabled = trackerClass.takeFaceData || trackerClass.takeEyeData;

				if (trackerClass.takeFaceData && trackerClass.takeEyeData) {
					EditorGUILayout.HelpBox("Eye closed will be collected as face data to avoid double access to face expression", MessageType.Info);
				}


				trackerClass.takeMovementData = EditorGUILayout.Toggle("Take Movement Data", trackerClass.takeMovementData);

				if (trackerClass.takeMovementData) {
					EditorGUILayout.HelpBox("Data from traker are taken from ovr camera rig using active tracker, enable trackers manually \nNote: Any obj with an active TrackObjectMovement component will be tracked", MessageType.Info);
				}
			}

			trackerClass.takeLipSync = EditorGUILayout.Toggle("Take LipSync", trackerClass.takeLipSync);

			// if (trackerClass.takeLipSync) {
			// 	if (Microphone.devices.Length == 0) {
			// 		EditorGUILayout.HelpBox("No microphone detected", MessageType.Warning);
			// 		trackerClass.RemoveLipsyncTracker();
			// 	} else {
			// 		var lipTrackerTranform = trackerClass.transform.Find("LipsyncTracker");
			//
			// 		if (!lipTrackerTranform) {
			// 			trackerClass.CreateLipsyncTracker();
			// 		} else {
			// 			if (Microphone.devices.Length > trackerClass.micSelected)
			// 				lipTrackerTranform.GetComponent<LipSyncTracker>().micSelected = Microphone.devices[trackerClass.micSelected];
			// 			else if (Microphone.devices.Length > 0)
			// 				lipTrackerTranform.GetComponent<LipSyncTracker>().micSelected = Microphone.devices[0];
			// 			else
			// 				lipTrackerTranform.GetComponent<LipSyncTracker>().micSelected = "";
			// 		}
			//
			// 		EditorGUI.indentLevel++;
			//
			// 		trackerClass.micSelected = EditorGUILayout.Popup("Mic", trackerClass.micSelected, Microphone.devices);
			//
			// 		EditorGUI.indentLevel--;
			// 	}
			// } else {
			// 	trackerClass.RemoveLipsyncTracker();
			// }


			if (string.IsNullOrEmpty(trackerClass.dataPath))
				trackerClass.dataPath = EditorGUILayout.TextField("Datapath", "Assets/Data/DataTracker");
			else
				trackerClass.dataPath = EditorGUILayout.TextField("Datapath", trackerClass.dataPath);

			if (!AssetDatabase.IsValidFolder(trackerClass.dataPath)) {
				EditorGUILayout.HelpBox("Folder doesn't exists", MessageType.Info);
				if (GUILayout.Button("Create folder")) {
					CreateFolder(trackerClass.dataPath);
				}
			} else {
				EditorGUI.indentLevel++;
				trackerClass.enableSaveIntoCSV = EditorGUILayout.Toggle("Save into csv", trackerClass.enableSaveIntoCSV);
				EditorGUI.indentLevel--;

				if (GUILayout.Button("Clear csv data")) {
					AssetDatabase.DeleteAsset(trackerClass.dataPath);
					CreateFolder(trackerClass.dataPath);
				}
			}
		}

		private void CreateFolder(string path) {
			var tempPath = "";
			foreach (var str in path.Split("/")) {
				if (!AssetDatabase.IsValidFolder(tempPath + "/" + str)) {
					AssetDatabase.CreateFolder(tempPath, str);
				}

				if (tempPath == "")
					tempPath += str;
				else
					tempPath += "/" + str;
			}
		}
	}

	#endregion




	//Parameters are not shown in inspector if public, only using the GUI above (override for editor purpose)

	private OVRFaceExpressions ovrexpr;
	// private LipSyncTracker lipsyncTracker;

	public bool takeTime, takeStartCycleTime, takeStartFixedUpdateTime, takeTimeDiff, takeUnityTime, takeUnityTimeDiff, takeEndFixedUpdateTime;

	public bool takeEyeData, takeFaceData, takeLipSync, takeMovementData;

	public bool takeEyeWorld, takeEyeTracking, takeEyeHead, takeEyeSemanticWorld;

	public bool enableSaveIntoCSV;

	public string dataPath;

	private GameObject leftEyeHead, rightEyeHead, leftEyeWorld, rightEyeWorld, leftEyeTracking, rightEyeTracking;

	// private List<EnemyAI> trackEnemies;

	private Dictionary<string, LinkedList<string>> collectedData;

	private long actualTime, timePrevious;
	private float unityTimePrevious;

	public int micSelected;

	private Dictionary<string, VelocityDiffTracker> velocityDiffTrackers;
	private int fixedFrameCounter;


	public LayerMask eyeCastLayer;

	public bool toggleDebugEye, collectDataOnStart;

	private bool collectData;
	int teleported;
	private MyPlayerController player;

	private string lastCommand = "";

	private RecorderWindow recorder;

	public void Start() {
		fixedFrameCounter = 1;

		collectedData = new();
		recorder = EditorWindow.GetWindow<RecorderWindow>();

		if (takeFaceData || takeEyeData) {
			ovrexpr = GetComponent<OVRFaceExpressions>();
			ovrexpr.enabled = true;
		}

		player = FindObjectOfType<MyPlayerController>();


		if (takeEyeData) {
			if (takeEyeHead) {
				leftEyeHead = transform.Find("leftEyeHead").gameObject;
				rightEyeHead = transform.Find("rightEyeHead").gameObject;
			}

			if (takeEyeTracking) {
				leftEyeTracking = transform.Find("leftEyeTracking").gameObject;
				rightEyeTracking = transform.Find("rightEyeTracking").gameObject;
			}

			if (takeEyeWorld) {
				leftEyeWorld = transform.Find("leftEyeWorld").gameObject;
				rightEyeWorld = transform.Find("rightEyeWorld").gameObject;
			}
		}

		// if (takeLipSync) {
		// 	Assert.AreEqual("Headset Microphone (Oculus Virtual Audio Device)", Microphone.devices[micSelected]);
		// 	lipsyncTracker = transform.Find("LipsyncTracker").GetComponent<LipSyncTracker>();
		// }

		// if (takeMovementData) {
		// 	trackEnemies = new();
		//
		// 	foreach (var enemy in FindObjectsByType<EnemyAI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)) {
		// 		if (enemy.isActiveAndEnabled) trackEnemies.Add(enemy);
		// 	}
		//
		// 	velocityDiffTrackers = new();
		// }

		// CommandRecognizer.OnCommandRecognized.AddListener(c=> lastCommand = c.name);
		collectData = collectDataOnStart;
		recorder.StartRecording();
	}

	public void StartCollectData() {
		collectData = true;
	}

	public void StopCollectData() {
		collectData = false;
		teleported++;
	}


	//IMPORTANTE: siccome i dati vengono presi da delle queue, è importante che vengano raccolti anche quando non disponibili
	//Altrimenti si rischiano queue di lunghezza diversa, quando non sono disponibili vengono salvati come NaN
	//Per gli occhi e face non sono disponibili sotto un certo valore di confidence (NON FUNZIONA LA CONFIDENCE)
	private void FixedUpdate() {
		if (collectData) {
			AddToDictionary("FrameUpdate", Time.frameCount);
			AddToDictionary("FrameFixed", fixedFrameCounter++);

			if (takeTime) {
				actualTime = Timestamp.GetTimestampMilliseconds();

				if (takeStartCycleTime) {
					AddToDictionary("timestampStartCycle", Timestamp.lastTimestamAtFrameStartString);
					AddToDictionary("timestampStartCycleMillisecond", Timestamp.lastTimestamAtFrameStartMilliseconds.ToString());
				}

				if (takeStartFixedUpdateTime) {
					AddToDictionary("timestampStartFixed", Timestamp.GetTimestamp());
					AddToDictionary("timestampStartFixedMillisecond", Timestamp.GetTimestampMilliseconds().ToString());
				}

				if (takeTimeDiff)
					AddToDictionary("timestampDifference", (actualTime - timePrevious).ToString());

				if (takeUnityTime)
					AddToDictionary("timestampUnityTime", Time.time.ToString());

				if (takeUnityTimeDiff)
					AddToDictionary("timestampUnityTimeDifference", (Time.time - unityTimePrevious).ToString());


				timePrevious = actualTime;
				unityTimePrevious = Time.time;
			}

			// if (takeFaceData) {
			// 	CollectFaceData();
			// }

			if (takeEyeData) {
				if (takeEyeHead) {
					CollectEyeTransform(rightEyeHead);
					CollectEyeTransform(leftEyeHead);
				}

				if (takeEyeTracking) {
					CollectEyeTransform(rightEyeTracking);
					CollectEyeTransform(leftEyeTracking);
				}

				if (takeEyeWorld) {
					CollectEyeTransform(rightEyeWorld);
					CollectEyeTransform(leftEyeWorld);
				}

				//Basta una volta dato che si basa sul face tracking e non sul gaze
				//Se il face tracking è attivo raccoglierei due volte i dati
				if (!takeFaceData)
					CollectEyeClosed();


				if (takeEyeSemanticWorld) {
					CollectEyeWorldSemantic(rightEyeWorld.transform, leftEyeWorld.transform);
				}
			}

			// if (takeLipSync) {
			// 	CollectLipSync();
			// }

			if (takeMovementData) {
				CollectOVRInputControllerTrackingInfo(OVRInput.Controller.LTouch);
				CollectOVRInputControllerTrackingInfo(OVRInput.Controller.RTouch);
				CollectOVRInputControllerButtonData();
				// CollectTrackerTrackingInfo();
				CollectOVRNodeTrackingInfo(OVRPlugin.Node.Head); //Is equal to OVRPlugin.Node.EyeCenter (tested)
			}

			if (takeEndFixedUpdateTime) {
				AddToDictionary("timestampEndFixed", Timestamp.GetTimestamp());
				AddToDictionary("timestampEndFixedMillisecond", actualTime.ToString());
			}

			AddToDictionary("teleportInfo", teleported.ToString());
			// AddToDictionary("LastCheckpoint", GameManager.Instance.lastCheckpointName);
			// AddToDictionary("IsGunGrabbed", gun.IsGrabbed.ToString());
			// AddToDictionary("Health", player.health.ToString());
			// AddToDictionary("Oxygen", player.ox.ToString());
			// AddToDictionary("Deaths", GameManager.Instance.deathCounter);
			AddToDictionary("Command", lastCommand);
			lastCommand = "";
		}
	}

	public void Save() {
		if (enableSaveIntoCSV) {
			SaveIntoCSV();
		}
	}



	private void OnApplicationQuit() {
		if (enableSaveIntoCSV)
			SaveIntoCSV();
		recorder.StopRecording();
	}


	#region Movement

	/// <summary>
	/// Used mantain track of the previous position and time to calculate the velocity
	/// </summary>
	protected class VelocityDiffTracker {
		private Vector3 previousPos = Vector3.zero;

		private long previousTime;
		private float previousTimef;

		public float CalculateSpeed(Vector3 newPosition, long newTime) {
			var distance = (newPosition - previousPos).magnitude;
			var deltaTimeMs = newTime - previousTime;

			var deltaTime = (deltaTimeMs != 0) ? deltaTimeMs / 1000.0f : 0;

			float speed;
			if (deltaTime > 0)
				speed = distance / deltaTime;
			else
				speed = 0;

			previousPos = newPosition;
			previousTime = newTime;

			return speed;
		}

		public float CalculateSpeed(Vector3 newPosition, float newTime) {
			var distance = (newPosition - previousPos).magnitude;
			var deltaTime = newTime - previousTimef;

			float speed;
			if (deltaTime > 0)
				speed = distance / deltaTime;
			else
				speed = 0;

			previousPos = newPosition;
			previousTimef = newTime;

			return speed;
		}
	};

	/// <summary>
	/// Get velocity diff tracker given a key
	/// Used to save multiple instances of VelocityDiffTrackers and bind them to a specific tracked position (Can be from OVR or TrackObjectMovement)
	/// </summary>
	/// <param name="key">
	/// Key to bind the velocity tracker to an object (using the label of the object)
	/// </param>
	private VelocityDiffTracker GetVelocityDiffTracker(string key) {
		if (!velocityDiffTrackers.ContainsKey(key))
			velocityDiffTrackers[key] = new();

		return velocityDiffTrackers[key];
	}



	/// <summary>
	/// Collect tracking info related to position and rotation of all trackers enabled into the current scene
	/// To track an object, add a TrackObjectMovement component to it (need to be enabled)
	/// </summary>
	// private void CollectTrackerTrackingInfo() {
	// 	foreach (var objTracked in trackEnemies) {
	// 		
	// 		var label = $"Tracker_{objTracked.name}";
	//
	// 		if (!objTracked.isActiveAndEnabled) {
	// 			AddToDictionary(label + "_Position", Vector3.zero, false);
	// 			AddToDictionary(label + "_Rotation", Quaternion.identity, false);
	// 		} else {
	// 			AddToDictionary(label + "_Position", objTracked.transform.position, true);
	// 			AddToDictionary(label + "_Rotation", objTracked.transform.rotation, true);
	// 		}
	// 	}
	// }

	/*
	 *
	 *
	 */
	/// <summary>
	/// Collect tracking info related to position and rotation of a Node from OVRPlugin 
	/// </summary>
	/// <param name="node">
	/// OVR node, can be like head, hand, centereye, ecc...
	/// </param>
	private void CollectOVRNodeTrackingInfo(OVRPlugin.Node node) {
		var label = "OVRNode" + node;

		var isValidDataPos = OVRPlugin.GetNodePositionTracked(node);
		var pose = OVRPlugin.GetNodePose(node, OVRPlugin.Step.Render).ToOVRPose();
		var position = pose.position;
		var velocity = OVRPlugin.GetNodeVelocity(node, OVRPlugin.Step.Render).FromVector3f();
		var acceleration = OVRPlugin.GetNodeAcceleration(node, OVRPlugin.Step.Render).FromVector3f();

		var isValidDataRot = OVRPlugin.GetNodeOrientationTracked(node);
		var orientation = pose.orientation;
		var angVelocity = OVRPlugin.GetNodeAngularVelocity(node, OVRPlugin.Step.Render).FromVector3f();
		var angAcceleration = OVRPlugin.GetNodeAngularAcceleration(node, OVRPlugin.Step.Render).FromVector3f();

		AddToDictionaryTrackingInfo(
		label: label,
		position: position,
		orientation: orientation,
		velocity: velocity,
		acceleration: acceleration,
		angVelocity: angVelocity,
		angAcceleration: angAcceleration,
		isValidDataPos: isValidDataPos,
		isValidDataRot: isValidDataRot
		);
	}


	/*
	 *
	 */
	/// <summary>
	/// Collect tracking info related to position and rotation of a controller from OVRInput
	/// </summary>
	/// <param name="controller">
	/// Selected OVR controller (touch is quest controller)
	/// </param>
	private void CollectOVRInputControllerTrackingInfo(OVRInput.Controller controller) {
		var label = "OVRInput" + controller;

		var isValidDataPos = OVRInput.IsControllerConnected(controller) && OVRInput.GetControllerPositionTracked(controller);
		var position = OVRInput.GetLocalControllerPosition(controller);
		var velocity = OVRInput.GetLocalControllerVelocity(controller);
		var acceleration = OVRInput.GetLocalControllerAcceleration(controller);

		var isValidDataRot = OVRInput.IsControllerConnected(controller) && OVRInput.GetControllerOrientationValid(controller);
		var orientation = OVRInput.GetLocalControllerRotation(controller);
		var angVelocity = OVRInput.GetLocalControllerAngularVelocity(controller);
		var angAcceleration = OVRInput.GetLocalControllerAngularAcceleration(controller);

		AddToDictionaryTrackingInfo(
		label: label,
		position: position,
		orientation: orientation,
		velocity: velocity,
		acceleration: acceleration,
		angVelocity: angVelocity,
		angAcceleration: angAcceleration,
		isValidDataPos: isValidDataPos,
		isValidDataRot: isValidDataRot);
	}

	/// <summary>
	/// Wrapper to calculate automatically speed and acceleration strength from vectors if not already calculated
	/// Then call the function to save all previous collected tracking info into dictionary using a standard
	/// Trackers already gives speed and strength, while OVR not
	/// </summary>
	/// <param name="label">Label to save in dictionary, usualli a prefix + the name of the object</param>
	/// <param name="position"></param>
	/// <param name="orientation"></param>
	/// <param name="velocity"></param>
	/// <param name="acceleration"></param>
	/// <param name="angVelocity"></param>
	/// <param name="angAcceleration"></param>
	/// <param name="isValidDataPos">Is position collected valid? This parameter will influence also velocity and acceleration</param>
	/// <param name="isValidDataRot">Is rotation collected valid? This parameter will influence also angular velocity and angular acceleration</param>
	private void AddToDictionaryTrackingInfo(string label, Vector3 position, Quaternion orientation, Vector3 velocity, Vector3 acceleration, Vector3 angVelocity, Vector3 angAcceleration, bool isValidDataPos, bool isValidDataRot) {
		AddToDictionaryTrackingInfo(label: label, position: position, orientation: orientation, speed: velocity.magnitude, velocity: velocity, acceleration: acceleration, accelerationStrength: acceleration.magnitude, angSpeed: angVelocity.magnitude, angVelocity: angVelocity,
		angAcceleration: angAcceleration, angAccelerationStrength: angAcceleration.magnitude, isValidDataPos: isValidDataPos, isValidDataRot: isValidDataRot);
	}

	/// <summary>
	/// Save all previous collected tracking info into dictionary using a standard
	/// </summary>
	/// <param name="label"></param>
	/// <param name="position"></param>
	/// <param name="orientation"></param>
	/// <param name="speed">Magnitude of velocity vector</param>
	/// <param name="velocity"></param>
	/// <param name="acceleration"></param>
	/// <param name="accelerationStrength">Magnitude of acceleration vector</param>
	/// <param name="angSpeed">Magnitude of angular velocity vector</param>
	/// <param name="angVelocity"></param>
	/// <param name="angAcceleration"></param>
	/// <param name="angAccelerationStrength">Magnitude of angular acceleration vector</param>
	/// <param name="isValidDataPos">Is position collected valid? This parameter will influence also velocity and acceleration</param>
	/// <param name="isValidDataRot">Is rotation collected valid? This parameter will influence also angular velocity and angular acceleration</param>
	private void AddToDictionaryTrackingInfo(string label, Vector3 position, Quaternion orientation, float speed, Vector3 velocity, Vector3 acceleration, float accelerationStrength, float angSpeed, Vector3 angVelocity, Vector3 angAcceleration, float angAccelerationStrength, bool isValidDataPos,
	bool isValidDataRot) {
		//Position connected parameters
		AddToDictionary(label + "Position", position, isValidDataPos);
		AddToDictionary(label + "Speed", speed, isValidDataPos);
		//AddToDictionary(label + "Speed(Calculated realtime)", GetVelocityDiffTracker(label + "realtime").CalculateSpeed(position, Timestamp.GetTimestampMilliseconds()), isValidDataPos);
		//AddToDictionary(label + "Speed(Calculated unity time)", GetVelocityDiffTracker(label + "unity").CalculateSpeed(position, Time.time), isValidDataPos);
		AddToDictionary(label + "Velocity", velocity, isValidDataPos);
		AddToDictionary(label + "Acceleration", acceleration, isValidDataPos);
		AddToDictionary(label + "AccelerationStrength", accelerationStrength, isValidDataPos);

		//Orientation connected parameters
		AddToDictionary(label + "Orientation", orientation, isValidDataRot);
		AddToDictionary(label + "AngSpeed", angSpeed, isValidDataRot);
		AddToDictionary(label + "AngVelocity", angVelocity, isValidDataRot);
		AddToDictionary(label + "AngAcceleration", angAcceleration, isValidDataRot);
		AddToDictionary(label + "AngAccelerationStrength", angAccelerationStrength, isValidDataRot);
	}


	/// <summary>
	/// Collect Button data from OVRInput class, using different types of sources
	/// </summary>
	private void CollectOVRInputControllerButtonData() {
		var isValidData = OVRInput.IsControllerConnected(OVRInput.Controller.RTouch) && OVRInput.IsControllerConnected(OVRInput.Controller.LTouch);
		GetOVRInputEnumInfo(OVRInput.RawButton.Any | OVRInput.RawButton.None | OVRInput.RawButton.Back | OVRInput.RawButton.LShoulder | OVRInput.RawButton.RShoulder | OVRInput.RawButton.DpadUp | OVRInput.RawButton.DpadDown | OVRInput.RawButton.DpadLeft | OVRInput.RawButton.DpadRight, isValidData);
		GetOVRInputEnumInfo(OVRInput.RawTouch.Any | OVRInput.RawTouch.None | OVRInput.RawTouch.LTouchpad | OVRInput.RawTouch.RTouchpad, isValidData);
		GetOVRInputEnumInfo(OVRInput.RawNearTouch.Any | OVRInput.RawNearTouch.None, isValidData);
		GetOVRInputEnumInfo(OVRInput.RawAxis1D.Any | OVRInput.RawAxis1D.None | OVRInput.RawAxis1D.LStylusForce | OVRInput.RawAxis1D.RStylusForce, isValidData);
		GetOVRInputEnumInfo(OVRInput.RawAxis2D.Any | OVRInput.RawAxis2D.None | OVRInput.RawAxis2D.LTouchpad | OVRInput.RawAxis2D.RTouchpad, isValidData);
	}


	/// <summary>
	/// Given an OVRInput enum, collect all button data connected to it
	/// </summary>
	/// <typeparam name="T">
	/// OVRInput enum type, correspond to the type of button data that will be collected by OVRInput
	/// It can be from RawButton, RawTouch, ecc...
	/// </typeparam>
	/// <param name="isValidCheck">
	/// Is input collected valid?
	/// </param>
	private void GetOVRInputEnumInfo<T>(T excludeFlags, bool isValidCheck = true) {
		foreach (var enumElement in (T[]) Enum.GetValues(typeof(T))) {
			if (!HasFlag(excludeFlags, enumElement))
				AddToDictionary(typeof(T) + enumElement.ToString(), enumElement.ToString(), isValidCheck);
		}
	}
	
	private bool HasFlag<T>(T flags, T flag) where T : Enum {
		long flagsValue = Convert.ToInt64(flags);
		long flagValue = Convert.ToInt64(flag);
		return (flagsValue & flagValue) == flagValue;
	}


	#endregion

	#region eyeTracking

	public void CreateEyes(OVREyeGaze.EyeTrackingMode eyeType) {
		if (eyeType == OVREyeGaze.EyeTrackingMode.HeadSpace) {
			rightEyeHead = CreateEye("rightEyeHead", OVREyeGaze.EyeId.Right, true, true, OVREyeGaze.EyeTrackingMode.HeadSpace);
			leftEyeHead = CreateEye("leftEyeHead", OVREyeGaze.EyeId.Left, true, true, OVREyeGaze.EyeTrackingMode.HeadSpace);
		} else if (eyeType == OVREyeGaze.EyeTrackingMode.WorldSpace) {
			rightEyeWorld = CreateEye("rightEyeWorld", OVREyeGaze.EyeId.Right, true, true, OVREyeGaze.EyeTrackingMode.WorldSpace);
			leftEyeWorld = CreateEye("leftEyeWorld", OVREyeGaze.EyeId.Left, true, true, OVREyeGaze.EyeTrackingMode.WorldSpace);
		} else {
			rightEyeTracking = CreateEye("rightEyeTracking", OVREyeGaze.EyeId.Right, true, true, OVREyeGaze.EyeTrackingMode.TrackingSpace);
			leftEyeTracking = CreateEye("leftEyeTracking", OVREyeGaze.EyeId.Left, true, true, OVREyeGaze.EyeTrackingMode.TrackingSpace);
		}
	}

	public void ConnectEyesToCameraRig(OVREyeGaze.EyeTrackingMode eyeType) {
		if (eyeType == OVREyeGaze.EyeTrackingMode.TrackingSpace) {
			ConnectEyeToCameraRig(transform.Find("rightEyeTracking").gameObject);
			ConnectEyeToCameraRig(transform.Find("leftEyeTracking").gameObject);
		} else if (eyeType == OVREyeGaze.EyeTrackingMode.WorldSpace) {
			ConnectEyeToCameraRig(transform.Find("rightEyeWorld").gameObject);
			ConnectEyeToCameraRig(transform.Find("leftEyeWorld").gameObject);
		}
	}

	public void ConnectEyeToCameraRig(GameObject eye) {
		var gaze = eye.GetComponent<OVREyeGaze>();
		if (gaze.Eye == OVREyeGaze.EyeId.Left)
			gaze.ReferenceFrame = GameObject.Find("LeftEyeAnchor").transform;
		else
			gaze.ReferenceFrame = GameObject.Find("RightEyeAnchor").transform;
	}

	private GameObject CreateEye(string name, OVREyeGaze.EyeId eyeId, bool applyPos, bool applyRot, OVREyeGaze.EyeTrackingMode trackingMode, Transform referenceFrame = null) {
		GameObject eye = new(name, typeof(OVREyeGaze));
		eye.transform.parent = transform;
		eye.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

		var eyeGaze = eye.GetComponent<OVREyeGaze>();
		eyeGaze.Eye = eyeId;
		eyeGaze.ApplyPosition = applyPos;
		eyeGaze.ApplyRotation = applyRot;
		eyeGaze.TrackingMode = trackingMode;
		eyeGaze.ReferenceFrame = referenceFrame;

		return eye;
	}

	public void RemoveUnusedEyes() {
		if (!takeEyeHead) {
			RemoveEye("rightEyeHead");
			RemoveEye("leftEyeHead");
		}

		if (!takeEyeWorld) {
			RemoveEye("rightEyeWorld");
			RemoveEye("leftEyeWorld");
		}

		if (!takeEyeTracking) {
			RemoveEye("rightEyeTracking");
			RemoveEye("leftEyeTracking");
		}
	}

	private void RemoveEye(string name) {
#if UNITY_EDITOR
		EditorApplication.delayCall += () => {
			Transform eyeTransform = null;
			if (this != null && transform != null)
				eyeTransform = transform.Find(name);

			if (eyeTransform) {
				DestroyImmediate(eyeTransform.gameObject);
			}
		};
#endif
	}

	private void CollectEyeTransform(GameObject eye) {
		var gaze = eye.GetComponent<OVREyeGaze>();
		var label = "Eye" + gaze.TrackingMode + gaze.Eye;

		var isValidData = gaze.Confidence >= 0.5f;

		if (gaze.TrackingMode != OVREyeGaze.EyeTrackingMode.HeadSpace) //Avoid to take eye position for head
			AddToDictionary(label + "Pos", eye.transform.position, isValidData);
		AddToDictionary(label + "Rot", eye.transform.rotation, isValidData);
	}

	private void CollectEyeWorldSemantic(Transform rightEye, Transform leftEye) {
		var rayCount = 10;
		if (toggleDebugEye) {
			Debug.Log($"rightEye:{rightEye}  leftEye:{leftEye}");
			Debug.Log("Gizmo" + GizmoModule.instance);
		}
		// Get gaze directions from both eyes
		var leftGazeDirection = leftEye.forward;
		var rightGazeDirection = rightEye.forward;

		// Calculate mid-point of the two eyes
		var midPoint = (leftEye.position + rightEye.position) / 2f;

		// Cast rays to find collision points for both eyes

		var leftCollision = Physics.Raycast(leftEye.position, leftGazeDirection, out var leftHit, Mathf.Infinity, ~LayerMask.GetMask("Body"), QueryTriggerInteraction.Ignore);
		var rightCollision = Physics.Raycast(rightEye.position, rightGazeDirection, out var rightHit, Mathf.Infinity, ~LayerMask.GetMask("Body"), QueryTriggerInteraction.Ignore);

		if (leftCollision || rightCollision) {
			Vector3 leftPoint, rightPoint;

			leftPoint = leftCollision ? leftHit.point : leftEye.transform.position + leftGazeDirection * rightHit.distance;
			rightPoint = rightCollision ? rightHit.point : rightEye.transform.position + rightGazeDirection * leftHit.distance;

			for (var i = 0; i < rayCount; i++) {
				var t = i / (float) (rayCount - 1); // Interpolation factor
				var pointToWatch = Vector3.Lerp(leftPoint, rightPoint, t);

				// Cast ray
				if (Physics.Raycast(midPoint, (pointToWatch - midPoint).normalized, out var hit, Mathf.Infinity, eyeCastLayer, QueryTriggerInteraction.Ignore)) {
					if (toggleDebugEye)
						GizmoModule.instance.DrawSphere(hit.point, 0.1f, Color.red);
					
					var go = hit.collider.gameObject;
					var goName = go.name;
					var goTag = go.tag;

					// if (go.layer == LayerMask.NameToLayer("Enemy") && go.GetComponent<EnemyAI>() == null) {
					// 	var tmp = go.GetComponentInParentRecursive<EnemyAI>();
					// 	if (tmp) {
					// 		goName = $"{tmp.gameObject.name}_{goName}";
					// 		goTag = tmp.tag;
					// 	}
					// }

					AddToDictionary("SemanticTag" + i, goTag, true);
					AddToDictionary("SemanticPoint" + i, hit.point, true);
					AddToDictionary("SemanticObj" + i, goName, true);
				} else {
					AddToDictionary("SemanticTag" + i, string.Empty, false);
					AddToDictionary("SemanticPoint" + i, Vector3.zero, false);
					AddToDictionary("SemanticObj" + i, string.Empty, false);
				}
			}
		} else {
			for (var i = 0; i < rayCount; i++) {
				AddToDictionary("SemanticTag" + i, string.Empty, false);
				AddToDictionary("SemanticPoint" + i, Vector3.zero, false);
				AddToDictionary("SemanticObj" + i, string.Empty, false);
			}
		}
	}


	private void CollectEyeClosed() {
		float eyeClosedL = 0;
		float eyeClosedR = 0;

		var isValidData = ovrexpr.ValidExpressions;

		if (ovrexpr.ValidExpressions) {
			eyeClosedL = ovrexpr[OVRFaceExpressions.FaceExpression.EyesClosedL];
			eyeClosedR = ovrexpr[OVRFaceExpressions.FaceExpression.EyesClosedR];

			//If blendshape is active, sum blendshape offset to recover true eyeclosed value
			if (ovrexpr.EyeFollowingBlendshapesValid) {
				var blendShapeOffset = Mathf.Min(ovrexpr[OVRFaceExpressions.FaceExpression.EyesLookDownL], ovrexpr[OVRFaceExpressions.FaceExpression.EyesLookDownR]);
				eyeClosedL += blendShapeOffset;
				eyeClosedR += blendShapeOffset;
			}
		}

		AddToDictionary("EyeClosedL", eyeClosedL.ToString(), isValidData);
		AddToDictionary("EyeClosedR", eyeClosedR.ToString(), isValidData);
	}

	#endregion

	#region dictionary utils

	private void AddToDictionary(string label, string value, bool isValid = true) {
		if (!collectedData.ContainsKey(label) || collectedData[label] == null) {
			collectedData[label] = new();
		}

		collectedData[label].AddLast(isValid ? value : "NaN");
	}

	private void AddToDictionary(string label, float value, bool isValid = true) {
		AddToDictionary(label, value.ToString(), isValid);
	}

	private void AddToDictionary(string label, bool value, bool isValid = true) {
		AddToDictionary(label, ((value) ? 1 : 0).ToString(), isValid);
	}

	private void AddToDictionary(string vectorNamelabel, Vector3 vectorValue, bool isValid = true) {
		AddToDictionary(vectorNamelabel + "X", vectorValue.x.ToString(), isValid);
		AddToDictionary(vectorNamelabel + "Y", vectorValue.y.ToString(), isValid);
		AddToDictionary(vectorNamelabel + "Z", vectorValue.z.ToString(), isValid);
	}

	private void AddToDictionary(string vectorNamelabel, Vector2 vectorValue, bool isValid = true) {
		AddToDictionary(vectorNamelabel + "X", vectorValue.x.ToString(), isValid);
		AddToDictionary(vectorNamelabel + "Y", vectorValue.y.ToString(), isValid);
	}

	private void AddToDictionary(string quatNamelabel, Quaternion quatValue, bool isValid = true) {
		AddToDictionary(quatNamelabel + "X", quatValue.x.ToString(), isValid);
		AddToDictionary(quatNamelabel + "Y", quatValue.y.ToString(), isValid);
		AddToDictionary(quatNamelabel + "Z", quatValue.z.ToString(), isValid);
		AddToDictionary(quatNamelabel + "W", quatValue.w.ToString(), isValid);
	}




	private void SaveIntoCSV() {
		List<string> labels = new(collectedData.Keys);

		var tmp = labels.Aggregate("sep=;\n", (current, str) => current + str + ";");

		tmp = tmp.Remove(tmp.Length - 1);

		FileManager fileManager = new(dataPath);

		fileManager.CreateCSV(tmp);

		var keyBound = "FrameUpdate";
		if (!collectedData.ContainsKey(keyBound))
			keyBound = collectedData.Keys.First();

		while (collectedData[keyBound].Count > 0) {
			List<object> lst = new();

			foreach (var str in labels) {
				lst.Add(collectedData[str].First.Value);
				collectedData[str].RemoveFirst();
			}


			fileManager.SaveDataLine(lst);
		}
	}

	private string GetLastCollectedData(string key) {
		return collectedData[key].Last.Value;
	}

	public Dictionary<string, string> GetLastCollectedDataList() {
		Dictionary<string, string> datas = new();

		foreach (var str in collectedData.Keys) {
			datas[str] = GetLastCollectedData(str);
		}

		return datas;
	}

	public void PrintLastCollectedData() {
		Debug.Log("NEW PRINTED DATA");
		foreach (var str in collectedData.Keys) {
			Debug.Log(str + ": " + GetLastCollectedData(str));
		}
		Debug.Log("END OF PRINT");
	}

	#endregion


}
