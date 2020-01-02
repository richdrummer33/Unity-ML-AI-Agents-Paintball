//Put this script on your blue cube.

using System.Collections;
using UnityEngine;
using MLAgents;
using System.Linq;

public class PushAgentBasic : Agent
{
    /// <summary>
    /// The ground. The bounds are used to spawn the elements.
    /// </summary>
    public GameObject ground;

    public GameObject area;

    /// <summary>
    /// The area bounds.
    /// </summary>
    [HideInInspector]
    public Bounds areaBounds;

    PushBlockAcademy m_Academy;

    public bool useVectorObs;

    Rigidbody m_AgentRb;  //cached on initialization
    Material m_GroundMaterial; //cached on Awake()
    Material m_AgentMaterial;
    public Material m_reloadMat; 
    RayPerception m_RayPer;

    float[] m_RayAngles; // = { 0f, 45f, 90f, 135f, 180f, 110f, 70f };
    string[] m_DetectableObjects = { "wall", "cover", "agent", "projectile" };

    /// <summary>
    /// We will be changing the ground material based on success/failue
    /// </summary>
    Renderer m_GroundRenderer;
    public Renderer m_AgentRenderer;

    public GameObject[] cover;

    BehaviorParameters behavior;

    GlobalParamResetController resetController;

    float rewardMultiplier = 1f;

    void Awake()
    {
        m_Academy = FindObjectOfType<PushBlockAcademy>(); //cache the academy
    }

    public override void InitializeAgent()
    {
        base.InitializeAgent();
        m_RayPer = GetComponent<RayPerception>();

        // Cache the agent rigidbody
        m_AgentRb = GetComponent<Rigidbody>();
        // Get the ground's bounds
        areaBounds = ground.GetComponent<Collider>().bounds;
        // Get the ground renderer so we can change the material when a goal is scored
        m_GroundRenderer = ground.GetComponent<Renderer>();
        // Starting material
        m_GroundMaterial = m_GroundRenderer.material;

        float rayAngleStep = 15f;
        float minAngle = 0f;
        float maxAngle = 180f;

       m_RayAngles = new float[Mathf.CeilToInt((maxAngle - minAngle) / rayAngleStep) + 1];

        for (int i = 0; i < m_RayAngles.Length; i++)
        {
            m_RayAngles[i] = minAngle + i * rayAngleStep;
        }

        m_AgentMaterial = m_AgentRenderer.material;

        hopperAmmoLeft = hopperSize;

        addedCover = new GameObject[0];

        behavior = GetComponent<BehaviorParameters>();

        resetController = transform.root.GetComponent<GlobalParamResetController>();

        SetResetParameters();
    }

    public override void CollectObservations()
    {
        if (useVectorObs)
        {
            var rayDistance = 32.5f;

            AddVectorObs(m_RayPer.Perceive(rayDistance, m_RayAngles, m_DetectableObjects, 0f, 0f));
            AddVectorObs(m_RayPer.Perceive(rayDistance, m_RayAngles, m_DetectableObjects, 1.5f, 0f));
        }

        AddVectorObs(hopperAmmoLeft);
        AddVectorObs(totalAmmoLeft);
        AddVectorObs(isReloading);
    }

    /// <summary>
    /// Use the ground's bounds to pick a random spawn position.
    /// </summary>
    public Vector3 GetRandomSpawnPos(bool isAnAgent)
    {
        var foundNewSpawnLocation = false;
        var randomSpawnPos = Vector3.zero;
        int numAttempts = 0;

        while (foundNewSpawnLocation == false)
        {
            var randomPosX = Random.Range(-areaBounds.extents.x * m_Academy.spawnAreaMarginMultiplier,
                areaBounds.extents.x * m_Academy.spawnAreaMarginMultiplier);

            var randomPosZ = Random.Range(-areaBounds.extents.z * m_Academy.spawnAreaMarginMultiplier,
                areaBounds.extents.z * m_Academy.spawnAreaMarginMultiplier);

            randomSpawnPos = ground.transform.position + new Vector3(randomPosX, 1f, randomPosZ);

            if (Physics.CheckBox(randomSpawnPos, new Vector3(2f, 0.01f, 2f)) == false)
            {
                foundNewSpawnLocation = true;
            }
            numAttempts++;

            if(numAttempts > 20)
                foundNewSpawnLocation = true;
        }
        return randomSpawnPos;
    }

    /// <summary>
    /// Called when the agent moves the block into the goal.
    /// </summary>
    public void ScoredAGoal()
    {
        // We use a reward of 5.
        AddReward(5f * rewardMultiplier);

        // By marking an agent as done AgentReset() will be called automatically.

        StartCoroutine(DelayedDone());

        // Swap ground material for a bit to indicate we scored.
        StartCoroutine(GoalScoredSwapGroundMaterial(m_Academy.goalScoredMaterial, 0.5f));

        UiController._instance.OpponentHit(behavior.UsingHeuristic());
    }

    IEnumerator DelayedDone()
    {
        yield return new WaitForSeconds(0.1f);

        Done();
    }

    public GameObject coverPrefab;
    GameObject[] addedCover;
    private void ResetCover()
    {
        // Clear last set of spawned cover objs
        foreach (GameObject c in addedCover)
        {
            Destroy(c);
        }

        // Set new cover positions
        foreach (GameObject c in cover)
        {
            c.transform.position = GetRandomSpawnPos(false);
        }

        // Spawn new possible cover objs - only one agent should do this
        if (coverPrefab)
        {
            int amtNewCover = Mathf.RoundToInt(Random.Range(0, 4));
            addedCover = new GameObject[amtNewCover];

            for (int i = 0; i < amtNewCover; i++)
            {
                addedCover[i] = Instantiate(coverPrefab, Vector3.zero + Vector3.up * 10f, coverPrefab.transform.rotation, transform.root); // Spawn away from things
                addedCover[i].transform.position = GetRandomSpawnPos(false);
            }
        }
    }

    public void Hit()
    {
        // We use a reward of 5.
        AddReward(-4f * rewardMultiplier);

        ResetCover();

        // By marking an agent as done AgentReset() will be called automatically.
        StartCoroutine(DelayedDone());
    }

    /// <summary>
    /// Swap ground material, wait time seconds, then swap back to the regular material.
    /// </summary>
    IEnumerator GoalScoredSwapGroundMaterial(Material mat, float time)
    {
        m_GroundRenderer.material = mat;
        yield return new WaitForSeconds(time); // Wait for 2 sec
        m_GroundRenderer.material = m_GroundMaterial;
    }

    

    /// <summary>
    /// Moves the agent according to the selected action.
    /// </summary>
    public void MoveAgent(float[] act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var action = Mathf.FloorToInt(act[0]);

        Vector3 dir;

        if(!behavior.UsingHeuristic())
            dir = transform.forward;
        else
            dir = Vector3.forward;

        // Fwd/back 
        switch (action)
        {
            case 1:
                dirToGo = dir * 1f;
                break;
            case 2:
                dirToGo = dir * -1f;
                break;
        }


        action = Mathf.FloorToInt(act[1]);

        if (!behavior.UsingHeuristic())
            dir = transform.right;
        else
            dir = Vector3.right;

        // Left/right strafe
        switch (action)
        {
            case 1:
                dirToGo = dir * -0.75f;
                break;
            case 2:
                dirToGo = dir * 0.75f;
                break;
        }

        action = Mathf.FloorToInt(act[2]);

        // Turn
        switch (action)
        {
            case 1:
                rotateDir = transform.up * 1f;
                break;
            case 2:
                rotateDir = transform.up * -1f;
                break;
        }

        transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f);
        m_AgentRb.AddForce(dirToGo * m_Academy.agentRunSpeed,
            ForceMode.VelocityChange);
    }

    int actionStep;
    float lastTime;
    /// <summary>
    /// Called every step of the engine. Here the agent takes an action.
    /// </summary>
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        //Debug.Log("rate = " + 1 / (Time.time - lastTime));
        lastTime = Time.time;
        // Move the agent using the action.
        MoveAgent(vectorAction);

        // Penalty given each step to encourage agent to finish task quickly.
        AddReward(-1f / agentParameters.maxStep / 2f * rewardMultiplier);

        AssessGunAction(vectorAction[3]);
    }

    /// <summary>
    /// Fire pball if have ammo
    /// </summary>
    void AssessGunAction(float act)
    {
        actionStep++;

        var action = act; // var just to be consistent just in case
        if (actionStep > 60 / agentParameters.numberOfActionsBetweenDecisions)
        {
            switch (action)
            {
                case 1:
                    if (hopperAmmoLeft > 0 && !isReloading)
                    {
                        Fire();
                        actionStep = 0;
                    }
                    break;
                case 2:
                    if (!isReloading)
                    {
                        StartCoroutine(Reload());
                        AddReward(-Mathf.Clamp((float)hopperAmmoLeft/hopperSize * 0.5f, 0f, 1f) * rewardMultiplier);
                        
                    }
                    break;
            }
        }
    }

    public GameObject projectilePrefab;
    public int ammoSize = 80;
    public int hopperSize = 20;
    [SerializeField]
    int hopperAmmoLeft;
    [SerializeField]
    int totalAmmoLeft;

    void  Fire()
    {
        if (totalAmmoLeft > 0)
        {
            GameObject inst = Instantiate(projectilePrefab, transform.position + transform.forward, Quaternion.identity, null);
            inst.GetComponent<Rigidbody>().AddForce(transform.forward * 25f, ForceMode.Impulse);
            inst.GetComponent<GoalDetect>().agent = this;
            AddReward(-0.0075f * rewardMultiplier); //AddReward(-2.5f / ammoSize); // Added -0.005 to account for NearMiss rewards
            totalAmmoLeft--;
            hopperAmmoLeft--;
        }
        /*else
        {
            Done();
            AddReward(-2.5f); // AddReward(-4f);
            // Swap ground material for a bit to indicate we scored.
            StartCoroutine(GoalScoredSwapGroundMaterial(m_Academy.goalScoredMaterial, 0.5f));
        }*/
    }

    private void Update()
    {
        if (behavior.UsingHeuristic())
        {
            UiController._instance.hopperAmmo = hopperAmmoLeft;
        }
    }

    float reloadTime = 3f;
    bool isReloading;

    IEnumerator Reload()
    {
        isReloading = true;
        m_AgentRenderer.material = m_reloadMat;

        yield return new WaitForSeconds(reloadTime);

        m_AgentRenderer.material = m_AgentMaterial;
        hopperAmmoLeft = 20;
        isReloading = false;

        if (behavior.UsingHeuristic())
        {
            UiController._instance.totalAmmo = totalAmmoLeft;
        }
    }

    float refillTime = 3f;
    float atReloadStation;
    bool refilling;

    public void StartRefillAmmo()
    {
        if(!refilling)
        {
            StartCoroutine(RefillAmmo());
        }
    }

    public void StopRefillAmmo()
    {
        refilling = false;
    }

    private IEnumerator RefillAmmo()
    {
        refilling = true;

        while (refilling && totalAmmoLeft < ammoSize)
        {
            yield return new WaitForSeconds(3f / hopperSize);

            totalAmmoLeft = Mathf.CeilToInt(Mathf.Clamp(totalAmmoLeft + 1, 0f, ammoSize));
        }
    }

    public void NearMiss(float distance)
    {
        float norm = (5f - distance) / 4.25f; // 5m is the radius of the pball's trigger sense sphere, and subtract 0.75f from denominator to acct for radius of the agent plus 0.25f of fudge distance (radius of paintball)

        // Min clamp to 0.005 to account for negative reward when firing
        float score = Mathf.Clamp(norm * 0.05f, 0.005f, 0.05f); // 1/16f == 0.0625 * 80 == 5f i.e. a hit. Clamped just in case, really. Ideal world, not necessary

        AddReward(score * rewardMultiplier);
    }

    public void TotalMiss()
    {
        AddReward(-0.025f * rewardMultiplier); // -2f for 80 shots

        if (!behavior.UsingHeuristic())
        {
            Debug.Log("TotalMiss");
        }
    }

    public bool useGamepad;
    public override float[] Heuristic()
    {
        var action = new float[4];

        // ----- Move ----- //
        if (!useGamepad) // KB
        {
            if (Input.GetKey(KeyCode.W))
            {
                action[0] = 1;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                action[0] = 2;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                action[1] = 1;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                action[1] = 2;
            }
            else
                action[0] = 0;
        }
        else // Gamepad
        {
            if (Input.GetAxis("Axis 2") < -0.2f)
            {
                action[0] = 1;
            }
            else if (Input.GetAxis("Axis 2") > 0.2f)
            {
                action[0] = 2;
            }
            else
                action[0] = 0;

            if (Input.GetAxis("Axis 1") < -0.2f)
            {
                action[1] = 1;
            }
            else if (Input.GetAxis("Axis 1") > 0.2f)
            {
                action[1] = 2;
            }
            else
                action[1] = 0;
        }
        // ----- Move ----- //


        // ----- Turn ----- //
        if (!useGamepad)
        {
            if (Input.GetKey(KeyCode.RightArrow))
            {
                action[1] = 1;
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                action[1] = 2;
            }
            else
                action[1] = 0;
        }
        else
        {
            if (Input.GetAxis("Axis 4") > 0.2f)
            {
                action[2] = 1;
            }
            else if (Input.GetAxis("Axis 4") < -0.2f)
            {
                action[2] = 2;
            }
            else
                action[2] = 0;
        }
        // ----- Turn ----- //


        // ----- Shoot ----- //
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetAxis("Axis 3") > 0.2f) // Bumper trigger Axis
        {
            action[3] = 1;
        }
        else if (Input.GetKey(KeyCode.Space) || Input.GetButton("Fire1")) // Reload (X-button on pad)
        {
            action[3] = 2;
        }
        else
            action[3] = 0;
        // ----- Shoot ----- //


        return action;
    }

    public bool randomAmmoOnSpawn;
    /// <summary>
    /// In the editor, if "Reset On Done" is checked then AgentReset() will be
    /// called automatically anytime we mark done = true in an agent script.
    /// </summary>
    public override void AgentReset()
    {
        var rotation = Random.Range(0, 4);
        var rotationAngle = rotation * 90f;
        area.transform.Rotate(new Vector3(0f, rotationAngle, 0f));

        transform.position = GetRandomSpawnPos(true);
        m_AgentRb.velocity = Vector3.zero;
        m_AgentRb.angularVelocity = Vector3.zero;

        //totalAmmoLeft = ammoSize;
        //hopperAmmoLeft = hopperSize;

        if (randomAmmoOnSpawn)
        {
            totalAmmoLeft = Mathf.RoundToInt(ammoSize * Random.Range(0.125f, 1f));
            hopperAmmoLeft = Mathf.Clamp(hopperSize, 0, totalAmmoLeft);
        }
        else
        {
            totalAmmoLeft = ammoSize;
            hopperAmmoLeft = hopperSize;
        }

        if (behavior.UsingHeuristic())
        {
            UiController._instance.totalAmmo = totalAmmoLeft;
            UiController._instance.hopperAmmo = hopperAmmoLeft;
        }

        SetResetParameters();
    }

    public void SetGroundMaterialFriction()
    {
        var resetParams = m_Academy.resetParameters;

        var groundCollider = ground.GetComponent<Collider>();

        groundCollider.material.dynamicFriction = resetParams["dynamic_friction"];
        groundCollider.material.staticFriction = resetParams["static_friction"];
    }

    public void SetResetParameters()
    {
        SetGroundMaterialFriction();
    }
}
