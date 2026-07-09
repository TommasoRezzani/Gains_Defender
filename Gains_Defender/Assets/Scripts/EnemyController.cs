using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private Vector3 vTargetPosition;
    private GameController gGameController;
    [SerializeField] private float fSpeed = 3f;
    [SerializeField] private int iPointsValue = 10;

    protected void InitEnemy()
    {
        vTargetPosition = Vector3.zero;
        gGameController = Object.FindFirstObjectByType<GameController>();
    }

    void Start()
    {
        InitEnemy();
    }

    void Update()
    {
        MoveTowardsBase();
    }

    public void OnDefeated()
    {
        if (gGameController != null)
        {
            gGameController.AddScore(iPointsValue);
        }
        Destroy(gameObject);
    }

    private void MoveTowardsBase()
    {
        transform.position = Vector3.MoveTowards(transform.position, vTargetPosition, fSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, vTargetPosition) < 0.1f)
        {
            if (gGameController != null)
            {
                gGameController.TriggerGameOver();
            }
            Destroy(gameObject);
        }
    }
}