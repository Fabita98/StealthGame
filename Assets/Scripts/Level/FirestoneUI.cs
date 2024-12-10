using UnityEngine;

namespace Assets.Scripts.GazeTrackingFeature {
    public class FirestoneUI : MonoBehaviour {
        [SerializeField] private GameObject sleepButton;

        public void EnableSleepButton() {
            if (sleepButton.activeSelf) return;
            sleepButton.SetActive(true);
        }

        public void DisableSleepButton() {
            if (!sleepButton.activeSelf) return;
            sleepButton.SetActive(false);
        }
    }
}