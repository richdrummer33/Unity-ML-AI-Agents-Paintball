using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NearMissDetector : MonoBehaviour
{
    public PushAgentBasic enemyAgent;
    public PushAgentBasic thisAgent; 

    private void OnTriggerEnter(Collider other)
    {
        GoalDetect paintball = other.GetComponent<GoalDetect>();

        if (paintball)
        {
            if(paintball.agent.gameObject == enemyAgent.gameObject)
            {
                float distance = Vector3.Distance(paintball.transform.position, transform.position);
                enemyAgent.NearMiss(distance);
                Debug.Log("near miss distance " + distance + " from " + thisAgent.gameObject.name);
            }

            paintball.NearMissDetected();
        }
    }
}
