// Made with Amplify Shader Editor v1.9.5.1
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Water"
{
	Properties
	{
		_TessValue( "Max Tessellation", Range( 1, 32 ) ) = 26.5
		_TessMin( "Tess Min Distance", Float ) = 11.57
		_TessMax( "Tess Max Distance", Float ) = 14.19
		_TessPhongStrength( "Phong Tess Strength", Range( 0, 1 ) ) = 0.704
		_DeepColor("DeepColor", Color) = (0,0.3250514,0.5471698,0)
		_DeepRange("DeepRange", Range( 0 , 50)) = 5
		_ShallowColor("ShallowColor", Color) = (0.5528213,0.9528302,0.8406455,0)
		_ReflectFresnel("ReflectFresnel", Float) = 1
		_CausticsScale("CausticsScale", Float) = 1
		_NormalSpeed("NormalSpeed", Vector) = (0,0,0,0)
		_CausticsSpeed("CausticsSpeed", Vector) = (0,0,0,0)
		_CausticsRange("CausticsRange", Float) = 1
		_CausticsIntensity("CausticsIntensity", Float) = 0
		_CausticsTex("CausticsTex", 2D) = "white" {}
		_ShoreRange("ShoreRange", Float) = 1
		_ShoreColor("ShoreColor", Color) = (0,0,0,0)
		_SpecularPower("SpecularPower", Range( 0 , 1000)) = 0
		_SpecularVal("SpecularVal", Float) = 0
		_FoamRange("FoamRange", Float) = 1
		_FoamSpeed("FoamSpeed", Float) = 1
		_FoamBlend("FoamBlend", Range( -1 , 2)) = 0
		_FoamNoiseSize("FoamNoiseSize", Vector) = (10,10,0,0)
		_FoamDissolve("FoamDissolve", Range( 0 , 2)) = 0
		_FoamWidth("FoamWidth", Float) = 0
		_FoamColor("FoamColor", Color) = (0,0,0,0)
		_Fresnel("Fresnel", Color) = (0,0,0,0)
		_NormalScale("NormalScale", Float) = 1
		_Normal("Normal", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Transparent+0" "IsEmissive" = "true"  }
		Cull Back
		GrabPass{ }
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "UnityStandardUtils.cginc"
		#include "Tessellation.cginc"
		#include "Lighting.cginc"
		#pragma target 5.0
		#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex);
		#else
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex)
		#endif
		struct Input
		{
			float4 screenPos;
			float3 worldPos;
			float3 worldNormal;
			float2 uv_texcoord;
		};

		struct SurfaceOutputCustomLightingCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		ASE_DECLARE_SCREENSPACE_TEXTURE( _GrabTexture )
		uniform sampler2D _CausticsTex;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _CausticsScale;
		uniform float4 _CausticsSpeed;
		uniform float _CausticsRange;
		uniform float _CausticsIntensity;
		uniform float4 _DeepColor;
		uniform float4 _ShallowColor;
		uniform float _DeepRange;
		uniform float4 _Fresnel;
		uniform float _ReflectFresnel;
		uniform float4 _ShoreColor;
		uniform float _ShoreRange;
		uniform float _FoamBlend;
		uniform float _FoamRange;
		uniform float _FoamWidth;
		uniform float _FoamSpeed;
		uniform float2 _FoamNoiseSize;
		uniform float _FoamDissolve;
		uniform float4 _FoamColor;
		uniform sampler2D _Normal;
		uniform float _NormalScale;
		uniform float4 _NormalSpeed;
		uniform float _SpecularPower;
		uniform float _SpecularVal;
		uniform float _TessValue;
		uniform float _TessMin;
		uniform float _TessMax;
		uniform float _TessPhongStrength;


inline float4 ASE_ComputeGrabScreenPos( float4 pos )
{
	#if UNITY_UV_STARTS_AT_TOP
	float scale = -1.0;
	#else
	float scale = 1.0;
	#endif
	float4 o = pos;
	o.y = pos.w * 0.5f;
	o.y = ( pos.y - o.y ) * _ProjectionParams.x * scale + o.y;
	return o;
}


		float2 UnStereo( float2 UV )
		{
			#if UNITY_SINGLE_PASS_STEREO
			float4 scaleOffset = unity_StereoScaleOffset[ unity_StereoEyeIndex ];
			UV.xy = (UV.xy - scaleOffset.zw) / scaleOffset.xy;
			#endif
			return UV;
		}


		float3 InvertDepthDir72_g1( float3 In )
		{
			float3 result = In;
			#if !defined(ASE_SRP_VERSION) || ASE_SRP_VERSION <= 70301
			result *= float3(1,1,-1);
			#endif
			return result;
		}


		//https://www.shadertoy.com/view/XdXGW8
		float2 GradientNoiseDir( float2 x )
		{
			const float2 k = float2( 0.3183099, 0.3678794 );
			x = x * k + k.yx;
			return -1.0 + 2.0 * frac( 16.0 * k * frac( x.x * x.y * ( x.x + x.y ) ) );
		}
		
		float GradientNoise( float2 UV, float Scale )
		{
			float2 p = UV * Scale;
			float2 i = floor( p );
			float2 f = frac( p );
			float2 u = f * f * ( 3.0 - 2.0 * f );
			return lerp( lerp( dot( GradientNoiseDir( i + float2( 0.0, 0.0 ) ), f - float2( 0.0, 0.0 ) ),
					dot( GradientNoiseDir( i + float2( 1.0, 0.0 ) ), f - float2( 1.0, 0.0 ) ), u.x ),
					lerp( dot( GradientNoiseDir( i + float2( 0.0, 1.0 ) ), f - float2( 0.0, 1.0 ) ),
					dot( GradientNoiseDir( i + float2( 1.0, 1.0 ) ), f - float2( 1.0, 1.0 ) ), u.x ), u.y );
		}


		float4 tessFunction( appdata_full v0, appdata_full v1, appdata_full v2 )
		{
			return UnityDistanceBasedTess( v0.vertex, v1.vertex, v2.vertex, _TessMin, _TessMax, _TessValue );
		}

		void vertexDataFunc( inout appdata_full v )
		{
		}

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			float3 ase_worldPos = i.worldPos;
			float3 temp_output_175_0 = ( _WorldSpaceCameraPos - ase_worldPos );
			float3 normalizeResult223 = normalize( temp_output_175_0 );
			float3 normalizeResult375 = normalize( ( _WorldSpaceLightPos0.xyz - ase_worldPos ) );
			float3 normalizeResult381 = normalize( ( normalizeResult223 + normalizeResult375 ) );
			float2 temp_output_415_0 = ( (ase_worldPos).xz / _NormalScale );
			float2 appendResult419 = (float2(_NormalSpeed.x , _NormalSpeed.y));
			float2 appendResult420 = (float2(_NormalSpeed.z , _NormalSpeed.w));
			float3 SurfaceNormal52 = BlendNormals( tex2D( _Normal, ( temp_output_415_0 + ( _Time.y * appendResult419 * 0.1 ) ) ).rgb , UnpackNormal( tex2D( _Normal, ( temp_output_415_0 + ( _Time.y * appendResult420 * 0.1 ) ) ) ) );
			float3 normalizeResult297 = normalize( SurfaceNormal52 );
			float dotResult382 = dot( normalizeResult381 , normalizeResult297 );
			float temp_output_384_0 = ( pow( saturate( dotResult382 ) , _SpecularPower ) * _SpecularVal );
			float SpecularColor240 = temp_output_384_0;
			float temp_output_242_0 = SpecularColor240;
			float3 temp_cast_9 = (temp_output_242_0).xxx;
			c.rgb = temp_cast_9;
			c.a = 1;
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_grabScreenPos = ASE_ComputeGrabScreenPos( ase_screenPos );
			float4 ase_grabScreenPosNorm = ase_grabScreenPos / ase_grabScreenPos.w;
			float4 screenColor85 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture,ase_grabScreenPosNorm.xy);
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float2 UV22_g3 = ase_screenPosNorm.xy;
			float2 localUnStereo22_g3 = UnStereo( UV22_g3 );
			float2 break64_g1 = localUnStereo22_g3;
			float clampDepth69_g1 = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy );
			#ifdef UNITY_REVERSED_Z
				float staticSwitch38_g1 = ( 1.0 - clampDepth69_g1 );
			#else
				float staticSwitch38_g1 = clampDepth69_g1;
			#endif
			float3 appendResult39_g1 = (float3(break64_g1.x , break64_g1.y , staticSwitch38_g1));
			float4 appendResult42_g1 = (float4((appendResult39_g1*2.0 + -1.0) , 1.0));
			float4 temp_output_43_0_g1 = mul( unity_CameraInvProjection, appendResult42_g1 );
			float3 temp_output_46_0_g1 = ( (temp_output_43_0_g1).xyz / (temp_output_43_0_g1).w );
			float3 In72_g1 = temp_output_46_0_g1;
			float3 localInvertDepthDir72_g1 = InvertDepthDir72_g1( In72_g1 );
			float4 appendResult49_g1 = (float4(localInvertDepthDir72_g1 , 1.0));
			float3 PositionFromDepth4 = (mul( unity_CameraToWorld, appendResult49_g1 )).xyz;
			float2 temp_output_110_0 = ( (PositionFromDepth4).xz / _CausticsScale );
			float2 appendResult116 = (float2(_CausticsSpeed.x , _CausticsSpeed.y));
			float2 appendResult117 = (float2(_CausticsSpeed.z , _CausticsSpeed.w));
			float3 ase_worldPos = i.worldPos;
			float VerticalDepth31 = ( ase_worldPos.y - PositionFromDepth4.y );
			float3 CausticsColor134 = ( ( min( tex2D( _CausticsTex, ( temp_output_110_0 + ( appendResult116 * _Time.y * 0.1 ) ) ).rgb , tex2D( _CausticsTex, -( temp_output_110_0 + ( appendResult117 * _Time.y * 0.1 ) ) ).rgb ) * exp( ( -VerticalDepth31 / _CausticsRange ) ) ) * _CausticsIntensity );
			float4 UnderWaterColor90 = ( screenColor85 + float4( CausticsColor134 , 0.0 ) );
			float3 ase_worldViewDir = Unity_SafeNormalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float dotResult24 = dot( ase_worldViewDir , ( ase_worldPos - PositionFromDepth4 ) );
			float WaterDepth19 = dotResult24;
			float clampResult27 = clamp( exp( ( -WaterDepth19 / _DeepRange ) ) , 0.0 , 1.0 );
			float4 lerpResult16 = lerp( _DeepColor , _ShallowColor , clampResult27);
			float4 WaterColor95 = lerpResult16;
			float Opaque100 = (lerpResult16).a;
			float4 lerpResult96 = lerp( UnderWaterColor90 , WaterColor95 , Opaque100);
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV37 = dot( ase_worldNormal, ase_worldViewDir );
			float f037 = 0.0;
			float fresnelNode37 = ( f037 + ( 1.0 - f037 ) * pow( 1.0 - fresnelNdotV37, 5 ) );
			float clampResult107 = clamp( fresnelNode37 , 0.0 , 1.0 );
			float Fresnel77 = pow( clampResult107 , _ReflectFresnel );
			float3 ReflectColor76 = ( _Fresnel.rgb * Fresnel77 );
			float4 lerpResult400 = lerp( lerpResult96 , float4( ReflectColor76 , 0.0 ) , Fresnel77);
			float4 GrabColor156 = screenColor85;
			float3 ShoreColor151 = (( float4( _ShoreColor.rgb , 0.0 ) * GrabColor156 )).rgb;
			float clampResult144 = clamp( exp( ( -VerticalDepth31 / _ShoreRange ) ) , 0.0 , 1.0 );
			float WaterShore145 = clampResult144;
			float4 lerpResult152 = lerp( lerpResult400 , float4( ShoreColor151 , 0.0 ) , WaterShore145);
			float clampResult248 = clamp( ( VerticalDepth31 / _FoamRange ) , 0.0 , 1.0 );
			float temp_output_249_0 = ( 1.0 - clampResult248 );
			float smoothstepResult256 = smoothstep( 0.0 , _FoamBlend , temp_output_249_0);
			float gradientNoise260 = GradientNoise(( i.uv_texcoord * _FoamNoiseSize ),1.0);
			gradientNoise260 = gradientNoise260*0.5 + 0.5;
			float3 FoamColor272 = ( ( smoothstepResult256 * step( ( temp_output_249_0 - _FoamWidth ) , ( ( sin( ( ( _FoamSpeed * _Time.y ) + ( clampResult248 * 10.0 ) ) ) + gradientNoise260 ) - _FoamDissolve ) ) ) * _FoamColor.rgb );
			float4 temp_output_241_0 = ( lerpResult152 + float4( FoamColor272 , 0.0 ) );
			o.Emission = temp_output_241_0.rgb;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardCustomLighting keepalpha fullforwardshadows exclude_path:deferred vertex:vertexDataFunc tessellate:tessFunction tessphong:_TessPhongStrength 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float4 screenPos : TEXCOORD3;
				float3 worldNormal : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.worldNormal = worldNormal;
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.screenPos = ComputeScreenPos( o.pos );
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = IN.worldNormal;
				surfIN.screenPos = IN.screenPos;
				SurfaceOutputCustomLightingCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputCustomLightingCustom, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19501
Node;AmplifyShaderEditor.FunctionNode;2;-2240,-288;Inherit;False;Reconstruct World Position From Depth;-1;;1;e7094bcbcc80eb140b2a3dbe6a861de8;0;0;1;FLOAT4;0
Node;AmplifyShaderEditor.SwizzleNode;3;-1870,-288;Inherit;False;FLOAT3;0;1;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;4;-1712,-288;Float;False;PositionFromDepth;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;108;-2414,1568;Inherit;False;4;PositionFromDepth;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;29;-1456,-288;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.WorldPosInputsNode;5;-2064,-448;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector4Node;114;-2416,1856;Inherit;False;Property;_CausticsSpeed;CausticsSpeed;14;0;Create;True;0;0;0;False;0;False;0,0,0,0;2,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;243;-2640,1808;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;50;-2560,1712;Inherit;False;Constant;_Float0;Float 0;8;0;Create;True;0;0;0;False;0;False;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;111;-2350,1680;Inherit;False;Property;_CausticsScale;CausticsScale;12;0;Create;True;0;0;0;False;0;False;1;1.88;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;117;-2046,1936;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SwizzleNode;109;-2158,1552;Inherit;False;FLOAT2;0;2;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;30;-1342,-288;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;116;-2046,1776;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;110;-1998,1568;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;119;-1838,1968;Inherit;False;3;3;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;6;-1760,-416;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;31;-1184,-288;Float;False;VerticalDepth;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;22;-1808,-576;Inherit;False;World;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector4Node;418;-3792,896;Inherit;False;Property;_NormalSpeed;NormalSpeed;13;0;Create;True;0;0;0;False;0;False;0,0,0,0;1,-1,1,1;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldPosInputsNode;413;-3952,576;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;118;-1822,1776;Inherit;False;3;3;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;113;-1614,1792;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DotProductOpNode;24;-1584,-448;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;125;-1646,2032;Inherit;False;31;VerticalDepth;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;419;-3584,896;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;420;-3584,1008;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SwizzleNode;414;-3760,592;Inherit;False;FLOAT2;0;2;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;416;-3776,768;Inherit;False;Property;_NormalScale;NormalScale;36;0;Create;True;0;0;0;False;0;False;1;3.48;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;112;-1598,1584;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.NegateNode;124;-1646,1936;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;244;-462,1552;Inherit;False;31;VerticalDepth;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;245;-446,1680;Inherit;False;Property;_FoamRange;FoamRange;25;0;Create;True;0;0;0;False;0;False;1;2.11;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;127;-1422,2032;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;19;-1440,-448;Float;False;WaterDepth;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;128;-1566,2128;Inherit;False;Property;_CausticsRange;CausticsRange;15;0;Create;True;0;0;0;False;0;False;1;1.49;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;422;-3424,912;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;423;-3424,1024;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;415;-3520,688;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;246;-240,1536;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;120;-1440,1568;Inherit;True;Property;_CausticsTex;CausticsTex;17;0;Create;True;0;0;0;False;0;False;-1;None;bdf93234f5473674e8ee00ad373856c8;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SamplerNode;121;-1438,1776;Inherit;True;Property;_TextureSample1;Texture Sample 1;17;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;120;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleDivideOpNode;129;-1262,2032;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;15;-2400,432;Inherit;False;19;WaterDepth;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;417;-3328,656;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;421;-3232,912;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;252;-462,1776;Inherit;False;Property;_FoamSpeed;FoamSpeed;26;0;Create;True;0;0;0;False;0;False;1;5.37;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;248;-128,1536;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;253;-224,1664;Inherit;False;Constant;_FoamFrequency;FoamFrequency;28;0;Create;True;0;0;0;False;0;False;10;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMinOpNode;132;-1134,1568;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ExpOpNode;130;-1102,2032;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;33;-2192,448;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;26;-2382,528;Inherit;False;Property;_DeepRange;DeepRange;6;0;Create;True;0;0;0;False;0;False;5;3.8;0;50;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;391;-3168,624;Inherit;True;Property;_Normal;Normal;39;0;Create;True;0;0;0;False;0;False;-1;None;852af351f94599c4ab83361043fca7ce;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SamplerNode;392;-3120,816;Inherit;True;Property;_TextureSample0;Texture Sample 0;39;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;True;Instance;391;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.WorldPosInputsNode;174;-2320,-1488;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceCameraPos;177;-2400,-1648;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceLightPos;157;-2448,-1312;Inherit;False;0;3;FLOAT4;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;250;-176,1760;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;262;-16,1760;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;263;16,1872;Inherit;False;Property;_FoamNoiseSize;FoamNoiseSize;28;0;Create;True;0;0;0;False;0;False;10,10;100,100;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;254;16,1664;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;25;-2030,448;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;131;-1086,1712;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;390;-1088,1872;Inherit;False;Property;_CausticsIntensity;CausticsIntensity;16;0;Create;True;0;0;0;False;0;False;0;1.53;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;37;-1296,592;Inherit;False;Schlick;TangentNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.BlendNormalsNode;411;-2464,736;Inherit;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;175;-2096,-1616;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;373;-2080,-1344;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GrabScreenPosition;84;-3104,2752;Inherit;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;264;224,1776;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;251;176,1632;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;323;-967.8567,1551.37;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ExpOpNode;28;-1904,448;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;80;-1280,768;Inherit;False;Property;_ReflectFresnel;ReflectFresnel;11;0;Create;True;0;0;0;False;0;False;1;1.98;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;107;-1088,608;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;52;-2160,736;Inherit;False;SurfaceNormal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;223;-1888,-1616;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;375;-1936,-1328;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ScreenColorNode;85;-2078,2768;Inherit;False;Global;_GrabScreen0;Grab Screen 0;12;0;Create;True;0;0;0;False;0;False;Object;-1;False;False;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;139;-958,2400;Inherit;False;31;VerticalDepth;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;260;384,1760;Inherit;False;Gradient;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;255;288,1648;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;13;-2272,224;Inherit;False;Property;_DeepColor;DeepColor;5;0;Create;True;0;0;0;False;0;False;0,0.3250514,0.5471698,0;0,0.3250508,0.5471698,0.682353;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.ColorNode;14;-2014,256;Inherit;False;Property;_ShallowColor;ShallowColor;7;0;Create;True;0;0;0;False;0;False;0.5528213,0.9528302,0.8406455,0;0.5528213,0.9528301,0.8406455,0.2627451;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.ClampOpNode;27;-1726,368;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;134;-816,1648;Inherit;False;CausticsColor;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PowerNode;82;-944,640;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;159;-1792,-1328;Inherit;False;52;SurfaceNormal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;380;-1744,-1472;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NegateNode;140;-734,2400;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;143;-878,2496;Inherit;False;Property;_ShoreRange;ShoreRange;21;0;Create;True;0;0;0;False;0;False;1;1.7;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;265;208,1888;Inherit;False;Property;_FoamDissolve;FoamDissolve;29;0;Create;True;0;0;0;False;0;False;0;1.13;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;269;32,1552;Inherit;False;Property;_FoamWidth;FoamWidth;30;0;Create;True;0;0;0;False;0;False;0;0.3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;249;32,1472;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;259;464,1648;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;136;-2110,3072;Inherit;False;134;CausticsColor;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;16;-1710,208;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;77;-752,640;Inherit;False;Fresnel;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;156;-1664,2960;Inherit;False;GrabColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.NormalizeNode;297;-1552,-1328;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;381;-1632,-1472;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;141;-574,2400;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;147;-912,2784;Inherit;False;156;GrabColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;146;-912,2592;Inherit;False;Property;_ShoreColor;ShoreColor;22;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.7643734,0.8396226,0.8144101,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RangedFloatNode;257;-320,1424;Inherit;False;Property;_FoamBlend;FoamBlend;27;0;Create;True;0;0;0;False;0;False;0;2;-1;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;268;224,1504;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;266;608,1632;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;39;-1520,160;Inherit;False;FLOAT;3;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;399;-2000,2416;Inherit;False;Property;_Fresnel;Fresnel;35;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.5754716,0.5754716,0.5754716,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.GetLocalVarNode;79;-1984,2608;Inherit;False;77;Fresnel;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;135;-1824,2784;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DotProductOpNode;382;-1440,-1472;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ExpOpNode;142;-464,2400;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;148;-544,2640;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SmoothstepOpNode;256;192,1376;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;267;512,1472;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;95;-1376,336;Inherit;False;WaterColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;90;-1646,2816;Inherit;False;UnderWaterColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;100;-1312,192;Inherit;False;Opaque;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;78;-1600,2432;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;172;-1616,-1248;Inherit;False;Property;_SpecularPower;SpecularPower;23;0;Create;True;0;0;0;False;0;False;0;36.83202;0;1000;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;386;-1344,-1424;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;270;64,1168;Inherit;False;Property;_FoamColor;FoamColor;31;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.8679245,0.8679245,0.8679245,1;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.ClampOpNode;144;-352,2400;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;150;-384,2640;Inherit;False;FLOAT3;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;258;384,1376;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;93;-656,-64;Inherit;False;95;WaterColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;98;-672,-128;Inherit;False;90;UnderWaterColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;99;-656,0;Inherit;False;100;Opaque;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;76;-1310,2496;Inherit;False;ReflectColor;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PowerNode;383;-1216,-1424;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;198;-1328,-1248;Inherit;False;Property;_SpecularVal;SpecularVal;24;0;Create;True;0;0;0;False;0;False;0;1.87;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;145;-192,2400;Inherit;False;WaterShore;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;151;-240,2640;Inherit;False;ShoreColor;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;271;576,1360;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;96;-464,-128;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;105;-656,128;Inherit;False;77;Fresnel;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;101;-656,64;Inherit;False;76;ReflectColor;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;384;-1072,-1456;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;155;-656,192;Inherit;False;151;ShoreColor;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;154;-656,256;Inherit;False;145;WaterShore;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;272;736,1360;Inherit;False;FoamColor;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;400;-352,-16;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;273;-656,320;Inherit;False;272;FoamColor;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;152;-208,-128;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;240;-1056,-1888;Inherit;False;SpecularColor;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;222;-1744,-1600;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;376;-1632,-1600;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DotProductOpNode;226;-1440,-1600;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;232;-1344,-1552;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;229;-1216,-1600;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;231;-1072,-1584;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;379;-2304,-1872;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.LightAttenuation;378;-2368,-2000;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;377;-1776,-1936;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;387;-1776,-1792;Inherit;False;4;4;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;389;-1552,-1872;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;372;-1552,-1760;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;388;-1312,-1808;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.NormalizeNode;439;-1168,-2048;Inherit;False;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;241;-64,-112;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;242;-240,128;Inherit;False;240;SpecularColor;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;36;-1584,816;Inherit;False;Property;_FresnelPower;FresnelPower;8;0;Create;True;0;0;0;False;0;False;0;16.6;-5;50;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;187;-320,464;Inherit;False;Property;_tessMin;tessMin;18;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;186;-320,400;Inherit;False;Property;_tessVal;tessVal;19;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;188;-320,528;Inherit;False;Property;_tessMax;tessMax;20;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceBasedTessNode;185;-128,368;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TangentVertexDataNode;283;320,2144;Inherit;True;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NormalVertexDataNode;284;304,2528;Inherit;True;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldPosInputsNode;275;608,2176;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CrossProductOpNode;285;320,2320;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector4Node;277;128,2160;Inherit;False;Property;_Wave1;Wave1;33;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector4Node;303;128,2528;Inherit;False;Property;_Wave3;Wave3;34;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector4Node;302;128,2336;Inherit;False;Property;_Wave2;Wave2;32;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CustomExpressionNode;300;592,2464;Float;False;float steepness = wave.z@$float wavelength = wave.w@$$float k = 2 * UNITY_PI / wavelength@$float c = sqrt(9.8 / k)@$float2 d = normalize(wave.xy)@$float f = k * (dot(d, position.xz) - c * _Time.y)@$float a = steepness / k@$			$tangent += float3($-d.x * d.x * (steepness * sin(f)),$d.x * (steepness * cos(f)),$-d.x * d.y * (steepness * sin(f))$)@$binormal += float3($-d.x * d.y * (steepness * sin(f)),$d.y * (steepness * cos(f)),$-d.y * d.y * (steepness * sin(f))$)@$return float3($d.x * (a * cos(f)),$a * sin(f),$d.y * (a * cos(f))$)@;3;Create;4;True;position;FLOAT3;0,0,0;In;;Float;False;True;tangent;FLOAT3;1,0,0;InOut;;Float;False;True;binormal;FLOAT3;0,0,1;InOut;;Float;False;True;wave;FLOAT4;0,0,0,0;In;;Float;False;My Custom Expression;False;False;0;;False;4;0;FLOAT3;0,0,0;False;1;FLOAT3;1,0,0;False;2;FLOAT3;0,0,1;False;3;FLOAT4;0,0,0,0;False;3;FLOAT3;0;FLOAT3;2;FLOAT3;3
Node;AmplifyShaderEditor.CustomExpressionNode;301;592,2608;Float;False;float steepness = wave.z@$float wavelength = wave.w@$$float k = 2 * UNITY_PI / wavelength@$float c = sqrt(9.8 / k)@$float2 d = normalize(wave.xy)@$float f = k * (dot(d, position.xz) - c * _Time.y)@$float a = steepness / k@$			$tangent += float3($-d.x * d.x * (steepness * sin(f)),$d.x * (steepness * cos(f)),$-d.x * d.y * (steepness * sin(f))$)@$binormal += float3($-d.x * d.y * (steepness * sin(f)),$d.y * (steepness * cos(f)),$-d.y * d.y * (steepness * sin(f))$)@$return float3($d.x * (a * cos(f)),$a * sin(f),$d.y * (a * cos(f))$)@;3;Create;4;True;position;FLOAT3;0,0,0;In;;Float;False;True;tangent;FLOAT3;1,0,0;InOut;;Float;False;True;binormal;FLOAT3;0,0,1;InOut;;Float;False;True;wave;FLOAT4;0,0,0,0;In;;Float;False;My Custom Expression;False;False;0;;False;4;0;FLOAT3;0,0,0;False;1;FLOAT3;1,0,0;False;2;FLOAT3;0,0,1;False;3;FLOAT4;0,0,0,0;False;3;FLOAT3;0;FLOAT3;2;FLOAT3;3
Node;AmplifyShaderEditor.CustomExpressionNode;274;592,2320;Float;False;float steepness = wave.z@$float wavelength = wave.w@$$float k = 2 * 3.14159265359 / wavelength@$float c = sqrt(9.8 / k)@$float2 d = normalize(wave.xy)@$float f = k * (dot(d, position.xz) - c * _Time.y)@$float a = steepness / k@$			$tangent += float3($-d.x * d.x * (steepness * sin(f)),$d.x * (steepness * cos(f)),$-d.x * d.y * (steepness * sin(f))$)@$binormal += float3($-d.x * d.y * (steepness * sin(f)),$d.y * (steepness * cos(f)),$-d.y * d.y * (steepness * sin(f))$)@$return float3($d.x * (a * cos(f)),$a * sin(f),$d.y * (a * cos(f))$)@;3;Create;4;True;position;FLOAT3;0,0,0;In;;Float;False;True;tangent;FLOAT3;1,0,0;InOut;;Float;False;True;binormal;FLOAT3;0,0,1;InOut;;Float;False;True;wave;FLOAT4;0,0,0,0;In;;Float;False;My Custom Expression;False;False;0;;False;4;0;FLOAT3;0,0,0;False;1;FLOAT3;1,0,0;False;2;FLOAT3;0,0,1;False;3;FLOAT4;0,0,0,0;False;3;FLOAT3;0;FLOAT3;2;FLOAT3;3
Node;AmplifyShaderEditor.SimpleAddOpNode;304;816,2464;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;305;816,2592;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;298;928,2352;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;299;928,2416;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;290;1072,2416;Inherit;False;Binormal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;278;816,2336;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TransformPositionNode;279;800,2176;Inherit;False;World;Object;False;Fast;True;1;0;FLOAT3;0,0,0;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LengthOpNode;339;-2336,1376;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;350;-2352,1456;Inherit;False;Property;_WaveField;WaveField;37;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;353;-2208,1472;Inherit;False;Property;_WaveBlend;WaveBlend;38;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;345;-2192,1376;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;347;-2032,1456;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;291;-2528,1088;Inherit;False;289;Tangent;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;355;-1888,1456;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.CrossProductOpNode;294;-2304,1136;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;354;-1792,1440;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalizeNode;356;-2144,1152;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SmoothstepOpNode;349;-2048,1344;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;337;-2320,1248;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;292;-2544,1168;Inherit;False;290;Binormal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;280;1008,2208;Inherit;False;WaterPos;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;281;-224,208;Inherit;False;280;WaterPos;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;289;1072,2352;Inherit;False;Tangent;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SwizzleNode;67;-2448,2096;Inherit;False;FLOAT2;0;1;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;72;-2688,2336;Inherit;False;Property;_ReflectDisort;ReflectDisort;10;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;68;-2688,2240;Inherit;False;52;SurfaceNormal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;74;-2688,2400;Inherit;False;Constant;_Float1;Float 1;9;0;Create;True;0;0;0;False;0;False;0.01;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;73;-2480,2288;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;66;-2672,2064;Float;False;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;71;-2336,2288;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;69;-2288,2176;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.NormalizeNode;351;-1968,1168;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;75;-2176,2112;Inherit;True;Property;_ReflectionTex;ReflectionTex;9;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SwizzleNode;70;-2464,2192;Inherit;False;FLOAT2;0;1;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WorldNormalVector;338;-2736,1216;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;435;-2848,592;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;2,2,2;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;436;-2832,800;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;2,2,2;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;437;-2672,592;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;1,1,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;438;-2656,832;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;1,1,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;397;16,-32;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;371;144,-176;Float;False;True;-1;7;ASEMaterialInspector;0;0;CustomLighting;Water;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Translucent;0.5;True;True;0;False;Opaque;;Transparent;ForwardOnly;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;True;0;26.5;11.57;14.19;True;0.704;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;0;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;3;0;2;0
WireConnection;4;0;3;0
WireConnection;29;0;4;0
WireConnection;117;0;114;3
WireConnection;117;1;114;4
WireConnection;109;0;108;0
WireConnection;30;0;5;2
WireConnection;30;1;29;1
WireConnection;116;0;114;1
WireConnection;116;1;114;2
WireConnection;110;0;109;0
WireConnection;110;1;111;0
WireConnection;119;0;117;0
WireConnection;119;1;243;0
WireConnection;119;2;50;0
WireConnection;6;0;5;0
WireConnection;6;1;4;0
WireConnection;31;0;30;0
WireConnection;118;0;116;0
WireConnection;118;1;243;0
WireConnection;118;2;50;0
WireConnection;113;0;110;0
WireConnection;113;1;119;0
WireConnection;24;0;22;0
WireConnection;24;1;6;0
WireConnection;419;0;418;1
WireConnection;419;1;418;2
WireConnection;420;0;418;3
WireConnection;420;1;418;4
WireConnection;414;0;413;0
WireConnection;112;0;110;0
WireConnection;112;1;118;0
WireConnection;124;0;113;0
WireConnection;127;0;125;0
WireConnection;19;0;24;0
WireConnection;422;0;243;0
WireConnection;422;1;419;0
WireConnection;422;2;50;0
WireConnection;423;0;243;0
WireConnection;423;1;420;0
WireConnection;423;2;50;0
WireConnection;415;0;414;0
WireConnection;415;1;416;0
WireConnection;246;0;244;0
WireConnection;246;1;245;0
WireConnection;120;1;112;0
WireConnection;121;1;124;0
WireConnection;129;0;127;0
WireConnection;129;1;128;0
WireConnection;417;0;415;0
WireConnection;417;1;422;0
WireConnection;421;0;415;0
WireConnection;421;1;423;0
WireConnection;248;0;246;0
WireConnection;132;0;120;5
WireConnection;132;1;121;5
WireConnection;130;0;129;0
WireConnection;33;0;15;0
WireConnection;391;1;417;0
WireConnection;392;1;421;0
WireConnection;250;0;252;0
WireConnection;250;1;243;0
WireConnection;254;0;248;0
WireConnection;254;1;253;0
WireConnection;25;0;33;0
WireConnection;25;1;26;0
WireConnection;131;0;132;0
WireConnection;131;1;130;0
WireConnection;411;0;391;0
WireConnection;411;1;392;0
WireConnection;175;0;177;0
WireConnection;175;1;174;0
WireConnection;373;0;157;1
WireConnection;373;1;174;0
WireConnection;264;0;262;0
WireConnection;264;1;263;0
WireConnection;251;0;250;0
WireConnection;251;1;254;0
WireConnection;323;0;131;0
WireConnection;323;1;390;0
WireConnection;28;0;25;0
WireConnection;107;0;37;0
WireConnection;52;0;411;0
WireConnection;223;0;175;0
WireConnection;375;0;373;0
WireConnection;85;0;84;0
WireConnection;260;0;264;0
WireConnection;255;0;251;0
WireConnection;27;0;28;0
WireConnection;134;0;323;0
WireConnection;82;0;107;0
WireConnection;82;1;80;0
WireConnection;380;0;223;0
WireConnection;380;1;375;0
WireConnection;140;0;139;0
WireConnection;249;0;248;0
WireConnection;259;0;255;0
WireConnection;259;1;260;0
WireConnection;16;0;13;0
WireConnection;16;1;14;0
WireConnection;16;2;27;0
WireConnection;77;0;82;0
WireConnection;156;0;85;0
WireConnection;297;0;159;0
WireConnection;381;0;380;0
WireConnection;141;0;140;0
WireConnection;141;1;143;0
WireConnection;268;0;249;0
WireConnection;268;1;269;0
WireConnection;266;0;259;0
WireConnection;266;1;265;0
WireConnection;39;0;16;0
WireConnection;135;0;85;0
WireConnection;135;1;136;0
WireConnection;382;0;381;0
WireConnection;382;1;297;0
WireConnection;142;0;141;0
WireConnection;148;0;146;5
WireConnection;148;1;147;0
WireConnection;256;0;249;0
WireConnection;256;2;257;0
WireConnection;267;0;268;0
WireConnection;267;1;266;0
WireConnection;95;0;16;0
WireConnection;90;0;135;0
WireConnection;100;0;39;0
WireConnection;78;0;399;5
WireConnection;78;1;79;0
WireConnection;386;0;382;0
WireConnection;144;0;142;0
WireConnection;150;0;148;0
WireConnection;258;0;256;0
WireConnection;258;1;267;0
WireConnection;76;0;78;0
WireConnection;383;0;386;0
WireConnection;383;1;172;0
WireConnection;145;0;144;0
WireConnection;151;0;150;0
WireConnection;271;0;258;0
WireConnection;271;1;270;5
WireConnection;96;0;98;0
WireConnection;96;1;93;0
WireConnection;96;2;99;0
WireConnection;384;0;383;0
WireConnection;384;1;198;0
WireConnection;272;0;271;0
WireConnection;400;0;96;0
WireConnection;400;1;101;0
WireConnection;400;2;105;0
WireConnection;152;0;400;0
WireConnection;152;1;155;0
WireConnection;152;2;154;0
WireConnection;240;0;384;0
WireConnection;222;0;223;0
WireConnection;222;1;157;1
WireConnection;376;0;222;0
WireConnection;226;0;376;0
WireConnection;226;1;297;0
WireConnection;232;0;226;0
WireConnection;229;0;232;0
WireConnection;229;1;172;0
WireConnection;231;0;229;0
WireConnection;231;1;198;0
WireConnection;377;0;231;0
WireConnection;377;1;379;0
WireConnection;387;0;384;0
WireConnection;387;1;379;0
WireConnection;387;2;378;0
WireConnection;387;3;379;2
WireConnection;389;0;377;0
WireConnection;389;2;157;2
WireConnection;372;1;387;0
WireConnection;372;2;157;2
WireConnection;388;0;389;0
WireConnection;388;1;372;0
WireConnection;439;0;388;0
WireConnection;241;0;152;0
WireConnection;241;1;273;0
WireConnection;185;0;186;0
WireConnection;185;1;187;0
WireConnection;185;2;188;0
WireConnection;285;0;283;0
WireConnection;285;1;284;0
WireConnection;300;0;275;0
WireConnection;300;1;283;0
WireConnection;300;2;285;0
WireConnection;300;3;302;0
WireConnection;301;0;275;0
WireConnection;301;1;283;0
WireConnection;301;2;285;0
WireConnection;301;3;303;0
WireConnection;274;0;275;0
WireConnection;274;1;283;0
WireConnection;274;2;285;0
WireConnection;274;3;277;0
WireConnection;304;0;274;2
WireConnection;304;1;300;2
WireConnection;304;2;301;2
WireConnection;305;0;274;3
WireConnection;305;1;300;3
WireConnection;305;2;301;3
WireConnection;298;0;304;0
WireConnection;299;0;305;0
WireConnection;290;0;299;0
WireConnection;278;0;274;0
WireConnection;278;1;300;0
WireConnection;278;2;301;0
WireConnection;279;0;278;0
WireConnection;339;0;175;0
WireConnection;345;0;339;0
WireConnection;345;1;350;0
WireConnection;347;0;353;0
WireConnection;355;0;345;0
WireConnection;355;1;347;0
WireConnection;294;0;292;0
WireConnection;294;1;291;0
WireConnection;354;0;355;0
WireConnection;356;0;294;0
WireConnection;349;0;354;0
WireConnection;337;0;356;0
WireConnection;337;1;338;0
WireConnection;337;2;349;0
WireConnection;280;0;279;0
WireConnection;289;0;298;0
WireConnection;67;0;66;0
WireConnection;73;0;72;0
WireConnection;73;1;74;0
WireConnection;71;0;70;0
WireConnection;71;1;73;0
WireConnection;69;0;67;0
WireConnection;69;1;71;0
WireConnection;351;0;337;0
WireConnection;75;1;69;0
WireConnection;70;0;68;0
WireConnection;437;0;435;0
WireConnection;438;0;436;0
WireConnection;397;0;241;0
WireConnection;397;1;242;0
WireConnection;371;2;241;0
WireConnection;371;13;242;0
ASEEND*/
//CHKSM=C2BBA0B0FC72F494203CB7DC302897527599115F