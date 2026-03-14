using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.ComponentModel;
using UnityEngine.Rendering;

public class RunnerScript : Agent
{
    [SerializeField] private bool denseRewards = true;
    [SerializeField] private bool wallHitEndsEpisode = false;
    [SerializeField] private EnviroManager enviroManager;
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Transform pursuerTransform;

    private float previousDistanceToGoal;

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

        float moveSpeed = 3f;
        transform.localPosition += new Vector3(moveX, 0f, moveZ) * Time.deltaTime * moveSpeed;

        AddDenseReward();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal") * Time.deltaTime;
        continuousActions[1] = Input.GetAxisRaw("Vertical") * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Goal"))
        {
            enviroManager.SetFloorMaterial(enviroManager.runnerWin);
            SetReward(+1f);
            EndEpisode();
        }
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
        
        if(collision.gameObject.CompareTag("Pursuer"))
        {
            enviroManager.SetFloorMaterial(enviroManager.pursuerWin);
            SetReward(-1f);
            EndEpisode();
        }
    }
}
