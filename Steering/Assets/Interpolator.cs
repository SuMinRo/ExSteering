using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interpolator : MonoBehaviour
{
    [SerializeField]
    Collider north;

    public void StreamLines(VectorField vf)
    {
        for (int j = 0; j < 1000; j++)
        {
            float x;
            float z;
            do
            {
                x = Random.Range(-25, 25);
                z = Random.Range(-25, 25);
            } while (Mathf.Abs(x) > 10 && Mathf.Abs(z) > 10);
            Vector3 pos = new Vector3(x, 1, z);
            for (int i = 0; i < 100; i++)
            {
                Vector3 vector = vf.Interpolate(pos, Cardinal.North).normalized;
                Debug.DrawRay(pos, vector * 0.1f, BlendColors(Vector3.Distance(north.ClosestPoint(pos), pos)), 1000.0f, true);
                pos = pos + vector * 0.1f;
                if (pos.x < -25 || pos.x > 25 || pos.z < -25 || pos.z > 25)
                    break;
            }
        }
    }

    Color BlendColors(float distance)
    {
        Color c = new Color(1, 0, 1);
        if (distance > 25)
            c.b = c.b - (distance - 25.0f) / 25.0f;
        else
            c.r = c.r - (25.0f - distance) / 25.0f;
        return c;
    }
}
