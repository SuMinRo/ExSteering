using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pedestrian : MonoBehaviour
{
    int numberOfDebug;

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
    //alpha, speed, ttca, dca
    [SerializeField]
    Vector4 sigmas;
    Collider spawnerTarget;

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
        numberOfDebug = 2;
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
            spawnerTarget = GameObject.Find(target.ToString() + "Spawner").GetComponent<Collider>();
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
            Vector2 partialDerivativesObstacles = new Vector2(0.0f, 0.0f);
            int amountOfHits = 0;
            foreach (RaycastObject hit in hits)
            {
                if (Physics.Raycast(transform.position, transform.TransformDirection(hit.dir * Vector3.forward), out hit.hit, 12, layerMask))
                {
                    Debug.DrawRay(transform.position, transform.TransformDirection(hit.dir * Vector3.forward) * hit.hit.distance, Color.red);
                    hit.Perception();
                    amountOfHits++;
                    hit.Evaluation(sigmas[2], sigmas[3]);
                    partialDerivativesObstacles += hit.PartialDerivativeObstacles(sigmas[2], sigmas[3]);
                }
                else
                {
                    Debug.DrawRay(transform.position, transform.TransformDirection(hit.dir * Vector3.forward) * 12, Color.green);
                }
            }
            if(amountOfHits != 0)
                partialDerivativesObstacles /= amountOfHits;
            Vector2 partialDerivativesMovement = PartialDerivativeMovement(sigmas[0], sigmas[1]);
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

    /*
    float CostMovement(float alphaSigma, float speedSigma)
    {

        alphaG = Mathf.Acos(Vector3.Dot(Vector3.Normalize(spawnerTarget.ClosestPointOnBounds(transform.position) - transform.position), transform.forward));
        if (alphaG > Mathf.PI)
            alphaG -= 2 * Mathf.PI;

        float alphaCost = Mathf.Exp(-0.5f * Mathf.Pow(alphaG / alphaSigma, 2));
        //Debug.Log(alphaG);

        float speedCost = Mathf.Exp(-0.5f * Mathf.Pow((mSpeed - maxSpeed) / speedSigma, 2));
        return 1.0f - 0.5f * (alphaCost + speedCost);
    }*/

    Vector2 PartialDerivativeMovement(float alphaSigma, float speedSigma)
    {
        float alphaG  = Mathf.Acos(Vector3.Dot(Vector3.Normalize(spawnerTarget.ClosestPointOnBounds(transform.position) - transform.position), transform.forward));
        if (alphaG > Mathf.PI)
            alphaG -= 2 * Mathf.PI;
        float speed = ((mSpeed - maxSpeed) / (2 * speedSigma)) * Mathf.Exp(Mathf.Pow(-0.5f * (mSpeed - maxSpeed) / speedSigma, 2));
        float angle = (-alphaG / (2 * alphaSigma)) * Mathf.Exp(Mathf.Pow(-0.5f * alphaG / alphaSigma, 2));

        return new Vector2(speed, angle);
    }

    void Action(float newSpeed, float newAngle)
    { 
        mSpeed -= newSpeed;
        //Vector3 gradualRotation = Vector3.RotateTowards(transform.forward, Quaternion.Euler(0, newAngle * 180.0f / Mathf.PI, 0) * transform.forward, -Time.deltaTime, 0.0f);
        Debug.DrawRay(transform.position, Quaternion.Euler(0, newAngle * 180.0f / Mathf.PI, 0) * transform.forward, Color.blue);
        //transform.rotation = Quaternion.LookRotation(gradualRotation);
        transform.Rotate(Vector3.up, newAngle * 180.0f / Mathf.PI);
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
    int numberOfDebug;

    Pedestrian agent;
    Collider target;

    public Quaternion dir;
    Vector3 pos;
    Vector3 vel;
    public GameObject obj;
    public RaycastHit hit;

    float ttca;
    float dca;

    float cost;

    public RaycastObject(Pedestrian ped, int angle)
    {
        numberOfDebug = 1;

        agent = ped;

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

    public void Evaluation(float ttcaSigma, float dcaSigma)
    { 
        cost = Mathf.Exp(-0.5f * (Mathf.Pow(ttca / ttcaSigma, 2) + Mathf.Pow(dca / dcaSigma, 2)));
    }

    public Vector2 PartialDerivativeObstacles(float ttcaSigma, float dcaSigma)
    {
        Vector4 ttcaAndDcaPartialDerivatives = TtcaAndDcaPartialDerivatives();
        float speed = -cost * (ttcaAndDcaPartialDerivatives[0] * (ttca / Mathf.Pow(ttcaSigma, 2))
                                        + ttcaAndDcaPartialDerivatives[2] * (dca / Mathf.Pow(dcaSigma, 2)));
        float angle = -cost * (ttcaAndDcaPartialDerivatives[1] * (ttca / Mathf.Pow(ttcaSigma, 2))
                                        + ttcaAndDcaPartialDerivatives[3] * (dca / Mathf.Pow(dcaSigma, 2)));
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