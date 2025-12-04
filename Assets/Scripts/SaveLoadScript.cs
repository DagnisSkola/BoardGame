using UnityEngine;
using System.IO;
using System;

public class SaveLoadScript : MonoBehaviour
{
    public string saveFileName = "mansFails.json";

    [Serializable]
    public class GameData
    {
        public int character;
        public string characterName;
        // Add other game state fields here
    }

    private GameData gameData = new GameData();

    public void SaveGame(int character, string characterName)
    {
        gameData.character = character;
        gameData.characterName = characterName;
        string json = JsonUtility.ToJson(gameData);

        File.WriteAllText(Application.persistentDataPath + "/" + saveFileName, json);
        Debug.Log("Game saved to " + Application.persistentDataPath + "/" + saveFileName);
    }

    public void LoadGame()
    {
        string filePath = Application.persistentDataPath + "/" + saveFileName;

        if(File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            gameData = JsonUtility.FromJson<GameData>(json);

            //Use gamedata to restore game state
        }
        else
        {
            Debug.LogWarning("Save file not found: " + filePath);
        }
    }
}
