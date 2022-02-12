using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

// RollerAgent
public class RollerAgent : Agent
{
    public Transform target;
    Rigidbody rBody;

    // 初期化時に呼ばれる
    public override void Initialize()
    {
        rBody = GetComponent<Rigidbody>();
    }

    // エピソード開始時に呼ばれる
    public override void OnEpisodeBegin()
    {
        // RollerAgentの落下時
        if (this.transform.localPosition.y < 0)
        {
            // RollerAgentの位置と速度をリセット
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
        // RollerAgentに力を加える
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = vectorAction[0];
        controlSignal.z = vectorAction[1];
        rBody.AddForce(controlSignal * 10);

        // RollerAgentがTargetの位置に到着
        float distanceToTarget = Vector3.Distance(
            this.transform.localPosition, target.localPosition);
        if (distanceToTarget < 1.42f)
        {
            AddReward(1.0f);
            EndEpisode();
        }

        // RollerAgentが落下
        if (this.transform.localPosition.y < 0)
        {
            EndEpisode();
        }
    }

    // ヒューリスティックモードの行動決定時に呼ばれる
    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = Input.GetAxis("Horizontal");
        actionsOut[1] = Input.GetAxis("Vertical");
    }
}