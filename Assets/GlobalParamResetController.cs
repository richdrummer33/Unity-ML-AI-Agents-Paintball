using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class GlobalParamResetController : MonoBehaviour
{
    List<PushAgentBasic> agents;
    public float ammoStartSizeModifier = 1f;

    // Start is called before the first frame update
    void Start()
    {
        var academy = FindObjectOfType<Academy>();
        academy.AgentForceReset += _AgentReset;
    }

    void _AgentReset()
    {
        ammoStartSizeModifier = Random.Range(0.0125f, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
