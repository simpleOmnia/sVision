using System;
using System.IO;
using System.Linq;
using svision_internal;
using UnityEngine;
using ShaderHandler = svision_internal.ShaderHandler;

public class svision : MonoBehaviour
{
    public enum EyeDisplay
    { Right, Left, Both };

    public EyeDisplay eyeDisplay = EyeDisplay.Both;
    public int postprocBlurAmount = 21;
    public int preprocBlurAmount = 21;
    public bool runPreprocessing = false; 
    public bool runBionicVision = false;
    public bool invertPreproc = false;
    public SpatialModelType modelType = SpatialModelType.AxonMapModel; 
    
    private Electrode[] electrodes;
    public ComputeElectrode[] c_electrodes;
    
    public ComputeBuffer electrodesBuffer;
    public ComputeBuffer c_electrodesBuffer; 
    public ComputeBuffer electrodeGaussBuffer;
    public ComputeBuffer axonContributionsBuffer; 
    public ComputeBuffer axonIdxStartBuffer;
    
    private float[] dim1Values;
    public ComputeBuffer oneDimImage; 
    
    public int xResolution, yResolution; // xRes/yRes after downscaling original image
    private int buffID, simulatedResX, simulatedResY, minScreenX, minScreenY, maxScreenX, maxScreenY, simulatedScreenX, simulatedScreenY;
    
    // The simulation is only performed for a subset of pixels defined by the user.  
    // These values are used to create a smaller 1-dimensional array that has pre-processing
    // performed on it and subsequently electrode activation are pulled from those values
    public float minimumScreenPositionX = 0;  
    public float minimumScreenPositionY= 0;  
    public float maximumScreenPositionX= 1;  
    public float maximumScreenPositionY= 1;
    public float simulatedScreenSizeX = 1;
    public float simulatedScreenSizeY = 1; 
    public float simulatedResolutionX;
    public float simulatedResolutionY;
    public float electrodeThreshold = 0.01f;
    public float brightnessThreshold = 0.01f; 
    
    private DeviceHandler dh;
    private ShaderHandler sh;

    public int currentFrame; 

    
    public float headsetHorizontalFOV =60f;
    public float headsetVerticalFOV = 60f;

    public bool Initialize()
    {
        xResolution = 500;
        yResolution = 500;
        
        minimumScreenPositionX = 150f / 500f;
        maximumScreenPositionX = 350f / 500f; 
        minimumScreenPositionY = 150f / 500f;
        maximumScreenPositionY = 350f / 500f;
        
        simulatedResolutionX = xResolution * (maximumScreenPositionX - minimumScreenPositionX);
        simulatedResolutionY = yResolution * (maximumScreenPositionY - minimumScreenPositionY);
        
        simulatedScreenSizeX = maximumScreenPositionX - minimumScreenPositionX;
        simulatedScreenSizeY = maximumScreenPositionY - minimumScreenPositionY;
        

        SetComputeElectrodes();
        oneDimImage?.Release();
        int simSize = (int)Math.Ceiling(xResolution * (maximumScreenPositionX - minimumScreenPositionX)) *
                            (int)Math.Ceiling(yResolution * (maximumScreenPositionY - minimumScreenPositionY));
        Debug.Log("Initialized simulation (size="+simSize+" with "+c_electrodes.Length+" electrodes)\n" +
                  "ScreenX: "+minimumScreenPositionX+", "+maximumScreenPositionX+"("+simulatedResolutionX+")\n" +
                  "ScreenY: "+minimumScreenPositionY+", "+maximumScreenPositionY+"("+simulatedResolutionY+")");
        
        oneDimImage = new ComputeBuffer(simSize, 4);
        Graphics.SetRandomWriteTarget(1, oneDimImage); 
        dim1Values = new float[simSize]; 
        
        string path_to_axonFiles = "implantHorFOV36_headsetHorFOV60implantVerFOV36_headsetVerFOV60_downscale2_xRes1000_yRes1000_rho100_lambda150_numAxons1000_numSegments1000_Right";
        AxonSegment[] segmentsContrib = AxonSegmentHandler.ReadAxonSegments(FolderPaths.axonMapModelsPath + path_to_axonFiles + "_axon_contrib.dat");
        float[] axonContributions = new float[segmentsContrib.Length];
        for (int i = 0; i < segmentsContrib.Length; i++)
            axonContributions[i] = segmentsContrib[i].brightnessContribution;
        axonContributionsBuffer?.Release();
        axonContributionsBuffer = new ComputeBuffer(axonContributions.Length, sizeof(float));
        Graphics.SetRandomWriteTarget(2, axonContributionsBuffer, true);
        axonContributionsBuffer.SetData(axonContributions);
        Debug.Log("contrib length: "+axonContributions.Length); 
        
        return true; 
    }
    

    public void SetComputeElectrodes()
    {
        Debug.Log("INIT: "+electrodes.Length);
        if (electrodes.Length > 0)
        {
            c_electrodes = new ComputeElectrode[electrodes.Length];
            for (int i = 0; i < electrodes.Length; i++)
                c_electrodes[i] = new ComputeElectrode(electrodes[i]);
            electrodesBuffer = new ComputeBuffer(electrodes.Length,
                System.Runtime.InteropServices.Marshal.SizeOf(typeof(ComputeElectrode)));
            Graphics.SetRandomWriteTarget(2, electrodesBuffer, true);
            electrodesBuffer.SetData(c_electrodes);
        }
        else
        {
            Debug.LogWarning("sVision - attempted to set compute electrodes with invalid electrodes array. " +
                             "Make sure to set electrodes before calling SetComputeElectrodes()");
        }
    }

    /// <summary>
    /// Gets the electrode currents from 1D greyscale preprocessed values, requires Greyscale1D shader
    /// to be used first (must be called from within ShaderHandler, between Graphics.Blit
    /// and any ComputeShader that uses electrode current values
    /// </summary>
    public void GetElectrodeCurrents() {
        if (c_electrodes?.Any() == true&& oneDimImage!=null)
        {
            string toPrint="";
            oneDimImage.GetData(dim1Values);
            for (int i=0; i<dim1Values.Length; i+=dim1Values.Length/500)
                toPrint+="["+i+": "+dim1Values[i]+"]";
            
            for (int i = 0; i < c_electrodes.Length; i++)
                c_electrodes[i].current = dim1Values[c_electrodes[i].location1D];
            if (currentFrame % 1000 == 0)
            {
                Debug.Log(toPrint);
                foreach (var c_elec in c_electrodes)
                {
                    Debug.Log(c_elec.ToString());
                }
            }
        }
    }

    // public void LoadModelParameters(PrebuiltDemoModel.PrebuiltDemoModels prebuiltDemoModel)
    // {
    //     PrebuiltDemoModel prebuilt = PrebuiltDemoModel.GetModel(prebuiltDemoModel);
    //     string whichEye = eyeDisplay == EyeDisplay.Left ? "_Left" : "_Right"; 
    //     
    //     electrodes = BinaryHandler.ReadElectrodes(prebuilt.ElectrodeModelPath + whichEye + "_electrodes");
    //     UpdateElectrodesToScreenPos();
    //     float[] axonContrib = BinaryHandler.ReadFloatsFromBinaryFile(prebuilt.SpatialModelPath + whichEye + "_axon_contrib.dat").ToArray();
    //     int[] axonIDX = BinaryHandler.ReadFromBinaryFile(prebuilt.SpatialModelPath + whichEye + "_axon_idx_start.dat").ToArray();
    //     
    //     electrodesBuffer?.Release();
    //     electrodesBuffer = new ComputeBuffer(electrodes.Length,  System.Runtime.InteropServices.Marshal.SizeOf(typeof(Electrode)));
    //     Graphics.SetRandomWriteTarget(1, electrodesBuffer, true);
    //     electrodesBuffer.SetData(electrodes);
    //     Debug.Log("elecNum"+electrodes.Length); 
    //     
    //     axonContribBuffer?.Release();
    //     axonContribBuffer = new ComputeBuffer(axonContrib.Length, sizeof(float));
    //     Graphics.SetRandomWriteTarget(3, axonContribBuffer, true);
    //     axonContribBuffer.SetData(axonContrib);
    //     Debug.Log("ax contrib length: " + axonContrib.Length); 
    //     
    //     axonIDX = axonIDX.Append(axonIDX.Length - 1).ToArray(); 
    //     axonIdxStartBuffer?.Release();
    //     axonIdxStartBuffer = new ComputeBuffer(axonIDX.Length, sizeof(int));
    //     Graphics.SetRandomWriteTarget(2, axonIdxStartBuffer, true);
    //     axonIdxStartBuffer.SetData(axonIDX);
    //     Debug.Log(axonIDX.Length);
    //     
    //     Debug.Log(prebuilt.ElectrodeModelPath +whichEye);
    //     float[] electrodeGauss = BinaryHandler.ReadFloatsFromBinaryFile(prebuilt.ElectrodeModelPath +whichEye).ToArray();
    //     
    //     electrodeGaussBuffer?.Release();
    //     electrodeGaussBuffer = new ComputeBuffer(electrodeGauss.Length, sizeof(float));
    //     Graphics.SetRandomWriteTarget(4, electrodeGaussBuffer, true);
    //     electrodeGaussBuffer.SetData(electrodeGauss);
    //     Debug.Log("gauss: " + electrodeGauss.Length);
    //
    //     xResolution = prebuilt.XResolution;
    //     yResolution = prebuilt.YResolution;
    //     coverage = prebuilt.Coverage;
    //     Debug.Log(xResolution+","+yResolution+": "+coverage);
    //     
    //     sh.InitializeReadWriteBuffers();
    // }

    /// <summary>
    /// Used to set the properties for 1D cropped area
    /// </summary>
    /// <param name="shaderName"></param>
    public void UpdateShaderDimensions(ShaderHandler.ShaderName shaderName){

        oneDimImage.SetData(dim1Values); 
        
        ShaderHandler.Instance.ModifyShader(shaderName, buffID, oneDimImage); 
        ShaderHandler.Instance.ModifyShader(shaderName, simulatedResX, simulatedResolutionX); 
        ShaderHandler.Instance.ModifyShader(shaderName, simulatedResY, simulatedResolutionY);
        ShaderHandler.Instance.ModifyShader(shaderName, minScreenX, minimumScreenPositionX); 
        ShaderHandler.Instance.ModifyShader(shaderName, minScreenY, minimumScreenPositionX);
        ShaderHandler.Instance.ModifyShader(shaderName, maxScreenX, maximumScreenPositionX); 
        ShaderHandler.Instance.ModifyShader(shaderName, maxScreenY, maximumScreenPositionY);
        ShaderHandler.Instance.ModifyShader(shaderName, simulatedScreenX, simulatedScreenSizeX);
        ShaderHandler.Instance.ModifyShader(shaderName, simulatedScreenY, simulatedScreenSizeY);
        
    }
    
    public void Update()
    {

        
        // if (sxr.InitialKeyPress(KeyCode.Alpha1)){
        //     sh.SetPreprocessors(ShaderHandler.ShaderName.Greyscale);
        //     runPreprocessing = true; 
        // }
        //
        // if (sxr.InitialKeyPress(KeyCode.Alpha2))
        // {
        //     runBionicVision = true;
        //     if (runBionicVision)
        //         sh.SetModel(ShaderHandler.ShaderName.AxonModel);
        //     else
        //         sh.SetModel(ShaderHandler.ShaderName.None); 
        // }
        
        // if(sxr.InitialKeyPress(KeyCode.Alpha3))
        //     sh.SetPreprocessors(ShaderHandler.ShaderName.EdgeDetection);
        // if (sxr.InitialKeyPress(KeyCode.Alpha4))
        // {
        //     sh.SetPostProcessors(ShaderHandler.ShaderName.PostProcessingHorizontalBlur,
        //         ShaderHandler.ShaderName.PostProcessingVerticalBlur);
        //     sh.SetPostprocessorBlurAmount(postprocBlurAmount);
        // }

        if (sxr.InitialKeyPress(KeyCode.A))
        {
            electrodesBuffer.GetData(electrodes);
            foreach (var elec in electrodes)
                Debug.Log(elec); 
        }
        if (sxr.InitialKeyPress(KeyCode.B))
        {
            Debug.Log(electrodeGaussBuffer.count); 
            float[] arr = new float[electrodeGaussBuffer.count];
            electrodeGaussBuffer.GetData(arr); 
            foreach (var fl in arr)
            {
                if (fl > .05)
                    Debug.Log(fl); 
            }
        }
        if (sxr.InitialKeyPress(KeyCode.C))
        {
            Debug.Log("CCC"); 
            
        }

        currentFrame++; 
    }

    private void OnApplicationQuit()
    {
        electrodesBuffer?.Dispose();
        electrodeGaussBuffer?.Dispose();
        axonContributionsBuffer?.Dispose();
        axonIdxStartBuffer?.Dispose();
    }
    
    private void Start()
    {
        buffID = Shader.PropertyToID("IMAGE1D");
        simulatedResX = Shader.PropertyToID("SimulatedXResolution");
        simulatedResY = Shader.PropertyToID("SimulatedYResolution");
        minScreenX = Shader.PropertyToID("MinimumScreenPositionX");
        minScreenY = Shader.PropertyToID("MinimumScreenPositionY");
        maxScreenX = Shader.PropertyToID("MaximumScreenPositionX");
        maxScreenY = Shader.PropertyToID("MaximumScreenPositionY");
        simulatedScreenX = Shader.PropertyToID("SimulatedScreenSizeX"); 
        simulatedScreenY = Shader.PropertyToID("SimulatedScreenSizeY"); 
         
        
        DeviceHandler dh =  new DeviceHandler();
        dh.LoadDevice("Test");
        electrodes = dh.GetElectrodes();
        Initialize(); 
        // sh = ShaderHandler.Instance;
        //
        // //LoadModelParameters(PrebuiltDemoModel.PrebuiltDemoModels.Scoreboard);
        // DeviceHandler dh =  new DeviceHandler();
        // dh.LoadDevice("Test");
        // electrodes = dh.GetElectrodes();
        // UpdateElectrodesToScreenPos();
        // electrodesBuffer?.Release();
        // electrodesBuffer = new ComputeBuffer(electrodes.Length,  System.Runtime.InteropServices.Marshal.SizeOf(typeof(Electrode)));
        // Graphics.SetRandomWriteTarget(1, electrodesBuffer, true);
        // electrodesBuffer.SetData(electrodes);
        // Debug.Log("elecNum"+electrodes.Length); 
        
        //
        //
        //
        // int[] axonIDX = BinaryHandler.ReadFromBinaryFile(FolderPaths.axonMapModelsPath + "MakeTest10x10_rho100_axon_idx_start.dat").ToArray();
        // axonIDX = axonIDX.Append(axonIDX.Length - 1).ToArray(); 
        // axonIdxStartBuffer?.Release();
        // axonIdxStartBuffer = new ComputeBuffer(axonIDX.Length, sizeof(int));
        // Graphics.SetRandomWriteTarget(3, axonIdxStartBuffer, true);
        // axonIdxStartBuffer.SetData(axonIDX);
        // Debug.Log("axon idx length: "+axonIDX.Length);
        //
        // electrodeGaussBuffer?.Release();
        // float[] electrodeGauss = BinaryHandler
        //     .ReadFloatsFromBinaryFile(FolderPaths.electrodeModelsPath + "Test10x10Gauss").ToArray();
        // electrodeGaussBuffer = new ComputeBuffer(electrodeGauss.Length, sizeof(float)); 
        // Graphics.SetRandomWriteTarget(4, electrodeGaussBuffer, true);
        // electrodeGaussBuffer.SetData(electrodeGauss); 
        // Debug.Log("gauss: " + electrodeGaussBuffer.count);
        //
        // xResolution = 10;
        // yResolution = 10;
        // coverage = 1f; 
        // Debug.Log(xResolution+","+yResolution+": "+coverage);
        //
        // sh.InitializeReadWriteBuffers();
    }

    public static svision Instance;
    private void Awake() {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject.transform.root); }
        else  Destroy(gameObject); }
}
