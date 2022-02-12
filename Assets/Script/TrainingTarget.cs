using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TrainingTarget
public class TrainingTarget : MonoBehaviour
{
    public DogAgent trainee; // 子犬
    public Transform spawnArea; // 出現エリア
    private Bounds spawnAreaBounds; // 出現エリア領域

    // 初期化
    void Start () {
        spawnAreaBounds = spawnArea.GetComponent<Collider>().bounds;
        SpawnItemTraining();
        trainee.target = this.transform;
    }

    // フレーム毎に呼ばれる
    void FixedUpdate () {
        // 子犬が棒に到達した時
        if (trainee.dirToTarget.magnitude < 1)
        {
            TouchedTargetTraining();
        }
    }

    // 棒をランダム位置に出現
    public void SpawnItemTraining()
    {
        Vector3 randomSpawnPos = Vector3.zero;
        float randomPosX = Random.Range(-spawnAreaBounds.extents.x, spawnAreaBounds.extents.x);
        float randomPosZ = Random.Range(-spawnAreaBounds.extents.z, spawnAreaBounds.extents.z);
        transform.position = spawnArea.transform.position + new Vector3(randomPosX, 1f, randomPosZ);
    }

    // 子犬が棒に到達した時に呼ばれる
    public void TouchedTargetTraining()
    {
        // 子犬が棒に到達した時の報酬
        trainee.AddReward(1);

        // 棒をランダム位置に出現
        SpawnItemTraining();

        // エピソード完了
        trainee.EndEpisode();
    }
}
