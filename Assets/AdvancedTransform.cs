using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AdvancedTransform : MonoBehaviour
{
    public enum TransformType {Twist, Bend, Repeat};

    public TransformType transformType;
    public float scale = 1.0f;

}
