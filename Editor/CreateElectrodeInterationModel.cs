using System;
using System.IO;
using svision_internal;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;


public class CreateElectrodeInteractionModel : EditorWindow 
{
    
    
    private RectOffset rctOffButton, rctOffTextField, rctOffToggle, rctOffSlider;

    private GUIStyle myStyle;
    
    private string electrodeInteractionModelName; 
    
    private DeviceHandler.PrebuiltImplant device;

    private SpatialModelType modelType;

    private string spatialModelName; 
    
    private string implantCSV; 

    private int latticeCountX, latticeCountY, latticeCountZ, latticeSpacingX, latticeSpacingY, latticeSpacingZ;
    
    private Vector3 arrayPosition, arrayRotation;

    private bool usePrebuilt;

    private bool runCalculation = false; 

    [MenuItem("sVision/ElectrodeInteractionModel")]
    static void Init() {
        CreateElectrodeInteractionModel window = (CreateElectrodeInteractionModel)EditorWindow.GetWindow(typeof(CreateElectrodeInteractionModel));
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
        
        GUILayout.Label("Settings for Electrode Model\n", myStyle);

        EditorGUI.BeginChangeCheck();
        
        GUILayout.Label(new GUIContent("Model name: ", "Name to save the electrode interaction model under Resources/PreComputedModels/ElectrodeInteractionModels/"));
        electrodeInteractionModelName = GUILayout.TextField(electrodeInteractionModelName); 
        
        GUILayout.Space(25);
        GUILayout.Label(new GUIContent("X Position[" + $"{arrayPosition.x:0.0}" + "]: ", "Sets the X position"));
        arrayPosition.x = Mathf.Round(GUILayout.HorizontalSlider(arrayPosition.x, -1000, 1000) * 10f) / 10f;

        GUILayout.Space(15);
        GUILayout.Label(new GUIContent("Y Position[" + $"{arrayPosition.y:0.0}" + "]: ", "Sets the Y position"));
        arrayPosition.y = Mathf.Round(GUILayout.HorizontalSlider(arrayPosition.y, -1000, 1000) * 10f) / 10f;

        GUILayout.Space(15);
        GUILayout.Label(new GUIContent("Z Position[" + $"{arrayPosition.z:0.0}" + "]: ", "Sets the Z position"));
        arrayPosition.z = Mathf.Round(GUILayout.HorizontalSlider(arrayPosition.z, -1000, 1000) * 10f) / 10f;

        GUILayout.Space(15);

        GUILayout.Label(new GUIContent("X Rotation[" + $"{arrayRotation.x:0.0}" + "]: ", "Sets the X rotation"));
        arrayRotation.x = Mathf.Round(GUILayout.HorizontalSlider(arrayRotation.x, -180, 180) * 10f) / 10f;

        GUILayout.Space(15);
        GUILayout.Label(new GUIContent("Y Rotation[" + $"{arrayRotation.y:0.0}" + "]: ", "Sets the Y rotation"));
        arrayRotation.y = Mathf.Round(GUILayout.HorizontalSlider(arrayRotation.y, -180, 180) * 10f) / 10f;

        GUILayout.Space(15);
        GUILayout.Label(new GUIContent("Z Rotation[" + $"{arrayRotation.z:0.0}" + "]: ", "Sets the Z rotation"));
        arrayRotation.z = Mathf.Round(GUILayout.HorizontalSlider(arrayRotation.z, -180, 180) * 10f) / 10f;

        GUILayout.Space(25);
        usePrebuilt = GUILayout.Toggle(usePrebuilt,
            new GUIContent("   Use Prebuilt Implant",
                "Click to use a prebuilt implant."));

        if (usePrebuilt)
        {
            GUILayout.Space(15);
            device = (DeviceHandler.PrebuiltImplant) EditorGUILayout.EnumPopup("Implant:", device);
            if (device == DeviceHandler.PrebuiltImplant.Lattice)
            {
                GUILayout.Space(15);
                GUILayout.Label(new GUIContent("X-electrode count[" + $"{latticeCountX:0}" + "]: ",
                    "Sets the number of electrode columns in the lattice"));
                int xCountLatInt = Mathf.RoundToInt(latticeCountX);
                xCountLatInt = Mathf.RoundToInt(GUILayout.HorizontalSlider(xCountLatInt, 1, 100));
                latticeCountX = xCountLatInt; 
                
                GUILayout.Space(15);
                GUILayout.Label(new GUIContent("Y-electrode count[" + $"{latticeCountY:0}" + "]: ",
                    "Sets the number of electrode rows in the lattice"));
                int yCountLatInt = Mathf.RoundToInt(latticeCountY);
                yCountLatInt = Mathf.RoundToInt(GUILayout.HorizontalSlider(yCountLatInt, 1, 100));
                latticeCountY = yCountLatInt;
                
                GUILayout.Space(15);
                GUILayout.Label(new GUIContent("Z-electrode count[" + $"{latticeCountZ:0}" + "]: ",
                    "Sets the number of electrode layers in the lattice"));
                int zLatInt = Mathf.RoundToInt(latticeCountZ);
                zLatInt = Mathf.RoundToInt(GUILayout.HorizontalSlider(zLatInt, 1, 100));
                latticeCountZ = zLatInt;
                
                
                GUILayout.Space(15);
                GUILayout.Label(new GUIContent("X-electrode spacing (micrometers)[" + $"{latticeSpacingX:0}" + "]: ",
                    "Sets the spacing between electrode columns in the lattice"));
                int xSpacingInt = Mathf.RoundToInt(latticeSpacingX);
                xSpacingInt = Mathf.RoundToInt(GUILayout.HorizontalSlider(xSpacingInt, 1, 1000));
                latticeSpacingX = xSpacingInt;

                GUILayout.Space(15);
                GUILayout.Label(new GUIContent("Y-electrode spacing (micrometers)[" + $"{latticeSpacingY:0}" + "]: ",
                    "Sets the spacing between electrode rows in the lattice"));
                int ySpacingInt = Mathf.RoundToInt(latticeSpacingY);
                ySpacingInt = Mathf.RoundToInt(GUILayout.HorizontalSlider(ySpacingInt, 1, 1000));
                latticeSpacingY = ySpacingInt;

                GUILayout.Space(15);
                GUILayout.Label(new GUIContent("Z-electrode spacing (micrometers)[" + $"{latticeSpacingZ:0}" + "]: ",
                    "Sets the spacing between electrode layers in the lattice"));
                int zSpacingInt = Mathf.RoundToInt(latticeSpacingZ);
                zSpacingInt = Mathf.RoundToInt(GUILayout.HorizontalSlider(zSpacingInt, 1, 1000));
                latticeSpacingY = zSpacingInt;
            }
        }
        else
        {
            GUILayout.Space(15);
            GUILayout.Label(new GUIContent("Implant CSV Location: ", "Location of csv file containing electrode coordinates"));
            implantCSV = GUILayout.TextField(implantCSV); 
        }

        GUILayout.Space(25);
        GUILayout.Label(new GUIContent("Spatial Model name (leave blank to use last ran spatial model): ", "Name of pre-calculated model to use, should be in Resources/PreComputedModels"));
        spatialModelName = GUILayout.TextField(spatialModelName); 
        
        modelType = (SpatialModelType) EditorGUILayout.EnumPopup("Spatial model type:", modelType);
        
        GUILayout.Space(50);
        runCalculation = GUILayout.Toggle(runCalculation,
            new GUIContent("   Calculate electrode gaussian",
                "Click to calculate and store electrode gaussian info."));

        if (EditorGUI.EndChangeCheck())
        {
            if (runCalculation)
            {
                implantCSV = usePrebuilt ?  device.ToString()+".csv"  
                    : implantCSV+".csv";
                
                DeviceHandler dh = new DeviceHandler();

                if (device == DeviceHandler.PrebuiltImplant.Lattice) {
                    latticeSpacingZ = latticeCountZ == 1 ? 0 : latticeSpacingZ; 
                    dh.CreateLattice(latticeCountX, latticeCountY, latticeCountZ, latticeSpacingX, latticeSpacingY,
                        latticeSpacingZ);
                }
                else
                    dh.LoadDevice(implantCSV);
                
                dh.MoveAndRotateElectrodeArray(arrayPosition.x, arrayPosition.y, arrayPosition.z,
                    arrayRotation.x, arrayRotation.y, arrayRotation.z);

                SpatialModelHandler smh = new SpatialModelHandler(dh.GetElectrodes());

                spatialModelName = string.IsNullOrEmpty(spatialModelName)
                    ? new sxr_internal.FileHandler().ReadLastLine(
                        FolderPaths.modelsPath + modelType + "s" + Path.DirectorySeparatorChar + "lastModel.txt") 
                    : spatialModelName;
                Debug.Log("sVision - Using last spatial model: " + spatialModelName); 
                string spatialModelPath =
                    FolderPaths.modelsPath + modelType + "s" + Path.DirectorySeparatorChar + spatialModelName + 
                    (spatialModelName.Contains("_axon_contrib.dat") ? "" : "_axon_contrib.dat");
                string electrodeModelPath =
                    FolderPaths.electrodeModelsPath + electrodeInteractionModelName;
                BinaryHandler.WriteFloatArray(electrodeModelPath, smh.GetElectrodeToAxonSegmentGauss(spatialModelPath));
                BinaryHandler.WriteElectrodeLocations(electrodeModelPath+"_electrodes", dh.GetElectrodes());
                
                runCalculation = false; 
            }
        }

    }

    void OnEnable()
    {
        arrayPosition = new Vector3();
        arrayRotation = new Vector3();
        latticeCountX = 10;
        latticeCountY = 10;
        latticeCountZ = 10;
        latticeSpacingX = 100;
        latticeSpacingY = 100;
        latticeSpacingZ = 100; 

    }
    void OnDisable() {new ObjectPreview().Cleanup(); }

}
