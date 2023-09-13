using System.IO;
using svision_internal;
using UnityEngine;
using UnityEditor;

public class CreateAxonMapModel : EditorWindow 
{
    
    
    private RectOffset rctOffButton, rctOffTextField, rctOffToggle, rctOffSlider;

    private GUIStyle myStyle;

    private AxonMapModel axonModel = new AxonMapModel(); 

    private bool runCalculation = false; 

    [MenuItem("sVision/AxonMapModel")]
    static void Init() {
        CreateAxonMapModel window = (CreateAxonMapModel)EditorWindow.GetWindow(typeof(CreateAxonMapModel));
        window.minSize = new Vector2(500, 800); 
        window.Show(); }

    
    
    void OnGUI()
    {
        rctOffButton = GUI.skin.button.margin;
        rctOffButton.left = 25;
        rctOffButton.right = 25;

        rctOffTextField = GUI.skin.textField.margin;
        rctOffTextField.left = 25;
        rctOffTextField.right = 25;
        
        rctOffToggle = GUI.skin.toggle.margin;
        rctOffToggle.left = 10;

        rctOffSlider = GUI.skin.horizontalSlider.margin;
        rctOffSlider.left = 25;
        rctOffSlider.right = 25;

        rctOffToggle = GUI.skin.toggle.margin;

        myStyle = new GUIStyle(GUI.skin.label) {fontSize = 25};
        
        GUILayout.Label("Settings for AxonMap\n", myStyle);

        EditorGUI.BeginChangeCheck();
        
        GUILayout.Label(new GUIContent("Model name: ", "name to use with the current model"));
        axonModel.saveName = GUILayout.TextField(axonModel.saveName); 
        
        GUILayout.Space(15);
        GUILayout.Label(new GUIContent("Downscale Factor[" + $"{axonModel.downscaleFactor:0}" + "]: ",
            "Sets the downscaling factor to be used in computations"));
        int downscaleFactorInt = Mathf.RoundToInt(axonModel.downscaleFactor);
        downscaleFactorInt = Mathf.RoundToInt(GUILayout.HorizontalSlider(downscaleFactorInt, 1, 11));
        axonModel.downscaleFactor = downscaleFactorInt;
        
        GUILayout.Space(15);
        GUILayout.Label(new GUIContent("X-resolution[" + $"{axonModel.xRes:0}" + "]: ",
            "Sets the pre-downscaling x-resolution to be used in computations"));
        int xResInt = Mathf.RoundToInt(axonModel.xRes);
        xResInt = Mathf.RoundToInt(GUILayout.HorizontalSlider(xResInt, 100, 3000));
        axonModel.xRes = xResInt;
        GUILayout.Space(15);
        GUILayout.Label(new GUIContent("Y-resolution[" + $"{axonModel.yRes:0}" + "]: ",
            "Sets the pre-downscaling y-resolution to be used in computations"));
        int yResInt = Mathf.RoundToInt(axonModel.yRes);
        yResInt = Mathf.RoundToInt(GUILayout.HorizontalSlider(yResInt, 100, 3000));
        axonModel.yRes = yResInt;
        GUILayout.Space(15);
        
        GUILayout.Space(15);
        GUILayout.Label(new GUIContent("Headset Horizontal field of view [" + $"{axonModel.headsetFOV_Horizontal:0.00}" + "]: ",
            "Full horizontal field of view being simulated"));
        float fovHorRound = Mathf.Round(axonModel.headsetFOV_Horizontal * 10f) / 10f; 
        fovHorRound = GUILayout.HorizontalSlider(fovHorRound, -100f, 100f);
        axonModel.headsetFOV_Horizontal = fovHorRound;
        GUILayout.Space(15);
        
        GUILayout.Space(15);
        GUILayout.Label(new GUIContent("Headset Vertical field of view [" + $"{axonModel.headsetFOV_Vertical:0.00}" + "]: ",
            "Full vertical field of view being simulated"));
        float fovVerRound = Mathf.Round(axonModel.headsetFOV_Vertical * 10f) / 10f; 
        fovVerRound = GUILayout.HorizontalSlider(fovVerRound, -100f, 100f);
        axonModel.headsetFOV_Vertical = fovVerRound;
        GUILayout.Space(15);
        
        GUILayout.Space(15);
        GUILayout.Label(new GUIContent("X-Min [" + $"{axonModel.xMin:0.00}" + "]: ",
            "Sets the lowest x bound in degrees visual angle"));
        float roundedXMin = Mathf.Round(axonModel.xMin * 10f) / 10f; 
        roundedXMin = GUILayout.HorizontalSlider(roundedXMin, -100f, 100f);
        axonModel.xMin = roundedXMin;
        GUILayout.Space(15);
        GUILayout.Label(new GUIContent("X-Max [" + $"{axonModel.xMax:0.00}" + "]: ",
            "Sets the highest x bound in degrees visual angle"));
        float roundedXMax = Mathf.Round(axonModel.xMax * 10f) / 10f; 
        roundedXMax = GUILayout.HorizontalSlider(roundedXMax, -100f, 100f);
        axonModel.xMax = roundedXMax;
        
        GUILayout.Space(15);
        GUILayout.Label(new GUIContent("Y-Min [" + $"{axonModel.yMin:0.00}" + "]: ",
            "Sets the lowest y bound in degrees visual angle"));
        float roundedYMin = Mathf.Round(axonModel.yMin * 10f) / 10f; 
        roundedYMin = GUILayout.HorizontalSlider(roundedYMin, -100f, 100f);
        axonModel.yMin = roundedYMin;
        GUILayout.Space(15);
        GUILayout.Label(new GUIContent("Y-Max [" + $"{axonModel.yMax:0.00}" + "]: ",
            "Sets the highest y bound in degrees visual angle"));
        float roundedYMax = Mathf.Round(axonModel.yMax * 10f) / 10f; 
        roundedYMax = GUILayout.HorizontalSlider(roundedYMax, -100f, 100f);
        axonModel.yMax = roundedYMax;
        
        GUILayout.Space(15);
        GUILayout.Label(new GUIContent("rho [" + $"{axonModel.rho:0}" + "]: ",
            "Sets the rho value (um)"));
        int rhoInt = Mathf.RoundToInt(axonModel.rho); 
        rhoInt = Mathf.RoundToInt(GUILayout.HorizontalSlider(rhoInt, 0, 1000f));
        axonModel.rho = rhoInt;
        
        
        GUILayout.Space(15);
        GUILayout.Label(new GUIContent("lambda [" + $"{axonModel.lambda:0.00}" + "]: ",
            "Sets lambda value (um)"));
        int lambdaInt = Mathf.RoundToInt(axonModel.lambda); 
        lambdaInt= Mathf.RoundToInt(GUILayout.HorizontalSlider(lambdaInt, 0f, 3000f));
        axonModel.lambda = lambdaInt;
        
        GUILayout.Space(15);
        GUILayout.Label(new GUIContent("Axon threshold [" + $"{axonModel.axon_threshold:0.00}" + "]: ",
            "Sets axon threshold value"));
        float roundedThresh = Mathf.Round(axonModel.axon_threshold * 10f) / 10f; // 
        roundedThresh= GUILayout.HorizontalSlider(roundedThresh, 0f, 3000f);
        axonModel.axon_threshold = roundedThresh;
        
        GUILayout.Space(15);
        axonModel.useLeftEye = GUILayout.Toggle(axonModel.useLeftEye,
            new GUIContent("   Use Left Eye",
                "Click to calculate values for the left eye."));
        
        GUILayout.Space(15);
        GUILayout.Label(new GUIContent("Number of axons[" + $"{axonModel.number_axons:0}" + "]: ",
            "Sets the number of simulated axons"));
        int axonInt = Mathf.RoundToInt(axonModel.number_axons);
        axonInt = Mathf.RoundToInt(GUILayout.HorizontalSlider(axonInt, 100, 3000));
        axonModel.number_axons = axonInt; 
        GUILayout.Space(15);
        GUILayout.Label(new GUIContent("Number of axon segments[" + $"{axonModel.number_axon_segments:0}" + "]: ",
            "Sets the number of segments to be simulated (per axon)"));
        int segmentsInt = Mathf.RoundToInt(axonModel.number_axon_segments);
        segmentsInt = Mathf.RoundToInt(GUILayout.HorizontalSlider(segmentsInt, 100, 3000));
        axonModel.number_axon_segments = segmentsInt;
        GUILayout.Space(15);
        
        GUILayout.Space(50);
        runCalculation = GUILayout.Toggle(runCalculation,
            new GUIContent("   Build axon map",
                "Click to calculate and store the axon map with the given settings."));

        if (EditorGUI.EndChangeCheck())
        {
            if (runCalculation)
            {
                axonModel.SetSimulationBounds();
                axonModel.BuildModel();
                runCalculation = false;
            }
        }

    }

    void OnEnable()
    {
        axonModel.savedSettingsPath = FolderPaths.axonMapModelsPath; 
        axonModel.headsetFOV_Horizontal = 60;
        axonModel.headsetFOV_Vertical = 60; 
        axonModel.downscaleFactor = 1;
        axonModel.xMin = -30;
        axonModel.xMax = 30;
        axonModel.yMin = -30;
        axonModel.yMax = 30;
        axonModel.rho = 150;
        axonModel.lambda = 1000;
        axonModel.xRes = 1080;
        axonModel.yRes = 1200;
        axonModel.axon_threshold = .1f;
        axonModel.number_axon_segments = 1000;
        axonModel.number_axons = 1000; 

    }
    void OnDisable() {new ObjectPreview().Cleanup(); }

}
