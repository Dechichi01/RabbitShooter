using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour {

	public Wave[] waves;
	public Enemy enemy;

    LivingEntity playerEntity;
    Transform playerT;

	Wave currentWave;
	int currentWaveNumber = 0;

	int enemiesRemainingToSpawn;
	float nextSpawnTime = 0f;

	int enemiesRemainingAlive;
    private MapGenerator map;

    //
    float timeBetweenCampingChecks = 2;
    float campThresholdDist = 2.5f;
    Vector3 campPositionOld;
    bool isCamping;
    //

    public event System.Action<int> OnNewWave;

	void Start(){
        playerEntity = FindObjectOfType<Player>();
        playerT = playerEntity.GetComponent<Transform>();
        //
        campPositionOld = playerT.position;
        playerEntity.OnDeath += OnPlayerDeath;
        StartCoroutine(CheckCamping());
        //
        map = FindObjectOfType<MapGenerator>();
		NextWave();
	}

	void Update(){

		if (enemiesRemainingToSpawn > 0 && Time.time > nextSpawnTime){
			enemiesRemainingToSpawn--;
			nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;

            StartCoroutine(SpawnEnemy());
		} 
	}

    IEnumerator CheckCamping()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeBetweenCampingChecks);

            if (playerT.position != null)
            {
                isCamping = (playerT.position - campPositionOld).sqrMagnitude < Mathf.Pow(campThresholdDist, 2);
                campPositionOld = playerT.position;
            }
            
        }
    }
    
    IEnumerator SpawnEnemy()
    {
        float spawnDelay = 1.5f;
        float tileFlashSpeed = 4;

        Transform spawnTile = map.GetRandomOpenTile();
        if (isCamping)
            spawnTile = map.GetTileFromPosition(playerT.position);

        Material tileMat = spawnTile.GetComponent<Renderer>().material;

        Color initialColor = tileMat.color;
        Color flashColor = Color.red;
        float spawnTimer = 0;

        while (spawnTimer < spawnDelay)
        {
            tileMat.color = Color.Lerp(initialColor, flashColor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));

            spawnTimer += Time.deltaTime;
            yield return null; //wait for a frame
        }

        Enemy spawnedEnemy = (Enemy)Instantiate(enemy, spawnTile.position + Vector3.up, Quaternion.identity) as Enemy;
        spawnedEnemy.OnDeath += OnEnemyDeath;
    }

	void OnEnemyDeath(){
		enemiesRemainingAlive--;

		if (enemiesRemainingAlive == 0){
			NextWave();
		}
	}

    void OnPlayerDeath()
    {
        StopAllCoroutines();
    }

    void ResetPlayerPosition()
    {
        playerT.position = map.GetTileFromPosition(Vector3.zero).position + Vector3.up*3;
    }

	void NextWave(){
		currentWaveNumber++;
		if (currentWaveNumber - 1 < waves.Length){
			currentWave = waves[currentWaveNumber - 1];

			enemiesRemainingToSpawn = currentWave.enemyCount;
			enemiesRemainingAlive = enemiesRemainingToSpawn;

            if (OnNewWave != null)
                OnNewWave(currentWaveNumber);
            //ResetPlayerPosition();
		}
	}

	[System.Serializable]
	public class Wave{
		public int enemyCount;
		public float timeBetweenSpawns;
	}
}
