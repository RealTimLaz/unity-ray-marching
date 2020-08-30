using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.TerrainAPI;
using UnityEngine;

[ExecuteInEditMode]
public class Shape : MonoBehaviour
{
    public enum ShapeType {Sphere, Cube, Torus, MandelBulb};

    public ShapeType shapeType;

    [Range(0, 1)]
    public float roughness = 0.0f;
    [Range(0, 1)]
    public float metalness = 0.0f;

    public Color color = Color.white;

    public Vector3 Position
    {
        get
        {
            return transform.position;
        }
    }

    public Vector3 Scale
    {
        get
        {
            return transform.localScale;
        }
    }

    public Quaternion Rotation
    {
        get
        {
            return transform.localRotation;
        }
    }
}
