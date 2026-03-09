using UnityEngine;

public class EnviroManager : MonoBehaviour
{
    [SerializeField] private MeshRenderer floor;

    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float checkRadius = 0.5f;

    private float minX = -8f, minZ = -8f;
    private float maxX = 8f, maxZ = 8f;

    public Material green;
    public Material orange;
    public Material yellow;

    public Vector3 GetValidRandomPosition()
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
    public void SetFloorMaterial(Material m)
    {
        floor.material = m;
    }
}
