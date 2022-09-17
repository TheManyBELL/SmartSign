Shader "Custom/SplitTargetNoMesh" {
	Properties{
		_PointSize("Point Size", Float) = 0.5
	}

	SubShader
	{
		Cull Off
		Pass
		{
			CGPROGRAM
// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct appdata members vertex,color)
#pragma exclude_renderers d3d11
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex;
				float4 color;
			};

			struct v2f
			{
				float4 vertex;
				float4 color;
			};

			struct g2f
			{
				float4 vertex;
				float4 color;
			};

			float _PointSize;

			[maxvertexcount(4)]
			void geom(point v2f i[1], inout TriangleStream<g2f> triStream)
			{
				g2f o;
				float4 v = i[0].vertex;
				float4 c = i[0].color;
				v.y = -v.y;

				float2 p = _PointSize;
				p.y *= _ScreenParams.x / _ScreenParams.y;

				o.vertex = UnityObjectToClipPos(v);
				o.vertex += float4(-p.x, p.y, 0, 0);
				o.color = c;
				triStream.Append(o);

				o.vertex = UnityObjectToClipPos(v);
				o.vertex += float4(-p.x, p.y, 0, 0);
				o.color = c;
				triStream.Append(o);

				o.vertex = UnityObjectToClipPos(v);
				o.vertex += float4(-p.x, p.y, 0, 0);
				o.color = c;
				triStream.Append(o);

				o.vertex = UnityObjectToClipPos(v);
				o.vertex += float4(-p.x, p.y, 0, 0);
				o.color = c;
				triStream.Append(o);
			}

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = v.vertex;
				o.color = v.color;
				return o;
			}

			fixed4 frag(g2f i) : SV_Target
			{
				return i.color;
			}
			ENDCG
		}
	}
}
