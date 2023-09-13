using System;

namespace svision_internal
{
    public class PrebuiltDemoModel
    {
        public enum PrebuiltDemoModels
        {
            Prima,
            ArgusII,
            NeuraLink,
            Scoreboard
        }

        private SpatialModelType modelType; 
        private string spatialModelPath;
        private string electrodeModelPath;
        private int xResolution;
        private int yResolution;
        private float coverage;
        
        public SpatialModelType ModelType => modelType; 
        
        public string SpatialModelPath => spatialModelPath;
    
        public string ElectrodeModelPath => electrodeModelPath;

        public int XResolution => xResolution;
        
        public int YResolution => yResolution;

        public float Coverage => coverage;

        public PrebuiltDemoModel(SpatialModelType modelType, string spatialModelPath, string electrodeModelPath, int xResolution, int yResolution, float coverage)
        {
            this.spatialModelPath = spatialModelPath;
            this.electrodeModelPath = electrodeModelPath;
            this.xResolution = xResolution;
            this.yResolution = yResolution; 
            this.coverage = coverage;
        }

        public static PrebuiltDemoModel GetModel(PrebuiltDemoModels model)
        {
            switch (model)
            {
                case PrebuiltDemoModels.Prima:
                    return new PrebuiltDemoModel(SpatialModelType.AxonMapModel,
                        FolderPaths.axonMapModelsPath +
                        "implantHorFOV20_headsetHorFOV60implantVerFOV20_headsetVerFOV60_downscale2_xRes1080_yRes1200_rho75_lambda100_numAxons1000_numSegments1000",
                        FolderPaths.electrodeModelsPath + "PrimaDemo", 540, 600, .333333f);
                
                case PrebuiltDemoModels.ArgusII: 
                    return new PrebuiltDemoModel(SpatialModelType.AxonMapModel,
                        FolderPaths.axonMapModelsPath +
                        "implantHorFOV20_headsetHorFOV60implantVerFOV20_headsetVerFOV60_downscale2_xRes1080_yRes1200_rho75_lambda100_numAxons1000_numSegments1000",
                        FolderPaths.electrodeModelsPath + "PrimaFullRes", 540, 600, .333333f);
                
                case PrebuiltDemoModels.NeuraLink: 
                    return new PrebuiltDemoModel(SpatialModelType.AxonMapModel,
                        FolderPaths.axonMapModelsPath +
                        "implantHorFOV20_headsetHorFOV60implantVerFOV20_headsetVerFOV60_downscale2_xRes1080_yRes1200_rho75_lambda100_numAxons1000_numSegments1000",
                        FolderPaths.electrodeModelsPath + "PrimaFullRes", 540, 600, .333333f);
                
                case PrebuiltDemoModels.Scoreboard: 
                    return new PrebuiltDemoModel(SpatialModelType.AxonMapModel,
                        FolderPaths.axonMapModelsPath +
                        "implantHorFOV60_headsetHorFOV60implantVerFOV60_headsetVerFOV60_downscale1_xRes500_yRes500_rho150_lambda495_numAxons352_numSegments445",
                        FolderPaths.electrodeModelsPath + "Test", 500, 500,  1);
                
                default: 
                    return new PrebuiltDemoModel(SpatialModelType.AxonMapModel,
                        FolderPaths.axonMapModelsPath +
                        "implantHorFOV20_headsetHorFOV60implantVerFOV20_headsetVerFOV60_downscale2_xRes1080_yRes1200_rho75_lambda100_numAxons1000_numSegments1000",
                        FolderPaths.electrodeModelsPath + "PrimaFullRes", 540, 600,  .333333f);
            }
        }
    }
}