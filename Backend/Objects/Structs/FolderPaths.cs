using System.IO;
using UnityEngine;

namespace svision_internal
{
    public struct FolderPaths
    {
        public static string modelsPath = Application.dataPath + Path.DirectorySeparatorChar +
                                           "sVision" + Path.DirectorySeparatorChar + "Resources" +
                                           Path.DirectorySeparatorChar + "PreComputedModels" +
                                           Path.DirectorySeparatorChar ;
        
        public static string axonMapModelsPath = Application.dataPath + Path.DirectorySeparatorChar +
                                          "sVision" + Path.DirectorySeparatorChar + "Resources" +
                                          Path.DirectorySeparatorChar + "PreComputedModels" +
                                          Path.DirectorySeparatorChar + "AxonMapModels" + Path.DirectorySeparatorChar;
        
        public static string electrodeModelsPath = Application.dataPath + Path.DirectorySeparatorChar +
                                                 "sVision" + Path.DirectorySeparatorChar + "Resources" +
                                                 Path.DirectorySeparatorChar + "PreComputedModels" +
                                                 Path.DirectorySeparatorChar + "ElectrodeInteractionModels" + Path.DirectorySeparatorChar;
        
        public static string corticalModelsPath = Application.dataPath + Path.DirectorySeparatorChar +
                                                   "sVision" + Path.DirectorySeparatorChar + "Resources" +
                                                   Path.DirectorySeparatorChar + "PreComputedModels" +
                                                   Path.DirectorySeparatorChar + "CorticalModels" + Path.DirectorySeparatorChar;
        
        public static string implantCSVPath = Application.dataPath + Path.DirectorySeparatorChar +
                                              "sVision" + Path.DirectorySeparatorChar + "Resources" +
                                              Path.DirectorySeparatorChar + "ImplantCSVs" + Path.DirectorySeparatorChar;
    }
}