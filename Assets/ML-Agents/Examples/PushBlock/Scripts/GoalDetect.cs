//Detect when the orange block has touched the goal.
//Detect when the orange block has touched an obstacle.
//Put this script onto the orange block. There's nothing you need to set in the editor.
//Make sure the goal is tagged with "goal" in the editor.

using UnityEngine;

public class GoalDetect : MonoBehaviour
{
    /// <summary>
    /// The associated agent.
    /// This will be set by the agent script on Initialization.
    /// Don't need to manually set.
    /// </summary>
    [HideInInspector]
    public PushAgentBasic agent;
    bool nearMiss = false;
    public float minDist = Mathf.Infinity;

    void OnCollisionEnter(Collision col)
    {
        // Touched goal.
        if (col.gameObject.CompareTag("agent") && col.gameObject != agent.gameObject)
        {
            col.gameObject.GetComponent<PushAgentBasic>().Hit();

            agent.ScoredAGoal();
        }

        if (!nearMiss)
        {
            //agent.TotalMiss();
        }

        if (minDist != Mathf.Infinity)
        {
            agent.NearMiss(minDist);
        }

        Destroy(this.gameObject);
    }

    public void NearMissDetected()
    {
        nearMiss = true;
    }
}
