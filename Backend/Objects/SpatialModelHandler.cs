using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace svision_internal{
    public class SpatialModelHandler
    {
        private Electrode[] electrodes;

        public SpatialModelHandler(Electrode[] electrodes)
        { this.electrodes = electrodes; }
        public SpatialModelHandler() 
        { Debug.LogError("sVision - SpatialModelHandler requires electrodes"); }

        private static string savedSettingsPath = Application.dataPath + Path.DirectorySeparatorChar +
                                                  "sVision" + Path.DirectorySeparatorChar + "Resources" +
                                                  Path.DirectorySeparatorChar + "PreComputedModels" +
                                                  Path.DirectorySeparatorChar;

        public enum SpatialModel {
            AxonMapModel,
            CorticalModel
        }

        public static void SetAxonMapModel(int downscaleFactor,
            float headsetFOV_Horizontal, float headsetFOV_Vertical, float xMin, float xMax,
            float yMin, float yMax, int xRes, int yRes, int rho, int lambda,
            float axon_threshold, int number_axons, int number_axon_segments, bool useLeftEye) 
        {
            AxonMapModel axonMapModel = new AxonMapModel(
                savedSettingsPath + "AxonMapModels" + Path.DirectorySeparatorChar,
                "", downscaleFactor, headsetFOV_Horizontal, headsetFOV_Vertical, xMin, xMax,
                yMin, yMax, xRes, yRes, rho, lambda, axon_threshold, number_axons, number_axon_segments, useLeftEye);
        }

        public float[] GetElectrodeToAxonSegmentGauss(string axonContribDataFilePath) {
            int numberElectrodes = electrodes.Length;
            AxonSegment[] segmentsContrib = AxonSegmentHandler.ReadAxonSegments(axonContribDataFilePath);
            int rho = 0;
            foreach (var item in axonContribDataFilePath.Split("_"))
                if (item.Contains("rho"))
                    if (!Int32.TryParse(item.Replace("rho", ""), out rho))
                        Debug.LogError(
                            "sVision - Could not parse rho value from filename during GetElectrodeToAxonSegmentGauss");

            long numberElectrodesToSegments =
                (long) segmentsContrib.Length * (long) numberElectrodes;

            ComputeBuffer axonSegmentGaussToElectrodes = new ComputeBuffer(1, 4); 

            ComputeShader computeShader = Resources.Load<ComputeShader>(
                "Shaders" + Path.DirectorySeparatorChar + "ComputeShaders" +
                Path.DirectorySeparatorChar + "ElectrodeContribution");

            Debug.Log(segmentsContrib.Length + "*" + numberElectrodes);
            int kernel = computeShader.FindKernel("calculateGauss");

            if (numberElectrodesToSegments == 0 || numberElectrodesToSegments * 4 > 2147483648) {
                Debug.LogError(
                    "sVision - Calculating axon segment electrode interactions not computationally feasible." +
                    "  Please raise downscaling factor or lower number of electrodes");
                return new float[0]; 
            }
            else {
                axonSegmentGaussToElectrodes = new ComputeBuffer((int) numberElectrodesToSegments, 4);
                ComputeBuffer electrodesBuffer =
                    new ComputeBuffer(electrodes.Length,
                        System.Runtime.InteropServices.Marshal.SizeOf(typeof(Electrode)), ComputeBufferType.Default);
                electrodesBuffer.SetData(electrodes);

                ComputeBuffer axonsBuffer =
                    new ComputeBuffer((int) numberElectrodesToSegments,
                        System.Runtime.InteropServices.Marshal.SizeOf(typeof(Neuron)), ComputeBufferType.Default);
                axonsBuffer.SetData(segmentsContrib
                    .Select(a => new Neuron(a.xPosition, a.yPosition, a.zPosition, a.brightnessContribution))
                    .ToArray());

                computeShader.SetBuffer(kernel, "Electrodes", electrodesBuffer);
                computeShader.SetBuffer(kernel, "Axons", axonsBuffer);
                computeShader.SetBuffer(kernel, "AxonSegmentGauss",
                    axonSegmentGaussToElectrodes);

                computeShader.SetInt("ElectrodeCount", numberElectrodes);
                computeShader.SetInt("NeuronCount", segmentsContrib.Length);
                computeShader.SetFloat("rho", rho);
                computeShader.Dispatch(kernel,
                    segmentsContrib.Length / 1024 + 1, 1, 1);
                electrodesBuffer.Release();
                axonsBuffer.Release();

            }

            float[] returnArray = new float[segmentsContrib.Length * numberElectrodes]; 
            axonSegmentGaussToElectrodes.GetData(returnArray);
            return returnArray;
        }
    }
}
