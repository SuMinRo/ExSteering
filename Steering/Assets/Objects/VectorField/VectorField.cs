using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorField : MonoBehaviour
{
    Color[] debugColors = new Color[] { Color.black, Color.red, Color.green, Color.blue };

    GameObject plane;
    Vector2Int dims = new Vector2Int(11, 11);
    Vector3[] vertices;
    Vector2 bBoxMin;
    Vector2 bBoxMax;
    Vector2 cellSize;
    Vector2[,,] vectors;
    Vector2[,] explicitVectors = new Vector2[,] {  
                                            { new Vector2(-1, -1), new Vector2(-1, -1), new Vector2(-1, -1), new Vector2(-1, 0), new Vector2(-1, 0), new Vector2(-1, 0), new Vector2(-1, 0), new Vector2(-1, 0), new Vector2(-1, 1), new Vector2(-1, 1), new Vector2(-1, 1) }, 
                                            { new Vector2(-1, -1), new Vector2(-1, -1), new Vector2(-1, -1), new Vector2(-1, 0), new Vector2(-1, 0), new Vector2(-1, 0), new Vector2(-1, 0), new Vector2(-1, 0), new Vector2(-1, 1), new Vector2(-1, 1), new Vector2(-1, 1) }, 
                                            { new Vector2(-1, -1), new Vector2(-1, -1), new Vector2(-1, -1), new Vector2(-1, 0), new Vector2(-1, 0), new Vector2(-1, 0), new Vector2(-1, 0), new Vector2(-1, 0), new Vector2(-1, 1), new Vector2(-1, 1), new Vector2(-1, 1) }, 
                                            { new Vector2(-0.8f, -1), new Vector2(-0.8f, -1), new Vector2(-0.8f, -1), new Vector2(-1, -0.5f), new Vector2(-1, -0.25f), new Vector2(-1, 0), new Vector2(-1, 0.25f), new Vector2(-1, 0.5f), new Vector2(-0.8f, 1), new Vector2(-0.8f, 1), new Vector2(-0.8f, 1) }, 
                                            { new Vector2(-0.6f, -1), new Vector2(-0.6f, -1), new Vector2(-0.6f, -1), new Vector2(-1, -0.5f), new Vector2(-1, -0.25f), new Vector2(-1, 0), new Vector2(-1, 0.25f), new Vector2(-1, 0.5f), new Vector2(-0.8f, 1), new Vector2(-0.8f, 1), new Vector2(-0.8f, 1) }, 
                                            { new Vector2(-0.4f, -1), new Vector2(-0.4f, -1), new Vector2(-0.4f, -1), new Vector2(-1, -0.5f), new Vector2(-1, -0.25f), new Vector2(-1, 0), new Vector2(-1, 0.25f), new Vector2(-1, 0.5f), new Vector2(-0.8f, 1), new Vector2(-0.8f, 1), new Vector2(-0.8f, 1) }, 
                                            { new Vector2(-0.2f, -1), new Vector2(-0.2f, -1), new Vector2(-0.2f, -1), new Vector2(-1, -0.5f), new Vector2(-1, -0.25f), new Vector2(-1, 0), new Vector2(-1, 0.25f), new Vector2(-1, 0.5f), new Vector2(-0.8f, 1), new Vector2(-0.8f, 1), new Vector2(-0.8f, 1) }, 
                                            { new Vector2(0, -1), new Vector2(0, -1), new Vector2(0, -1), new Vector2(-1, -0.5f), new Vector2(-1, -0.25f), new Vector2(-1, 0), new Vector2(-1, 0.25f), new Vector2(-1, 0.5f), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1) }, 
                                            { new Vector2(1, -1), new Vector2(1, -1), new Vector2(1, -1), new Vector2(-1, 0), new Vector2(-1, 0), new Vector2(-1, 0), new Vector2(-1, 0), new Vector2(-1, 0), new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1) }, 
                                            { new Vector2(1, -1), new Vector2(1, -1), new Vector2(1, -1), new Vector2(-1, 0), new Vector2(-1, 0), new Vector2(-1, 0), new Vector2(-1, 0), new Vector2(-1, 0), new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1) }, 
                                            { new Vector2(1, -1), new Vector2(1, -1), new Vector2(1, -1), new Vector2(-1, 0), new Vector2(-1, 0), new Vector2(-1, 0), new Vector2(-1, 0), new Vector2(-1, 0), new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1) } 
                                            };

    // Start is called before the first frame update
    void Start()
    {
        plane = transform.Find("Plane").gameObject;
        vertices = plane.GetComponent<MeshFilter>().sharedMesh.vertices;
        bBoxMin = new Vector2(vertices[0][0], vertices[0][2]);
        bBoxMax = new Vector2(vertices[dims[0] * dims[1] - 1][0], vertices[dims[0] * dims[1] - 1][2]);
        cellSize = new Vector2((bBoxMax[0] - bBoxMin[0]) / (dims[0] - 1), (bBoxMax[1] - bBoxMin[1]) / (dims[1] - 1)); 
        vectors = new Vector2[4, dims[0], dims[1]];

        foreach (Cardinal dir in Cardinal.GetValues(typeof(Cardinal)))
        {
            RotateVectorField();
            for (int j = 0; j < dims[1]; j++)
            {
                for (int i = 0; i < dims[0]; i++)
                {
                    //vertices[dims[0]*j+i] -> vectors[i, j]
                    Vector3 vertex = vertices[dims[0] * j + i];
                    //vectors[i, j] = new Vector2(vertex[2], -vertex[0]).normalized;
                    vectors[(int)dir, i, j] = explicitVectors[i, j];
                    //Debug.Log(i + ", " + j + " -> " + vectors[i, j]);
                    //Debug.DrawLine(plane.transform.TransformPoint(vertex) + new Vector3(0, 0.5f, 0), plane.transform.TransformPoint(vertex) + new Vector3(vectors[(int)dir, i, j][0], 0.5f, vectors[(int)dir, i, j][1]), debugColors[(int)dir], 10000.0f, true);
                }
            }
        }
        //Debug.Log(vertices[0] + ", " + vertices[dims[1] - 1] + ", " + vertices[dims[1] * (dims[0] - 1)] + ", " + vertices[dims[0]*dims[1]-1]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector3 InterpolateSimple(Vector3 position, Cardinal dir)
    {
        Vector3 localPosition = plane.transform.InverseTransformPoint(position);
        //Debug.Log(position + " -> " + localPosition);
        Vector2 proximities = new Vector2((localPosition[0] - vertices[0][0]) / (vertices[dims[0] - 1][0] - vertices[0][0]), (localPosition[2] - vertices[0][2]) / (vertices[(dims[0] - 1) * dims[1]][2] - vertices[0][2]));
        //Debug.Log(localPosition + " -> " + proximities);
        Vector2 interpolatedVector = (vectors[(int)dir, 0, 0] * (1 - proximities[0]) + vectors[(int)dir, dims[0] - 1, 0] * proximities[0]) * (1 - proximities[1]) + (vectors[(int)dir, 0, dims[1] - 1] * (1 - proximities[0]) + vectors[(int)dir, dims[0] - 1, dims[1] - 1] * proximities[0]) * proximities[1];
        //Debug.Log(interpolatedVector);
        //Debug.DrawLine(position, position + new Vector3(interpolatedVector[0], 0, interpolatedVector[1])*3, Color.red, 0.0f, true);
        return new Vector3(interpolatedVector[0], 0, interpolatedVector[1]);
    }

    Vector2Int GetGridCell(Vector3 position)
    {
        return new Vector2Int((int) Mathf.Clamp(Mathf.Floor(((position[0] - bBoxMin[0]) / (bBoxMax[0] - bBoxMin[0])) * (dims[0] - 1)), 0, dims[0] - 2), (int) Mathf.Clamp(Mathf.Floor(((position[2] - bBoxMin[1]) / (bBoxMax[1] - bBoxMin[1])) * (dims[1] - 1)), 0, dims[1] - 2));
    }

    public Vector3 Interpolate(Vector3 position, Cardinal dir)
    {
        Vector3 localPosition = plane.transform.InverseTransformPoint(position);
        //Debug.Log(position + " -> " + localPosition);
        Vector2Int gridCell = GetGridCell(localPosition);
        //Debug.Log(localPosition + " -> " + gridCell);
        Vector2 proximities = new Vector2((localPosition[0] - vertices[dims[0] * gridCell[1] + gridCell[0]][0]) / (vertices[dims[0] * gridCell[1] + gridCell[0] + 1][0] - vertices[dims[0] * gridCell[1] + gridCell[0]][0]),
                                          (localPosition[2] - vertices[dims[0] * gridCell[1] + gridCell[0]][2]) / (vertices[dims[0] * (gridCell[1] + 1) + gridCell[0]][2] - vertices[dims[0] * gridCell[1] + gridCell[0]][2]));
        //Debug.Log(localPosition + " -> " + proximities);
        Vector2 interpolatedVector = (vectors[(int)dir, gridCell[0], gridCell[1]] * (1 - proximities[0]) + vectors[(int)dir, gridCell[0] + 1, gridCell[1]] * proximities[0]) * (1 - proximities[1]) + (vectors[(int)dir, gridCell[0], gridCell[1] + 1] * (1 - proximities[0]) + vectors[(int)dir, gridCell[0] + 1, gridCell[1] + 1] * proximities[0]) * proximities[1];
        //Debug.Log(interpolatedVector);
        //Debug.DrawLine(position, position + new Vector3(interpolatedVector[0], 0, interpolatedVector[1]) * 7, debugColors[(int)dir], 0.0f, true);
        return new Vector3(interpolatedVector[0], 0, interpolatedVector[1]);
    }

    //Rotates the Vector Field 90 degrees clockwise.
    void RotateVectorField()
    {
        Vector2Int centerIndices = dims / 2;
        //Switch places of all explicit vectors.
        for(int j = 0; j <= centerIndices[1]; j++)
        {
            for(int i = 0; i < centerIndices[0]; i++)
            {
                RotateVectorsPositions(new Vector2Int(i, j), centerIndices);
            }
        }

        //Rotates them accordingly.
        for (int j = 0; j < dims[1]; j++)
        {
            for (int i = 0; i < dims[0]; i++)
            {
                explicitVectors[i, j] = new Vector2(explicitVectors[i, j][1], -explicitVectors[i, j][0]);
            }
        }
    }

    void RotateVectorsPositions(Vector2Int indices, Vector2Int centerIndices)
    {
        //lowlow
        Vector2Int lowlow = indices;
        //Debug.Log("lowlow: " + lowlow);

        //lowhigh
        Vector2Int offset = lowlow - centerIndices;
        Vector2Int lowhigh = centerIndices + new Vector2Int(offset[1], -offset[0]);
        //Debug.Log("lowhigh: " + lowhigh);

        //highhigh
        offset = lowhigh - centerIndices;
        Vector2Int highhigh = centerIndices + new Vector2Int(offset[1], -offset[0]);
        //Debug.Log("highhigh: " + highhigh);

        //highlow
        offset = highhigh - centerIndices;
        Vector2Int highlow = centerIndices + new Vector2Int(offset[1], -offset[0]);
        //Debug.Log("highlow: " + highlow);

        //Swapping them around clockwise
        Vector2 temp = explicitVectors[lowlow[0], lowlow[1]];
        explicitVectors[lowlow[0], lowlow[1]] = explicitVectors[highlow[0], highlow[1]];
        explicitVectors[highlow[0], highlow[1]] = explicitVectors[highhigh[0], highhigh[1]];
        explicitVectors[highhigh[0], highhigh[1]] = explicitVectors[lowhigh[0], lowhigh[1]];
        explicitVectors[lowhigh[0], lowhigh[1]] = temp;
    }
}