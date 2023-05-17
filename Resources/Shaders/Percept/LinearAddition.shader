Shader "sVision/Percept/LinearAdditionShader"{
    Properties{ [MainTexture] _MainTex ("Texture", 2D) = "white" {} } 
    
    SubShader{ 
        Cull Off ZWrite Off ZTest Always
       
        Pass{
            HLSLPROGRAM 

            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma target 5.0
          
            #if SXR_USE_URP
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
                o.vertex = TransformObjectToHClip(v.vertex); 
                #else
                o.vertex = UnityObjectToClipPos(v.vertex); 
                #endif
                
                o.uv = v.uv; 
                return o; }

            sampler2D _MainTex; 

            float4 _MainTex_TexelSize;

            struct Electrode
            {
                int electrodeNumber;
                float x;
                float y;
                float z;
                float current; 
            };

            uniform RWStructuredBuffer<Electrode> ElectrodesBuffer : register(u2);
            
            StructuredBuffer<float> ContributionsBuffer; 
            
            uint XResolution;
            uint YResolution;
            uint NumberElectrodes;
            uint RasterOffset; 

            float ElectrodeThreshold;
            float BrightnessThreshold; 

            uint pixelNumberX( float screenPos)
            {  return ceil(XResolution * screenPos);  }  
            
            uint pixelNumberY( float screenPos)
            { return ceil(YResolution * screenPos); }  
            
            float4 frag (v2f i) : SV_Target { 
                //float4 unmodified_current_pixel = tex2D(_MainTex, i.uv);
                
                // Get current pixel being shaded's one dimensional number
                uint xPixel = pixelNumberX(i.uv.x)-1;
                uint yPixel = pixelNumberY(i.uv.y)-1;
                
                uint loc1D = xPixel + (int(XResolution)+1) * yPixel;  // add +1 for pixel at each end of simulation (ie. -20 deg and 20 deg)return float4(i.uv.x, i.uv.y, unmodified_current_pixel.b, 1.0); } // outputs red and green values based on screen position, keeping the original textures blue value

                float brightness = 0; 
                
                for(uint e=0; e < NumberElectrodes; e++)
                   brightness += ElectrodesBuffer[e].current * ContributionsBuffer[(loc1D*NumberElectrodes) + ElectrodesBuffer[e].electrodeNumber]; 
                
                
                return fixed4(brightness, brightness, brightness, 1); 
            }
                
            ENDHLSL
        }
    }
}
