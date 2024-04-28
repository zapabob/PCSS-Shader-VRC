// PCSSLight.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PCSSLight : MonoBehaviour
{
    public int resolution = 4096;
    public bool customShadowResolution = false;

    [Space(20f)]
    [Range(1, 64)]
    public int Blocker_SampleCount = 16;
    [Range(1, 64)]
    public int PCF_SampleCount = 16;

    [Space(20f)]
    public Texture2D noiseTexture;

    [Space(20f)]
    [Range(0f, 7.5f)]
    public float Softness = 1f;
    [Range(0f, 5f)]
    public float SoftnessFalloff = 4f;

    [Space(20f)]
    [Range(0f, 0.15f)]
    public float MaxStaticGradientBias = .05f;
    [Range(0f, 1f)]
    public float Blocker_GradientBias = 0f;
    [Range(0f, 1f)]
    public float PCF_GradientBias = 1f;

    [Space(20f)]
    [Range(0f, 1f)]
    public float CascadeBlendDistance = .5f;

    [Space(20f)]
    public bool supportOrthographicProjection;

    [Space(20f)]
    public RenderTexture shadowRenderTexture;
    public RenderTextureFormat format = RenderTextureFormat.RFloat;
    public FilterMode filterMode = FilterMode.Bilinear;
    public enum antiAliasing
    {
        None = 1,
        Two = 2,
        Four = 4,
        Eight = 8,
    }
    public antiAliasing MSAA = antiAliasing.None;
    private LightEvent lightEvent = LightEvent.AfterShadowMap;

    public string shaderName = "liltoon/PCSS/PCSS";
    private int shadowmapPropID;

    private CommandBuffer copyShadowBuffer;
    [HideInInspector]
    public Light _light;

    private void Start()
    {
        Setup();
    }

    public void Setup()
    {
        _light = GetComponent<Light>();
        if (!_light)
            return;

        resolution = Mathf.ClosestPowerOfTwo(resolution);
        if (customShadowResolution)
            _light.shadowCustomResolution = resolution;
        else
            _light.shadowCustomResolution = 0;

        shadowmapPropID = Shader.PropertyToID("_ShadowMap");

        copyShadowBuffer = new CommandBuffer();
        copyShadowBuffer.name = "PCSS Shadows";

        _light.AddCommandBuffer(lightEvent, copyShadowBuffer);

        CreateShadowRenderTexture();
        UpdateShaderValues();
        UpdateCommandBuffer();
    }

    public void CreateShadowRenderTexture()
    {
        if (shadowRenderTexture != null)
        {
            DestroyShadowRenderTexture();
        }

        shadowRenderTexture = new RenderTexture(resolution, resolution, 0, format);
        shadowRenderTexture.filterMode = filterMode;
        shadowRenderTexture.useMipMap = false;
        shadowRenderTexture.antiAliasing = (int)MSAA;
    }

    public void DestroyShadowRenderTexture()
    {
        if (shadowRenderTexture != null)
        {
            shadowRenderTexture.Release();
            DestroyImmediate(shadowRenderTexture);
            shadowRenderTexture = null;
        }
    }

    public void UpdateShaderValues()
    {
        Shader.SetGlobalInt("Blocker_Samples", Blocker_SampleCount);
        Shader.SetGlobalInt("PCF_Samples", PCF_SampleCount);

        if (shadowRenderTexture)
        {
            if (shadowRenderTexture.format != format || shadowRenderTexture.antiAliasing != (int)MSAA)
                CreateShadowRenderTexture();
            else
            {
                shadowRenderTexture.filterMode = filterMode;
            }
        }

        Shader.SetGlobalFloat("Softness", Softness / 64f / Mathf.Sqrt(QualitySettings.shadowDistance));
        Shader.SetGlobalFloat("SoftnessFalloff", Mathf.Exp(SoftnessFalloff));
        SetFlag("USE_FALLOFF", SoftnessFalloff > Mathf.Epsilon);

        Shader.SetGlobalFloat("RECEIVER_PLANE_MIN_FRACTIONAL_ERROR", MaxStaticGradientBias);
        Shader.SetGlobalFloat("Blocker_GradientBias", Blocker_GradientBias);
        Shader.SetGlobalFloat("PCF_GradientBias", PCF_GradientBias);

        SetFlag("USE_CASCADE_BLENDING", CascadeBlendDistance > 0);
        Shader.SetGlobalFloat("CascadeBlendDistance", CascadeBlendDistance);

        SetFlag("USE_STATIC_BIAS", MaxStaticGradientBias > 0);
        SetFlag("USE_BLOCKER_BIAS", Blocker_GradientBias > 0);
        SetFlag("USE_PCF_BIAS", PCF_GradientBias > 0);

        if (noiseTexture)
        {
            Shader.SetGlobalVector("NoiseCoords", new Vector4(1f / noiseTexture.width, 1f / noiseTexture.height, 0f, 0f));
            Shader.SetGlobalTexture("_NoiseTexture", noiseTexture);
        }

        SetFlag("ORTHOGRAPHIC_SUPPORTED", supportOrthographicProjection);

        int maxSamples = Mathf.Max(Blocker_SampleCount, PCF_SampleCount);

        SetFlag("POISSON_32", maxSamples < 33);
        SetFlag("POISSON_64", maxSamples > 33);
    }

    public void UpdateCommandBuffer()
    {
        if (!_light)
            return;

        copyShadowBuffer.Clear();
        copyShadowBuffer.SetShadowSamplingMode(BuiltinRenderTextureType.CurrentActive, ShadowSamplingMode.RawDepth);
        copyShadowBuffer.Blit(BuiltinRenderTextureType.CurrentActive, shadowRenderTexture);
        copyShadowBuffer.SetGlobalTexture(shadowmapPropID, shadowRenderTexture);
    }

    private void SetFlag(string shaderKeyword, bool value)
    {
        if (value)
            Shader.EnableKeyword(shaderKeyword);
        else
            Shader.DisableKeyword(shaderKeyword);
    }
}