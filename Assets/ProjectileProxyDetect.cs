using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileProxyDetect : MonoBehaviour
{
    public GoalDetect thisDetector;
    
    private void OnTriggerStay(Collider other)
    {
        PushAgentBasic otherAgent = other.GetComponent<PushAgentBasic>();

        if (otherAgent)
        {
            if (otherAgent.gameObject != thisDetector.agent.gameObject)
            {
                float dist = Vector3.Distance(otherAgent.transform.position, transform.position);

                if (dist < thisDetector.minDist)
                {
                    thisDetector.minDist = dist;
                }
            }
        }
    }
}
