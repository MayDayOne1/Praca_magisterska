using Unity.MLAgents.Policies;
using UnityEngine;
using Cinemachine;

public class GameInitializer : MonoBehaviour
{
    public BehaviorParameters runnerParams;
    public BehaviorParameters pursuerParams;

    [SerializeField] private CinemachineVirtualCamera vcam;

    private void SetupCamera()
    {
        Transform defaultCameraTransform = vcam.transform;
        Transform targetToFollow = null;

        if (GameConfig.RunnerMode == GameConfig.ControlMode.Human)
        {
            targetToFollow = runnerParams.transform;
        }
        else if (GameConfig.PursuerMode == GameConfig.ControlMode.Human)
        {
            targetToFollow = pursuerParams.transform;
        }

        if (targetToFollow != null)
        {
            vcam.Follow = targetToFollow;
            vcam.LookAt = targetToFollow;
        }
    }

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

        SetupCamera();
    }
}
