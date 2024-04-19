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
Shader "PCSSliltoon"
{
    Properties
    {
        // liltoonのプロパティ
        // ...
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma target 5.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

            // liltoonの変数とヘルパー関数
            // ...

            sampler2D _PCSShadowMap;
            float4 _PCSShadowMap_TexelSize;
            float _PCSSoftness;
            float _PCSSSampleRadius;
            int _PCSSBlockerSampleCount;
            int _PCSSPCFSampleCount;
            float _PCSSMaxStaticGradientBias;
            float _PCSSBlockerGradientBias;
            float _PCSSPCFGradientBias;
            float _PCSSCascadeBlendDistance;
            float4 _PCSSNoiseCoords;
            sampler2D _PCSSNoiseTexture;
            int _PCSSupportOrthographicProjection;
            float _PCSShadowStrength;

            static const float2 PoissonOffsets[64] = {
                // ...
            };

            float SamplePCSSShadowMap(float4 shadowCoord, float softness, float sampleRadius)
            {
                float shadow = 0.0;

                #ifndef SHADER_API_GLES
                // RX5700XTでのキャッシュオーバーフローを防ぐため、ループ回数を制限
                int maxSamples = min(_PCSSBlockerSampleCount, 32);
                for (int i = 0; i < maxSamples; i++)
                {
                    float2 offset = PoissonOffsets[i] * sampleRadius;
                    shadow += tex2Dproj(_PCSShadowMap, shadowCoord + float4(offset, 0.0, 0.0)).r;
                }
                shadow /= maxSamples;
                #else
                // GLES環境ではPCFフィルタリングを使用
                shadow = tex2Dproj(_PCSShadowMap, shadowCoord).r;
                #endif

                float blockerDepth = shadow;

                if (blockerDepth < shadowCoord.z)
                {
                    float penumbraSize = (shadowCoord.z - blockerDepth) / blockerDepth;
                    float filterRadius = penumbraSize * softness;

                    #ifndef SHADER_API_GLES
                    // RX5700XTでのキャッシュオーバーフローを防ぐため、ループ回数を制限
                    int maxPCFSamples = min(_PCSSPCFSampleCount, 32);
                    for (int i = 0; i < maxPCFSamples; i++)
                    {
                        float2 offset = PoissonOffsets[i] * filterRadius;
                        shadow += tex2Dproj(_PCSShadowMap, shadowCoord + float4(offset, 0.0, 0.0)).r;
                    }
                    shadow /= maxPCFSamples;
                    #else
                    // GLES環境ではPCFフィルタリングを使用
                    shadow = tex2Dproj(_PCSShadowMap, shadowCoord).r;
                    #endif
                }

                return saturate(shadow);
            }

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                // liltoonの追加の入力データ
                // ...
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 worldPos : TEXCOORD2;
                float3 normal : TEXCOORD3;
                // liltoonの追加の varying 変数
                // ...
            };

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);

                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);

                // liltoonの頂点シェーダーの処理
                // ...

                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                // liltoonのフラグメントシェーダーの処理
                // ...

                // PCSSシャドウマップをサンプリング
                float4 shadowCoord = mul(unity_WorldToShadow[0], float4(i.worldPos, 1.0));
                float shadow = SamplePCSSShadowMap(shadowCoord, _PCSSoftness, _PCSSSampleRadius);
                shadow = lerp(1.0, shadow, _PCSShadowStrength);

                // シャドウをライティングに適用
                float3 lighting = (1.0 - shadow) * directLight;

                // liltoonのライティング処理
                // ...

                // 最終的な色の計算
                float4 col = float4(0.0, 0.0, 0.0, 0.0);

                // liltoonの最終的な色の計算
                // ...

                // アルファ値のクランプ
                col.a = saturate(col.a);

                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDHLSL
        }
    }

    Fallback "Diffuse"
    CustomEditor "lilToonInspector"
