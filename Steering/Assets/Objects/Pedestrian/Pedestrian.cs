using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pedestrian : MonoBehaviour
{
    Cardinal source;
    [HideInInspector]
    public Cardinal target;
    public Strategy strat;
    public SteeringAlgorithm method;
    
    public float mSpeed;
    [HideInInspector]
    public float maxSpeed;
    VectorField vectorField;

    RaycastObject[] hits;
    [SerializeField]
    int raycastFidelity;

    [HideInInspector]
    public Vector3 frontVector;
    [HideInInspector]
    public bool wallDetected;
    [HideInInspector]
    public Vector3 sideVector;
    [HideInInspector]
    public Vector3 closeVector;
    Vector3 newDir;

    [HideInInspector]
    public float startTime;

    // Start is called before the first frame update
    void Start()
    {
        if(method == SteeringAlgorithm.DevRules)
        {
            GameObject plane = GameObject.Find("VectorField");
            vectorField = plane.GetComponent<VectorField>();
        }
        else
        {
            hits = new RaycastObject[raycastFidelity + 1];
            for(int i = 0; i <= raycastFidelity; i++)
            {
                hits[i] = new RaycastObject(this, i - raycastFidelity / 2);
            }
            gameObject.transform.Find("FrontDetector").gameObject.SetActive(false);
            gameObject.transform.Find("SideDetector").gameObject.SetActive(false);
            gameObject.transform.Find("CloseDetector").gameObject.SetActive(false);
        }
        
        maxSpeed = mSpeed;

        frontVector = Vector3.zero;
        startTime = Time.time;
    }

    public void SetTargetAndSource(Cardinal spawner, Cardinal goal) 
    {
        source = spawner;
        target = goal;
    }


    // For Gradient-use only.
    public Vector3 GetVelocity()
    {
        return transform.forward * mSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.RightArrow))
        {
            //Rotate the sprite about the Y axis in the positive direction
            transform.Rotate(new Vector3(0, 20, 0) * Time.deltaTime * mSpeed, Space.World);
        }

        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            //Rotate the sprite about the Y axis in the negative direction
            transform.Rotate(new Vector3(0, -20, 0) * Time.deltaTime * mSpeed, Space.World);
        }
        //Autonomous behaviour
        else if (method == SteeringAlgorithm.DevRules)
        {
            if(frontVector.magnitude != 0.0f)
            {
                newDir = Vector3.RotateTowards(transform.forward, frontVector, Time.deltaTime, 0.0f);
            }
            else if(sideVector.magnitude != 0.0f)
            {
                newDir = Vector3.RotateTowards(transform.forward, sideVector, Time.deltaTime, 0.0f);
            }
            else
            {
                newDir = Vector3.RotateTowards(transform.forward, vectorField.Interpolate(transform.position, target), Time.deltaTime, 0.0f);
            }

            if (closeVector.magnitude > 0.0f && maxSpeed > 0.0f)
                maxSpeed -= 0.25f;
            else if (maxSpeed < mSpeed)
                maxSpeed += 0.25f;

            transform.rotation = Quaternion.LookRotation(newDir);
        }
        else
        {
            int layerMask = 1 << 0;
            Vector2 partialDerivativesMovement = Vector2.zero;
            Vector2 partialDerivativesObstacles = Vector2.zero;
            int amountOfHits
            foreach (RaycastObject hit in hits)
            {
                partialDerivativesMovement += hit.PartialDerivativeMovement();
                if (Physics.Raycast(transform.position, transform.TransformDirection(hit.dir * Vector3.forward), out hit.hit, 12, layerMask))
                {
                    Debug.DrawRay(transform.position, transform.TransformDirection(hit.dir * Vector3.forward) * hit.hit.distance, Color.red);
                    hit.Perception(true);
                }
                else
                {
                    Debug.DrawRay(transform.position, transform.TransformDirection(hit.dir * Vector3.forward) * 12, Color.green);
                }

                hit.Evaluation();
                partialDerivativesObstacles += hit.PartialDerivativeObstacles();

            }
            partialDerivativesMovement /= raycastFidelity + 1;
            partialDerivativesObstacles /= raycastFidelity + 1;
            Action(partialDerivativesMovement[0] + partialDerivativesObstacles[0], partialDerivativesMovement[1] + partialDerivativesObstacles[1]);
        }
        transform.position += transform.forward * Time.deltaTime * maxSpeed;

        if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (Time.timeScale == 0.0f)
                Time.timeScale = 1.0f;
            else
                Time.timeScale = 0.0f;
        }
    }

    public void Action(float newSpeed, float newAngle)
    { 
        mSpeed -= newSpeed;
        Vector3 gradualRotation = Vector3.RotateTowards(transform.forward, Quaternion.Euler(0, newAngle * 180.0f / Mathf.PI, 0) * transform.forward, -Time.deltaTime, 0.0f);
        transform.rotation = Quaternion.LookRotation(gradualRotation);
    }
}

public enum Strategy
{
    Priority,
    PriorityDither,
    Blend,
    BlendWeights,
    None
}

public enum SteeringAlgorithm
{
    DevRules,
    Gradient
}

// For Gradient-use only.
class RaycastObject
{
    bool debug = false;

    Pedestrian agent;
    Collider target;

    public Quaternion dir;
    Vector3 pos;
    Vector3 vel;
    public GameObject obj;
    public RaycastHit hit;

    float ttca;
    float dca;

    float costObstacles;
    float cost;

    float alphaG;

    public RaycastObject(Pedestrian ped, int angle)
    {
        agent = ped;
        target = GameObject.Find(agent.target.ToString() + "Spawner").GetComponent<Collider>(); ;

        dir = Quaternion.Euler(0, angle, 0);
        ttca = float.MaxValue;
        dca = float.MaxValue;
        cost = float.MaxValue;
    }

    public void Perception()
    {
        
        obj = hit.collider.gameObject;
        pos = hit.point - agent.transform.position;
        Pedestrian objScript = obj.GetComponent<Pedestrian>();
        if (objScript != null)
        {
            vel = objScript.GetVelocity() - agent.transform.forward * agent.mSpeed;
        }
        else
        {
            vel = -agent.transform.forward * agent.mSpeed;
        }
        

        if (vel != Vector3.zero)
            ttca = -Vector3.Dot(pos, vel) / Mathf.Pow(vel.magnitude, 2);
        else
            ttca = 0.0f;
        dca = (pos + ttca * vel).magnitude;
    }

    public void Evaluation()
    {
        //Angle then speed.        
        cost = CostMovement(2.0f, 3.3f) + CostObstacles(1.8f, 0.3f);
    }

    float CostMovement(float alphaSigma, float speedSigma)
    {
         
        alphaG = Mathf.Acos(Vector3.Dot(Vector3.Normalize(target.ClosestPointOnBounds(agent.transform.position) - agent.transform.position), agent.transform.forward)) + dir.eulerAngles[1] * Mathf.PI / 180;
        if (alphaG > Mathf.PI)
            alphaG -= 2 * Mathf.PI;

        float alphaCost = Mathf.Exp(-0.5f * Mathf.Pow(alphaG / alphaSigma, 2));
        //Debug.Log(alphaG);

        float speedCost = Mathf.Exp(-0.5f * Mathf.Pow((agent.mSpeed - agent.maxSpeed) / speedSigma, 2));
        return 1.0f - 0.5f * (alphaCost + speedCost);
    }

    float CostObstacles(float ttcaSigma, float dcaSigma)
    {
        costObstacles = Mathf.Exp(-0.5f * (Mathf.Pow(ttca / ttcaSigma, 2) + Mathf.Pow(dca / dcaSigma, 2)));
        return costObstacles;
    }

    public Vector2 PartialDerivativeMovement()
    {
        float speed = ((agent.mSpeed - agent.maxSpeed) / (2 * 3.3f)) * Mathf.Exp(Mathf.Pow(-0.5f * (agent.mSpeed - agent.maxSpeed) / 3.3f, 2));
        float angle = (-alphaG/(2 * 2.0f)) * Mathf.Exp(Mathf.Pow(-0.5f * alphaG / 2.0f, 2));
        
        return new Vector2(speed, angle);
    }

    public Vector2 PartialDerivativeObstacles()
    {
        Vector4 ttcaAndDcaPartialDerivatives = TtcaAndDcaPartialDerivatives();
        float speed = -costObstacles * (ttcaAndDcaPartialDerivatives[0] * (ttca / Mathf.Pow(1.8f, 2))
                                        + ttcaAndDcaPartialDerivatives[2] * (dca / Mathf.Pow(0.3f, 2)));
        float angle = -costObstacles * (ttcaAndDcaPartialDerivatives[1] * (ttca / Mathf.Pow(1.8f, 2))
                                        + ttcaAndDcaPartialDerivatives[3] * (dca / Mathf.Pow(0.3f, 2)));
        return new Vector2(speed, angle);
    }

    public Vector4 TtcaAndDcaPartialDerivatives()
    {
        float ttcaSpeed = Vector3.Dot(pos + 2 * ttca * vel, agent.transform.forward) / Vector3.Dot(vel, vel);
        float ttcaAngle = Vector3.Dot(pos + 2 * ttca * vel, new Vector3(agent.transform.forward.z, 0, -agent.transform.forward.x)) / Vector3.Dot(vel, vel);
        float dcaSpeed = Vector3.Dot(pos + ttca * vel, ttcaSpeed * vel - ttca * agent.transform.forward) / dca;
        float dcaAngle = Vector3.Dot(pos + ttca * vel, ttcaSpeed * vel + ttca * new Vector3(agent.transform.forward.z, 0, -agent.transform.forward.x)) / dca;
        return new Vector4(ttcaSpeed, ttcaAngle, dcaSpeed, dcaAngle);
    }
}