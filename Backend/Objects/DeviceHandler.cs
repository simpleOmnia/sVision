using System;
using System.IO;
using UnityEngine;

namespace svision_internal
{
    public class DeviceHandler 
    {
        private Electrode[] electrodes;
        
        public enum PrebuiltImplant
        {
            Prima,
            ArgusII,
            AlphaAMS,
            AlphaIMS,
            NeuraLink,
            Cortivis, 
            Lattice
        }

        public Electrode[] GetElectrodes() { 
            Electrode[] copy = new Electrode[electrodes.Length];
            Array.Copy(electrodes, copy, electrodes.Length);
            return copy; }

        public string ListElectrodes()
        {
            if (electrodes != null)
            {
                string output = "";
            foreach (var electrode in electrodes)
                output += electrode.ToString() + "   *****   ";
            Debug.Log(output);
            return output;
            }

            Debug.Log("svision - no electrodes");
            return "No Electrodes"; 
        }

        public void LoadDevice(PrebuiltImplant prebuiltImplant)
        { LoadDevice(prebuiltImplant.ToString()); }

        public void LoadDevice(string fileName) {
            TextAsset[] csvFiles = Resources.LoadAll<TextAsset>("ImplantCSVs");
            foreach (TextAsset csvFile in csvFiles) {
                if (fileName.StartsWith(csvFile.name)) {
                    string[] electrodeLocStrings = csvFile.text.Split("\n"); 
                    electrodes = new Electrode[electrodeLocStrings.Length];
                    for (int i = 0; i < electrodes.Length; i++) {
                        float xLoc, yLoc, zLoc = 0;
                        string[] electrodeLocString = electrodeLocStrings[i].Split(",");
                        if (float.TryParse(electrodeLocString[0], out xLoc) &
                            float.TryParse(electrodeLocString[1], out yLoc) &
                            (electrodeLocString.Length > 2 && float.TryParse(electrodeLocString[2], out zLoc) ||
                             electrodeLocString.Length == 2)) {
                            electrodes[i] = new Electrode(i, xLoc, yLoc, zLoc);
                            continue; }

                        Debug.LogError("Failed to load file: " + fileName); } } }
        }
        
        public void CreateLattice(int xCount, int yCount, int zCount, float xSpacing, float ySpacing, float zSpacing) {
            electrodes = new Electrode[xCount * yCount * zCount];

            float xOffset = (xCount - 1) * xSpacing * 0.5f;
            float yOffset = (yCount - 1) * ySpacing * 0.5f;
            float zOffset = (zCount - 1) * zSpacing * 0.5f;

            for (int x = 0; x < xCount; x++)
                for (int y = 0; y < yCount; y++)
                    for (int z = 0; z < zCount; z++) {
                        Vector3 position = new Vector3(x * xSpacing - xOffset, y * ySpacing - yOffset,
                            z * zSpacing - zOffset);
                        int index = x + y * xCount + z * xCount * yCount;
                        electrodes[index] = new Electrode(index, position.x, position.y, position.z); }
        }
        public void CreateLattice(int xCount, int yCount, float xSpacing, float ySpacing)
        { CreateLattice(xCount, yCount, 1, xSpacing, ySpacing, 0); }
        
        public void MoveAndRotateElectrodeArray(float xShift, float yShift, float zShift, float xRot, float yRot, float zRot) {
            // Calculate the centroid of the electrode array
            Vector3 centroid = Vector3.zero;
            for (int i = 0; i < electrodes.Length; i++)
                centroid += new Vector3(electrodes[i].x, electrodes[i].y, electrodes[i].z);
        
            centroid /= electrodes.Length;
        
            Quaternion rotation = Quaternion.Euler(xRot, yRot, zRot);

            for(int i = 0; i < electrodes.Length; i++)
            {
                Vector3 translatedPoint = new Vector3(electrodes[i].x, electrodes[i].y, electrodes[i].z) - centroid;
                Vector3 rotatedPoint = rotation * translatedPoint;
                Vector3 finalPoint = rotatedPoint + centroid + new Vector3(xShift, yShift, zShift);
            
                electrodes[i] = new Electrode(electrodes[i].electrodeNumber, finalPoint.x, finalPoint.y, finalPoint.z);
            }
        }
        public void MoveAndRotateElectrodeArray(float xShift, float yShift, float xRot, float yRot)
        { MoveAndRotateElectrodeArray(xShift, yShift, 0, xRot, yRot, 0); }
        
    }
}
