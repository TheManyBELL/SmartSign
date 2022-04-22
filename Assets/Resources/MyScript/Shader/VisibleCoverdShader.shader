Shader "Custom/VisibleCoverdShader"
{
    Properties
    {
        _MainColor("Main Color", color) = (0,0,1,0.3)
		_OutColor("Out Color", color) = (1,1,0,0.2)
    }
    SubShader
    {
       Tags{"queue" = "transparent"}
		pass {
			blend srcalpha oneminussrcalpha
			ztest greater                         
			zwrite off                               
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "unitycg.cginc"
            
			fixed4 _MainColor;
			fixed4 _OutColor;
            RWStructuredBuffer<int> covered;

			struct v2f {
				float4 pos:POSITION;
			};
			v2f vert(appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}
			fixed4 frag(v2f IN):COLOR
			{
				return _OutColor;
			}
			ENDCG
		}

		pass {
			blend srcalpha oneminussrcalpha
			ztest less                 //3.
			zwrite off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "unitycg.cginc"
			fixed4 _MainColor;
			fixed4 _OutColor;

			struct appdata
			{
				float4 vertex : POSITION;//获得顶点数据
				fixed4 color : COLOR;//顶点颜色
			};

			struct v2f {
				float4 pos : POSITION;
				fixed4 color : COLOR;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				return o;
			}

			fixed4 frag(v2f IN) :COLOR
			{
				return IN.color;
			}
			ENDCG
		}

    }
    FallBack "Diffuse"
}
