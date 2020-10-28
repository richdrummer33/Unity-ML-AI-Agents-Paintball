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

        rot = transform.rotation; // Buig it's getting spun 90 degrees?
    }

    void _AgentReset()
    {
        ammoStartSizeModifier = Random.Range(0.0125f, 1f);
    }

    Quaternion rot;

    // Update is called once per frame
    void Update()
    {
        if (transform.rotation != rot)
            transform.rotation = rot;
    }
}
