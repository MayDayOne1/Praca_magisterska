using UnityEngine;
using Unity.MLAgents;

public class SeedManager : MonoBehaviour
{
    [Header("Konfiguracja Ziarna")]
    [Tooltip("W³¹cz, aby wymusiæ powtarzalnoœæ testów.")]
    public bool useFixedSeed = true;

    [Tooltip("Dowolna liczba ca³kowita. Dla tej samej liczby u³o¿enie mapy zawsze bêdzie identyczne.")]
    public int seedValue = 42;

    void Awake()
    {
        // Sprawdzamy, czy aplikacja jest po³¹czona z Pythonem (czyli czy trwa Trening)
        bool isTrainingMode = Academy.Instance.IsCommunicatorOn;

        if (useFixedSeed)
        {
            if (isTrainingMode)
            {
                // Zabezpieczenie przed Overfittingiem
                Debug.LogWarning("<color=yellow>[SeedManager]</color> Trwa trening! " +
                                 "Ignorujê sta³e ziarno, aby agenci mogli poprawnie generalizowaæ œrodowisko.");
            }
            else
            {
                // Ustawiamy sta³e ziarno na potrzeby w pe³ni powtarzalnych testów
                Random.InitState(seedValue);
                Debug.Log($"<color=cyan>[SeedManager]</color> Aktywowano sta³e ziarno ({seedValue}). " +
                          $"Rozegrana sekwencja epok bêdzie identyczna w ka¿dym teœcie.");
            }
        }
    }
}