using System.Collections.Generic;
using UnityEngine;
using System.IO;
using svision_internal;

/// <summary>
/// 
/// </summary>
public static class BinaryHandler
{
    /// <summary>
    /// Reads ints from a given binary file and returns the list of ints.
    /// </summary>
    /// <param name="path">Path of the binary file to read from</param>
    /// <returns>List of ints read from given file</returns>
    public static List<int> ReadFromBinaryFile(string path) {
        Debug.Log("Reading from: " + path);
        using var filestream = File.Open(path, FileMode.Open);
        using var binaryStream = new BinaryReader(filestream);
        var pos = 0;
        List<int> result = new List<int>();
        var length = (float)binaryStream.BaseStream.Length;
        while (pos < length) {
            int element = binaryStream.ReadInt32();
            result.Add(element);
            pos += sizeof(int); }

        return result;
    }
    
    /// <summary>
    /// Reads floats from a given binary file and returns the list of floats.
    /// </summary>
    /// <param name="path">Path of the binary file to read from</param>
    /// <returns>List of floats read from given file</returns>
    public static List<float> ReadFloatsFromBinaryFile(string path) {
        Debug.Log("Reading from: " + path);
        using var filestream = File.Open(path, FileMode.Open);
        using var binaryStream = new BinaryReader(filestream);
        var pos = 0;
        List<float> result = new List<float>();
        var length = (float)binaryStream.BaseStream.Length;
        while (pos < length) {
            int element = binaryStream.ReadInt32();
            result.Add(element);
            pos += sizeof(int); }

        return result;
    }
    
    /// <summary>
    /// Reads floats from the given binary file and places them in order into a 2D array of the given size
    /// </summary>
    /// <param name="row">Number of rows</param>
    /// <param name="col">Number of columns</param>
    /// <param name="path">Path of the binary file to read from</param>
    /// <returns>2D array of floats read from given file</returns>
    public static float[,] Read2DArray_float32(int row, int col, string path) {
        float[,] rate_buff = new float[row, col];

        using BinaryReader reader = new BinaryReader(File.OpenRead(path));

        for (int i = 0; i < row; i++) {
            for (int j = 0; j < col; j++) {
                float temp = reader.ReadSingle();
                rate_buff[i, j] = temp; } }

        return rate_buff;
    }
    
    /// <summary>
    /// Reads info from the given binary file and translates that into AxonSegments
    /// </summary>
    /// <param name="path">Path of the binary file to read from</param>
    /// <returns>Array of AxonSegments read from given file</returns>
    public static AxonSegment[] ReadAxonSegments(string path) {
        using BinaryReader reader = new BinaryReader(File.OpenRead(path));
        AxonSegment[] axon_buff = new AxonSegment[reader.BaseStream.Length / 3 / sizeof(float)];

        for (int i = 0; i < axon_buff.Length; i++)
            axon_buff[i] = new AxonSegment(new float[] { reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() });

        return axon_buff;
    }
    
    /// <summary>
    /// Writes the given array of floats to the given binary file
    /// </summary>
    /// <param name="path">Path of the binary file to write to</param>
    /// <param name="axonSegments">Array of floats to write to file</param>
    public static void WriteFloatArray(string path, float[] floatArray) {
        CheckPath(path);
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        using var writer = new BinaryWriter(stream);

        foreach (float item in floatArray)
            writer.Write(item);
    }
  
    /// <summary>
    /// Checks that the given path exists
    /// </summary>
    /// <param name="path">Path to check</param>
    /// <returns>True if the given path exists : false</returns>
    public static bool CheckPath(string path)
    {
        string directory = Path.GetDirectoryName(path); 
        if(directory!=null & !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        return Directory.Exists(directory);
    }
}