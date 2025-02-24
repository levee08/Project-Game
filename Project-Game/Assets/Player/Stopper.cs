using UnityEngine;

public class Stopper : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject); // A stopper nem pusztul el jelenetváltáskor
    }
    private float startTime;
    private bool isRunning;

    public void StartStopper()
    {
        startTime = Time.time; // Az aktuális idő elmentése
        isRunning = true;
        Debug.Log("Stopper elindítva.");
    }

    public float StopStopper()
    {
        if (!isRunning)
        {
            Debug.LogWarning("A stopper még nincs elindítva.");
            return 0f;
        }

        isRunning = false;
        float elapsedTime = Time.time - startTime; // Eltelt idő kiszámítása
        Debug.Log($"Stopper leállítva. Eltelt idő: {elapsedTime:F2} másodperc.");
        return elapsedTime;
    }
}
