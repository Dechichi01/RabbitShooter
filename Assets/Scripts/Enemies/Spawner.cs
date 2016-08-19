using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Spawner : MonoBehaviour {

	public Wave[] waves;
	public Enemy enemy;

    LivingEntity playerEntity;
    Transform playerT;
    GameUI gameUI;

	Wave currentWave;
	int currentWaveNumber = 0;

	int enemiesRemainingToSpawn;
	float nextSpawnTime = 0f;

	int enemiesRemainingAlive;
    private BabyRoomGenerator map;
    private List<SpawnObject> spawnObjects;

    public event System.Action<int> OnNewWave;

	void Start(){
        PoolManager.instance.CreatePool(enemy.gameObject, 20);
        playerEntity = FindObjectOfType<Player>();
        playerT = playerEntity.GetComponent<Transform>();

        playerEntity.OnDeath += OnPlayerDeath;
        //
        map = FindObjectOfType<BabyRoomGenerator>();
        spawnObjects = new List<SpawnObject>();
        for (int i = 0; i < map.furnitures.Length; i++)
        {
            if (map.furnitures[i] is SpawnObject)
                spawnObjects.Add((SpawnObject) map.furnitures[i]);
        }

		NextWave();
        //
        gameUI = FindObjectOfType<GameUI>();
	}

	void Update(){

		if ((enemiesRemainingToSpawn > 0 || currentWave.infinity) && Time.time > nextSpawnTime){
			enemiesRemainingToSpawn--;
			nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;

            StartCoroutine(SpawnEnemy());
		} 
	}
    
    IEnumerator SpawnEnemy()
    {
        yield return new WaitForSeconds(1.5f);

        Vector3 startSpawnPostition = spawnObjects[Random.Range(0, spawnObjects.Count)].spawnPoint;
        Enemy spawnedEnemy = PoolManager.instance.ReuseObject(enemy.gameObject, startSpawnPostition + Vector3.up, Quaternion.identity).GetComponent<Enemy>();
        spawnedEnemy.OnDeath += OnEnemyDeath;
        spawnedEnemy.SetCharacteristics(currentWave.moveSpeed, currentWave.hitsToKillPlayer, currentWave.enemyHealth, currentWave.skinColor);
        //spawnedEnemy.StartChase();
        //

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
        if (currentWaveNumber > 0)
            AudioManager.instance.PlaySound2D("Level Complete");

        currentWaveNumber++;
		if (currentWaveNumber - 1 < waves.Length){
			currentWave = waves[currentWaveNumber - 1];

			enemiesRemainingToSpawn = currentWave.enemyCount;
			enemiesRemainingAlive = enemiesRemainingToSpawn;

            if (OnNewWave != null)
                OnNewWave(currentWaveNumber);
            //ResetPlayerPosition();
		}
        else
        {
            gameUI.OnWin();
        }
	}

	[System.Serializable]
	public class Wave{
        public bool infinity;
		public int enemyCount;
		public float timeBetweenSpawns;

        public float moveSpeed;
        public int hitsToKillPlayer;
        public float enemyHealth;
        public Color skinColor;
	}
}
