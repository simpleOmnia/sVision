using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace svision_internal
{
    /// <summary>
    /// This must be used in place of sXR ShaderHandler
    /// </summary>
    public class ShaderHandler : MonoBehaviour
    {
        private svision sv;
        private ShaderName[] _preprocessors;
        private ShaderName _model = ShaderName.None;
        private ShaderName[] _postprocessors;
        private bool invert;

        private Material[] shaderMaterials;

        [Header("Update either List, then click \"EditorUpdate\". Click \"ListShaders\" to output a list to the console")] 
        [SerializeField] private List<int> activePositions;
        [SerializeField] private List<string> activeNames; 
        [SerializeField] private bool editorUpdate;
        [SerializeField] private bool listShaders;

        [Header("List the currently active shaders (cannot edit:")]
        
        public List<int> currentActivePositions = new List<int>(); 
        public List<string> currentActiveNames = new List<string>();

        private int downscaleFactor = 1;
        private int Greyscale1D_ShaderNum, FillFrom1D_ShaderNum;

        public enum ShaderName {
            ElectrodeParser,
            EdgeDetection,
            Greyscale,
            Greyscale1D,
            FillFrom1D,
            HorizontalBlur,
            VerticalBlur,
            GazeShift,
            AxonModel,
            LinearModel,
            PostProcessingHorizontalBlur,
            PostProcessingVerticalBlur,
            TemporalModel,
            None
        }
        
        public void SetDownscaleFactor(int downscaleFactor)
        {this.downscaleFactor = downscaleFactor;}
        
        private static readonly Dictionary<ShaderName, string> shaderNameMap = new Dictionary<ShaderName, string> {
            { ShaderName.ElectrodeParser, "sVision/Utility/ElectrodeCurrentParser" },
            { ShaderName.EdgeDetection, "sXR/EdgeDetect" },
            { ShaderName.Greyscale, "sXR/Greyscale" },
            { ShaderName.Greyscale1D, "sVision/Greyscale1D" },
            { ShaderName.HorizontalBlur, "sXR/HorizontalBlur" },
            { ShaderName.VerticalBlur, "sXR/VerticalBlur" },
            { ShaderName.AxonModel, "sVision/Percept/AxonModelShader" },
            { ShaderName.LinearModel, "sVision/Percept/LinearAdditionShader" },
            { ShaderName.GazeShift, "sXR/Shift" },
            { ShaderName.PostProcessingHorizontalBlur, "sVision/PostProcessing/PostProcessHorizontalBlur" },
            { ShaderName.PostProcessingVerticalBlur, "sVision/PostProcessing/PostProcessHorizontalBlur" },
            { ShaderName.TemporalModel, "sVision/PostProcessing/TemporalModel" },
            { ShaderName.None, "sVision/PostProcessing/TemporalModel" }, 
            { ShaderName.FillFrom1D, "sVision/FillFrom1D"}
        };
        Dictionary<ShaderName, List<int>> shaderVariables = new Dictionary<ShaderName, List<int>>();

        public Material[] GetShaderMaterials()
        { return shaderMaterials; }
        
        public void Update() {
            if (editorUpdate) {
                if(!activePositions.SequenceEqual(currentActivePositions))
                    ActivateShaders(activePositions);
                else if (!activeNames.SequenceEqual(currentActiveNames))
                    ActivateShaders(activeNames); 
                editorUpdate = false; }

            if (listShaders) {
                ListShaders();
                listShaders = false; } }

        RenderTexture temp, processed;

        public void ActivateShaders(List<int> activePositions)
        { this.activePositions = activePositions; }
        public void ActivateShaders(List<string> shaderNames) {
            Debug.Log("Activate shaders by name: " + shaderNames.ToArray().ToCommaSeparatedString()); 
            activePositions.Clear();

            foreach (string name in shaderNames) {
                
                for(int i=0; i<shaderMaterials.Length; i++) {
                    if (shaderMaterials[i].name.ToLower().Equals(name.ToLower())) {
                        activePositions.Add(i);
                        break; } }
                Debug.LogWarning("Attempted to activate shader #" + name + ", but shader does not exist"); }

            ActivateShaders(activePositions); }

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            RenderTexture.ReleaseTemporary(processed);
            processed = RenderTexture.GetTemporary(src.width/downscaleFactor, src.height/downscaleFactor, 0 );
            Graphics.Blit(src, processed); // Render to downscaled texture
            
            GetGreyscale1D();
            sv.GetElectrodeCurrents();
            
            FillWith1Dim();
            
            Graphics.Blit(processed, dest); // Render back to fullscale texture
        }

        
        private void GetGreyscale1D()
        {
            //todo add gaze shift
            sv.UpdateShaderDimensions(ShaderName.Greyscale1D);
            temp = processed;
            processed = RenderTexture.GetTemporary(processed.width, processed.height, 0 );
            sv.UpdateShaderDimensions(ShaderName.Greyscale1D);
            Graphics.Blit(temp, processed, shaderMaterials[Greyscale1D_ShaderNum]);
            RenderTexture.ReleaseTemporary(temp);
            
        }

        private void CallAxonMapKernel()
        {
            int numberElectrodes = sv.c_electrodes.Length;
            
        }

        private void FillWith1Dim()
        {
            temp = processed; 
            processed = RenderTexture.GetTemporary(processed.width, processed.height, 0 );
            sv.UpdateShaderDimensions(ShaderName.FillFrom1D);
            Graphics.Blit(temp, processed, shaderMaterials[FillFrom1D_ShaderNum]);
            RenderTexture.ReleaseTemporary(temp); 
        }
        
        public bool ModifyShader<T>(string shaderName, int settingID, T settingValue) {
            foreach (var shaderMaterial in shaderMaterials) {
                if (shaderMaterial.name == shaderName) {
                    Type type = typeof(T);
                    
                    if (type == typeof(float))
                        shaderMaterial.SetFloat(settingID, (float) (object) settingValue);
                    else if (type == typeof(int))
                        shaderMaterial.SetInteger(settingID, (int) (object) settingValue);
                    else if (type == typeof(ComputeBuffer))
                        shaderMaterial.SetBuffer(settingID, (ComputeBuffer) (object) settingValue);
                    else
                        Debug.LogWarning("sXR: Type of "+type+" not supported in ModifyShader()");
                    
                    return true; } }
            Debug.LogWarning("sXR: Could not find shader - "+shaderName);
            return false; 
        }
        public bool ModifyShader<T>(string shaderName, string setting, T settingValue) 
        { return ModifyShader(shaderName, Shader.PropertyToID(setting), settingValue); }
        
        public void ListShaders() {
            string output = shaderMaterials.Length + " Shaders Detected: ";
            int count=0;
            foreach (var shader in shaderMaterials) {
                output += "\n" + count + ": "+shader.name;
                count++; }
            Debug.Log(output); }

        void Start() {
            sv = svision.Instance; 
            shaderVariables[ShaderName.ElectrodeParser] = new List<int>
            {
                Shader.PropertyToID("ElectrodesBuffer"),
                Shader.PropertyToID("NumberElectrodes"),
                Shader.PropertyToID("XResolution"),
                Shader.PropertyToID("YResolution"),
                Shader.PropertyToID("invert"),
                Shader.PropertyToID("GazeShiftX"),
                Shader.PropertyToID("GazeShiftY")
            };
            shaderVariables[ShaderName.AxonModel] = new List<int>
            {
                Shader.PropertyToID("ElectrodesBuffer"),
                Shader.PropertyToID("StartBuffer"),
                Shader.PropertyToID("AxonContributionsBuffer"),
                Shader.PropertyToID("ElectrodeToNeuronGaussBuffer"),
                Shader.PropertyToID("XResolution"),
                Shader.PropertyToID("YResolution"),
                Shader.PropertyToID("NumberElectrodes"),
                Shader.PropertyToID("RasterOffset"),
                Shader.PropertyToID("MinimumScreenPositionX"),
                Shader.PropertyToID("MinimumScreenPositionY"),
                Shader.PropertyToID("MaximumScreenPositionX"),
                Shader.PropertyToID("MaximumScreenPositionY"),
                Shader.PropertyToID("Coverage"),
                Shader.PropertyToID("ElectrodeThreshold"),
                Shader.PropertyToID("BrightnessThreshold")
            };
            foreach (var id in shaderVariables[ShaderName.AxonModel])
            {
                Debug.Log(id); 
            }
            
            Shader[] allShaders = Resources.LoadAll<Shader>("Shaders");
            List<Material> shaderMaterialsList = new List<Material>();

            for (int i = 0; i < allShaders.Length; i++)
                if (!allShaders[i].name.Contains("Hidden"))
                    shaderMaterialsList.Add(new Material(allShaders[i]));

            shaderMaterials = shaderMaterialsList.ToArray();

            for(int i=0; i<shaderMaterials.Length; i++) {
                int index=i; // inner scope for indexing in dict
                var actions = new Dictionary<string, Action>() // Determines which variable is being set by dictionary ref
                {
                    { shaderNameMap[ShaderName.Greyscale1D], () => Greyscale1D_ShaderNum = index },
                    { shaderNameMap[ShaderName.FillFrom1D], () => FillFrom1D_ShaderNum = index },
                };

                if (actions.TryGetValue(shaderMaterials[i].name, out var action)) 
                    action(); // calls action under shader name in dict above
            }

            
        }


        // private void ConcatenateShaders() {
        //     if(_preprocessors == null || (_preprocessors.Length==2 & 
        //     (!_preprocessors.Contains(ShaderName.ElectrodeParser) || !_preprocessors.Contains(ShaderName.Greyscale))))
        //         Debug.LogError("sVision - Preprocessor cannot be null, greyscale and electrode current information needed");
        //     else{
        //         ShaderName[] shaders = new ShaderName[_preprocessors.Length];
        //         Array.Copy(_preprocessors, shaders, _preprocessors.Length);
        //         
        //         if( _model!=ShaderName.None)
        //             shaders = shaders.Concat(new ShaderName[] { _model }).ToArray();
        //
        //         if (_postprocessors != null && (_postprocessors.Length > 1 || _postprocessors[0] != ShaderName.None))
        //             shaders = shaders.Concat(_postprocessors).ToArray();
        //         
        //         sxr.ApplyShaders(Array.ConvertAll(shaders, shaderName => shaderNameMap[shaderName]).ToList()); }
        // }
        //
        // public void SetPreprocessors(params ShaderName[] preprocessors) {
        //     if (!preprocessors.Contains(ShaderName.ElectrodeParser)) {
        //         Debug.LogWarning("Preprocessor List must contain electrode grab function, adding");
        //         preprocessors = preprocessors.Append(ShaderName.ElectrodeParser).ToArray(); }
        //
        //     foreach (var preprocessor in preprocessors) {
        //         if (!new ShaderName[]
        //         {ShaderName.ElectrodeParser, ShaderName.EdgeDetection, ShaderName.HorizontalBlur, ShaderName.VerticalBlur
        //         }.Contains(preprocessor))
        //             Debug.LogWarning(preprocessor.ToString()+" applied as preprocessor"); }
        //
        //     _preprocessors = preprocessors;
        //     foreach(var preproc in _preprocessors)
        //         Debug.Log(preproc);
        //     ConcatenateShaders();
        // }
        //
        // public void SetModel(ShaderName model)
        // {
        //     if(!new ShaderName[] {ShaderName.AxonModel, ShaderName.LinearModel}.Contains(model))
        //         Debug.LogWarning(model.ToString()+" applied as model");
        //     _model = model; 
        //     ConcatenateShaders();
        // }
        //
        // public void SetPostProcessors(params ShaderName[] postprocessors)
        // {
        //     foreach (var postprocessor in postprocessors) {
        //         if (!new ShaderName[]
        //         {ShaderName.GazeShift, ShaderName.TemporalModel
        //         }.Contains(postprocessor))
        //             Debug.LogWarning(postprocessor.ToString()+" applied as postprocessor"); }
        //
        //     _postprocessors = postprocessors; 
        //     ConcatenateShaders();
        // }

        public bool ModifyShader<T>(ShaderName shaderName, string setting, T settingValue)
        { return ModifyShader(shaderNameMap[shaderName], setting, settingValue); }
        
        public bool ModifyShader<T>(ShaderName shaderName, int settingID, T settingValue)
        { return ModifyShader(shaderNameMap[shaderName], settingID, settingValue); }

        // public void InitializeReadWriteBuffers()
        // {
        //     ModifyShader(ShaderName.ElectrodeParser, shaderVariables[ShaderName.ElectrodeParser][0],
        //         sv.electrodesBuffer);
        //
        //     if(sv.modelType == SpatialModelType.AxonMapModel){
        //         ModifyShader(ShaderName.AxonModel, shaderVariables[ShaderName.AxonModel][0], sv.electrodesBuffer);
        //         ModifyShader(ShaderName.AxonModel, shaderVariables[ShaderName.AxonModel][1], sv.axonIdxStartBuffer);
        //         ModifyShader(ShaderName.AxonModel, shaderVariables[ShaderName.AxonModel][2],
        //             sv.axonContributionsBuffer);
        //         ModifyShader(ShaderName.AxonModel, shaderVariables[ShaderName.AxonModel][3], sv.electrodeGaussBuffer);
        //     }
        // }
        //
        // public void UpdateElectrodeParser(int xRes, int yRes, int downscaleFactor, bool invert, float gazeShiftX, float gazeShiftY)
        // {
        //     sxr_internal.ShaderHandler.Instance.SetDownscaleFactor(downscaleFactor);
        //     if (_preprocessors!=null && !_preprocessors.Contains(ShaderName.ElectrodeParser))
        //         _preprocessors = _preprocessors.Append(ShaderName.ElectrodeParser).ToArray();
        //     this.invert = invert;
        //     ModifyShader(ShaderName.ElectrodeParser, shaderVariables[ShaderName.ElectrodeParser][1], sv.electrodesBuffer.count);
        //     ModifyShader(ShaderName.ElectrodeParser, shaderVariables[ShaderName.ElectrodeParser][2], xRes);
        //     ModifyShader(ShaderName.ElectrodeParser, shaderVariables[ShaderName.ElectrodeParser][3], yRes);
        //     ModifyShader(ShaderName.ElectrodeParser, shaderVariables[ShaderName.ElectrodeParser][4], invert ? 1 : 0);
        //     ModifyShader(ShaderName.ElectrodeParser, shaderVariables[ShaderName.ElectrodeParser][5], gazeShiftX);
        //     ModifyShader(ShaderName.ElectrodeParser, shaderVariables[ShaderName.ElectrodeParser][6], gazeShiftY);
        // }
        //
        // private void UpdateAxonModelShader()
        // {
        //     if(_model != ShaderName.AxonModel) SetModel(ShaderName.AxonModel);
        //     ModifyShader(ShaderName.AxonModel, shaderVariables[ShaderName.AxonModel][4], sv.xResolution);
        //     ModifyShader(ShaderName.AxonModel, shaderVariables[ShaderName.AxonModel][5], sv.yResolution);
        //     ModifyShader(ShaderName.AxonModel, shaderVariables[ShaderName.AxonModel][6], sv.electrodesBuffer.count);
        //     ModifyShader(ShaderName.AxonModel, shaderVariables[ShaderName.AxonModel][7], RasterizationHandler.Instance.RasterOffset);
        //     ModifyShader(ShaderName.AxonModel, shaderVariables[ShaderName.AxonModel][8], sv.minimumScreenPositionX);
        //     ModifyShader(ShaderName.AxonModel, shaderVariables[ShaderName.AxonModel][9], sv.minimumScreenPositionY);
        //     ModifyShader(ShaderName.AxonModel, shaderVariables[ShaderName.AxonModel][10], sv.maximumScreenPositionX);
        //     ModifyShader(ShaderName.AxonModel, shaderVariables[ShaderName.AxonModel][11], sv.maximumScreenPositionY);
        //     ModifyShader(ShaderName.AxonModel, shaderVariables[ShaderName.AxonModel][12], sv.coverage);
        //     ModifyShader(ShaderName.AxonModel, shaderVariables[ShaderName.AxonModel][13], sv.electrodeThreshold);
        //     ModifyShader(ShaderName.AxonModel, shaderVariables[ShaderName.AxonModel][14], sv.brightnessThreshold);
        // }
        //
        // private void UpdateShaderVariables()
        // {
        //     if(sv.runPreprocessing)
        //         UpdateElectrodeParser(sv.xResolution, sv.yResolution, sv.xResolution/Screen.width, sv.invertPreproc, 0.0f, 0.0f);
        //     if(sv.runBionicVision)
        //     {
        //         if (sv.modelType == SpatialModelType.AxonMapModel)
        //             UpdateAxonModelShader();
        //     }
        // }
        //
        // public void UpdatePreprocessorBlur(int blurAmount)
        // {
        //     ModifyShader(ShaderName.HorizontalBlur, "kernelSize", blurAmount);
        //     ModifyShader(ShaderName.VerticalBlur, "kernelSize", blurAmount); 
        // }
        //
        // public void SetPostprocessorBlurAmount(int blurAmount)
        // {
        //     ModifyShader(ShaderName.PostProcessingHorizontalBlur, "kernelSize", blurAmount);
        //     ModifyShader(ShaderName.PostProcessingVerticalBlur, "kernelSize", blurAmount); 
        // }
        //
        // private void Update()
        // {
        //     UpdateShaderVariables();
        // }
        //

        public static ShaderHandler Instance;
        private void Awake() {
            if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject.transform.root); }
            else  Destroy(gameObject); }
    }
}
