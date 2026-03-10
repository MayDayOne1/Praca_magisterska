using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class RunnerScript : Agent
{
    [SerializeField] private EnviroManager enviroManager;
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Transform pursuerTransform;

    public override void OnEpisodeBegin()
    {
        //transform.localPosition = new Vector3(1.50999975f, -0.370000124f, -4.10000038f);
        transform.localPosition = enviroManager.GetValidRandomPosition();
        targetTransform.localPosition = enviroManager.GetValidRandomPosition();
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
        if (collision.gameObject.CompareTag("Wall"))
        {
            enviroManager.SetFloorMaterial(enviroManager.wallHit);
            SetReward(-1f);
            EndEpisode();
        }
        else if(collision.gameObject.CompareTag("Pursuer"))
        {
            enviroManager.SetFloorMaterial(enviroManager.pursuerWin);
            SetReward(-1f);
            EndEpisode();
        }
    }
}
