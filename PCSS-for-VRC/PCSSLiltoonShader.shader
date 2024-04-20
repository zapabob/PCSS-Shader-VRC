// PCSSLiltoon.shader
Shader "PCSSLiltoon"
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
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "AutoLight.cginc"

            // liltoonの変数とヘルパー関数
            // ...

            sampler2D _PCSShadowMap;
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
                for (int i = 0; i < _PCSSBlockerSampleCount; i++)
                {
                    float2 offset = PoissonOffsets[i] * sampleRadius;
                    shadow += tex2Dproj(_PCSShadowMap, shadowCoord + float4(offset, 0.0, 0.0)).r;
                }
                shadow /= _PCSSBlockerSampleCount;

                float blockerDepth = shadow;

                if (blockerDepth < shadowCoord.z)
                {
                    float penumbraSize = (shadowCoord.z - blockerDepth) / blockerDepth;
                    float filterRadius = penumbraSize * softness;

                    for (int i = 0; i < _PCSSPCFSampleCount; i++)
                    {
                        float2 offset = PoissonOffsets[i] * filterRadius;
                        shadow += tex2Dproj(_PCSShadowMap, shadowCoord + float4(offset, 0.0, 0.0)).r;
                    }
                    shadow /= _PCSSPCFSampleCount;
                }

                return shadow;
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
                // liltoonの追加の varying変数
                // ...
            };

            v2f vert(appdata v)
            {
                v2f o;
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
                float4 col = // ...;

                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }

    Fallback "Diffuse"
}
    Fallback "Diffuse"
    CustomEditor "lilToonInspector"
