using Unity.MLAgents.Policies;
using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    public BehaviorParameters runnerParams;
    public BehaviorParameters pursuerParams;

    void Start()
    {
        if (GameConfig.RunnerMode == GameConfig.ControlMode.Human)
            runnerParams.BehaviorType = BehaviorType.HeuristicOnly;
        else
            runnerParams.BehaviorType = BehaviorType.InferenceOnly;

        if (GameConfig.PursuerMode == GameConfig.ControlMode.Human)
            pursuerParams.BehaviorType = BehaviorType.HeuristicOnly;
        else
            pursuerParams.BehaviorType = BehaviorType.InferenceOnly;
    }
}
