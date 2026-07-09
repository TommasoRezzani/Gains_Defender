using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    private bool bCanSpawn;
    private float fNextSpawnTime;
    [SerializeField] private GameObject gEnemy;
    [SerializeField] private float fSpawnInterval = 2f;
    [SerializeField] private float fSpawnRadius = 12f;

    protected void InitSpawner()
    {
        bCanSpawn = false;
        fNextSpawnTime = 0f;
    }

    void Start()
    {
        InitSpawner();
    }

    void Update()
    {
        if (bCanSpawn && Time.time >= fNextSpawnTime)
        {
            SpawnEnemyAtPerimeter();
            fNextSpawnTime = Time.time + fSpawnInterval;
        }
    }

    public void StartSpawning()
    {
        bCanSpawn = true;
        fNextSpawnTime = Time.time + fSpawnInterval;
    }

    public void StopSpawning()
    {
        bCanSpawn = false;
    }

    private void SpawnEnemyAtPerimeter()
    {
        if (gEnemy == null) return;

        float fAngle = Random.Range(0f, Mathf.PI * 2f);
        Vector3 vSpawnPosition = new Vector3(Mathf.Cos(fAngle) * fSpawnRadius, 0f, Mathf.Sin(fAngle) * fSpawnRadius);

        Instantiate(gEnemy, vSpawnPosition, Quaternion.identity);
    }
}