using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitDetect : MonoBehaviour
{
    /// <summary>
    /// The associated agent.
    /// This will be set by the agent script on Initialization.
    /// Don't need to manually set.
    /// </summary>
    [HideInInspector]
    public PushAgentBasic agent;  //

    void OnCollisionEnter(Collision col)
    {
        // Touched goal.
        if (col.gameObject.CompareTag("agent") && col.gameObject != this.gameObject)
        {
            agent.ScoredAGoal();
        }
    }
}
