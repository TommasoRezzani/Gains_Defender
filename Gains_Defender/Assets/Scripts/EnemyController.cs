using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private Vector3 vTargetPosition;
    [SerializeField] private float fSpeed = 3f;


    protected void InitEnemy()
    {
        vTargetPosition = Vector3.zero; 
    }

    void Start()
    {
        InitEnemy();
    }

    void Update()
    {
        MoveTowardsBase();
    }

    private void MoveTowardsBase()
    {
        transform.position = Vector3.MoveTowards(transform.position, vTargetPosition, fSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, vTargetPosition) < 0.1f)
        {
            Destroy(gameObject);
        }
    }
}