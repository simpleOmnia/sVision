using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace svision_internal
{
    public struct AxonMapModel
    {
        public string savedSettingsPath; 
        
        public string saveName; 
        public int downscaleFactor;
        public float headsetFOV_Horizontal; 
        public float headsetFOV_Vertical; 
        public float xMin;
        public float xMax;
        public float yMin;
        public float yMax;
        public int xRes;
        public int yRes;
        public int rho;
        public int lambda;
        public float axon_threshold;
        public int number_axons;
        public int number_axon_segments;
        public bool useLeftEye; 
        
        public AxonMapModel(string savedSettingsPath, string saveName, int downscaleFactor,
            float headsetFOV_Horizontal, float headsetFOV_Vertical, float xMin, float xMax,
            float yMin, float yMax, int xRes, int yRes, int rho, int lambda,
            float axon_threshold, int number_axons, int number_axon_segments, bool useLeftEye)
        {
            this.savedSettingsPath = savedSettingsPath;
            this.saveName = saveName;
            this.downscaleFactor = downscaleFactor;
            this.headsetFOV_Horizontal = headsetFOV_Horizontal;
            this.headsetFOV_Vertical = headsetFOV_Vertical;
            this.xMin = xMin;
            this.xMax = xMax;
            this.yMin = yMin;
            this.yMax = yMax;
            this.xRes = xRes;
            this.yRes = yRes;
            this.rho = rho;
            this.lambda = lambda;
            this.axon_threshold = axon_threshold;
            this.number_axons = number_axons;
            this.number_axon_segments = number_axon_segments;
            this.useLeftEye = useLeftEye;
            simulated_xResolution = simulated_yResolution = 0;
            simulation_xStep = simulation_yStep =
                simulation_xMin = simulation_xMax = simulation_yMin = simulation_yMax = 0;
            SetSimulationBounds();
        }
        
        private int simulated_xResolution, simulated_yResolution;
        private float simulation_xStep, simulation_yStep;
        private float simulation_xMin, simulation_xMax;
        private float simulation_yMin, simulation_yMax;
        public void SetSimulationBounds()
        {
            Debug.Log("Setting simulation bounds: " + xRes + ", " + yRes); 
            simulated_xResolution =
                (int) Math.Floor(
                    (double) xRes / (double) downscaleFactor);
            simulated_yResolution = (int) Math.Floor((double) yRes /
                                                     (double) downscaleFactor);

            Debug.Log("X-res" + simulated_xResolution);
            Debug.Log("Y-res" +simulated_yResolution);

            simulation_xStep =
                headsetFOV_Horizontal / simulated_xResolution;

            float xCenter = xMax + xMin / 2.0f;
            int numberStepsPerXDirection =
                (int) ((xMax - xMin /
                        (2 * simulation_xStep)) - 1);
            Debug.Log("X-center"+$"{xCenter:0.00}");
            Debug.Log("X-step size: "+$"{simulation_xStep:0.00}");
            
            simulation_xMin =
                (xCenter - .5f * simulation_xStep) -
                (simulation_xStep * (numberStepsPerXDirection));
            simulation_xMax =
                (xCenter + .5f * simulation_xStep) +
                (simulation_xStep * (numberStepsPerXDirection));
            
            simulation_yStep =
                headsetFOV_Vertical / simulated_yResolution;
           
            float yCenter = yMax + yMin / 2.0f;
            int numberStepsPerYDirection =
                (int) ((yMax - yMin /
                    (2 * simulation_yStep)) - 1);
            Debug.Log("Y-center"+$"{yCenter:0.00}");
            Debug.Log("Y-step size: "+$"{simulation_yStep:0.00}");
            
            simulation_yMin =
                (yCenter - .5f * simulation_yStep) -
                (simulation_yStep * (numberStepsPerYDirection));
            simulation_yMax =
                (yCenter + .5f * simulation_yStep) +
                (simulation_yStep * (numberStepsPerYDirection));

            if (simulation_xStep != simulation_yStep)
            {
                Debug.LogWarning("sVision - simulation_xstep conflicts with simulation_ystep, non-square pixels currently unsupported by pulse2percept");
            }
        }

        public void BuildModel()
        {
            if(!saveName.Contains("_rho")) Debug.LogWarning("sVision - AxonMapModel saveName must contain _rho(#rho_value) for computing electrode gaussian");
            saveName = (string.IsNullOrEmpty(saveName) || !saveName.Contains("_rho"))
                ? "implantHorFOV" + (xMax-xMin) + "_headsetHorFOV" + headsetFOV_Horizontal +
                  "implantVerFOV" + (yMax-yMin) + "_headsetVerFOV" + headsetFOV_Vertical +
                  "_xRes" + xRes + "_yRes" + yRes + "_rho" + rho + "_lambda" + lambda + "_numAxons" + number_axons +
                   "_numSegments" + number_axon_segments + (useLeftEye ? "_Left" : "_Right") : saveName;
            String pythonPath =  Application.dataPath + Path.DirectorySeparatorChar + "sVision" + Path.DirectorySeparatorChar +
                                 "Backend" + Path.DirectorySeparatorChar + "python" + Path.DirectorySeparatorChar;
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = "python";
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardOutput = false;
            processStartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            processStartInfo.CreateNoWindow = false;
            
            Debug.Log(savedSettingsPath);
            string pulse2perceptCmdCall = pythonPath+"build-p2p.py " +
                                          savedSettingsPath
                                          + " " + simulation_xMin + " " + simulation_xMax +
                                          " " + simulation_yMin + " " + simulation_yMax + " " + simulation_xStep + 
                                          " " + rho + " " + lambda + 
                                          " " + axon_threshold + " " + number_axons + " " + number_axon_segments + " " 
                                          + useLeftEye + " " + saveName;
            Debug.Log("Save name: "+saveName);
            
            Debug.Log(pulse2perceptCmdCall);
            processStartInfo.Arguments = pulse2perceptCmdCall;
            
            Process process = Process.Start(processStartInfo);
            process?.WaitForExit();
        }
    }
}