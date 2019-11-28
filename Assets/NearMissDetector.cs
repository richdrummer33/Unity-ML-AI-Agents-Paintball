using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NearMissDetector : MonoBehaviour
{
    public Transform enemyAgent;
    public PushAgentBasic thisAgent; 
    
    void Update()
    {
        transform.LookAt(enemyAgent);
    }

    private void OnTriggerEnter(Collider other)
    {
        GoalDetect detect = other.GetComponent<GoalDetect>();

        if (detect)
        {
            if(detect.agent == enemyAgent)
            {
                thisAgent.NearMiss(Vector3.Distance(other.transform.position, thisAgent.transform.position));
            }

            detect.NearMissDetected();
        }
    }
}
