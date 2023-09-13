Shader "sVision/Percept/AxonModelShader"{
    Properties{ [MainTexture] _MainTex ("Texture", 2D) = "white" {} } 
    
    SubShader{ 
        Cull Off ZWrite Off ZTest Always
       
        Pass{
            HLSLPROGRAM 

            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma target 5.0
            #pragma enable_d3d11_debug_symbols
          
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
            uniform RWStructuredBuffer<int> StartBuffer : register(u3);
            uniform RWStructuredBuffer<float> AxonContributionsBuffer : register(u4); 
            uniform RWStructuredBuffer<float> ElectrodeToNeuronGaussBuffer : register(u5);
            
            uint XResolution;
            uint YResolution;
            uint NumberElectrodes;
            uint RasterOffset; 

            float MinimumScreenPositionX;
            float MinimumScreenPositionY;
            float MaximumScreenPositionX;
            float MaximumScreenPositionY;
            float Coverage; 
            
            float ElectrodeThreshold;
            float BrightnessThreshold; 

            uint pixelNumberX( float screenPos)
            {  return ceil(XResolution * screenPos);  }  
            
            uint pixelNumberY( float screenPos)
            { return ceil(YResolution * screenPos); }  
            
            float4 frag (v2f i) : SV_Target {
                if(!(i.uv.x >= MinimumScreenPositionX) || !(i.uv.x <= MaximumScreenPositionX) ||
                    !(i.uv.y >= MinimumScreenPositionY) || !(i.uv.y <= MaximumScreenPositionY) ) {
                     tex2D(_MainTex, i.uv);  
                 }   
                
                // Get current pixel being shaded's one dimensional number
                const uint xPixel = pixelNumberX(i.uv.x - MinimumScreenPositionX)-1;
                const uint yPixel = pixelNumberY(i.uv.y - MinimumScreenPositionY)-1;

                const uint loc1D = xPixel + (int(XResolution * Coverage)+1) * yPixel;  // add +1 for pixel at each end of simulation (ie. -20 deg and 20 deg)return float4(i.uv.x, i.uv.y, unmodified_current_pixel.b, 1.0); } // outputs red and green values based on screen position, keeping the original textures blue value

                float brightestNeuron = 0.0f;
float maxGauss=0.0f; 
                // cycle through neurons affecting current pixel
                for (uint m = StartBuffer[loc1D]; m<StartBuffer[loc1D+1]; m++) {
                    float gauss = 0.0f;
                    
                    float curr_neuron_brightness = 0.0f;

                    for(uint j=0; j<NumberElectrodes; j++)
                        if(ElectrodesBuffer[j].current > ElectrodeThreshold){
                            int currPos = (m*NumberElectrodes) + ElectrodesBuffer[j].electrodeNumber;
                            gauss = ElectrodeToNeuronGaussBuffer[currPos];
                            if (gauss>maxGauss)
                                maxGauss = gauss; 
                            //float contBuffCurr = ContributionsBuffer[m];
                            float curr = ElectrodesBuffer[j].current; 
                            curr_neuron_brightness = curr_neuron_brightness + gauss * 1000.0 * curr * 1;//contBuffCurr; 
                        }

                    brightestNeuron = curr_neuron_brightness > brightestNeuron
                        ? curr_neuron_brightness : brightestNeuron; 
                }
                brightestNeuron = brightestNeuron > BrightnessThreshold ? brightestNeuron : 0;

                return fixed4(maxGauss, maxGauss, maxGauss, 1.0); 
            }
                
            ENDHLSL
        }
    }
}
