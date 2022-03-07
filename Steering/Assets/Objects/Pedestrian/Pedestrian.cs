using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pedestrian : MonoBehaviour
{
    Cardinal source;
    [HideInInspector]
    public Cardinal target;
    public Strategy strat;
    [SerializeField]
    float mSpeed;
    float maxSpeed;
    VectorField vectorField;

    [HideInInspector]
    public Vector3 frontVector;
    [HideInInspector]
    public bool wallDetected;
    [HideInInspector]
    public Vector3 sideVector;
    [HideInInspector]
    public Vector3 closeVector;
    Vector3 newDir;

    // Start is called before the first frame update
    void Start()
    {
        GameObject plane = GameObject.Find("VectorField");
        vectorField = plane.GetComponent<VectorField>();
        maxSpeed = mSpeed;

        frontVector = Vector3.zero;
    }

    public void SetTargetAndSource(Cardinal spawner, Cardinal goal) 
    {
        source = spawner;
        target = goal;
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
        else
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
