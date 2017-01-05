Shader "Hidden/VFX_0"
{
	SubShader
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Pass
		{
			Blend SrcAlpha One
			ZTest LEqual
			ZWrite Off
			Cull Off
			
			CGPROGRAM
			#pragma target 4.5
			
			#pragma vertex vert
			#pragma fragment frag
			
			#define VFX_LOCAL_SPACE
			
			#include "UnityCG.cginc"
			#include "UnityStandardUtils.cginc"
			#include "HLSLSupport.cginc"
			#include "../VFXCommon.cginc"
			
			CBUFFER_START(outputUniforms)
				float2 outputUniform1_kVFXValueOp;
				float outputUniform0_kVFXValueOp;
				uint outputUniforms_PADDING_0;
			
			CBUFFER_END
			
			CBUFFER_START(Uniform)
				float systemIndex;
			CBUFFER_END
			ByteAddressBuffer nbElements;
			
			Texture2D outputSampler0_kVFXValueOpTexture;
			SamplerState sampleroutputSampler0_kVFXValueOpTexture;
			
			Texture2D gradientTexture;
			SamplerState samplergradientTexture;
			
			struct OutputData
			{
				float3 position;
				float age;
				float3 velocity;
				float lifetime;
				float3 up;
				float texIndex;
				float3 side;
				uint _PADDING_0;
				float3 front;
				uint _PADDING_1;
				float2 size;
				uint2 _PADDING_2;
			};
			
			StructuredBuffer<OutputData> outputBuffer;
			
			struct ps_input
			{
				/*linear noperspective centroid*/ float4 pos : SV_POSITION;
				nointerpolation float4 col : COLOR0;
				float2 offsets : TEXCOORD0;
				nointerpolation float flipbookIndex : TEXCOORD1;
			};
			
			float4 sampleSignal(float v,float u) // sample gradient
			{
				return gradientTexture.SampleLevel(samplergradientTexture,float2(((0.9921875 * saturate(u)) + 0.00390625),v),0);
			}
			
			void VFXBlockSubPixelAA( inout float alpha,float3 position,inout float2 size)
			{
				#ifdef VFX_WORLD_SPACE
	float clipPosW = mul(UNITY_MATRIX_VP,float4(position,1.0f)).w;
	#else
	float clipPosW = mul(UNITY_MATRIX_MVP,float4(position,1.0f)).w;
	#endif
	float minSize = clipPosW / (0.5f * min(UNITY_MATRIX_P[0][0] * _ScreenParams.x,-UNITY_MATRIX_P[1][1] * _ScreenParams.y)); // max size in one pixel
	float2 clampedSize = max(size,minSize);
	float fade = (size.x * size.y) / (clampedSize.x * clampedSize.y);
	alpha *= fade;
	size = clampedSize;
			}
			
			void VFXBlockSetColorGradientOverLifetime( inout float3 color,inout float alpha,float age,float lifetime,float Gradient)
			{
				float ratio = saturate(age / lifetime);
	float4 rgba = SAMPLE(Gradient,ratio);
	color = rgba.rgb;
	alpha = rgba.a;
			}
			
			float2 GetSubUV(int flipBookIndex,float2 uv,float2 dim,float2 invDim)
			{
				float2 tile = float2(fmod(flipBookIndex,dim.x),dim.y - 1.0 - floor(flipBookIndex * invDim.x));
				return (tile + uv) * invDim;
			}
			
			ps_input vert (uint id : SV_VertexID, uint instanceID : SV_InstanceID)
			{
				ps_input o;
				uint index = (id >> 2) + instanceID * 2048;
				if (index < nbElements.Load(asuint(systemIndex) << 2))
				{
					OutputData outputData = outputBuffer[index];
					
					float local_alpha = (float)0;
					float3 local_color = (float3)0;
					
					VFXBlockSubPixelAA( local_alpha,outputData.position,outputData.size);
					VFXBlockSetColorGradientOverLifetime( local_color,local_alpha,outputData.age,outputData.lifetime,outputUniform0_kVFXValueOp);
					
					float2 size = outputData.size * 0.5f;
					o.offsets.x = 2.0 * float(id & 1) - 1.0;
					o.offsets.y = 2.0 * float((id & 2) >> 1) - 1.0;
					
					float3 position = outputData.position;
					
					float2 posOffsets = o.offsets.xy;
					float3 cameraPos = mul(unity_WorldToObject,float4(_WorldSpaceCameraPos.xyz,1.0)).xyz; // TODO Put that in a uniform!
					float3 side = outputData.side;
					float3 up = outputData.up;
					
					position += side * (posOffsets.x * size.x);
					position += up * (posOffsets.y * size.y);
					o.offsets.xy = o.offsets.xy * 0.5 + 0.5;
					o.flipbookIndex = outputData.texIndex;
					
					o.pos = mul (UNITY_MATRIX_MVP, float4(position,1.0f));
					o.col = float4(local_color.xyz,local_alpha);
				}
				else
				{
					o.pos = -1.0;
					o.col = 0;
				}
				
				return o;
			}
			
			struct ps_output
			{
				float4 col : SV_Target0;
			};
			
			ps_output frag (ps_input i)
			{
				ps_output o = (ps_output)0;
				
				float4 color = i.col;
				float2 dim = outputUniform1_kVFXValueOp;
				float2 invDim = 1.0 / dim; // TODO InvDim should be computed on CPU
				float ratio = frac(i.flipbookIndex);
				float index = i.flipbookIndex - ratio;
				
				float2 uv1 = GetSubUV(index,i.offsets.xy,dim,invDim);
				float4 col1 = outputSampler0_kVFXValueOpTexture.Sample(sampleroutputSampler0_kVFXValueOpTexture,uv1);
				
				float2 uv2 = GetSubUV(index + 1.0,i.offsets.xy,dim,invDim);
				float4 col2 = outputSampler0_kVFXValueOpTexture.Sample(sampleroutputSampler0_kVFXValueOpTexture,uv2);
				
				color *= lerp(col1,col2,ratio);
				
				o.col = color;
				return o;
			}
			
			ENDCG
		}
	}
	FallBack Off
}
