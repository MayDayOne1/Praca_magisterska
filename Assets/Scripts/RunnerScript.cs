using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.ComponentModel;
using UnityEngine.Rendering;

public class RunnerScript : Agent
{
    [SerializeField] private PursuerScript pursuer;
    [SerializeField] private bool denseRewards = true;
    [SerializeField] private bool wallHitEndsEpisode = false;
    [SerializeField] private EnviroManager enviroManager;
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Transform pursuerTransform;
    [SerializeField] private float speed = 70f;

    private float previousDistanceToGoal;

    public float dangerZoneRadius = 5f;
    private float _nearMissTime = 0f;
    private float _prevMoveX = 0f;
    private float _prevMoveZ = 0f;
    private float _totalJitter = 0f;

    public float GetAverageJitter()
    {
        return StepCount > 0 ? _totalJitter / StepCount : 0f;
    }

    public float GetNearMissTime()
    {
        return _nearMissTime;
    }

    private void Rotate(float moveX, float moveZ)
    {
        Vector3 moveDirection = new Vector3(moveX, 0f, moveZ);

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(-moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    private void AddDenseReward()
    {
        if (denseRewards)
        {
            float currentDistanceToGoal = Vector3.Distance(transform.position, targetTransform.position);
            float distanceDifference = previousDistanceToGoal - currentDistanceToGoal;
            AddReward(distanceDifference * 0.1f);
            previousDistanceToGoal = currentDistanceToGoal;
        }
    }

    public override void OnEpisodeBegin()
    {
        //transform.localPosition = new Vector3(1.50999975f, -0.370000124f, -4.10000038f);
        transform.localPosition = enviroManager.GetValidRandomPosition();
        targetTransform.localPosition = enviroManager.GetValidRandomPosition();

        previousDistanceToGoal = Vector3.Distance(transform.position, targetTransform.position);

        _nearMissTime = 0f;
        _prevMoveX = 0f;
        _prevMoveZ = 0f;
        _totalJitter = 0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(targetTransform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        float deltaX = Mathf.Abs(moveX - _prevMoveX);
        float deltaZ = Mathf.Abs(moveZ - _prevMoveZ);
        _totalJitter += (deltaX + deltaZ);

        _prevMoveX = moveX;
        _prevMoveZ = moveZ;

        float moveSpeed = 3f;
        transform.localPosition += new Vector3(moveX, 0f, moveZ) * Time.deltaTime * moveSpeed;
        Rotate(moveX, moveZ);
        AddDenseReward();

        // Jeœli min¹³ czas (Runner wygrywa)
        if (MaxStep > 0 && StepCount >= MaxStep - 1)
        {
            float myJitter = GetAverageJitter();
            float pursuerJitter = pursuer != null ? pursuer.GetAverageJitter() : 0f;

            StatsManager.Instance.SaveEpisodeStats("Draw", myJitter, pursuerJitter, _nearMissTime);

            if (pursuer != null)
            {
                pursuer.RunnerEscaped();
            }
            enviroManager.SetFloorMaterial(enviroManager.wallHit);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal") * Time.deltaTime * speed;
        continuousActions[1] = Input.GetAxisRaw("Vertical") * Time.deltaTime * speed;
    }

    private void FixedUpdate()
    {
        // Calculate time spent close to the pursuer
        if (pursuer != null)
        {
            float distance = Vector3.Distance(transform.position, pursuer.transform.position);
            if (distance <= dangerZoneRadius)
            {
                _nearMissTime += Time.fixedDeltaTime;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Goal"))
        {
            AddReward(1.0f);

            float myJitter = GetAverageJitter();
            float pursuerJitter = pursuer != null ? pursuer.GetAverageJitter() : 0f;

            StatsManager.Instance.SaveEpisodeStats("Runner", myJitter, pursuerJitter, _nearMissTime);

            if (pursuer != null)
            {
                pursuer.RunnerEscaped();
            }

            enviroManager.SetFloorMaterial(enviroManager.runnerWin);
            EndEpisode();
        }
    }
    public void GotCaught()
    {
        AddReward(-1.0f);
        EndEpisode(); // Nie wysy³amy statystyk, zrobi³ to ju¿ Pursuer!
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (wallHitEndsEpisode && collision.gameObject.CompareTag("Wall"))
        {
            enviroManager.SetFloorMaterial(enviroManager.wallHit);
            SetReward(-1f);
            EndEpisode();
        }
    }
}
