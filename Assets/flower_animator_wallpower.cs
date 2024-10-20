using UnityEngine;

public class Flower_animator_wallpower : MonoBehaviour
{
    public float dissolveDuration = 2.0f; // Durata della dissoluzione dei petali
    public float dissolveDelay = 0.5f; // Ritardo prima di iniziare la dissoluzione
    public ParticleSystem fallParticles; // Sistema di particelle per l'effetto visivo
    public float holdDuration = 2.0f; // Durata della pressione del tasto necessaria
    public Animator an;
    public GameObject player;
    public GameObject petalsGO;
    public AudioSource sound;

    private bool isConsuming = false;
    private float dissolveProgress = 0.0f;
    private float dissolveStartTime = 0.0f;
    private float holdStartTime = 0.0f;
    private bool isHolding, inHand = false;

    public static event BlueLotusPowerChangeHandler OnBlueLotusPowerChanged;
    public delegate bool BlueLotusPowerChangeHandler(bool value);

    public static void TriggerOnBlueLotusPowerChangeEvent(bool value) {
        OnBlueLotusPowerChanged?.Invoke(value);
    }

    void Update()
    {
        // Controlla se il tasto � premuto
        if ((Input.GetKeyDown(KeyCode.Space) || OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger)) && inHand)
        {
            holdStartTime = Time.time;
            isHolding = true;

            // Attiva il sistema di particelle
            if (fallParticles != null)
            {
                fallParticles.Play();
            }
        }

        // Controlla se il tasto � rilasciato
        if (Input.GetKeyUp(KeyCode.Space) || OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger))
        {
            isHolding = false;

            // Disattiva il sistema di particelle
            if (fallParticles != null)
            {
                fallParticles.Stop();
            }
        }

        // Verifica se il tasto � stato tenuto premuto per il tempo richiesto
        if (isHolding && Time.time - holdStartTime >= holdDuration)
        {
            StartConsuming();
            isHolding = false;

            // Disattiva il sistema di particelle
            if (fallParticles != null)
            {
                fallParticles.Stop();
            }
        }

        if (isConsuming)
        {
            float currentTime = Time.time;

            if (currentTime >= dissolveStartTime)
            {
                dissolveProgress += Time.deltaTime;
                float dissolveAmount = Mathf.Clamp01((currentTime - dissolveStartTime) / dissolveDuration);
                an.SetTrigger("activate");
                PlaySound();
                foreach (Transform petal in petalsGO.transform)
                {
                    Renderer renderer = petal.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        Color color = renderer.material.color;
                        color.a = Mathf.Lerp(1.0f, 0.0f, dissolveAmount); // Dissolvi il petalo
                        renderer.material.color = color;

                        if (dissolveAmount >= 1.0f)
                        {
                            Destroy(petal.gameObject); // Distruggi il petalo quando completamente dissolto
                            
                        }
                    }
                }

                if (dissolveAmount >= 1.0f)
                {
                    isConsuming = false;
                    int currentBlueLotusCounter = PlayerPrefsManager.GetInt(PlayerPrefsKeys.BlueLotus, 0);
                    PlayerPrefsManager.SetInt(PlayerPrefsKeys.BlueLotus, currentBlueLotusCounter + 1);
                    UIController.Instance.AbilitiesUI.SetAbilitiesCount();
                    OnBlueLotusPowerChanged?.Invoke(true);
                }
            }
        }
    }

    public void StartConsuming()
    {
        isConsuming = true;
        dissolveProgress = 0.0f;
        dissolveStartTime = Time.time + dissolveDelay;
        if (player.TryGetComponent<SpiritVision>(out _)) SpiritVision.mana += 8;
        else Debug.LogError("SpiritVision component not found on player");
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("RightHand") || other.CompareTag("LeftHand"))
        {
            if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LTouch) > 0.1f ||
                OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch) > 0.1f)
            {
                inHand = true;
            }
            else inHand = false;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("RightHand") || other.CompareTag("LeftHand"))
        {
            inHand = false;
        }
    }
    void PlaySound()
    {
        sound.Play();
    }
}
