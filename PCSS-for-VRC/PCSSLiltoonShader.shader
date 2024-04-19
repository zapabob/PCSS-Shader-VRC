// PCSSLightPlugin.cs
using UnityEngine;
using UnityEngine.Rendering;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class PCSSLightPlugin : MonoBehaviour
{
    public int resolution = 4096;
    public bool customShadowResolution = false;

    [Range(1, 64)]
    public int blockerSampleCount = 16;
    [Range(1, 64)]
    public int PCFSampleCount = 16;

    public Texture2D noiseTexture;

    [Range(0f, 1f)]
    public float softness = 0.5f;
    [Range(0f, 1f)]
    public float sampleRadius = 0.02f;

    [Range(0f, 1f)]
    public float maxStaticGradientBias = 0.05f;
    [Range(0f, 1f)]
    public float blockerGradientBias = 0f;
    [Range(0f, 1f)]
    public float PCFGradientBias = 1f;

    [Range(0f, 1f)]
    public float cascadeBlendDistance = 0.5f;

    public bool supportOrthographicProjection;

    public RenderTexture shadowRenderTexture;
    public RenderTextureFormat format = RenderTextureFormat.RFloat;
    public FilterMode filterMode = FilterMode.Bilinear;

    private int shadowmapPropID;
    private CommandBuffer copyShadowBuffer;
    private Light lightComponent;
    private GameObject avatarObject;

    private void OnEnable()
    {
        if (Application.isPlaying)
        {
            try
            {
                SetupLight();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error setting up PCSS light: {e.Message}");
            }
        }
    }

    private void OnDisable()
    {
        if (Application.isPlaying)
        {
            try
            {
                CleanupLight();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error cleaning up PCSS light: {e.Message}");
            }
        }
    }

    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            try
            {
                SetupLight();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error setting up PCSS light: {e.Message}");
            }
        }
    }
    #endif

    private void SetupLight()
    {
        lightComponent = GetComponent<Light>();
        if (lightComponent == null)
        {
            Debug.LogError("PCSSLightPlugin requires a Light component.");
            return;
        }

        if (customShadowResolution)
        {
            lightComponent.shadowCustomResolution = resolution;
        }
        else
        {
            lightComponent.shadowCustomResolution = 0;
        }

        shadowmapPropID = Shader.PropertyToID("_PCSShadowMap");
        copyShadowBuffer = new CommandBuffer();
        copyShadowBuffer.name = "PCSS Shadow Copy";

        lightComponent.AddCommandBuffer(LightEvent.AfterShadowMap, copyShadowBuffer);

        if (shadowRenderTexture == null)
        {
            shadowRenderTexture = new RenderTexture(resolution, resolution, 0, format);
            shadowRenderTexture.filterMode = filterMode;
            shadowRenderTexture.useMipMap = false;
            shadowRenderTexture.Create();
        }

        UpdateShaderProperties();
        UpdateCommandBuffer();
    }

    private void CleanupLight()
    {
        if (lightComponent != null)
        {
            lightComponent.RemoveCommandBuffer(LightEvent.AfterShadowMap, copyShadowBuffer);
        }

        if (shadowRenderTexture != null)
        {
            shadowRenderTexture.Release();
            DestroyImmediate(shadowRenderTexture);
            shadowRenderTexture = null;
        }
    }

    private void UpdateShaderProperties()
    {
        Shader.SetGlobalInt("_PCSSBlockerSampleCount", blockerSampleCount);
        Shader.SetGlobalInt("_PCSSPCFSampleCount", PCFSampleCount);

        Shader.SetGlobalFloat("_PCSSoftness", softness);
        Shader.SetGlobalFloat("_PCSSSampleRadius", sampleRadius);

        Shader.SetGlobalFloat("_PCSSMaxStaticGradientBias", maxStaticGradientBias);
        Shader.SetGlobalFloat("_PCSSBlockerGradientBias", blockerGradientBias);
        Shader.SetGlobalFloat("_PCSSPCFGradientBias", PCFGradientBias);

        Shader.SetGlobalFloat("_PCSSCascadeBlendDistance", cascadeBlendDistance);

        if (noiseTexture != null)
        {
            Shader.SetGlobalVector("_PCSSNoiseCoords", new Vector4(1f / noiseTexture.width, 1f / noiseTexture.height, 0f, 0f));
            Shader.SetGlobalTexture("_PCSSNoiseTexture", noiseTexture);
        }

        Shader.SetGlobalInt("_PCSSupportOrthographicProjection", supportOrthographicProjection ? 1 : 0);
    }

    private void UpdateCommandBuffer()
    {
        copyShadowBuffer.Clear();
        copyShadowBuffer.SetShadowSamplingMode(BuiltinRenderTextureType.CurrentActive, ShadowSamplingMode.RawDepth);
        copyShadowBuffer.Blit(BuiltinRenderTextureType.CurrentActive, shadowRenderTexture);
        copyShadowBuffer.SetGlobalTexture(shadowmapPropID, shadowRenderTexture);
    }

    private void Update()
    {
        if (avatarObject == null)
        {
            avatarObject = FindAvatarObject();
        }

        if (avatarObject != null)
        {
            float distance = Vector3.Distance(transform.position, avatarObject.transform.position);
            if (distance <= 10.0f)
            {
                // PCSSの効果を適用
                UpdateShaderProperties();
            }
            else
            {
                // PCSSの効果を徐々に減衰させる
                float t = Mathf.Clamp01((distance - 10.0f) / 5.0f);
                Shader.SetGlobalFloat("_PCSShadowStrength", Mathf.Lerp(1.0f, 0.0f, t));
            }
        }
    }

    private GameObject FindAvatarObject()
    {
        GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject rootObject in rootObjects)
        {
            if (rootObject.name == "Avatar")
            {
                return rootObject;
            }
        }
        return null;
    }
}