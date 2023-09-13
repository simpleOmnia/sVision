Shader "sVision/CropFOV" {
    Properties{ _MainTex ("Texture", 2D) = "white" {} }
    
    SubShader{
        Pass{
            HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest
#pragma target 5.0
#pragma enable_d3d11_debug_symbols
	    	
#if SXR_USE_URP
            // Must include Unity's URP ShaderLibrary 
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#else
#include "UnityCG.cginc"
#endif

            struct appdata{ 
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;};

            struct v2f { 
                float4 vertex : SV_POSITION; 
                float2 uv : TEXCOORD0; };

            v2f vert (appdata v){ 
                v2f o;

#if SXR_USE_URP
                o.vertex = TransformObjectToHClip(v.vertex); // Clips objects based on ZTest property, converts to homogenous coordinates
#else
                o.vertex = UnityObjectToClipPos(v.vertex); 
#endif
            	
                o.uv = v.uv; 
                return o; }

            sampler2D _MainTex;

            RWBuffer<float>  IMAGE1D : register(u1);
            float XResolution;
            float YResolution;
            float MinimumPixelPositionX;
            float MinimumPixelPositionY;
            float MaximumPixelPositionX;
            float MaximumPixelPositionY;
            float ScreenCoverageRatio; 

            uint pixelNumberX( float screenPos)
            {  return ceil(XResolution * screenPos);  }  
            
            uint pixelNumberY( float screenPos)
            { return ceil(YResolution * screenPos); }
            
            float4 frag (v2f i) : SV_Target{
                float currScreenX = i.uv.x - MinimumPixelPositionX/XResolution;
                float currScreenY = i.uv.y - MinimumPixelPositionY/YResolution;
                float maxX = MaximumPixelPositionX/XResolution;
                float maxY = MaximumPixelPositionY/YResolution;
                if (currScreenX < 0 || currScreenY< 0 || i.uv.x >maxX || i.uv.y >maxY)
                    return tex2D(_MainTex, i.uv);
                
                const uint xPixel = pixelNumberX(currScreenX)-1;
                const uint yPixel = pixelNumberY(currScreenY)-1;

                const uint loc1D = xPixel + (int(XResolution * ScreenCoverageRatio)+1) * yPixel;
                
                float3 colors = tex2D(_MainTex, i.uv).rgb;
                float lumValue = colors.r*.3 + colors.g*.59 + colors.b*.11;

                IMAGE1D[loc1D] = lumValue;
                
                return float4(lumValue, lumValue, lumValue, lumValue);
              
            }
            
            ENDHLSL
        } 
    }
}