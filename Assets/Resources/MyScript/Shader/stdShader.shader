Shader "Custom/stdShader"
{
	Properties
	{
		_Texture("Texture",2D) = "white"{} //2D��ͼ ��ɫ
		_Color("Color",Color) = (1,1,1,1) //��ɫ ��ɫ
	}

		SubShader
		{
			Tags{"Queue" = "Geometry" "RenderType" = "Opaque" }
			LOD 100 //Shader�ּ�
			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				sampler2D _Texture;//2D��ͼ������sample2D
				fixed4 _Color; //fixed4�Ǿ�����С�ġ����л���half4,float4(���)
				struct appdata {
					fixed4 vertex : POSITION;
					fixed2 uv : TEXCOORD0;
				};
				struct v2f {
					fixed4 vertex : SV_POSITION;//SV_POSITION�Ǹ���Unity���vertex���ݸ�����б�Ĵ������դ������ȴ���͸�������
					fixed2 uv : TEXCOORD0;
				};
				v2f vert(appdata i) //v2f����ֵ�Ǵ��ݻ�unity,unity�Ὣv2f�������ݵ�frag������
				{
					v2f o;
					o.uv = i.uv;
					o.vertex = UnityObjectToClipPos(i.vertex);
					return o;
				}
				fixed4 frag(v2f v) : SV_Target  //SV_Targetò���ǹ̶�д����������ֵ���ݸ�unity������Ⱦ����
				{
					return tex2D(_Texture, v.uv) * _Color;
				}
				ENDCG
			}
		}
}