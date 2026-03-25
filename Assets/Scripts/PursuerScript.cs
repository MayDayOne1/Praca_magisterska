using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class PursuerScript : Agent
{
    [SerializeField] private bool denseRewards = true;
    [SerializeField] private bool wallHitEndsEpisode = false;
    [SerializeField] private EnviroManager enviroManager;
    [SerializeField] private Transform runnerTransform;
    [SerializeField] private float speed = 70f;

    private float previousDistanceToGoal;

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

        float moveSpeed = 3f;
        transform.localPosition += new Vector3(moveX, 0f, moveZ) * Time.deltaTime * moveSpeed;
        Rotate(moveX, moveZ);
        AddDenseReward();
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
            Debug.Log("ok");
            SetReward(-1f);
            EndEpisode();
        }

        if (collision.gameObject.CompareTag("Runner"))
        {
            enviroManager.SetFloorMaterial(enviroManager.pursuerWin);
            SetReward(1f);
            EndEpisode();
        }
    }
}
