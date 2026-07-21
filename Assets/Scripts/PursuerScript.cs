using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class PursuerScript : Agent
{
    [SerializeField] private RunnerScript runner;
    [SerializeField] private bool denseRewards = true;
    [SerializeField] private bool wallHitEndsEpisode = false;
    [SerializeField] private EnviroManager enviroManager;
    [SerializeField] private Transform runnerTransform;
    [SerializeField] private float speed = 70f;

    private float previousDistanceToGoal;
    private float moveSpeed = 3f;

    private float _prevMoveX = 0f;
    private float _prevMoveZ = 0f;
    private float _totalJitter = 0f;

    public void RunnerEscaped()
    {
        AddReward(-1.0f);
        EndEpisode();
    }

    public float GetAverageJitter()
    {
        return StepCount > 0 ? _totalJitter / StepCount : 0f;
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
            float currentDistanceToRunner = Vector3.Distance(transform.position, runnerTransform.position);
            float distanceDifference = previousDistanceToGoal - currentDistanceToRunner;
            AddReward(distanceDifference * 0.1f);
            previousDistanceToGoal = currentDistanceToRunner;
        }
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = enviroManager.GetValidRandomPosition();

        _prevMoveX = 0f;
        _prevMoveZ = 0f;
        _totalJitter = 0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(runnerTransform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        UpdateJitter(moveX, moveZ);

        MoveAgent(moveX, moveZ);
        Rotate(moveX, moveZ);
        AddDenseReward();
    }

    private void UpdateJitter(float moveX, float moveZ)
    {
        float deltaX = Mathf.Abs(moveX - _prevMoveX);
        float deltaZ = Mathf.Abs(moveZ - _prevMoveZ);
        _totalJitter += (deltaX + deltaZ);

        _prevMoveX = moveX;
        _prevMoveZ = moveZ;
    }
    private void MoveAgent(float moveX, float moveZ)
    {
        transform.localPosition += new Vector3(moveX, 0f, moveZ) * Time.deltaTime * moveSpeed;
    }
    private void CollectAndSaveDataToCSV(string winner)
    {
        float myJitter = GetAverageJitter();
        float runnerJitter = runner != null ? runner.GetAverageJitter() : 0f;

        StatsManager.Instance.SaveEpisodeStats(winner, myJitter, runnerJitter, 0f, StepCount);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal") * Time.deltaTime * speed;
        continuousActions[1] = Input.GetAxisRaw("Vertical") * Time.deltaTime * speed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (wallHitEndsEpisode && collision.gameObject.CompareTag("Wall"))
        {
            enviroManager.SetFloorMaterial(enviroManager.wallHit);
            AddReward(-1f);
            EndEpisode();
        }

        if (collision.gameObject.CompareTag("Runner"))
        {
            enviroManager.SetFloorMaterial(enviroManager.pursuerWin);
            AddReward(1f);
            CollectAndSaveDataToCSV(winner: "Pursuer");

            if (runner != null) runner.GotCaught();

            EndEpisode();
        }
    }
}
