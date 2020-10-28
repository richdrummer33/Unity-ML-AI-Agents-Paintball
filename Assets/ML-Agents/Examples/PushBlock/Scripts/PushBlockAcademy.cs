//Every scene needs an academy script.
//Create an empty gameObject and attach this script.
//The brain needs to be a child of the Academy gameObject.

using UnityEngine;
using MLAgents;

public class PushBlockAcademy : Academy
{
    /// <summary>
    /// The "walking speed" of the agents in the scene.
    /// </summary>
    public float agentRunSpeed;

    /// <summary>
    /// The agent rotation speed.
    /// Every agent will use this setting.
    /// </summary>
    public float agentRotationSpeed;

    /// <summary>
    /// The spawn area margin multiplier.
    /// ex: .9 means 90% of spawn area will be used.
    /// .1 margin will be left (so players don't spawn off of the edge).
    /// The higher this value, the longer training time required.
    /// </summary>
    public float spawnAreaMarginMultiplier;

    /// <summary>
    /// When a goal is scored the ground will switch to this
    /// material for a few seconds.
    /// </summary>
    public Material goalScoredMaterial;

    /// <summary>
    /// When an agent fails, the ground will turn this material for a few seconds.
    /// </summary>
    public Material failMaterial;

    /// <summary>
    /// The gravity multiplier.
    /// Use ~3 to make things less floaty
    /// </summary>
    public float gravityMultiplier;

    public GameObject[] npcOpponents;
    int index = 0;

    void State()
    {
        Physics.gravity *= gravityMultiplier;
    }

    public override void AcademyReset()
    {
        base.AcademyReset();
    }

    public void KillConfirmed()
    {
        int lastIndex = index;
        if (npcOpponents.Length > 1)
        {
            index = Random.Range(0, npcOpponents.Length - 1);

            if (index != lastIndex)
            {
                npcOpponents[lastIndex].SetActive(false);

                npcOpponents[index].SetActive(true);
            }
        }
    }
}
