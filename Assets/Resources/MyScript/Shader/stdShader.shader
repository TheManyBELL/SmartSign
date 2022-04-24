Shader "Custom/stdShader"
{
	Properties
	{
		_Texture("Texture",2D) = "white"{} //2D贴图 白色
		_Color("Color",Color) = (1,1,1,1) //颜色 白色
	}

		SubShader
		{
			Tags{"Queue" = "Geometry" "RenderType" = "Opaque" }
			LOD 100 //Shader分级
			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				sampler2D _Texture;//2D贴图类型是sample2D
				fixed4 _Color; //fixed4是精度最小的、其中还有half4,float4(最大)
				struct appdata {
					fixed4 vertex : POSITION;
					fixed2 uv : TEXCOORD0;
				};
				struct v2f {
					fixed4 vertex : SV_POSITION;//SV_POSITION是告诉Unity这个vertex传递给你进行别的处理，如光栅化、深度处理、透明处理等
					fixed2 uv : TEXCOORD0;
				};
				v2f vert(appdata i) //v2f返回值是传递回unity,unity会将v2f变量传递到frag方法中
				{
					v2f o;
					o.uv = i.uv;
					o.vertex = UnityObjectToClipPos(i.vertex);
					return o;
				}
				fixed4 frag(v2f v) : SV_Target  //SV_Target貌似是固定写法，将返回值传递给unity进行渲染处理
				{
					return tex2D(_Texture, v.uv) * _Color;
				}
				ENDCG
			}
		}
}