using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateSectorColliderMesh : MonoBehaviour
{
    public float radius;
    [SerializeField]
    float angle;

    // Start is called before the first frame update
    void Awake()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length - 1];

        for (int i = 1; i < meshFilters.Length; i++)
        {
            var vertices = new Vector3[meshFilters[i].mesh.vertices.Length];
            for (int j = 0; j < meshFilters[i].mesh.vertices.Length; j++)
            {
                var vertex = meshFilters[i].mesh.vertices[j];
                var smallAngle = Mathf.PI * angle / (24 * 180);
                var bigAngle = smallAngle * 2;

                vertex.x = vertex.x * radius * Mathf.Sin(smallAngle) * 2;
                var zRadius = radius * Mathf.Cos(smallAngle);
                vertex.z = vertex.z * zRadius + zRadius / 2;

                var rotationAngle = -(Mathf.PI * angle / 360 + smallAngle) + i * bigAngle;
                var newX = vertex.x * Mathf.Cos(rotationAngle) - vertex.z * Mathf.Sin(rotationAngle);
                var newZ = vertex.x * Mathf.Sin(rotationAngle) + vertex.z * Mathf.Cos(rotationAngle);

                vertex.x = newX;
                vertex.z = newZ;

                vertex.y = vertex.y * 0.01f;

                vertices[j] = vertex;
            }
            meshFilters[i].mesh.vertices = vertices;

            combine[i - 1].mesh = meshFilters[i].mesh;
            combine[i - 1].transform = meshFilters[i].transform.localToWorldMatrix;

            meshFilters[i].gameObject.SetActive(false);
        }
        transform.GetComponent<MeshFilter>().mesh = new Mesh();
        GetComponent<MeshFilter>().mesh.CombineMeshes(combine, true, false, false);
    }

    // Update is called once per frame
    public Mesh GetMesh()
    {
        return GetComponent<MeshFilter>().mesh;
    }
}
