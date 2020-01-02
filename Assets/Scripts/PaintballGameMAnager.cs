using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintballGameMAnager : MonoBehaviour
{
    public float paintballForce = 15f;
    public int numShots = 20;

    private void Awake()
    {
        paintballForce = Random.Range(25f, 60f);
        numShots = Mathf.RoundToInt(Random.Range(15, 40));
    }
}
