using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgentsExamples;

// DogAgent
public class DogAgent : Agent
{

    // 棒
    [HideInInspector]
    public Transform target;

    // パーツ
    [Header("Body Parts")]
    public Transform mouthPosition;
    public Transform body;
    public Transform leg0_upper;
    public Transform leg1_upper;
    public Transform leg2_upper;
    public Transform leg3_upper;
    public Transform leg0_lower;
    public Transform leg1_lower;
    public Transform leg2_lower;
    public Transform leg3_lower;

    // 子犬の回転Y
    [Header("Body Rotation")]
    public float maxTurnSpeed;
    public ForceMode turningForceMode;

    // サウンド
    [Header("Sounds")]
    public bool canBark; // 訓練中はオフ
    public List<AudioClip> barkSounds = new List <AudioClip>();
    AudioSource audioSourceSFX;

    // JointDriveController
    JointDriveController jdController;

    [HideInInspector]
    public Vector3 dirToTarget; // 子犬から見た棒の方向
    float rotateBodyActionValue; // 子犬の回転Y

    // [HideInInspector]
    public bool runningToItem;
    // [HideInInspector]
    public bool returningItem;

    // 初期化時に呼ばれる
    void Awake()
    {
        // オーディオの初期化
        audioSourceSFX = body.gameObject.AddComponent<AudioSource>();
        audioSourceSFX.spatialBlend = .75f;
        audioSourceSFX.minDistance = .7f;
        audioSourceSFX.maxDistance = 5;
        if (canBark)
        {
            StartCoroutine(BarkBarkGame());
        }

        // JointDriveControllerの初期化
        jdController = GetComponent<JointDriveController>();
        jdController.SetupBodyPart(body);
        jdController.SetupBodyPart(leg0_upper);
        jdController.SetupBodyPart(leg0_lower);
        jdController.SetupBodyPart(leg1_upper);
        jdController.SetupBodyPart(leg1_lower);
        jdController.SetupBodyPart(leg2_upper);
        jdController.SetupBodyPart(leg2_lower);
        jdController.SetupBodyPart(leg3_upper);
        jdController.SetupBodyPart(leg3_lower);
    }

    // パーツの観察収集時に呼ばれる
    public void CollectObservationBodyPart(BodyPart bp, VectorSensor sensor)
    {
        var rb = bp.rb;
        sensor.AddObservation(bp.groundContact.touchingGround ? 1 : 0);
        if(bp.rb.transform != body)
        {
            sensor.AddObservation(bp.currentXNormalizedRot);
            sensor.AddObservation(bp.currentYNormalizedRot);
            sensor.AddObservation(bp.currentZNormalizedRot);
            sensor.AddObservation(bp.currentStrength/jdController.maxJointForceLimit);
        }
    }

    // 観察収集時に呼ばれる
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(dirToTarget.normalized);
        sensor.AddObservation(body.localPosition);
        sensor.AddObservation(jdController.bodyPartsDict[body].rb.velocity);
        sensor.AddObservation(jdController.bodyPartsDict[body].rb.angularVelocity);
        sensor.AddObservation(body.forward);
        sensor.AddObservation(body.up);
        foreach (var bodyPart in jdController.bodyPartsDict.Values)
        {
            CollectObservationBodyPart(bodyPart, sensor);
        }
    }

    // 子犬の回転Y
    void RotateBody(float act)
    {
        float speed = Mathf.Lerp(0, maxTurnSpeed, Mathf.Clamp(act, 0, 1));
        Vector3 rotDir = dirToTarget;
        rotDir.y = 0;
        jdController.bodyPartsDict[body].rb.AddForceAtPosition(
            rotDir.normalized * speed * Time.deltaTime, body.forward, turningForceMode);
        jdController.bodyPartsDict[body].rb.AddForceAtPosition(
            -rotDir.normalized * speed * Time.deltaTime, -body.forward, turningForceMode);
    }

    // 数秒毎回に吠える
    public IEnumerator BarkBarkGame()
    {
        while(true)
        {
            if(!returningItem)
            {
                audioSourceSFX.PlayOneShot(barkSounds[Random.Range( 0, barkSounds.Count)], 1);
            }
            yield return new WaitForSeconds(Random.Range(1, 10));
        }
    }

    // 行動実行時に呼ばれる
    public override void OnActionReceived(float[] vectorAction)
    {
        // 子犬から見た棒の方向の更新
        UpdateDirToTarget();

        // JointTargetControllerの更新
        var bpDict = jdController.bodyPartsDict;
        bpDict[leg0_upper].SetJointTargetRotation(vectorAction[0], vectorAction[1], 0);
        bpDict[leg1_upper].SetJointTargetRotation(vectorAction[2], vectorAction[3], 0);
        bpDict[leg2_upper].SetJointTargetRotation(vectorAction[4], vectorAction[5], 0);
        bpDict[leg3_upper].SetJointTargetRotation(vectorAction[6], vectorAction[7], 0);
        bpDict[leg0_lower].SetJointTargetRotation(vectorAction[8], 0, 0);
        bpDict[leg1_lower].SetJointTargetRotation(vectorAction[9], 0, 0);
        bpDict[leg2_lower].SetJointTargetRotation(vectorAction[10], 0, 0);
        bpDict[leg3_lower].SetJointTargetRotation(vectorAction[11], 0, 0);
        bpDict[leg0_upper].SetJointStrength(vectorAction[12]);
        bpDict[leg1_upper].SetJointStrength(vectorAction[13]);
        bpDict[leg2_upper].SetJointStrength(vectorAction[14]);
        bpDict[leg3_upper].SetJointStrength(vectorAction[15]);
        bpDict[leg0_lower].SetJointStrength(vectorAction[16]);
        bpDict[leg1_lower].SetJointStrength(vectorAction[17]);
        bpDict[leg2_lower].SetJointStrength(vectorAction[18]);
        bpDict[leg3_lower].SetJointStrength(vectorAction[19]);

        // 子犬の回転Y
        rotateBodyActionValue = vectorAction[20];
        RotateBody(rotateBodyActionValue);

        // 回転ペナルティ
        var bodyRotationPenalty = -0.001f * rotateBodyActionValue;
        AddReward(bodyRotationPenalty);

        // 方向ボーナス
        RewardFunctionMovingTowards();

        // タイムペナルティ
        RewardFunctionTimePenalty();
    }

    // 子犬から見た棒の方向の更新
    public void UpdateDirToTarget()
    {
        dirToTarget = target.position - jdController.bodyPartsDict[body].rb.position;
    }

    // 方向ボーナス
    void RewardFunctionMovingTowards()
    {
        float movingTowardsDot = Vector3.Dot(
            jdController.bodyPartsDict[body].rb.velocity, dirToTarget.normalized);
        AddReward(0.01f * movingTowardsDot);
    }

    // タイムペナルティ
    void RewardFunctionTimePenalty()
    {
        AddReward(-0.001f);
    }
}