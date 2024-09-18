using UnityEngine;

public class Flower_animator_mindcontrol : MonoBehaviour
{
    public float dissolveDuration = 2.0f; // Durata della dissoluzione dei petali
    public float dissolveDelay = 0.5f; // Ritardo prima di iniziare la dissoluzione
    public ParticleSystem fallParticles; // Sistema di particelle per l'effetto visivo
    public float holdDuration = 2.0f; // Durata della pressione del tasto necessaria
    public Animator an;

    private bool isConsuming = false;
    private float dissolveProgress = 0.0f;
    private float dissolveStartTime = 0.0f;
    private float holdStartTime = 0.0f;
    private bool isHolding, inHand = false;



    void Start()
    {

    }

    void Update()
    {
        // Controlla se il tasto è premuto
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

        // Controlla se il tasto è rilasciato
        if (Input.GetKeyUp(KeyCode.Space) || OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger))
        {
            isHolding = false;

            // Disattiva il sistema di particelle
            if (fallParticles != null)
            {
                fallParticles.Stop();
            }
        }

        // Verifica se il tasto è stato tenuto premuto per il tempo richiesto
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
                foreach (Transform petal in transform)
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
                }

            }
        }
    }

    public void StartConsuming()
    {
        isConsuming = true;
        dissolveProgress = 0.0f;
        dissolveStartTime = Time.time + dissolveDelay;
        //power variable to be added

    }
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Hand")
        {
            if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
            {
                inHand = true;
            }
            else inHand = false;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Hand")
        {
            inHand = false;
        }
    }

}
