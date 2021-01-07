using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SavingSystem
{
    public static void SaveGame(GameManager gm, ItemsList il)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/city.spook";
        FileStream stream = new FileStream(path, FileMode.Create);
        
        SavingData data = new SavingData(gm, il);
        
        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static SavingData LoadGame()
    {
        string path = Application.persistentDataPath + "/city.spook";
        if (File.Exists(path))
        {
            // save exists
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            
            SavingData data = formatter.Deserialize(stream) as SavingData;
            stream.Close();
            
            return data;
        }
        else
        {
            // no save
            return null;
        }
    }
}
