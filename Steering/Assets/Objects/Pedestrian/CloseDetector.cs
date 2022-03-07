using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class CloseDetector : MonoBehaviour
{
    [SerializeField]
    GameObject parent;
    Pedestrian pedestrian;
    private HashSet<Collider> colliders = new HashSet<Collider>();
    Vector3 closestColliderVector;

    // Start is called before the first frame update
    void Start()
    {
        pedestrian = parent.GetComponent<Pedestrian>();
    }

    // Update is called once per frame
    void Update()
    {
        closestColliderVector = Vector3.zero;
        
        foreach (Collider collide in colliders.ToList())
        {
            if (collide == null)
            {
                colliders.Remove(collide);
                continue;
            }

            Vector3 colliderVector = collide.transform.position - pedestrian.transform.position;
            if (Vector3.Dot(Vector3.Normalize(colliderVector), pedestrian.transform.forward) > Mathf.Cos(Mathf.PI/12) && Vector3.Dot(collide.transform.forward, parent.transform.forward) > 0 && (closestColliderVector.magnitude > colliderVector.magnitude || closestColliderVector == Vector3.zero))
                closestColliderVector = colliderVector;
        }
        //Debug.Log(closestColliderVector);

        pedestrian.closeVector = closestColliderVector;
    }

    void OnTriggerEnter(Collider collide)
    {
        if (collide.gameObject.tag == "Pedestrian")
        {
            colliders.Add(collide);
        }
    }

    void OnTriggerExit(Collider collide)
    {
        if (collide.gameObject.tag == "Pedestrian")
        {
            colliders.Remove(collide);
        }
    }
}
