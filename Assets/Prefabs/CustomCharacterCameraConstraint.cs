using System;
using UnityEngine;

/// <summary>
/// This component is responsible for moving the character capsule to match the HMD, fading out the camera or blocking movement when 
/// collisions occur, and adjusting the character capsule height to match the HMD's offset from the ground.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(MyPlayerController))]
public class CustomCharacterCameraConstraint : MonoBehaviour {
	/// <summary>
	/// This should be a reference to the OVRCameraRig that is usually a child of the PlayerController.
	/// </summary>
	[Tooltip("This should be a reference to the OVRCameraRig that is usually a child of the PlayerController.")]
	public OVRCameraRig CameraRig;
	public Rigidbody leftHand;
	public Rigidbody rightHand;
	/// <summary>
	/// Collision to check if head is inside object
	/// </summary>
	[Tooltip("Collision to check if head is inside object.")]
	public LayerMask collisionMask;

	public float threshold = 0.1f;
	public float maxLookOutLenght = 0.35f;


	/// <summary>
	/// When true, adjust the character controller height on the fly to match the HMD's offset from the ground which will allow ducking to go through smaller spaces.
	/// </summary>
	[Tooltip(
	"When true, adjust the character controller height on the fly to match the HMD's offset from the ground which will allow ducking to go through smaller spaces.")]
	public bool DynamicHeight;


	readonly Action _cameraUpdateAction;
	readonly Action _preCharacterMovementAction;
	CharacterController _character;
	MyPlayerController _playerController;

	CustomCharacterCameraConstraint() {
		_cameraUpdateAction = CameraUpdate;
		_preCharacterMovementAction = PreCharacterMovement;
	}

	void Awake() {
		_character = GetComponent<CharacterController>();
		_playerController = GetComponent<MyPlayerController>();
	}

	void OnEnable() {
		_playerController.CameraUpdated += _cameraUpdateAction;
		_playerController.PreCharacterMove += _preCharacterMovementAction;
	}

	void OnDisable() {
		_playerController.PreCharacterMove -= _preCharacterMovementAction;
		_playerController.CameraUpdated -= _cameraUpdateAction;
	}

	/// <summary>
	/// This method is the handler for the PlayerController.CameraUpdated event, which is used
	/// to update the character height based on camera position.
	/// </summary>
	void CameraUpdate() {
		// If dynamic height is enabled, try to adjust the controller height to the height of the camera.
		if (!DynamicHeight) return;

		var cameraHeight = _playerController.CameraHeight;

		// If the new height is less than before, just accept the reduced height.
		if (cameraHeight <= _character.height) {
			_character.height = cameraHeight - _character.skinWidth;
		} else {
			// Attempt to increase the controller height to the height of the camera.
			// It is important to understand that this will prevent the character from growing into colliding 
			// geometry, and that the camera might go above the character controller. For instance, ducking through
			// a low tunnel and then standing up in the middle would allow the player to see outside the world.
			// The CharacterCameraConstraint is designed to detect this problem and provide feedback to the user,
			// however it is useful to keep the character controller at a size that fits the space because this would allow
			// the player to move to a taller space. If the character controller was simply made as tall as the camera wanted,
			// the player would then be stuck and unable to move at all until the player ducked back down to the 
			// necessary elevation.
			var bottom = _character.transform.position;
			bottom += _character.center;
			bottom.y -= _character.height / 2.0f + _character.radius;
			var pad = _character.radius - _character.skinWidth;

			if (Physics.SphereCast(bottom, _character.radius, Vector3.up, out var info, cameraHeight + pad,
			    _character.gameObject.layer, QueryTriggerInteraction.Ignore)) {
				_character.height = info.distance - _character.radius - _character.skinWidth;
				var t = _character.transform;
				var p = t.position;
				p.y -= cameraHeight - info.distance + pad;
				t.position = p;
			} else {
				_character.height = cameraHeight - _character.skinWidth;
			}
		}
	}

	/// <summary>
	/// This method is the handler for the PlayerController.PreCharacterMove event, which is used
	/// to do the work of fading out the camera or adjust the position depending on the 
	/// settings and the relationship of where the camera is and where the character is.
	/// </summary>
	void PreCharacterMovement() {
		//OldVersion();
		//WithOffset();
		WithOffsetAndCollisions();
	}

	void OldVersion() {
		if (_playerController.Teleported)
			return;

		var oldCameraPos = CameraRig.transform.position;
		var headPos = CameraRig.centerEyeAnchor.position;
		var headDisplacement = headPos - transform.position;
		headDisplacement.y = 0;
		float len = headDisplacement.magnitude;
		if (len > 0.0f) {
			_character.Move(headDisplacement);
			var currentHeadDisplacement = transform.position - headPos;
			currentHeadDisplacement.y = 0;

			float CurrentDistance = currentHeadDisplacement.magnitude;
			CameraRig.transform.position = oldCameraPos;
			if (CurrentDistance > 0) {
				CameraRig.transform.position -= headDisplacement;
			}
		}
	}

	void WithOffset() {
		if (_playerController.Teleported)
			return;

		var oldCameraPos = CameraRig.transform.position;
		var headPos = CameraRig.centerEyeAnchor.position;
		var headDisplacement = headPos - transform.position;
		headDisplacement.y = 0;
		float len = headDisplacement.magnitude;
		if (len > 0.0f) {
			_character.Move(headDisplacement);
			var currentHeadDisplacement = transform.position - headPos;
			currentHeadDisplacement.y = 0;


			float correctionOffset = Mathf.Max(0, currentHeadDisplacement.magnitude - maxLookOutLenght);
			CameraRig.transform.position = oldCameraPos - headDisplacement.normalized * correctionOffset;
		}
	}

	void WithOffsetAndCollisions() {
		if (_playerController.Teleported)
			return;
		var oldCameraPos = CameraRig.transform.position;
		var headPos = CameraRig.centerEyeAnchor.position;
		var headDisplacement = headPos - transform.position;
		headDisplacement.y = 0;

		if (headDisplacement.magnitude > threshold) {
			_character.Move(headDisplacement);
			var currentHeadDisplacement = transform.position - headPos;
			currentHeadDisplacement.y = 0;
			CameraRig.transform.position = oldCameraPos;

			var correctionOffset = Mathf.Max(0, currentHeadDisplacement.magnitude - maxLookOutLenght);

			if (Physics.Raycast(headPos, headDisplacement.normalized, out var hit, _character.radius, collisionMask)) {
				correctionOffset = Mathf.Max(0, _character.radius - hit.distance);
			}


			CameraRig.transform.position -= headDisplacement.normalized * correctionOffset;
		}
	}
}
