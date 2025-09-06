Shader "Custom/UnoccludedTransparent" {
    Properties {
        _Color ("Main Color", Color) = (1,1,1,0.5)
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader {
        
        Tags { 
            "Queue" = "Transparent" 
            "RenderType" = "Transparent"
        }
        // 使用两个Pass实现深度排序
        Pass {
            Cull Off
            // 第一个Pass：仅写入深度
            ZWrite On
            ColorMask 0  // 不输出颜色
        }


        

        Pass {
            Cull Off
            // 第一步：关闭深度写入
            ZWrite Off
            // 设置混合模式
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _Color;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                return col;
            }
            ENDCG
        }
    }
}