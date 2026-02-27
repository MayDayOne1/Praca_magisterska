using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class RunnerScript : Agent
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Material green;
    [SerializeField] private Material red;
    [SerializeField] private MeshRenderer meshRenderer;

    public override void OnEpisodeBegin()
    {
        //transform.localPosition = new Vector3(1.50999975f, -0.370000124f, -4.10000038f);
        transform.localPosition = new Vector3(Random.Range(-1.52f, 5.16f), 0f, Random.Range(-6.22f, 0.96f));
        targetTransform.localPosition = new Vector3(Random.Range(-1.52f, 5.16f), 0f, Random.Range(-6.22f, 0.96f));
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
            meshRenderer.material = green;
            SetReward(+1f);
            EndEpisode();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            meshRenderer.material = red;
            SetReward(-1f);
            EndEpisode();
        }
    }
}
