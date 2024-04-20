// PCSSLightPlugin.cs
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Collections.Generic;

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
    private VRCAvatarDescriptor avatarDescriptor;

    private readonly Dictionary<int, float> shadowStrengthCache = new Dictionary<int, float>();
    private const int MaxShadowStrengthCache = 1000;

    private void OnEnable()
    {
        if (Application.isPlaying)
        {
            SetupLight();
        }
    }

    private void OnDisable()
    {
        if (Application.isPlaying)
        {
            CleanupLight();
        }
    }

    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            SetupLight();
        }
    }
    #endif

    private void SetupLight()
    {
        try
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

            if (shadowRenderTexture == null ||
                shadowRenderTexture.width != resolution ||
                shadowRenderTexture.height != resolution ||
                shadowRenderTexture.format != format)
            {
                if (shadowRenderTexture != null)
                {
                    shadowRenderTexture.Release();
                    DestroyImmediate(shadowRenderTexture);
                }

                shadowRenderTexture = new RenderTexture(resolution, resolution, 0, format);
                shadowRenderTexture.filterMode = filterMode;
                shadowRenderTexture.useMipMap = false;
                shadowRenderTexture.Create();
            }

            UpdateShaderProperties();
            UpdateCommandBuffer();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error setting up PCSS light: {e.Message}");
        }
    }

    private void CleanupLight()
    {
        try
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

            shadowStrengthCache.Clear();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error cleaning up PCSS light: {e.Message}");
        }
    }

    private void UpdateShaderProperties()
    {
        try
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
        catch (Exception e)
        {
            Debug.LogError($"Error updating shader properties: {e.Message}");
        }
    }

    private void UpdateCommandBuffer()
    {
        try
        {
            copyShadowBuffer.Clear();
            copyShadowBuffer.SetShadowSamplingMode(BuiltinRenderTextureType.CurrentActive, ShadowSamplingMode.RawDepth);
            copyShadowBuffer.Blit(BuiltinRenderTextureType.CurrentActive, shadowRenderTexture);
            copyShadowBuffer.SetGlobalTexture(shadowmapPropID, shadowRenderTexture);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error updating command buffer: {e.Message}");
        }
    }

    private void Update()
    {
        try
        {
            if (avatarDescriptor == null)
            {
                avatarDescriptor = GetAvatarDescriptor();
            }

            if (avatarDescriptor != null)
            {
                int ownerID = avatarDescriptor.ownerId;
                float distance = Vector3.Distance(transform.position, avatarDescriptor.gameObject.transform.position);

                if (distance <= 10.0f)
                {
                    // PCSSの効果を適用
                    UpdateShaderProperties();
                }
                else
                {
                    // PCSSの効果を徐々に減衰させる
                    if (!shadowStrengthCache.TryGetValue(ownerID, out float shadowStrength))
                    {
                        shadowStrength = 1.0f;
                    }

                    float t = Mathf.Clamp01((distance - 10.0f) / 5.0f);
                    shadowStrength = Mathf.Lerp(shadowStrength, 0.0f, t);
                    shadowStrengthCache[ownerID] = shadowStrength;

                    // キャッシュのサイズを制限
                    if (shadowStrengthCache.Count > MaxShadowStrengthCache)
                    {
                        int oldestOwnerID = 0;
                        float oldestAccessTime = float.MaxValue;
                        foreach (var pair in shadowStrengthCache)
                        {
                            if (pair.Value < oldestAccessTime)
                            {
                                oldestOwnerID = pair.Key;
                                oldestAccessTime = pair.Value;
                            }
                        }
                        shadowStrengthCache.Remove(oldestOwnerID);
                    }

                    Shader.SetGlobalFloat("_PCSShadowStrength", shadowStrength);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error updating PCSS effect: {e.Message}");
        }
    }

    private VRCAvatarDescriptor GetAvatarDescriptor()
    {
        try
        {
            VRCAvatarDescriptor[] avatarDescriptors = FindObjectsOfType<VRCAvatarDescriptor>();
            foreach (VRCAvatarDescriptor descriptor in avatarDescriptors)
            {
                if (descriptor.gameObject.activeInHierarchy)
                {
                    return descriptor;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error getting avatar descriptor: {e.Message}");
        }
        return null;
    }
}