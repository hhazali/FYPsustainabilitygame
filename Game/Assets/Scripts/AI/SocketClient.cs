using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SocketClient : MonoBehaviour
{
    TcpClient client;
    NetworkStream stream;

    void Start()
    {
        try
        {
            client = new TcpClient("127.0.0.1", 65432);
            stream = client.GetStream();
            Debug.Log("✅ Connected to Python server.");
        }
        catch (Exception e)
        {
            Debug.LogError("❌ Connection failed: " + e.Message);
        }
    }

    public void SendFeatures(float trashPicked, float helpHints, float spawnMores, float timeTaken)
    {
        if (stream == null) return;

        try
        {
            float[] features = { trashPicked, helpHints, spawnMores, timeTaken };

            // Serialize the feature array to binary
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, features);
            byte[] data = ms.ToArray();

            // Send to Python
            stream.Write(data, 0, data.Length);

            // Receive response
            byte[] response = new byte[1024];
            int bytes = stream.Read(response, 0, response.Length);
            ms = new MemoryStream(response, 0, bytes);
            object result = bf.Deserialize(ms);

            Debug.Log("🔮 Prediction received: " + result);
        }
        catch (Exception e)
        {
            Debug.LogError("❌ Error during communication: " + e.Message);
        }
    }

    void OnApplicationQuit()
    {
        stream?.Close();
        client?.Close();
    }
}