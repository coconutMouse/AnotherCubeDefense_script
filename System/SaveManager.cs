using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveManager
{
    public static void Save<T>(string savePath, T obj)
    {
        {
            using (FileStream stream = new FileStream(savePath, FileMode.Create))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
                stream.Close();
            }
        }
    }
    public static T Load<T>(string savePath)
    {
        if (File.Exists(savePath))
        {
            using (FileStream stream = new FileStream(savePath, FileMode.Open))
            {
                var formatter = new BinaryFormatter();
                T obj = (T)formatter.Deserialize(stream);
                stream.Close();
                return obj;
            }
        }
        else Debug.Log("File at " + savePath + " does not exist.");
        return default(T);
    }
}