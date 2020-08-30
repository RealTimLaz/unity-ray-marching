using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class RayMarchingController : MonoBehaviour
{

    public ComputeShader rayMarchingShader;
    public int maxSteps;
    public float precision = 1.0f;

    private Camera cam;
    private RenderTexture target;
    private List<ComputeBuffer> buffersToDispose;


    struct ShapeData
    {
        public Matrix4x4 transformation;
        public Vector3 scale;
        public Vector3 color;
        public int shapeType;
        public int numTransforms;
        public float roughness;
        public float metalness;

        public static int GetSize()
        {
            return System.Runtime.InteropServices.Marshal.SizeOf(typeof(ShapeData));
        }
    }

    struct LightData
    {
        public Vector3 position;
        public int isDirectional;
        public Vector3 radiance;

        public static int GetSize()
        {
            return System.Runtime.InteropServices.Marshal.SizeOf(typeof(LightData));
        }
    }

    struct TransformData
    {
        public int transformType;
        public float scale;

        public static int GetSize()
        {
            return System.Runtime.InteropServices.Marshal.SizeOf(typeof(TransformData));
        }
    }

    private void LoadLightData()
    {
        List<Light> allLights = new List<Light>(FindObjectsOfType<Light>());
        LightData[] lightData;

        if (allLights.Count > 0)
        {
            lightData = new LightData[allLights.Count];

            for (int i = 0; i < allLights.Count; i++)
            {
                Light l = allLights[i];
                Vector3 radiance = new Vector3(l.color.r, l.color.g, l.color.b) * l.intensity * 2;
                radiance = Vector3.Scale(radiance, radiance);

                lightData[i] = new LightData()
                {
                    position = l.type == LightType.Directional ? l.transform.forward : l.transform.position,
                    isDirectional = l.type == LightType.Directional ? (byte)1 : (byte)0,
                    radiance = radiance
                };
            }

            rayMarchingShader.SetInt("numLights", lightData.Length);
        }
        else
        {
            lightData = new LightData[1];
            rayMarchingShader.SetInt("numLights", 0);
        }

        ComputeBuffer lightBuffer = new ComputeBuffer(lightData.Length, LightData.GetSize());
        lightBuffer.SetData(lightData);
        rayMarchingShader.SetBuffer(0, "lights", lightBuffer);
        buffersToDispose.Add(lightBuffer);
    }

    private void Init()
    {
        cam = GetComponent<Camera>();
        Camera.main.depthTextureMode = DepthTextureMode.Depth;
        cam.depthTextureMode = cam.depthTextureMode | DepthTextureMode.Depth;
        InitRenderTexture();
    }

    private void InitRenderTexture()
    {
        if (target == null || target.width != cam.pixelWidth || target.height != cam.pixelHeight)
        {
            if (target != null)
            {
                target.Release();
            }

            target = new RenderTexture(cam.pixelWidth, cam.pixelHeight, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.sRGB);
            target.enableRandomWrite = true;
            target.Create();
        }
    }

    private void SetShaderInputs(RenderTexture source)
    {
        rayMarchingShader.SetTexture(0, "Source", source);
        rayMarchingShader.SetTexture(0, "Destination", target);
        rayMarchingShader.SetTextureFromGlobal(0, "_DepthTexture", "_CameraDepthTexture");

        rayMarchingShader.SetMatrix("_CameraToWorld", cam.cameraToWorldMatrix);
        rayMarchingShader.SetMatrix("_CameraInverseProjection", cam.projectionMatrix.inverse);
        rayMarchingShader.SetVector("_CameraPosition", cam.transform.position);

        rayMarchingShader.SetInt("_MaxSteps", (int) (maxSteps * precision));
        rayMarchingShader.SetFloat("_Precision", precision);

        rayMarchingShader.SetFloat("_CameraFarClippingPlane", cam.farClipPlane);
        rayMarchingShader.SetFloat("_CameraNearClippingPlane", cam.nearClipPlane);
    }

    private void CreateScene()
    {
        List<Shape> allShapes = new List<Shape>(FindObjectsOfType<Shape>());
        int numTransforms = FindObjectsOfType<AdvancedTransform>().Length;

        int transformIndex = 0;

        ShapeData[] shapeData;
        TransformData[] transformData = null;
        

        if (allShapes.Count > 0)
        {
            shapeData = new ShapeData[allShapes.Count];
            
            if (numTransforms > 0)
            {
                transformData = new TransformData[numTransforms];
            }
            
            for (int i = 0; i < allShapes.Count; i++)
            {
                Shape s = allShapes[i];
                Vector3 col = new Vector3(s.color.r, s.color.g, s.color.b);

                Matrix4x4 transformationMatrix = (Matrix4x4.Translate(s.Position) * Matrix4x4.Rotate(s.Rotation)).inverse;
                List<AdvancedTransform> shapeTransforms = new List<AdvancedTransform>(s.GetComponentsInParent<AdvancedTransform>());

                shapeData[i] = new ShapeData()
                {
                    transformation = transformationMatrix,
                    scale = new Vector3(Mathf.Abs(s.Scale.x), Mathf.Abs(s.Scale.y), Mathf.Abs(s.Scale.z)),
                    color = col,
                    shapeType = (int)s.shapeType,
                    numTransforms = shapeTransforms.Count,
                    roughness = s.roughness,
                    metalness = s.metalness
                };

                if (shapeTransforms.Count > 0) {
                    foreach (AdvancedTransform t in shapeTransforms) {
                        transformData[transformIndex] = new TransformData()
                        {
                            transformType = (int)t.transformType,
                            scale = t.scale
                        };
                        transformIndex++;
                    }
                }
            }
            rayMarchingShader.SetInt("numShapes", shapeData.Length);
        }
        else
        {
            shapeData = new ShapeData[1];
            rayMarchingShader.SetInt("numShapes", 0);
        }

        if (numTransforms == 0)
        {
            transformData = new TransformData[1];
        }

        ComputeBuffer shapeBuffer = new ComputeBuffer(shapeData.Length, ShapeData.GetSize());
        shapeBuffer.SetData(shapeData);
        rayMarchingShader.SetBuffer(0, "shapes", shapeBuffer);
        buffersToDispose.Add(shapeBuffer);

        ComputeBuffer transformBuffer = new ComputeBuffer(transformData.Length, TransformData.GetSize());
        transformBuffer.SetData(transformData);
        rayMarchingShader.SetBuffer(0, "advancedTransforms", transformBuffer);
        buffersToDispose.Add(transformBuffer);

    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Init();

        buffersToDispose = new List<ComputeBuffer>();

        SetShaderInputs(source);
        CreateScene();
        LoadLightData();

        int threadGroupsX = Mathf.CeilToInt(cam.pixelWidth / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(cam.pixelHeight / 8.0f);
        rayMarchingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        Graphics.Blit(target, destination);

        foreach (ComputeBuffer b in buffersToDispose)
        {
            b.Dispose();
        }
    }



}
