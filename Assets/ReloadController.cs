using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadController : MonoBehaviour
{
    Renderer myRend;

    private void Start()
    {
        myRend = GetComponent<Renderer>();
        myRend.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        PushAgentBasic agent = other.GetComponent<PushAgentBasic>();

        if (agent)
        {
            agent.StartRefillAmmo();
            myRend.enabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PushAgentBasic agent = other.GetComponent<PushAgentBasic>();

        if (agent)
        {
            agent.StopRefillAmmo();
            myRend.enabled = false;
        }
    }
}
