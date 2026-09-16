using UnityEngine;
using DG.Tweening;

public class MovingObstacle : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;

    public float minAnimationTime = 0.5f;
    public float maxAnimationTime = 1.5f;

    private void Start()
    {
        if (startPoint == null || endPoint == null)
        {
            Debug.LogError("Brak przypisanych punktów trasy w obiekcie: " + gameObject.name);
            return;
        }
        transform.position = startPoint.position;

        float duration = Random.Range(minAnimationTime, maxAnimationTime);

        transform.DOMove(endPoint.position, duration)
            .SetEase(Ease.InOutSine)      
            .SetLoops(-1, LoopType.Yoyo)  
            .SetUpdate(UpdateType.Fixed);
    }

    private void OnDrawGizmos()
    {
        if (startPoint != null && endPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(startPoint.position, 0.25f);
            Gizmos.DrawWireSphere(endPoint.position, 0.25f);
            Gizmos.DrawLine(startPoint.position, endPoint.position);
        }
    }
}