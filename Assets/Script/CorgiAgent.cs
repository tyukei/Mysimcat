using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgentsExamples;

public class CorgiAgent : Agent
{
    public Transform target;
    Rigidbody rBody;
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

    // JointDriveController
    JointDriveController jdController;

    // 初期化時に呼ばれる
    public override void Initialize()
    {
        rBody = GetComponent<Rigidbody>();
    }

    // 初期化時に呼ばれる
    void Awake()
    {
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

    // 観察収集時に呼ばれる
    public override void CollectObservations(VectorSensor sensor)
    {
        foreach (var bodyPart in jdController.bodyPartsDict.Values)
        {
            BodyPart bp = bodyPart;
            var rb = bp.rb;
            sensor.AddObservation(bp.groundContact.touchingGround ? 1 : 0);
            if (bp.rb.transform != body)
            {
                sensor.AddObservation(bp.currentXNormalizedRot);
                sensor.AddObservation(bp.currentYNormalizedRot);
                sensor.AddObservation(bp.currentZNormalizedRot);
                sensor.AddObservation(bp.currentStrength / jdController.maxJointForceLimit);
            }
        }
    }

    // エピソード開始時に呼ばれる
    public override void OnEpisodeBegin()
    {
        // Agentの落下時
        if (this.transform.localPosition.y < -10)
        {
            // Agentの位置と速度をリセット
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;
            this.transform.localPosition = new Vector3(0.0f, 0.5f, 0.0f);
        }

        // Targetの位置のリセット
        target.localPosition = new Vector3(
            Random.value * 8 - 4, 0.5f, Random.value * 8 - 4);
    }

    // 行動実行時に呼ばれる
    public override void OnActionReceived(float[] vectorAction)
    {
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



        // AgentがTargetの位置に到着
        float distanceToTarget = Vector3.Distance(
            this.transform.localPosition, target.localPosition);
        if (distanceToTarget < 1.42f)
        {
            AddReward(1.0f);
            EndEpisode();
        }

        // Agentが落下
        if (this.transform.localPosition.y < -10)
        {
            EndEpisode();
        }

        // タイムペナルティ
        AddReward(-0.001f);
    }

}
