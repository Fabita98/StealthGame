using UnityEngine;

public class FlowerPetals : MonoBehaviour
{
    public float dissolveDuration = 2.0f; // Durata della dissoluzione dei petali
    public float fallForce = 0.1f; // Forza applicata per far cadere i petali (ridotta per una caduta più delicata)
    public float dissolveDelay = 0.5f; // Ritardo prima di iniziare la dissoluzione
    public ParticleSystem fallParticles; // Sistema di particelle per l'effetto visivo
    public float customGravity = -2.0f; // Gravità personalizzata (ridotta per una caduta più delicata)
    public float holdDuration = 2.0f; // Durata della pressione del tasto necessaria
    public float drag = 1.0f; // Drag per rallentare la caduta
    public float angularDrag = 1.0f; // Angular Drag per rallentare la rotazione

    private bool isConsuming = false;
    private float dissolveProgress = 0.0f;
    private float dissolveStartTime = 0.0f;
    private float holdStartTime = 0.0f;
    private bool isHolding = false;

    public GameObject light;

    void Start()
    {
        // Disabilita i Rigidbody inizialmente
        foreach (Transform petal in transform)
        {
            Rigidbody rb = petal.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.drag = drag;
                rb.angularDrag = angularDrag;
                rb.useGravity = false; // Disattiva la gravità di Unity
            }
        }
    }

    void Update()
    {
        // Controlla se il tasto è premuto
        if (Input.GetKeyDown(KeyCode.Space) || OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
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

                    Rigidbody rb = petal.GetComponent<Rigidbody>();
                    if (rb != null && !rb.isKinematic)
                    {
                        rb.AddForce(Vector3.up * customGravity * Time.deltaTime, ForceMode.Acceleration); // Applica la gravità personalizzata
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
        light.GetComponent<lightHR>().currentEnergy += 15;

        foreach (Transform petal in transform)
        {
            ActivateRigidBody(petal);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Hand")
        {

        }
    }
    private void ActivateRigidBody(Transform petal)
{
    Rigidbody rb = petal.GetComponent<Rigidbody>();
    if (rb != null)
    {
        rb.isKinematic = false;
        rb.useGravity = false; // Assicura che non sia influenzato dalla gravità di Unity
        rb.velocity = Vector3.zero; // Resetta la velocità per evitare interferenze
        rb.angularVelocity = Vector3.zero; // Resetta la velocità angolare per evitare interferenze
        rb.AddForce(Vector3.down * fallForce, ForceMode.Impulse); // Applica la forza verso il basso
    }
}

}
