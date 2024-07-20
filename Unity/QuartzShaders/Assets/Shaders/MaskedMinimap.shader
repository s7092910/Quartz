Shader "Unlit/MaskedMinimap"
{
    Properties {
        _Color ("_Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGBA)", 2D) = "white" { }
        _Background ("Background (RGB)", 2D) = "white" { }
        _Mask ("Alpha (A)", 2D) = "white" {}
        //_MainMapPosAndScale ("Map Position And Scale()" , Vector) = (1, 1, 1, 1)
    }
    SubShader
    {
        LOD 100
        Tags 
        { "IGNOREPROJECTOR" = "true" "QUEUE" = "Transparent-2" "RenderType" = "Transparent" }

        Pass 
        {
            Blend SrcAlpha OneMinusSrcAlpha, SrcAlpha OneMinusSrcAlpha
            ColorMask RGB 0
            ZWrite Off
            Cull Off
            Offset -1, -1

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				float2 maskUv : TEXCOORD1;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
				float2 maskUv : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _Background;
            sampler2D _Mask;
            float4 _MainTex_ST;

            float _MapRotation;
            float _MapOpacity;

            float4 _Color;
            float4 _MainMapPosAndScale;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                //rotation
                o.uv.xy = o.uv * 2 -1;

                float c = cos(_MapRotation);
                float s = sin(_MapRotation);
                float2x2 mat = float2x2 (c, -s,
                                         s, c);

                o.uv.xy = mul(mat, o.uv.xy);
                o.uv.xy = o.uv * 0.5 + 0.5;

                o.maskUv = v.maskUv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 mapCoords = float2(-0.49000001, 0.49000001) / _MainMapPosAndScale.zz;
                mapCoords += float2(0.5, 0.5);

                bool xLessThanX = i.uv.x < mapCoords.x;
                bool xLessThanY = i.uv.y < mapCoords.x;

                bool yLessThanX = mapCoords.y < i.uv.x;
                bool yLessThanY = mapCoords.y < i.uv.y;

                xLessThanX = yLessThanX || xLessThanX;
                xLessThanX = xLessThanY || xLessThanX;
                xLessThanX = yLessThanY || xLessThanX;

                float2 textCoords = i.uv.xy * _MainMapPosAndScale.zw + _MainMapPosAndScale.xy;
                
                float4 col = tex2D(_MainTex, textCoords);

                col.a = xLessThanX ? 0.0 : col.a;
                mapCoords = textCoords * float2(5.0, 5.0);

                float4 backgroundCol1 = tex2D(_Background, textCoords);
                float4 backgroundCol2 = tex2D(_Background, mapCoords);
                backgroundCol2 = backgroundCol2 * float4(0.5, 0.5, 0.5, 0.5);
                backgroundCol2 = backgroundCol1 * float4(0.5, 0.5, 0.5, 0.5) + backgroundCol2;
                backgroundCol2.a = backgroundCol2.a * _Color.a;

                col = lerp(backgroundCol2, col, col.a);

                col.rgb = col.rgb * _Color.rgb;

                //Mask
                i.maskUv.x -= _MainTex_ST.z;
                i.maskUv.x /= _MainTex_ST.x;
                i.maskUv.y -= _MainTex_ST.w;
                i.maskUv.y /= _MainTex_ST.y;
                col.a *= tex2D(_Mask, i.maskUv).a;

                col.a *= _MapOpacity;

                const float gamma = 2.2;
                col.rgb = pow(col.rgb, gamma);

                return col;
            }
            ENDCG
        }
    }
}
