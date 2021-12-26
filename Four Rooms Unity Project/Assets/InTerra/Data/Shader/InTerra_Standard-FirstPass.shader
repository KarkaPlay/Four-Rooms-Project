Shader "InTerra/Terrain (Standard With Features)" 
{
    Properties {
        [HideInInspector] _MainTex ("BaseMap (RGB)", 2D) = "white" {}
        [HideInInspector] _Color ("Main Color", Color) = (1,1,1,1)
        [HideInInspector] _TerrainHolesTexture("Holes Map (RGB)", 2D) = "white" {}
		_HT_distance("Distance",  vector) = (3,10,0,25)
        _HT_distance_scale("Scale",   Range(0,0.5)) = 0.25
        _HT_cover("Cover strenght",   Range(0,1)) = 0.6 
        _HeightTransition("Height blending Sharpness",   Range(0,60)) = 50
        _Distance_HeightTransition("Distance Height blending Sharpness ", Range(0,60)) = 10        
        _TriplanarSharpness("Triplanar Sharpness",   Range(4,10)) = 9
        [HideInInspector] _TerrainSizeXZPosY("",  Vector) = (0,0,0)
        [HideInInspector] _NumLayersCount("", Float) = 0
        [HideInInspector] _TriplanarOneToAllSteep("", Float) = 0 
    } 

    SubShader { 
        Tags {
            "Queue" = "Geometry-100"
            "RenderType" = "Opaque" 
            "TerrainCompatible" = "True"
        }

        CGPROGRAM
        #pragma surface surf Standard vertex:SplatmapVert finalcolor:SplatmapFinalColor finalgbuffer:SplatmapFinalGBuffer addshadow fullforwardshadows
        #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap forwardadd
        #pragma multi_compile_fog // needed because finalcolor oppresses fog code generation.

        #define _ALPHATEST_ON //you can delete this line if you are not using Terrain Holes - the multi_compile_local was removed because there are already too many variants and this Keyword has insignificant impact on performance 
        #pragma multi_compile_local __ _NORMALMAP

        #pragma target 3.0
        #include "UnityPBSLighting.cginc"
                
        #pragma shader_feature_local __ _TERRAIN_MASK_MAPS _TERRAIN_NORMAL_IN_MASK
        #pragma shader_feature_local __ _TERRAIN_TRIPLANAR_ONE _TERRAIN_TRIPLANAR

        #pragma shader_feature_local _TERRAIN_DISTANCEBLEND        
        #pragma shader_feature_local _TERRAIN_BLEND_HEIGHT

        #pragma shader_feature_local TWO_LAYERS

        #define TERRAIN_INSTANCED_PERPIXEL_NORMAL
        #define TERRAIN_SURFACE_OUTPUT SurfaceOutputStandard   

        #include "InTerra_InputsAndFunctions.cginc"
        #include "InTerra_Mixing.cginc"
       
        //============================================================================
        //---------------------------------  SURFACE ---------------------------------
        //============================================================================
        void surf (Input IN, inout SurfaceOutputStandard o) {
            half weight;
            fixed4 mixedDiffuse;

            #ifdef TWO_LAYERS
                half4 defaultSmoothness = half4(_Smoothness0, _Smoothness1, 0, 0);
            #else
                half4 defaultSmoothness = half4(_Smoothness0, _Smoothness1, _Smoothness2, _Smoothness3);
            #endif

            SplatmapMix(IN, defaultSmoothness, weight, mixedDiffuse, o.Normal, o.Occlusion, o.Metallic);
            o.Albedo = mixedDiffuse.rgb;
            o.Alpha = weight;
            o.Smoothness = mixedDiffuse.a;
        }
        ENDCG

        UsePass "Hidden/Nature/Terrain/Utilities/PICKING"
        UsePass "Hidden/Nature/Terrain/Utilities/SELECTION"
    }

    Dependency "AddPassShader"    = "Hidden/InTerra/InTerra-AddPass"
    Dependency "BaseMapShader"    = "Hidden/InTerra/InTerra-Base"
    Dependency "BaseMapGenShader" = "Hidden/InTerra/InTerra-BaseGen"

    Fallback "Nature/Terrain/Diffuse"
	
	CustomEditor "InTerra.InTerra_TerrainShaderGUI" 
}