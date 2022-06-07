using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pedestrian : MonoBehaviour
{
    public int numberOfDebug;

    public Cardinal source;
    public Cardinal target;
    public Vector3 origin;
    public Strategy strat;
    public SteeringAlgorithm method;
    public float startTime;
    public float distanceTraveled;
    
    public float mSpeed;
    [HideInInspector]
    public float maxSpeed;
    VectorField vectorField;

    RaycastObject[] hits;
    [SerializeField]
    int raycastFidelity;
    //alpha, speed, ttca, dca
    Vector4 sigmas;
    Collider spawnerTarget;
    bool skip;
    Stats stats;

    [HideInInspector]
    public Vector3 frontVector;
    [HideInInspector]
    public bool wallDetected;
    [HideInInspector]
    public Vector3 sideVector;
    [HideInInspector]
    public Vector3 closeVector;
    Vector3 newDir;
    float frontRng;
    float sideRng;
    [SerializeField]
    FrontDetector frontDetector;
    [SerializeField]
    SideDetector sideDetector;
    [SerializeField]
    CloseDetector closeDetector;
    [HideInInspector]
    public Collider frontThreat;
    [HideInInspector]
    public Collider sideThreat;
    [HideInInspector]
    public bool avoidFrontCongestion;
    float maxRotationDeviationCos;

    // Start is called before the first frame update
    void Start()
    {
        numberOfDebug = 1;
        if(method == SteeringAlgorithm.DevRules)
        {
            GameObject plane = GameObject.Find("VectorField");
            vectorField = plane.GetComponent<VectorField>();
            NewSeed(true);
            NewSeed(false);
            frontThreat = null;
            sideThreat = null;
            int maxRotationDeviationRNG = Random.Range(0, 62);
            if (maxRotationDeviationRNG < 6)
                maxRotationDeviationCos = Mathf.Cos(15.0f * Mathf.PI / 180.0f);
            else if (maxRotationDeviationRNG < 37)
                maxRotationDeviationCos = Mathf.Cos(45.0f * Mathf.PI / 180.0f);
            else
                maxRotationDeviationCos = Mathf.Cos(75.0f * Mathf.PI / 180.0f);
            if (gameObject.name[7] != '0')
            {
                gameObject.transform.Find("FrontDetector").gameObject.GetComponent<Renderer>().enabled = false;
                gameObject.transform.Find("SideDetector").gameObject.GetComponent<Renderer>().enabled = false;
                gameObject.transform.Find("CloseDetector").gameObject.GetComponent<Renderer>().enabled = false;
            }
        }
        else
        {
            if (strat == Strategy.None)
            {
                gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                gameObject.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                gameObject.transform.GetChild(0).GetChild(0).gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            }

            hits = new RaycastObject[raycastFidelity + 1];
            for(int i = 0; i <= raycastFidelity; i++)
            {
                hits[i] = new RaycastObject(this, i - raycastFidelity / 2);
            }
            gameObject.transform.Find("FrontDetector").gameObject.SetActive(false);
            gameObject.transform.Find("SideDetector").gameObject.SetActive(false);
            gameObject.transform.Find("CloseDetector").gameObject.SetActive(false);
            spawnerTarget = GameObject.Find(target.ToString() + "Spawner").GetComponent<Collider>();
            stats = GameObject.Find("Stats").GetComponent<Stats>();
            sigmas = stats.GetSigmas();
        }
        
        maxSpeed = mSpeed;

        frontVector = Vector3.zero;
        startTime = Time.time;
        distanceTraveled = 0.0f;
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
            float rotateFactor = 1.0f;
            if (closeVector.magnitude > 0.0f && mSpeed > 0.0f && strat != Strategy.None)
            {
                mSpeed -= 0.125f;
                rotateFactor = 3.0f;
            }
            else if (mSpeed < maxSpeed)
                mSpeed += 0.125f;

            if(strat != Strategy.None)
            {
                frontDetector.UpdatePriority(avoidFrontCongestion);
                sideDetector.UpdatePriority();
            }
            
            if(frontVector.magnitude != 0.0f)
            {
                int actionResponse = PeekAction(true, 1);
                if (!wallDetected)
                {
                    GameObject threatObject = frontThreat.gameObject;
                    if (CollisionCounter.StringLessThan(gameObject.name, threatObject.name))
                        actionResponse = evalCoop(actionResponse, threatObject.GetComponent<Pedestrian>().PeekAction(true, 1), true);
                }
                if (actionResponse == 1)
                    newDir = Vector3.RotateTowards(transform.forward, frontVector, Time.deltaTime * rotateFactor, 0.0f);
                else
                    newDir = Vector3.RotateTowards(transform.forward, vectorField.Interpolate(transform.position, target), Time.deltaTime * rotateFactor, 0.0f);
            }
            else if(sideVector.magnitude != 0.0f)
            {
                int actionResponse = PeekAction(true, 0);
                GameObject threatObject = sideThreat.gameObject;
                if (CollisionCounter.StringLessThan(gameObject.name, threatObject.name))
                    actionResponse = evalCoop(actionResponse, threatObject.GetComponent<Pedestrian>().PeekAction(true, 0), false);
                if (actionResponse == -1)
                    newDir = Vector3.RotateTowards(transform.forward, sideVector, Time.deltaTime * rotateFactor, 0.0f);
                else if(actionResponse == 1)
                    newDir = Vector3.RotateTowards(transform.forward, sideVector, -Time.deltaTime * rotateFactor, 0.0f);
                else
                    newDir = Vector3.RotateTowards(transform.forward, vectorField.Interpolate(transform.position, target), Time.deltaTime * rotateFactor, 0.0f);
            }
            else
            {
                newDir = Vector3.RotateTowards(transform.forward, vectorField.Interpolate(transform.position, target), Time.deltaTime * rotateFactor, 0.0f);
            }

            if(newDir != Vector3.zero)
            {
                if (CheckMaxRotation(newDir))
                    transform.rotation = Quaternion.LookRotation(newDir);
                else
                    transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, vectorField.Interpolate(transform.position, target), Time.deltaTime * rotateFactor, 0.0f));
            }
                
        }
        else if (method == SteeringAlgorithm.Gradient)
        {
            int layerMask = 1 << 0;
            Vector2 partialDerivativesObstacles = new Vector2(0.0f, 0.0f);
            int amountOfHits = 0;
            foreach (RaycastObject hit in hits)
            {
                if (Physics.Raycast(transform.position, transform.TransformDirection(hit.dir * Vector3.forward), out hit.hit, 12, layerMask))
                {
                    if (gameObject.name[7] == '0')
                        Debug.DrawRay(transform.position, transform.TransformDirection(hit.dir * Vector3.forward) * hit.hit.distance, Color.red);
                    hit.Perception();
                    amountOfHits++;
                    hit.Evaluation(sigmas[2], sigmas[3]);
                    partialDerivativesObstacles += hit.PartialDerivativeObstacles(sigmas[2], sigmas[3]);
                }
                else
                {
                    if (gameObject.name[7] == '0')
                        Debug.DrawRay(transform.position, transform.TransformDirection(hit.dir * Vector3.forward) * 12, Color.green);
                }
            }
            if(amountOfHits != 0)
                partialDerivativesObstacles /= amountOfHits;
            
            Vector2 partialDerivativesMovement = PartialDerivativeMovement(sigmas[0], sigmas[1]);
            if(float.IsNaN(partialDerivativesMovement[0] + partialDerivativesMovement[1] + partialDerivativesObstacles[0] + partialDerivativesObstacles[1]) || float.IsInfinity(partialDerivativesMovement[0] + partialDerivativesMovement[1] + partialDerivativesObstacles[0] + partialDerivativesObstacles[1]))
            {
                //Debug.Log(partialDerivativesMovement + "\n" + partialDerivativesObstacles);
            }
            
            Action(partialDerivativesMovement[0] + partialDerivativesObstacles[0], partialDerivativesMovement[1] + partialDerivativesObstacles[1]);
        }

        Vector3 distanceMoved = transform.forward * Time.deltaTime * mSpeed;
        transform.position += distanceMoved;
        distanceTraveled += distanceMoved.magnitude;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (Time.timeScale == 0.0f)
                Time.timeScale = 1.0f;
            else
                Time.timeScale = 0.0f;
        }

        //Debug.Log(rng);
    }

    bool CheckMaxRotation(Vector3 dir)
    {
        if (Vector3.Dot(vectorField.Interpolate(transform.position, target).normalized, transform.forward) < maxRotationDeviationCos)
            return false;
        return true;
    }

    public void NewSeed(bool front)
    {
        if (front) frontRng = Random.Range(0.0f, 1.0f);
        else sideRng = Random.Range(0.0f, 1.0f);
    }

    public void UpdateThreat(Collider coll, bool front)
    {
        if (front)
        {
            if (frontThreat == null || coll != frontThreat)
            {
                frontThreat = coll;
                NewSeed(front);
            }
        }
        else
        {
            if (sideThreat == null || coll != sideThreat)
            {
                sideThreat = coll;
                NewSeed(front);
            }
        }
        
    }

    //direction = -1, 0, 1. -1 same, 0 perp, 1 opp.
    //Returns int depending on action. -1 towards (if applicable), 0 no action, 1 away.
    public int PeekAction(bool sus, int direction)
    {
        if(sus)
        {
            if(direction == 1)
            {
                if (frontRng < 0.7407407407 || wallDetected) return 1;
                else return 0;
            }
            else // == 0
            {
                if (sideRng < 0.4748201439) return -1;
                else if (sideRng < 0.7248201439) return 1;
                else return 0;
            }
        }
        else
        {
            if (direction == 1)
            {
                if (frontRng < 0.7525773196) return 1;
                else return 0;
            }
            else if (direction == 0)
            {
                if (sideRng < 0.3689839572) return -1;
                else if (sideRng < 0.679144385) return 1;
                else return 0;
            }
            else // == -1
            {
                if (frontRng < 0.7252734398) return 1;
                else return 0;
            }
        }
    }

    int evalCoop(int me, int other, bool front)
    {
        //NOT FINISHED
        if(front)
        {
            if(other == 0)
            {
                //Returns 1 to make at least one avoid.
                if(closeDetector.SearchCollider(frontThreat))
                    transform.position += -Vector3.Project(frontThreat.transform.position - transform.position, transform.right).normalized * Time.deltaTime * mSpeed;
                return 1;
            }
        }
        else
        {
            if (me == other)
            {
                //Returns whatever else is not already chosen.
                return ((me + Random.Range(2, 4)) % 3) - 1;
            }
        }
        return me;
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
        Vector3 targetVector = spawnerTarget.ClosestPointOnBounds(transform.position) - transform.position;
        float alphaG  = Mathf.Acos(Mathf.Clamp(Vector3.Dot(Vector3.Normalize(targetVector), transform.forward), -1.0f, 1.0f));
        if (Vector3.Cross(transform.forward, targetVector).y > 0)
            alphaG = -alphaG;
        //Debug.DrawLine(transform.position, targetVector + transform.position, Color.magenta);
        float speed = ((mSpeed - maxSpeed) / (2 * speedSigma)) * Mathf.Exp(Mathf.Pow(-0.5f * (mSpeed - maxSpeed) / speedSigma, 2));
        float angle = (-alphaG / (2 * alphaSigma)) * Mathf.Exp(Mathf.Pow(-0.5f * alphaG / alphaSigma, 2));
        return new Vector2(speed, angle);
    }

    void Action(float newSpeed, float newAngle)
    { 
        mSpeed -= newSpeed;
        if (mSpeed < 0.0f) mSpeed = 0.0f;
        Vector3 gradualRotation = Vector3.RotateTowards(transform.forward, Quaternion.Euler(0, (newAngle + Random.Range(-0.1f, 0.1f)) * 180.0f / Mathf.PI, 0) * transform.forward, mSpeed * Time.deltaTime, 0.0f);
        //Debug.DrawRay(transform.position, Quaternion.Euler(0, newAngle * 180.0f / Mathf.PI, 0) * transform.forward, Color.blue);
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

    bool showRayCast;

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
        //pos = pos.normalized * (pos.magnitude - 0.5f);
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
        dca = (pos + ttca * vel).magnitude + Mathf.Epsilon;
    }

    public void Evaluation(float ttcaSigma, float dcaSigma)
    { 
        cost = Mathf.Exp(-0.5f * (Mathf.Pow(ttca / ttcaSigma, 2) + Mathf.Pow(dca / dcaSigma, 2)));
    }

    public Vector2 PartialDerivativeObstacles(float ttcaSigma, float dcaSigma)
    {
        Vector4 ttcaAndDcaPartialDerivatives = TtcaAndDcaPartialDerivatives();
        //if(float.IsNaN(ttcaAndDcaPartialDerivatives[0] + ttcaAndDcaPartialDerivatives[1] + ttcaAndDcaPartialDerivatives[2] + ttcaAndDcaPartialDerivatives[3]) || float.IsInfinity(ttcaAndDcaPartialDerivatives[0] + ttcaAndDcaPartialDerivatives[1] + ttcaAndDcaPartialDerivatives[2] + ttcaAndDcaPartialDerivatives[3]))
        //    Debug.Log(ttcaAndDcaPartialDerivatives + "\n" + vel + ", " + ttca + ", " + dca);
        float speed = ClampMax(-cost * (ttcaAndDcaPartialDerivatives[0] * (ttca / Mathf.Pow(ttcaSigma, 2))
                                        + ttcaAndDcaPartialDerivatives[2] * (dca / Mathf.Pow(dcaSigma, 2))));
        float angle = ClampMax(-cost * (ttcaAndDcaPartialDerivatives[1] * (ttca / Mathf.Pow(ttcaSigma, 2))
                                        + ttcaAndDcaPartialDerivatives[3] * (dca / Mathf.Pow(dcaSigma, 2))));
        return new Vector2(speed, angle);
    }

    public Vector4 TtcaAndDcaPartialDerivatives()
    {
        float ttcaSpeed = ClampMax(Vector3.Dot(pos + 2 * ttca * vel, agent.transform.forward) / (Vector3.Dot(vel, vel)+Mathf.Epsilon));
        float ttcaAngle = ClampMax(Vector3.Dot(pos + 2 * ttca * vel, new Vector3(agent.transform.forward.z, 0, -agent.transform.forward.x)) / (Vector3.Dot(vel, vel) + Mathf.Epsilon));
        float dcaSpeed = ClampMax(Vector3.Dot(pos + ttca * vel, ttcaSpeed * vel - ttca * agent.transform.forward) / dca);
        float dcaAngle = ClampMax(Vector3.Dot(pos + ttca * vel, ttcaSpeed * vel + ttca * new Vector3(agent.transform.forward.z, 0, -agent.transform.forward.x)) / dca);
        return new Vector4(ttcaSpeed, ttcaAngle, dcaSpeed, dcaAngle);
    }

    float ClampMax(float val)
    {
        return Mathf.Clamp(val, float.MinValue, float.MaxValue);
    }
}