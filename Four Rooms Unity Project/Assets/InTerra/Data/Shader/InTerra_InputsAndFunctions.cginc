#if defined(_TERRAIN_MASK_MAPS) || defined(_TERRAIN_NORMAL_IN_MASK) || defined(_TERRAIN_BLEND_HEIGHT) && !defined(DIFFUSE)
    #define TERRAIN_MASK
#endif

#if (defined (_TERRAIN_TRIPLANAR) || defined (_OBJECT_TRIPLANAR) || defined (_TERRAIN_TRIPLANAR_ONE))
    #define TRIPLANAR
#endif

#ifndef TERRAIN_BASEGEN
    struct Input
    {   
        float3 worldPos;
        #if !defined(INTERRA_OBJECT)
            float4 tc;
        #else 
            float4 mainUV_tWeightY_hOffset;
            #ifdef _OBJECT_DETAIL
                float2 uv_DetailAlbedoMap;
            #endif          
        #endif
        #if defined(INTERRA_OBJECT) || defined(TRIPLANAR) || defined (_TERRAIN_TRIPLANAR_ONE)
            float3 worldNormal;
            float3 wOrigNormal;
        #endif
        #ifndef TERRAIN_BASE_PASS
            UNITY_FOG_COORDS(0) // needed because finalcolor oppresses fog code generation.
            INTERNAL_DATA
        #endif
    };
#endif

#ifndef INTERRA_OBJECT
    #if defined(UNITY_INSTANCING_ENABLED) && !defined(SHADER_API_D3D11_9X)
        sampler2D _TerrainHeightmapTexture;
        sampler2D _TerrainNormalmapTexture;
        float4    _TerrainHeightmapRecipSize;   // float4(1.0f/width, 1.0f/height, 1.0f/(width-1), 1.0f/(height-1))
        float4    _TerrainHeightmapScale;       // float4(hmScale.x, hmScale.y / (float)(kMaxHeight), hmScale.z, 0.0f)
    #endif

    UNITY_INSTANCING_BUFFER_START(Terrain)
    UNITY_DEFINE_INSTANCED_PROP(float4, _TerrainPatchInstanceData) // float4(xBase, yBase, skipScale, ~)
    UNITY_INSTANCING_BUFFER_END(Terrain)

    #ifdef _ALPHATEST_ON
        sampler2D _TerrainHolesTexture;

        void ClipHoles(float2 uv)
        {
            float hole = tex2D(_TerrainHolesTexture, uv).r;
            clip(hole == 0.0f ? -1 : 1);
        }
    #endif
        
    #if defined(TERRAIN_BASE_PASS) && (defined(UNITY_PASS_META) || defined(TRIPLANAR))
    // When we render albedo for GI baking, we actually need to take the ST, or for triplanar mapping
    float4 _MainTex_ST;
    #endif

    #ifdef TRIPLANAR
        float3 _TerrainSizeXZPosY;
    #endif
#endif

sampler2D _Control; 

float4 _Control_ST, _Control_TexelSize;
float4 _TerrainHeightmapTexture_TexelSize;
float4 _TerrainPosition, _TerrainSize;

float3 _HeightmapScale;

half4 _HT_distance;
float _HT_distance_scale, _HT_cover;
fixed _HeightTransition, _Distance_HeightTransition, _NumLayersCount, _TriplanarOneToAllSteep, _TriplanarSharpness;


#ifdef INTERRA_OBJECT 
    sampler2D _MainTex, _BumpMap, _MainMask;
    half4 _Color;
    half _BumpScale, _Ao, _Glossiness, _Metallic;
    float4 _MainTex_ST;

    fixed _HasMask, _PassNumber;
    half4 _MaskMapRemapScale, _MaskMapRemapOffset;

    sampler2D _DetailAlbedoMap, _DetailNormalMap;
    half _DetailStrenght, _DetailNormalMapScale;
    
    half4  _Intersection, _Intersection2, _NormIntersect;
        
    half _Sharpness;   
    half _Steepness, _SteepDistortion;

    half4 _TerrainSmoothness, _TerrainMetallic;
    fixed _DisableOffsetY, _DisableDistanceBlending;

    sampler2D _TerrainHeightmapTexture;
    sampler2D _TerrainNormalmapTexture;

    #if defined(_NORMALMAP) || defined(_TERRAIN_NORMAL_IN_MASK)
        float4 _TerrainNormalScale;
    #endif
#endif


#ifdef ONE_LAYER
    sampler2D _Splat0;
    sampler2D _Mask0;
    float4 _SplatUV0;
    fixed4 _MaskMapRemapScale0;
    fixed4 _MaskMapRemapOffset0;
    #if defined(_NORMALMAP) && !defined(_TERRAIN_NORMAL_IN_MASK)
        sampler2D _Normal0;
    #endif
#endif

#ifdef TWO_LAYERS    
    sampler2D _Splat0, _Splat1; 
    sampler2D _Mask0, _Mask1;
    fixed4 _MaskMapRemapScale0, _MaskMapRemapScale1;
    fixed4 _MaskMapRemapOffset0, _MaskMapRemapOffset1;
    fixed _ControlNumber, _LayerIndex2, _LayerIndex1;
    #if defined(_NORMALMAP) && !defined(_TERRAIN_NORMAL_IN_MASK)
        sampler2D _Normal0, _Normal1;
    #endif
    #ifdef INTERRA_OBJECT 
        float4 _SplatUV0, _SplatUV1;
    #else
        float4 _Splat0_ST, _Splat1_ST;
        half _Metallic0, _Metallic1;
        half _Smoothness0, _Smoothness1;
        float _NormalScale0, _NormalScale1;
    #endif 
#endif

#if !defined(ONE_LAYER) && !defined(TWO_LAYERS)
    #if defined(TERRAIN_BASEGEN) || defined(DIFFUSE)  
        sampler2D _Splat0, _Splat1, _Splat2, _Splat3;
    #endif
    #ifndef TERRAIN_BASEGEN 
        #ifndef DIFFUSE
            UNITY_DECLARE_TEX2D(_Splat0);
            UNITY_DECLARE_TEX2D(_Splat1);
            UNITY_DECLARE_TEX2D(_Splat2);
            UNITY_DECLARE_TEX2D(_Splat3);

        
            UNITY_DECLARE_TEX2D_NOSAMPLER(_Mask0);
            UNITY_DECLARE_TEX2D_NOSAMPLER(_Mask1);
            UNITY_DECLARE_TEX2D_NOSAMPLER(_Mask2);
            UNITY_DECLARE_TEX2D_NOSAMPLER(_Mask3);
        #endif 
        #if defined(_NORMALMAP) && !defined(_TERRAIN_NORMAL_IN_MASK)
            #ifdef DIFFUSE 
                sampler2D _Normal0, _Normal1, _Normal2, _Normal3;
            #else
                UNITY_DECLARE_TEX2D_NOSAMPLER(_Normal0);
                UNITY_DECLARE_TEX2D_NOSAMPLER(_Normal1);
                UNITY_DECLARE_TEX2D_NOSAMPLER(_Normal2);
                UNITY_DECLARE_TEX2D_NOSAMPLER(_Normal3);
            #endif
        #endif
    #else
        sampler2D _Mask0, _Mask1, _Mask2, _Mask3;
    #endif
    fixed4 _MaskMapRemapScale0, _MaskMapRemapScale1, _MaskMapRemapScale2, _MaskMapRemapScale3;
    fixed4 _MaskMapRemapOffset0, _MaskMapRemapOffset1, _MaskMapRemapOffset2, _MaskMapRemapOffset3;

    #ifdef INTERRA_OBJECT 
        float4 _SplatUV0, _SplatUV1, _SplatUV2, _SplatUV3;
    #else
        float4 _Splat0_ST, _Splat1_ST, _Splat2_ST, _Splat3_ST;
        half _Metallic0, _Metallic1, _Metallic2, _Metallic3;
        half _Smoothness0, _Smoothness1, _Smoothness2, _Smoothness3;
        float _NormalScale0, _NormalScale1, _NormalScale2, _NormalScale3;
    #endif               
#endif


//=======================================================================================
//===================================   FUNCTIONS   =====================================
//=======================================================================================
float2 ObjectFrontUV(float posOffset, half4 splatUV, float offsetZ)
{
    return  float2((posOffset + splatUV.z) / splatUV.x, (offsetZ + splatUV.w) / splatUV.y);
}

float2 ObjectSideUV(float posOffset, half4 splatUV, float offsetX)
{
    return  float2((offsetX + splatUV.z) / splatUV.x, (posOffset + splatUV.w) / splatUV.y);
}

half3 WorldTangent(float3 wTangent, float3 wBTangent, half3 mixedNormal)
{
    mixedNormal.xy = mul(float2x2(wTangent.xz, wBTangent.xz), mixedNormal.xy);
    return  half3(mixedNormal);
}

half3 WorldTangentFrontSide(float3 wTangent, float3 wBTangent, half3 mixedNormal, half3 normal_front, half3 normal_side, fixed3 flipUV, half3 weights)
{
    normal_front.y *= -flipUV.z;
    normal_front.xy = mul(float2x2(wTangent.xy, wBTangent.xy), normal_front.xy);

    normal_side.x *= -flipUV.x;
    normal_side.xy = mul(float2x2(wTangent.yz, wBTangent.yz), normal_side.xy);

    return  half3(mixedNormal * weights.y + normal_front * weights.z + normal_side * weights.x);
}

half2 HeightBlendTwoTextures(float2 splat, float2 heights, fixed sharpness)
{
    splat *= (1 / (1 * pow(2, heights * (-(sharpness)))) + 1) * 0.5;
    splat /= (splat.r + splat.g);

    return  splat;
}

half3 UnpackNormalGAWithScale(half4 packednormal, float scale)
{
    fixed3 normal;
    normal.xy = (packednormal.wy * 2 - 1) * scale;
    normal.z = sqrt(1 - saturate(dot(normal.xy, normal.xy)));

    return normal;
}

#if defined(TRIPLANAR) && !defined(INTERRA_OBJECT)
    float2 TerrainFrontUV(float3 wPos, half4 splatUV, float2 tc) 
    {
        return  float2(tc.x, (wPos.y - _TerrainSizeXZPosY.z) * (splatUV.y / _TerrainSizeXZPosY.y) + splatUV.w);
    }

    float2 TerrainSideUV(float3 wPos, half4 splatUV,  float2 tc)
    {
        return  float2(tc.y, (wPos.y - _TerrainSizeXZPosY.z) * (splatUV.x / _TerrainSizeXZPosY.x) + splatUV.z); 
    }
#endif

void TriplanarOneToAllSteep(in out float4 splat_control, float weightY, in out half splatWeight)
{   
    if (_TriplanarOneToAllSteep == 1)
    {
       #if !defined(TERRAIN_SPLAT_ADDPASS) 
            splat_control = float4(saturate(splat_control.r + weightY), saturate(splat_control.gba - weightY));
            splatWeight = saturate(splatWeight + weightY);
        #else
            splat_control = float4(saturate(splat_control.rgba - weightY));
           splatWeight = saturate(splatWeight - weightY);
        #endif
    }
}  

half3 TriplanarNormal(half3 normal, half3 tangent, half3 bTangent, half3 normal_front, half3 normal_side, float3 weights, fixed3 flipUV)
{
    #ifdef INTERRA_OBJECT
        normal_front.y *= -flipUV.z;
        normal_front.xy = mul(float2x2(tangent.xy, bTangent.xy), normal_front.xy);

        normal_side.x *= -flipUV.x;
        normal_side.xy = mul(float2x2(tangent.yz, bTangent.yz), normal_side.xy);
    #else
         normal_front.y *= -flipUV.z;
         normal_side.xy = normal_side.yx; //this is needed because the uv was rotated
         normal_side.x *= -flipUV.x;
    #endif

    return half3 (normal * weights.y + normal_front * weights.z + normal_side * weights.x);
}

void TriplanarBase(in out half4 baseMap, half4 front, half4 side, float3 weights, float2 splat, fixed firstToAllSteep)
{
    baseMap = firstToAllSteep == 1 ? (baseMap * weights.y + front * weights.z + side * weights.x) : (baseMap * saturate(weights.y + (1 - splat.g))) + (((front * weights.z) + (side * weights.x)) * (splat.r));
}


//=======================================================================================
//-----------------------------------   ONE LAYER   -------------------------------------
//=======================================================================================
#ifdef ONE_LAYER
    void SampleSplat(out half4 splat, float2 uv, half defaultAlpha, half4 mask)
    {
        splat = tex2D(_Splat0, uv);
        #ifndef DIFFUSE
            #if defined(_TERRAIN_MASK_MAPS) && !defined(_TERRAIN_NORMAL_IN_MASK)
                splat.a = mask.a;
            #else
                splat.a = splat.a * defaultAlpha.r;
            #endif
        #endif
    }

    void SplatWeight(out half4 mixedDiffuse, half4 splat, float4 splat_control)
    {
        mixedDiffuse = splat;
    }

    void TriplanarWeight(inout half4 mask, half4 mask_front, half4 mask_side, float3 weights)
    {
        mask = (mask * weights.y) + (mask_front * weights.z) + (mask_side * weights.x);
    }

    #ifndef DIFFUSE
        void SampleMask(out half4 mask, float2 uv)
        {
            mask = tex2D(_Mask0, uv);

            #ifdef _TERRAIN_NORMAL_IN_MASK
                mask.rb = mask.rb * _MaskMapRemapScale0.gb + _MaskMapRemapOffset0.gb;
            #else
                mask.rgba = mask.rgba * _MaskMapRemapScale0.rgba + _MaskMapRemapOffset0.rgba;
            #endif
        }

        void MaskWeight(inout half4 mask, float4 mask_front, float4 mask_side, float3 weights)
        {
            mask = (mask * weights.y) + (mask_front * weights.z) + (mask_side * weights.x);
        }
    #endif

    #if defined(_NORMALMAP) && !defined(_TERRAIN_NORMAL_IN_MASK)
        half3 SampleNormal(float2 uv, float splat_control, half4 normalScale)
        {
            return  half3 (UnpackNormalWithScale(tex2D(_Normal0, uv), normalScale.x));
        }
    #endif

    #ifdef _TERRAIN_NORMAL_IN_MASK
        half3 MaskNormal(float4 mask, float4 splat_control, half4 normalScale)
        {
            return  half3 (UnpackNormalGAWithScale(mask, normalScale.x));
        }
    #endif

    void UvSplat(out float2 uvSplat, float2 posOffset)
    {
        uvSplat = ((posOffset + _SplatUV0.zw) / _SplatUV0.xy);
    }
    
    void UvSplatDistort(out float2 uvSplat, float2 posOffset, fixed distortion)
    {        
        uvSplat = ((posOffset + (_SplatUV0.zw + distortion)) / _SplatUV0.xy);
    }

    void DistantUV(out float2 distantUV, float2 uvSplat)
    {
        distantUV = uvSplat * _HT_distance_scale;
    }

    void UvSplatFront(out float2 uvSplat, float worldPos, float offset)
    {
        uvSplat = (ObjectFrontUV(worldPos, _SplatUV0, offset));
    }

    void UvSplatSide(out float2 uvSplat, float worldPos, float offset)
    {
        uvSplat = (ObjectSideUV(worldPos, _SplatUV0, offset));
    }

    #ifdef TERRAIN_MASK       
        half AmbientOcclusion(half4 mask, half4 splat_control)
        {
            #ifdef _TERRAIN_NORMAL_IN_MASK
                return  mask.r;
            #else
                return  mask.g;
            #endif  
        }

        half MetallicMask(half4 mask, half4 splat_control)
        {
             return  mask.r;
        }
    #endif

    half HeightSum(half4 mask, half4 splat_control)
    {
        #ifdef DIFFUSE
            return half(mask.a);
        #else 
            return half(mask.b);
        #endif        
    }

    half Metallic(half4 splat_control)
    {
        return  _TerrainMetallic.x;
    }
#endif 


//=======================================================================================
//-----------------------------------   TWO LAYERS   ------------------------------------
//=======================================================================================
#ifdef TWO_LAYERS
    #ifdef _TERRAIN_BLEND_HEIGHT
        void  HeightBlend(half4 mask[2], inout float4 splat_control, fixed sharpness)
        {
            #ifdef DIFFUSE
                half2 height = half2(mask[0].a, mask[1].a);
            #else
                half2 height = half2(mask[0].b, mask[1].b);
            #endif
            splat_control.rg *= (1 / (1 * pow(2, (height + splat_control.rg) * (-(sharpness)))) + 1) * 0.5;
            splat_control.rg /= (splat_control.r + splat_control.g);
        }
    #endif 

    void SampleSplat(out half4 splat[2], float2 uv[2], half4 defaultAlpha, half4 mask[2])
    {
        splat[0] = tex2D(_Splat0, uv[0]);
        splat[1] = tex2D(_Splat1, uv[1]);
  
        #ifndef DIFFUSE
            #if defined(_TERRAIN_MASK_MAPS) && !defined(_TERRAIN_NORMAL_IN_MASK)
                splat[0].a = mask[0].a;
                splat[1].a = mask[1].a;
            #else
                splat[0].a *= defaultAlpha.r;
                splat[1].a *= defaultAlpha.g;
            #endif
        #endif  
    }

    void SplatWeight(out half4 mixedDiffuse, half4 splat[2], float4 splat_control)
    {  
        mixedDiffuse = splat[0] * splat_control.r + splat[1] * splat_control.g;
    }

    void TriplanarWeight(inout half4 mask[2], half4 mask_front[2], half4 mask_side[2], float3 weights)
    {
        for (int i = 0; i < 2; ++i)
        {
            mask[i] = (mask[i] * weights.y) + (mask_front[i] * weights.z) + (mask_side[i] * weights.x); 
        }
    }

    #ifndef DIFFUSE
        void SampleMask(out half4 mask[2], float2 uv[2])
        {
            mask[0] = tex2D(_Mask0, uv[0]);
            mask[1] = tex2D(_Mask1, uv[1]);

            #ifdef _TERRAIN_NORMAL_IN_MASK
                mask[0].rb = mask[0].rb * _MaskMapRemapScale0.gb + _MaskMapRemapOffset0.gb;
                mask[1].rb = mask[1].rb * _MaskMapRemapScale1.gb + _MaskMapRemapOffset1.gb;
            #else
                mask[0].rgba = mask[0].rgba * _MaskMapRemapScale0.rgba + _MaskMapRemapOffset0.rgba;
                mask[1].rgba = mask[1].rgba * _MaskMapRemapScale1.rgba + _MaskMapRemapOffset1.rgba;
            #endif
        }

        void MaskWeight(inout half4 mask[2], half4 mask_front[2], half4 mask_side[2], float3 weights)
        {
            for (int i = 0; i < 2; ++i)
            {
                mask[i] = (mask[i] * weights.y) + (mask_front[i] * weights.z) + (mask_side[i] * weights.x);
            }
        }
    #endif
    #if defined(_NORMALMAP) && !defined(_TERRAIN_NORMAL_IN_MASK)
        half3 SampleNormal(float2 uv[2], float4 splat_control, half4 normalScale)
        {
            half3 normal;
            normal  = UnpackNormalWithScale(tex2D(_Normal0, uv[0]), normalScale.x) * splat_control.r;
            normal += UnpackNormalWithScale(tex2D(_Normal1, uv[1]), normalScale.y) * splat_control.g;
            return  normal;
        }
    #endif

    #ifdef INTERRA_OBJECT
        void UvSplat(out float2 uvSplat[2],float2 posOffset)
        {
            uvSplat[0] = (posOffset + _SplatUV0.zw) / _SplatUV0.xy;
            uvSplat[1] = (posOffset + _SplatUV1.zw) / _SplatUV1.xy;
        }

        void UvSplatDistort(out float2 uvSplat[2], float2 posOffset, fixed distortion)
        {
            uvSplat[0] = (posOffset + (_SplatUV0.zw + distortion)) / _SplatUV0.xy;
            uvSplat[1] = (posOffset + (_SplatUV1.zw + distortion)) / _SplatUV1.xy;
        }

        void UvSplatFront(out float2 uvSplat[2], float worldPos, float offset)
        {
            uvSplat[0] = ObjectFrontUV(worldPos, _SplatUV0, offset);
            uvSplat[1] = ObjectFrontUV(worldPos, _SplatUV1, offset);
        }

        void UvSplatSide(out float2 uvSplat[2], float worldPos, float offset)
        {
            uvSplat[0] = ObjectSideUV(worldPos, _SplatUV0, offset);
            uvSplat[1] = ObjectSideUV(worldPos, _SplatUV1, offset);
        }
    #endif

    void DistantUV(out float2 distantUV[2], float2 uvSplat[2])
    {
        for (int i = 0; i < 2; ++i)
        {
            distantUV[i] = uvSplat[i] * _HT_distance_scale;
        }
    }

    #ifdef _TERRAIN_NORMAL_IN_MASK    
        half3 MaskNormal(half4 mask[2], float4 splat_control, half4 normalScale)
        {
            half3 normal;
            normal  = UnpackNormalGAWithScale(mask[0], normalScale.x) * splat_control.r;
            normal += UnpackNormalGAWithScale(mask[1], normalScale.y) * splat_control.g;
            return normal;
        } 
    #endif

    #ifdef TERRAIN_MASK
        half AmbientOcclusion(half4 mask[2], half4 splat_control)
        {            
            #ifdef _TERRAIN_NORMAL_IN_MASK
                half2 ao = half2(mask[0].r, mask[1].r);
            #else
                half2 ao = half2(mask[0].g, mask[1].g);
            #endif  

            return  half(dot(splat_control.rg, half2(ao.r, ao.g)));
        }

        half MetallicMask(half4 mask[2], half4 splat_control)
        {            
            return  dot(splat_control.rg, half2(mask[0].r, mask[1].r));
        }
    #endif

    half Metallic(half4 splat_control)
    {
        #ifdef INTERRA_OBJECT 
            return dot(splat_control.rg, _TerrainMetallic.xy);
        #else
            return dot(splat_control.rg, half2(_Metallic0, _Metallic1));
        #endif           
    }

    half HeightSum(half4 mask[2], half4 splat_control)
    {
        #ifdef DIFFUSE
            return half(dot(splat_control.rg, half2(mask[0].a, mask[1].a)));
        #else 
            return half(dot(splat_control.rg, half2(mask[0].b, mask[1].b)));
        #endif        
    }

    void SampleSplatTOL(out half4 splat[2], half4 noTriplanarSplat[2], float2 uv, half defaultAlpha, float4 splat_control, half4 mask)
    {
        splat[0] = tex2D(_Splat0, uv);

        #if defined(_TERRAIN_MASK_MAPS) && !defined(_TERRAIN_NORMAL_IN_MASK)
            splat[0].a = mask.a;
        #else
            splat[0].a *= defaultAlpha.r;
        #endif 

        splat[1] = noTriplanarSplat[1];
    }

    #ifdef TERRAIN_MASK
        void SampleMaskTOL(out half4 mask[2], half4 noTriplanarMask[2], float2 uv)
        {
            mask[0] = tex2D(_Mask0, uv);
            
            #ifdef _TERRAIN_NORMAL_IN_MASK
                mask[0].rb = mask[0].rb * _MaskMapRemapScale0.gb + _MaskMapRemapOffset0.gb;
            #else
                mask[0].rgba = mask[0].rgba * _MaskMapRemapScale0.rgba + _MaskMapRemapOffset0.rgba;
            #endif

            mask[1] = noTriplanarMask[1];
        }
    #endif

    #if defined(_NORMALMAP) && !defined(_TERRAIN_NORMAL_IN_MASK)
        half3 SampleNormalTOL(float2 uv[2], half3 noTriplanarNormal, float4 splat_control, half4 normalScale)
        {
            half3 normal = (UnpackNormalWithScale(tex2D(_Normal0, uv[0]), normalScale.x));

            normal *= splat_control.r;
            noTriplanarNormal *= splat_control.g;
            normal += noTriplanarNormal;

            return normal;
        }
    #endif

    #ifdef _TERRAIN_NORMAL_IN_MASK
        half3 MaskNormalTOL(half4 mask[2], half3 noTriplanarNormal, float4 splat_control, half4 normalScale)
        {
            half3 normal;
            normal = UnpackNormalGAWithScale(mask[0], normalScale.x) * splat_control.r;
            normal *= splat_control.r;
            noTriplanarNormal *= splat_control.g;
            normal += noTriplanarNormal;
            return  normal;
        }
    #endif
#endif

//=======================================================================================
//-----------------------------------    ONE PASS   -------------------------------------
//=======================================================================================
#if !defined(ONE_LAYER) && !defined(TWO_LAYERS)
    #ifdef _TERRAIN_BLEND_HEIGHT
        void HeightBlend(half4 mask[4], inout float4 splat_control, fixed sharpness)
        {
            #ifdef DIFFUSE
                half4 height = half4 (mask[0].a, mask[1].a, mask[2].a, mask[3].a);
            #else
                half4 height = half4 (mask[0].b, mask[1].b, mask[2].b, mask[3].b);
            #endif
            splat_control.rgba *= (1 / (1 * pow(2, (height + splat_control.rgba) * (-(sharpness)))) + 1) * 0.5;
            splat_control.rgba /= (splat_control.r + splat_control.g + splat_control.b + splat_control.a);
        }
    #endif 

    void SampleSplat(out half4 splat[4], float2 uv[4], half4 defaultAlpha, half4 mask[4])
    {
        #if defined(TERRAIN_BASEGEN) || defined(DIFFUSE)
            splat[0] = tex2D(_Splat0, uv[0]);
            splat[1] = tex2D(_Splat1, uv[1]);
            splat[2] = tex2D(_Splat2, uv[2]);
            splat[3] = tex2D(_Splat3, uv[3]);
        #else
            splat[0] = UNITY_SAMPLE_TEX2D(_Splat0, uv[0]);
            splat[1] = UNITY_SAMPLE_TEX2D(_Splat1, uv[1]);
            splat[2] = UNITY_SAMPLE_TEX2D(_Splat2, uv[2]);
            splat[3] = UNITY_SAMPLE_TEX2D(_Splat3, uv[3]);
        #endif  

        #ifndef DIFFUSE
            #if defined(_TERRAIN_MASK_MAPS) && !defined(_TERRAIN_NORMAL_IN_MASK)
                splat[0].a = mask[0].a;
                splat[1].a = mask[1].a;
                splat[2].a = mask[2].a;
                splat[3].a = mask[3].a;
            #else
                splat[0].a *= defaultAlpha.r;
                splat[1].a *= defaultAlpha.g;
                splat[2].a *= defaultAlpha.b;
                splat[3].a *= defaultAlpha.a;
            #endif
        #endif  
    }

    void SplatWeight(out half4 mixedDiffuse, half4 splat[4], float4 splat_control)
    {  
        mixedDiffuse = splat[0] * splat_control.r + splat[1] * splat_control.g + splat[2] * splat_control.b + splat[3] * splat_control.a;
    }

    void TriplanarWeight(inout half4 mask[4], half4 mask_front[4], half4 mask_side[4], float3 weights)
    {
        for (int i = 0; i < 4; ++i)
        {
            mask[i] = (mask[i] * weights.y) + (mask_front[i] * weights.z) + (mask_side[i] * weights.x);
        }
    }

    #ifndef DIFFUSE
        void SampleMask(out half4 mask[4], float2 uv[4])
        {
            #ifndef TERRAIN_BASEGEN
                mask[0] = UNITY_SAMPLE_TEX2D_SAMPLER(_Mask0, _Splat0, uv[0]);
                mask[1] = UNITY_SAMPLE_TEX2D_SAMPLER(_Mask1, _Splat1, uv[1]);
                mask[2] = UNITY_SAMPLE_TEX2D_SAMPLER(_Mask2, _Splat2, uv[2]);
                mask[3] = UNITY_SAMPLE_TEX2D_SAMPLER(_Mask3, _Splat3, uv[3]);
            #else
                mask[0] = tex2D(_Mask0, uv[0]);
                mask[1] = tex2D(_Mask1, uv[1]);
                mask[2] = tex2D(_Mask2, uv[2]);
                mask[3] = tex2D(_Mask3, uv[3]);
            #endif

            #ifdef _TERRAIN_NORMAL_IN_MASK
                mask[0].rb = mask[0].rb * _MaskMapRemapScale0.gb + _MaskMapRemapOffset0.gb;
                mask[1].rb = mask[1].rb * _MaskMapRemapScale1.gb + _MaskMapRemapOffset1.gb;
                mask[2].rb = mask[2].rb * _MaskMapRemapScale2.gb + _MaskMapRemapOffset2.gb;
                mask[3].rb = mask[3].rb * _MaskMapRemapScale3.gb + _MaskMapRemapOffset3.gb;
            #else
                mask[0].rgba = mask[0].rgba * _MaskMapRemapScale0.rgba + _MaskMapRemapOffset0.rgba;
                mask[1].rgba = mask[1].rgba * _MaskMapRemapScale1.rgba + _MaskMapRemapOffset1.rgba;
                mask[2].rgba = mask[2].rgba * _MaskMapRemapScale2.rgba + _MaskMapRemapOffset2.rgba;
                mask[3].rgba = mask[3].rgba * _MaskMapRemapScale3.rgba + _MaskMapRemapOffset3.rgba;
            #endif
        }

        void MaskWeight(inout half4 mask[4], half4 mask_front[4], half4 mask_side[4], float3 weights)
        {
            for (int i = 0; i < 4; ++i)
            {
            mask[i] = (mask[i] * weights.y) + (mask_front[i] * weights.z) + (mask_side[i] * weights.x);
            }
        }
    #endif  

    #ifdef TERRAIN_MASK
        half AmbientOcclusion(half4 mask[4], half4 splat_control)
        {
            #ifdef _TERRAIN_NORMAL_IN_MASK
                half4 ao = half4(mask[0].r, mask[1].r, mask[2].r, mask[3].r);
            #else
                half4 ao = half4(mask[0].g, mask[1].g, mask[2].g, mask[3].g);
            #endif  

            return  half(dot(splat_control, half4(ao.r, ao.g, ao.b, ao.a)));
        }

        half MetallicMask(half4 mask[4], half4 splat_control)
        {       
            return dot(splat_control, half4(mask[0].r, mask[1].r, mask[2].r, mask[3].r));
        } 
    #endif  

    half Metallic(half4 splat_control)
    {
        #ifdef INTERRA_OBJECT 
            return dot(splat_control, _TerrainMetallic);
        #else
            return dot(splat_control, half4(_Metallic0, _Metallic1, _Metallic2, _Metallic3));
        #endif         
    }

    half HeightSum(half4 mask[4], half4 splat_control)
    {        
        #ifdef DIFFUSE
            return half(dot(splat_control, half4(mask[0].a, mask[1].a, mask[2].a, mask[3].a)));
        #else 
            return half(dot(splat_control, half4(mask[0].b, mask[1].b, mask[2].b, mask[3].b)));
        #endif
    }

    #if defined(_NORMALMAP) && !defined(_TERRAIN_NORMAL_IN_MASK)
        half3 SampleNormal(float2 uv[4], float4 splat_control, half4 normalScale)
        {
            half4 normal[4];
            #ifdef DIFFUSE 
                normal[0] = tex2D(_Normal0, uv[0]);
                normal[1] = tex2D(_Normal1, uv[1]);
                normal[2] = tex2D(_Normal2, uv[2]);
                normal[3] = tex2D(_Normal3, uv[3]);
            #else
                normal[0] = UNITY_SAMPLE_TEX2D_SAMPLER(_Normal0, _Splat0, uv[0]);
                normal[1] = UNITY_SAMPLE_TEX2D_SAMPLER(_Normal1, _Splat1, uv[1]);
                normal[2] = UNITY_SAMPLE_TEX2D_SAMPLER(_Normal2, _Splat2, uv[2]);
                normal[3] = UNITY_SAMPLE_TEX2D_SAMPLER(_Normal3, _Splat3, uv[3]);
            #endif  
            half3 mixedNormal;
            mixedNormal  = UnpackNormalWithScale(normal[0], normalScale.x) * splat_control.r;
            mixedNormal += UnpackNormalWithScale(normal[1], normalScale.y) * splat_control.g;
            mixedNormal += UnpackNormalWithScale(normal[2], normalScale.z) * splat_control.b;
            mixedNormal += UnpackNormalWithScale(normal[3], normalScale.w) * splat_control.a;
            return  mixedNormal;
        }
    #endif   

    #ifdef _TERRAIN_NORMAL_IN_MASK 
        half3 MaskNormal(half4 mask[4], float4 splat_control, half4 normalScale)
        {
            half3 normal;
            normal  = UnpackNormalGAWithScale(mask[0], normalScale.x) * splat_control.r;
            normal += UnpackNormalGAWithScale(mask[1], normalScale.y) * splat_control.g;
            normal += UnpackNormalGAWithScale(mask[2], normalScale.z) * splat_control.b;
            normal += UnpackNormalGAWithScale(mask[3], normalScale.w) * splat_control.a;
            return  normal;
        }
    #endif
                
    #ifdef INTERRA_OBJECT
        void UvSplat(out float2 uvSplat[4], float2 posOffset)
        {
            uvSplat[0] = (posOffset + _SplatUV0.zw) / _SplatUV0.xy;
            uvSplat[1] = (posOffset + _SplatUV1.zw) / _SplatUV1.xy;
            uvSplat[2] = (posOffset + _SplatUV2.zw) / _SplatUV2.xy;
            uvSplat[3] = (posOffset + _SplatUV3.zw) / _SplatUV3.xy;
        }

        void UvSplatDistort(out float2 uvSplat[4], float2 posOffset, fixed distortion)
        {
            uvSplat[0] = (posOffset + (_SplatUV0.zw + distortion)) / _SplatUV0.xy;
            uvSplat[1] = (posOffset + (_SplatUV1.zw + distortion)) / _SplatUV1.xy;
            uvSplat[2] = (posOffset + (_SplatUV2.zw + distortion)) / _SplatUV2.xy;
            uvSplat[3] = (posOffset + (_SplatUV3.zw + distortion)) / _SplatUV3.xy;
        }

        void UvSplatFront (out float2 uvSplat[4], float worldPos, float offset)
        {
            uvSplat[0] = ObjectFrontUV(worldPos, _SplatUV0, offset);
            uvSplat[1] = ObjectFrontUV(worldPos, _SplatUV1, offset);
            uvSplat[2] = ObjectFrontUV(worldPos, _SplatUV2, offset);
            uvSplat[3] = ObjectFrontUV(worldPos, _SplatUV3, offset);
        }

        void UvSplatSide(out float2 uvSplat[4], float worldPos, float offset)
        {
            uvSplat[0] = ObjectSideUV(worldPos, _SplatUV0, offset);
            uvSplat[1] = ObjectSideUV(worldPos, _SplatUV1, offset);
            uvSplat[2] = ObjectSideUV(worldPos, _SplatUV2, offset);
            uvSplat[3] = ObjectSideUV(worldPos, _SplatUV3, offset);
        }
    #endif

    void DistantUV(out float2 distantUV[4], float2 uvSplat[4])
    {
        for (int i = 0; i < 4; ++i)
        {
            distantUV[i] = uvSplat[i] * _HT_distance_scale;
        }
    }

    #ifndef TERRAIN_BASEGEN
        void SampleSplatTOL(out half4 splat[4], half4 noTriplanarSplat[4], float2 uv, half defaultAlpha, float4 splat_control, half4 mask)
        {
            #ifdef DIFFUSE                
                splat[0] = tex2D(_Splat0, uv);
            #else
                splat[0] = UNITY_SAMPLE_TEX2D(_Splat0, uv);
            #endif 

            #if defined(_TERRAIN_MASK_MAPS) && !defined(_TERRAIN_NORMAL_IN_MASK)
                splat[0].a = mask.a;
            #else
                splat[0].a *= defaultAlpha.r;
            #endif 

            splat[1] = noTriplanarSplat[1];
            splat[2] = noTriplanarSplat[2];
            splat[3] = noTriplanarSplat[3];
        }

        #if defined(TERRAIN_MASK) && !defined(DIFFUSE)
            void SampleMaskTOL(out half4 mask[4], half4 noTriplanarMask[4], float2 uv)
            {
                #ifndef TERRAIN_BASEGEN
                    mask[0] = UNITY_SAMPLE_TEX2D_SAMPLER(_Mask0, _Splat0, uv);
                #else
                    mask[0] = tex2D(_Mask0, uv);
                #endif
                        
                #ifdef _TERRAIN_NORMAL_IN_MASK
                    mask[0].rb = mask[0].rb * _MaskMapRemapScale0.gb + _MaskMapRemapOffset0.gb;
                #else
                    mask[0].rgba = mask[0].rgba * _MaskMapRemapScale0.rgba + _MaskMapRemapOffset0.rgba;
                #endif

                mask[1] = noTriplanarMask[1];
                mask[2] = noTriplanarMask[2];
                mask[3] = noTriplanarMask[3];
            }
        #endif

        #if defined(_NORMALMAP) && !defined(_TERRAIN_NORMAL_IN_MASK) && !defined(TERRAIN_BASEGEN)
            half3 SampleNormalTOL(float2 uv[4], half3 noTriplanarNormal, float4 splat_control, half4 normalScale)
            {
                #ifdef DIFFUSE
                    half3 normal = UnpackNormalWithScale(tex2D(_Normal0, uv[0]), normalScale.x);
                #else
                    half3 normal = UnpackNormalWithScale(UNITY_SAMPLE_TEX2D_SAMPLER(_Normal0, _Splat0, uv[0]), normalScale.x);
                #endif
                
                normal *= splat_control.r;
                noTriplanarNormal *= splat_control.g + splat_control.b + splat_control.a;
                normal += noTriplanarNormal;

                return normal;
            }
        #endif

        #ifdef _TERRAIN_NORMAL_IN_MASK
            half3 MaskNormalTOL(half4 mask[4], half3 noTriplanarNormal, float4 splat_control, half4 normalScale)
            {
                half3 normal;
                normal = UnpackNormalGAWithScale(mask[0], normalScale.x) * splat_control.r;
                normal *= splat_control.r;
                noTriplanarNormal *= splat_control.g + splat_control.b + splat_control.a;
                normal += noTriplanarNormal;
                return  normal;
            }
        #endif
    #endif
#endif


     