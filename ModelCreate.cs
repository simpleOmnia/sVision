using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using svision_internal;
using ShaderHandler = svision_internal.ShaderHandler;

public class ModelCreate : MonoBehaviour
{
    private ComputeBuffer oneDimImage;
    private int buffID, xResID, yResID, minXID, minYID, maxXID, maxYID, screenCoverID;
    private float[] dim1Values;
    private int currentFrame = 0; 
   
    void Start()
    {
       sxr_internal.ShaderHandler.Instance.SetDownscaleFactor(2);

        oneDimImage = new ComputeBuffer((100 * 100), 4);
        Graphics.SetRandomWriteTarget(1, oneDimImage, true); 
        //ShaderHandler.Instance.SetPreprocessors(ShaderHandler.ShaderName.Greyscale1D);

        dim1Values = new float[100 * 100]; 

        buffID = Shader.PropertyToID("IMAGE1D");
        xResID = Shader.PropertyToID("XResolution");
        yResID = Shader.PropertyToID("YResolution");
        minXID = Shader.PropertyToID("MinimumPixelPositionX");
        minYID = Shader.PropertyToID("MinimumPixelPositionY");
        maxXID = Shader.PropertyToID("MaximumPixelPositionX");
        maxYID = Shader.PropertyToID("MaximumPixelPositionY");
        screenCoverID = Shader.PropertyToID("ScreenCoverageRatio"); 
    }
   
    // Update is called once per frame
    void Update()
    {
        ShaderHandler.Instance.ModifyShader(ShaderHandler.ShaderName.Greyscale1D, buffID, oneDimImage); 
        ShaderHandler.Instance.ModifyShader(ShaderHandler.ShaderName.Greyscale1D, xResID, 500.0f); 
        ShaderHandler.Instance.ModifyShader(ShaderHandler.ShaderName.Greyscale1D, yResID, 500.0f); 
        ShaderHandler.Instance.ModifyShader(ShaderHandler.ShaderName.Greyscale1D, minXID, 150f); 
        ShaderHandler.Instance.ModifyShader(ShaderHandler.ShaderName.Greyscale1D, minYID, 150f);
        ShaderHandler.Instance.ModifyShader(ShaderHandler.ShaderName.Greyscale1D, maxXID, 350f); 
        ShaderHandler.Instance.ModifyShader(ShaderHandler.ShaderName.Greyscale1D, maxYID, 350f);
        ShaderHandler.Instance.ModifyShader(ShaderHandler.ShaderName.Greyscale1D, screenCoverID, .50f);

        if (currentFrame % 1000 ==0)
            for (int i = 0; i < dim1Values.Length; i+=dim1Values.Length/100)
                Debug.Log(dim1Values[i]); 


        currentFrame++; 
    }
}
