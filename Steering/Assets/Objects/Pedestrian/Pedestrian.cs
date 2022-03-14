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
    [SerializeField]
    float mSpeed;
    float maxSpeed;
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
                hits[i] = new RaycastObject(i - raycastFidelity / 2);
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
            foreach(RaycastObject hit in hits)
            {
                if (Physics.Raycast(transform.position, transform.TransformDirection(hit.dir * Vector3.forward), out hit.hit, 12, layerMask))
                {
                    Debug.DrawRay(transform.position, transform.TransformDirection(hit.dir * Vector3.forward) * hit.hit.distance, Color.red);
                    hit.UpdateObj(transform.position);
                    //Debug.Log("Hit " + hit.hit.collider.gameObject);
                }
                else
                {
                    Debug.DrawRay(transform.position, transform.TransformDirection(hit.dir * Vector3.forward) * 12, Color.green);
                    //Debug.Log("Did not Hit");
                }
            }
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
    public Quaternion dir;
    Vector3 pos;
    Vector3 vel;
    public GameObject obj;
    public RaycastHit hit;

    float ttca;
    float dca;

    public RaycastObject(int angle)
    {
        dir = Quaternion.Euler(0, angle, 0);
    }

    public void UpdateObj(Vector3 ownPos)
    { 
        obj = hit.collider.gameObject;
        pos = hit.point - ownPos;
        Pedestrian objScript = obj.GetComponent<Pedestrian>();
        if (objScript != null)
        {
            vel = objScript.GetVelocity();
            ttca = -Vector3.Dot(pos, vel) / Mathf.Pow(vel.magnitude, 2);
        }    
        else
        {
            vel = Vector3.zero;
            ttca = 0.0f;
        }

        dca = (pos + ttca * vel).magnitude;
            
        Debug.Log(ttca + "\n" + dca);
    }
}