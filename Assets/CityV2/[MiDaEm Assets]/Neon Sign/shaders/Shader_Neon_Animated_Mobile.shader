// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "MiDaEm/Neon Animated Mobile" {
    Properties {
        _DIF ("DIF", 2D) = "white" {}
        _Em_Power ("Em_Power", Float ) = 1
        _ScanLines ("ScanLines", Float ) = 20
        _SpeedScanLines ("SpeedScanLines", Float ) = 5
        _HardnessScanLines ("HardnessScanLines", Float ) = 1
        _MinAlphaScanLines ("MinAlphaScanLines", Range(0, 1)) = 0.6
        _MaxAlphaScanLines ("MaxAlphaScanLines", Range(0, 1)) = 1
        _BlinkSpeed ("BlinkSpeed", Float ) = 5
        _MinAlphaBlink ("MinAlphaBlink", Range(0, 1)) = 0.5
        _MaxAlphaBlink ("MaxAlphaBlink", Range(0, 1)) = 1
        _RandomBlinkTexture ("RandomBlinkTexture", 2D) = "white" {}
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma exclude_renderers d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 2.0
            uniform float4 _TimeEditor;
            uniform float _BlinkSpeed;
            uniform float _ScanLines;
            uniform float _MinAlphaScanLines;
            uniform float _MaxAlphaScanLines;
            uniform float _SpeedScanLines;
            uniform float _HardnessScanLines;
            uniform sampler2D _RandomBlinkTexture; uniform float4 _RandomBlinkTexture_ST;
            uniform float _MinAlphaBlink;
            uniform float _MaxAlphaBlink;
            uniform sampler2D _DIF; uniform float4 _DIF_ST;
            uniform float _Em_Power;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                UNITY_FOG_COORDS(1)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
////// Lighting:
////// Emissive:
                float4 _DIF_var = tex2D(_DIF,TRANSFORM_TEX(i.uv0, _DIF));
                float3 emissive = (_DIF_var.rgb*_Em_Power);
                float3 finalColor = emissive;
                float4 node_2784 = _Time + _TimeEditor;
                float4 node_5075 = _Time + _TimeEditor;
                float node_5836 = (_BlinkSpeed*(node_5075.g/15.0));
                float2 node_1903 = float2(node_5836,node_5836);
                float4 _RandomBlinkTexture_var = tex2D(_RandomBlinkTexture,TRANSFORM_TEX(node_1903, _RandomBlinkTexture));
                fixed4 finalRGBA = fixed4(finalColor,(lerp(_MinAlphaScanLines,_MaxAlphaScanLines,saturate(pow((sin((_ScanLines*(i.uv0.g-(node_2784.r*_SpeedScanLines))*6.28318530718))*0.5+0.5),_HardnessScanLines)))*lerp(_MinAlphaBlink,_MaxAlphaBlink,_RandomBlinkTexture_var.r)));
                UNITY_APPLY_FOG_COLOR(i.fogCoord, finalRGBA, fixed4(0,0,0,1));
                return finalRGBA;
            }
            ENDCG
        }
    }

}
