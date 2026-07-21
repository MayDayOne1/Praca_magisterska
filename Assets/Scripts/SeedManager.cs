using UnityEngine;
using Unity.MLAgents;

public class SeedManager : MonoBehaviour
{
    public bool useFixedSeed = true;

    public int seedValue = 42;

    void Awake()
    {
        bool isTrainingMode = Academy.Instance.IsCommunicatorOn;

        if (useFixedSeed)
        {
            if (isTrainingMode)
            {
                Debug.LogWarning("<color=yellow>[SeedManager]</color> Training, seed inactive");
            }
            else
            {
                Random.InitState(seedValue);
                Debug.Log($"<color=cyan>[SeedManager]</color> Seed active: ({seedValue}). ");
            }
        }
    }
}