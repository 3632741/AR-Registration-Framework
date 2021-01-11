/*// Shader for gaussian and box blur
Shader "Custom/gaussian"
{
	// values that appear in the inspector
	Properties
	{
		[HideInInspector]_MainTex("Texture", 2D) = "white" {}
		_BlurSize("Blur Size", Range(0,0.1)) = 0
		_StDev("Standard Deviation", Range(0,0.1)) = 0.03
		_Gauss("Gaussian Blur", float) = 0
	}

	SubShader
	{
		// VERTICAL PASS
		Pass
		{
			CGPROGRAM
			// define vertex and fragment shader
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			#define SAMPLES 30 // mask size
			#define PI 3.14159265359
			#define E 2.71828182846

			sampler2D _MainTex;
			float _BlurSize;
			float _StDev;
			float _Gauss;

			// data structure for the fragment shader
			struct v2f
			{
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			// fragment shader
			float4 frag(v2f i) : SV_TARGET
			{
				// avoid NaN condition
				if (_Gauss == 1)
					if (_StDev == 0)
						return tex2D(_MainTex, i.uv);
				float4 col = 0;
				float sum = 0;
				// selection between box blur and gaussian blur
				if (_Gauss == 1)
					sum = 0;
				else
					sum = SAMPLES;
				// iterate over samples
				for (float index = 0; index < SAMPLES; index++)
				{
					float offset = (index / (SAMPLES - 1) - 0.5) * _BlurSize;
					float2 uv = i.uv + float2(0, offset);
					// box blur
					if (_Gauss == 0)
						col += tex2D(_MainTex, uv);
					// gaussian blur
					else
					{
						float StDevSq = _StDev * _StDev;
						float gaussian = (1 / sqrt(2 * PI * StDevSq)) * pow(E, -((offset*offset) / (2 * StDevSq)));
						sum += gaussian;
						col += tex2D(_MainTex, uv) * gaussian;
					}
				}
				col = col / sum;
				return col;
			}
			ENDCG
		}

		// HORIZONTAL PASS
		Pass
		{
			CGPROGRAM
			// define vertex and fragment shader
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"
			
			#define SAMPLES 30 // mask size
			#define PI 3.14159265359
			#define E 2.71828182846

			sampler2D _MainTex;
			float _BlurSize;
			float _StDev;
			float _Gauss;

			// data structure for the fragment shader
			struct v2f
			{
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			// fragment shader
			float4 frag(v2f i) : SV_TARGET
			{
				// avoid NaN condition
				if (_Gauss == 1)
					if (_StDev == 0)
						return tex2D(_MainTex, i.uv);
				float4 col = 0;
				float sum = 0;
				// selection between box blur and gaussian blur
				if (_Gauss == 1)
					sum = 0;
				else
					sum = SAMPLES;
				// iterate over samples
				for (float index = 0; index < SAMPLES; index++)
				{
					float offset = (index / (SAMPLES - 1) - 0.5) * _BlurSize;
					float2 uv = i.uv + float2(offset, 0);
					// box blur
					if (_Gauss == 0)
						col += tex2D(_MainTex, uv);
					// gaussian blur
					else
					{
						float StDevSq = _StDev * _StDev;
						float gaussian = (1 / sqrt(2 * PI * StDevSq)) * pow(E, -((offset*offset) / (2 * StDevSq)));
						sum += gaussian;
						col += tex2D(_MainTex, uv) * gaussian;
					}
				}
				col = col / sum;
				return col;
			}
			ENDCG
		}
	}
}
*/

// Shader for grayscale output
Shader "Custom/gaussian"
{
	/*Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_bwBlend("Black & White blend", Range(0, 1)) = 0
	}
	SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform float _bwBlend;

			float4 frag(v2f_img i) : COLOR
			{
				float4 c = tex2D(_MainTex, i.uv);

				float lum = c.r*.3 + c.g*.59 + c.b*.11;
				float3 bw = float3(lum, lum, lum);

				float4 result = c;
				result.rgb = lerp(c.rgb, bw, _bwBlend);
				return result;
			}

			half4 vert();
			ENDCG
		}
	}
}
*/ 
	Properties{
 _MainTex("Base (RGB)", 2D) = "white" {}
 _MaskTex("Mask texture", 2D) = "white" {}
 _maskBlend("Mask blending", Float) = 0.5
 _maskSize("Mask Size", Float) = 1
}
SubShader{
Pass {
CGPROGRAM
#pragma vertex vert_img
#pragma fragment frag
#include "UnityCG.cginc"

uniform sampler2D _MainTex;
uniform sampler2D _MaskTex;

fixed _maskBlend;
fixed _maskSize;

fixed4 frag(v2f_img i) : COLOR {
fixed4 mask = tex2D(_MaskTex, i.uv * _maskSize);
fixed4 base = tex2D(_MainTex, i.uv);
return lerp(base, mask, _maskBlend);
}
ENDCG
}
 }
}