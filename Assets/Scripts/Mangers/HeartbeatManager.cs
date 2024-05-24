using UnityEngine;

public class HeartbeatManager : MonoBehaviour
{
    private HeartbeatStatistics heartbeatStats;
    string id = "null";
    public TMPro.TextMeshProUGUI max, min, avg;
    public static int avgINT = 0;

    private void Start()
    {
        // Inizializza il gestore delle statistiche dei battiti cardiaci
        heartbeatStats = new HeartbeatStatistics();
    }

    private void Update()
    {
        Invoke("setId",3);
        avgINT = (int)GetAverageHeartbeatForId(id);
        // Esempio di aggiornamento delle statistiche dei battiti cardiaci
        UpdateHeartbeat(id, GetPlayerHeartbeat());
        max.text = ("max " + GetMaxHeartbeatForId(id));
        min.text = ("min " + GetMinHeartbeatForId(id));
        avg.text = ("avg " + GetAverageHeartbeatForId(id));
    }

    private void setId()
    {
        id = "player";
    }
    private int GetPlayerHeartbeat()
    {
        // Simula il recupero del battito cardiaco del giocatore
        // Puoi sostituire questo con la logica effettiva per ottenere il battito cardiaco del giocatore
        return hyperateSocket.value;
    }

    private void UpdateHeartbeat(string id, int heartbeat)
    {
        // Aggiorna le statistiche dei battiti cardiaci per l'ID specificato
        heartbeatStats.UpdateHeartbeat(id, heartbeat);
    }

    public int GetMinHeartbeatForId(string id)
    {
        // Ottieni il battito cardiaco minimo per l'ID specificato
        return heartbeatStats.GetMinHeartbeatForId(id);
    }

    public int GetMaxHeartbeatForId(string id)
    {
        // Ottieni il battito cardiaco massimo per l'ID specificato
        return heartbeatStats.GetMaxHeartbeatForId(id);
    }

    public double GetAverageHeartbeatForId(string id)
    {
        // Ottieni il battito cardiaco medio per l'ID specificato
        return heartbeatStats.GetAverageHeartbeatForId(id);
    }
}