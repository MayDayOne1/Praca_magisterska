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
    
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float checkRadius = 0.5f;
    
    private float minX = -8f, minZ = -8f;
    private float maxX = 8f, maxZ = 8f;

    private Vector3 GetValidRandomPosition()
    {
        Vector3 safePos = new Vector3(0.23f, 0f, -1.66f);
        Vector3 randomLocalPos = Vector3.zero;
        bool isValid = false;

        int maxAttempts = 50;
        int attempts = 0;

        while (!isValid && attempts < maxAttempts)
        {
            randomLocalPos = new Vector3(Random.Range(minX, maxX), 0f, Random.Range(minZ, maxZ));

            Vector3 globalPos = transform.parent != null
                ? transform.parent.TransformPoint(randomLocalPos)
                : randomLocalPos;

            if (!Physics.CheckSphere(globalPos, checkRadius, wallLayer))
            {
                isValid = true;
            }
            attempts++;
        }

        if (!isValid)
        {
            return safePos;
        }

        return randomLocalPos;
    }

    public override void OnEpisodeBegin()
    {
        //transform.localPosition = new Vector3(1.50999975f, -0.370000124f, -4.10000038f);
        transform.localPosition = GetValidRandomPosition();
        targetTransform.localPosition = GetValidRandomPosition();
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
