using System.IO;
using UnityEngine;

namespace svision_internal
{
    public static class AxonSegmentHandler
    {
        /// <summary>
        /// Used to read pulse2percept output file axon_contrib.dat
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static AxonSegment[] ReadAxonSegments(string path)
        {
            
            string directory = Path.GetDirectoryName(path); 
            if(!Directory.Exists(directory))
            {
                Debug.LogError("sVision - ReadAxonSegment path does not have a valid directory");
                return new AxonSegment[0]; 
            }
            
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
            {
                AxonSegment[] axon_buff = new AxonSegment[reader.BaseStream.Length/3/sizeof(float)];

                for (int i = 0; i < axon_buff.Length; i++)
                {
                    axon_buff[i] = new AxonSegment(new float[] {reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()});
                }
                return axon_buff;
            }
        }
    }
}