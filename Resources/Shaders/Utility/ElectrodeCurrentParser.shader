Shader "sVision/Utility/ElectrodeCurrentParser"{
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

            uniform RWStructuredBuffer<Electrode> ElectrodesBuffer : register(u1);
            
            uint NumberElectrodes;
            uint XResolution; 
            uint YResolution;
            int invert; 
            // float GazeShiftX;
            // float GazeShiftY; 

            uint pixelNumberX( float screenPos)
            {  return ceil(XResolution * screenPos);  }  
            
            uint pixelNumberY( float screenPos)
            { return ceil(YResolution * screenPos); }  
            
            float4 frag (v2f i) : SV_Target { 
                 float _texelw = _MainTex_TexelSize.x;
                
                // int pixelsX = round((.5-GazeShiftX)/_texelw);
                // int pixelsY = round((.5-GazeShiftY)/_texelw); 
                
                float shiftY = 0; //pixelsY * _texelw;
                float shiftX = 0; //pixelsX * _texelw;
                
                float2 newUV;
                newUV.x = i.uv.x - shiftX;
                newUV.y = i.uv.y - shiftY;

                float invVal = invert==1 ? .6 - tex2D(_MainTex, newUV).a : tex2D(_MainTex, newUV).a;
                
                for(int currElec=0; currElec<NumberElectrodes; currElec++)
                {
                    if( pixelNumberX(i.uv.x) == pixelNumberX(ElectrodesBuffer[currElec].x) 
                    && pixelNumberY(i.uv.y) == pixelNumberY(ElectrodesBuffer[currElec].y))
                        ElectrodesBuffer[currElec].current = invVal;

                }
                
                return fixed4(invVal, invVal, invVal, invVal); ;
            }
                
            ENDHLSL
        }
    }
}
