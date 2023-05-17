using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace svision_internal
{
    public class ShaderHandler : MonoBehaviour
    {
        public enum ShaderName {
            ElectrodeParser,
            EdgeDetection,
            Greyscale,
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
        
        private static readonly Dictionary<ShaderName, string> shaderNameMap = new Dictionary<ShaderName, string> {
            { ShaderName.ElectrodeParser, "sVision/Utility/ElectrodeCurrentParser" },
            { ShaderName.EdgeDetection, "sXR/EdgeDetect" },
            { ShaderName.Greyscale, "sXR/Greyscale" },
            { ShaderName.HorizontalBlur, "sXR/HorizontalBlur" },
            { ShaderName.VerticalBlur, "sXR/VerticalBlur" },
            { ShaderName.AxonModel, "sVision/Percept/AxonModelShader" },
            { ShaderName.LinearModel, "sVision/Percept/LinearAdditionShader" },
            { ShaderName.GazeShift, "sXR/Shift" },
            { ShaderName.PostProcessingHorizontalBlur, "sVision/PostProcessing/PostProcessHorizontalBlur" },
            { ShaderName.PostProcessingVerticalBlur, "sVision/PostProcessing/PostProcessHorizontalBlur" },
            { ShaderName.TemporalModel, "sVision/PostProcessing/TemporalModel" },
            { ShaderName.None, "sVision/PostProcessing/TemporalModel" }
        };
        Dictionary<ShaderName, List<int>> shaderVariables = new Dictionary<ShaderName, List<int>>();

        private ShaderName[] _preprocessors;
        private ShaderName _model = ShaderName.None;
        private ShaderName[] _postprocessors;

        private void ConcatenateShaders() {
            if(_preprocessors == null || (_preprocessors.Length==1 & _preprocessors[0]!=ShaderName.None))
                Debug.LogError("sVision - Preprocessor cannot be null, electrode current information is needed");
            else{
                ShaderName[] shaders = new ShaderName[_preprocessors.Length];
                Array.Copy(_preprocessors, shaders, _preprocessors.Length);
                
                if( _model!=ShaderName.None)
                    shaders = shaders.Concat(new ShaderName[] { _model }).ToArray();

                if (_postprocessors != null && (_postprocessors.Length > 1 || _postprocessors[0] != ShaderName.None))
                    shaders = shaders.Concat(_postprocessors).ToArray();
                
                sxr.ApplyShaders(Array.ConvertAll(shaders, shaderName => shaderNameMap[shaderName]).ToList()); }
        }

        public void SetPreprocessors(params ShaderName[] preprocessors) {
            if (!preprocessors.Contains(ShaderName.ElectrodeParser)) {
                Debug.LogWarning("Preprocessor List must contain electrode grab function, adding");
                preprocessors = preprocessors.Append(ShaderName.ElectrodeParser).ToArray(); }

            foreach (var preprocessor in preprocessors) {
                if (!new ShaderName[]
                {ShaderName.ElectrodeParser, ShaderName.EdgeDetection, ShaderName.HorizontalBlur, ShaderName.VerticalBlur
                }.Contains(preprocessor))
                    Debug.LogWarning(preprocessor.ToString()+" applied as preprocessor"); }

            _preprocessors = preprocessors;
            foreach(var preproc in _preprocessors)
                Debug.Log(preproc);
            ConcatenateShaders();
        }

        public void SetModel(ShaderName model)
        {
            if(!new ShaderName[] {ShaderName.AxonModel, ShaderName.LinearModel}.Contains(model))
                Debug.LogWarning(model.ToString()+" applied as model");
            _model = model; 
            ConcatenateShaders();
        }

        public void SetPostProcessors(params ShaderName[] postprocessors)
        {
            foreach (var postprocessor in postprocessors) {
                if (!new ShaderName[]
                {ShaderName.GazeShift, ShaderName.TemporalModel
                }.Contains(postprocessor))
                    Debug.LogWarning(postprocessor.ToString()+" applied as postprocessor"); }

            _postprocessors = postprocessors; 
            ConcatenateShaders();
        }

        public bool ModifyShader<T>(ShaderName shaderName, string setting, T settingValue)
        { return sxr.SetShaderVariables(shaderNameMap[shaderName], setting, settingValue); }
        
        public bool ModifyShader<T>(ShaderName shaderName, int settingID, T settingValue)
        { return sxr.SetShaderVariables(shaderNameMap[shaderName], settingID, settingValue); }

        public void InitializeElectrodeParser(int xRes, int yRes, bool invert)
        {
            ModifyShader(ShaderName.ElectrodeParser, shaderVariables[ShaderName.ElectrodeParser][0], xRes);
            ModifyShader(ShaderName.ElectrodeParser, shaderVariables[ShaderName.ElectrodeParser][1], yRes);
            ModifyShader(ShaderName.ElectrodeParser, shaderVariables[ShaderName.ElectrodeParser][2], invert ? 1 : 0);
        }

        public void UpdateGazeElectrodeParser(float gazeShiftX, float gazeShiftY) {
            ModifyShader(ShaderName.ElectrodeParser, shaderVariables[ShaderName.ElectrodeParser][3], gazeShiftX);
            ModifyShader(ShaderName.ElectrodeParser, shaderVariables[ShaderName.ElectrodeParser][4], gazeShiftY);
        }
        
        public void SetPreprocessorBlurAmount(int blurAmount)
        {
            ModifyShader(ShaderName.HorizontalBlur, "kernelSize", blurAmount);
            ModifyShader(ShaderName.VerticalBlur, "kernelSize", blurAmount); 
        }
        
        public void SetPostprocessorBlurAmount(int blurAmount)
        {
            ModifyShader(ShaderName.PostProcessingHorizontalBlur, "kernelSize", blurAmount);
            ModifyShader(ShaderName.PostProcessingVerticalBlur, "kernelSize", blurAmount); 
        }
        

        public void Start()
        {
            shaderVariables[ShaderName.ElectrodeParser] = new List<int>
            {
                Shader.PropertyToID("XResolution"),
                Shader.PropertyToID("YResolution"),
                Shader.PropertyToID("invert"),
                Shader.PropertyToID("GazeShiftX"),
                Shader.PropertyToID("GazeShiftY")
            };

        }

        public static ShaderHandler Instance;
        private void Awake() {
            if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject.transform.root); }
            else  Destroy(gameObject); }
    }
}
