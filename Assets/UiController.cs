using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiController : MonoBehaviour
{
    public static UiController _instance;

    public Text playerScore;
    public Text agentScore;
    public Text ammo;

    int playerScoreCt;
    int agentScoreCt;
    public int hopperAmmo;
    public int reserveAmmo;

    string playerText;
    string agentText;
    string ammotext;

    // Start is called before the first frame update
    void Start()
    {
        _instance = this;

        playerText = playerScore.text;
        agentText = agentScore.text;
        ammotext = ammo.text;
    }

    private void Update()
    {
        ammo.text = ammotext + hopperAmmo + " / " + reserveAmmo;
    }


    public void OpponentHit(bool isPlayer)
    {
        if(!isPlayer)
        {
            agentScoreCt++;
            agentScore.text = agentText + agentScoreCt;
        }
        else
        {
            playerScoreCt++;
            playerScore.text = playerText + playerScoreCt;
        }
    }
}
