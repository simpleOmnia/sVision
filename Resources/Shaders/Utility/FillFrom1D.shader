Shader "sVision/FillFrom1D" {
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

            RWStructuredBuffer<float>  IMAGE1D : register(u1);
            
            float SimulatedXResolution;
            float SimulatedYResolution;
            float MinimumScreenPositionX;
            float MinimumScreenPositionY;
            float MaximumScreenPositionX;
            float MaximumScreenPositionY;
            float SimulatedScreenSizeX;
            float SimulatedScreenSizeY; 

            uint pixelNumberX( float screenPos)
            {  return ceil(SimulatedXResolution * screenPos);  }  
            
            uint pixelNumberY( float screenPos)
            { return ceil(SimulatedYResolution * screenPos); }
            
            float4 frag (v2f i) : SV_Target{
                const float currScreenX = (i.uv.x - MinimumScreenPositionX)/SimulatedScreenSizeX;
                const float currScreenY = (i.uv.y - MinimumScreenPositionY)/SimulatedScreenSizeY;
                
                if (currScreenX < 0 || currScreenY< 0 || i.uv.x >MaximumScreenPositionX || i.uv.y >MaximumScreenPositionY)
                    return tex2D(_MainTex, i.uv);
                
                const uint xPixel = pixelNumberX(currScreenX);
                const uint yPixel = pixelNumberY(currScreenY);

                const uint loc1D = xPixel + (SimulatedXResolution) * (yPixel) ;
                
                float lumValue = IMAGE1D[loc1D];
                
                return float4(lumValue, lumValue, lumValue, lumValue);
              
            }
            
            ENDHLSL
        } 
    }
}