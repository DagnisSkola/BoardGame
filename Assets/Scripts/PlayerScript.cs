using UnityEngine;
using System.IO;

public class PlayerScript : MonoBehaviour
{
    public GameObject[] playerPrefabs;
    int characterIndex;
    public GameObject spawnPoint;
    int[] otherPlayers;
    int index;
    private const string textFileName = "PlayerNames";

    void Start()
    {
        characterIndex = PlayerPrefs.GetInt("SelectedCharacter", 0);
        GameObject mainCharacter = Instantiate(playerPrefabs[characterIndex], spawnPoint.transform.position, Quaternion.identity);
        mainCharacter.GetComponent<NameScript>().SetName(PlayerPrefs.GetString("PlayerName", "John Doe"));

        otherPlayers = new int[PlayerPrefs.GetInt("PlayerCount")];
        string[] nameArray = ReadLinesFromFile(textFileName);

        for(int i=0; i<otherPlayers.Length-1; i++)
        {
            spawnPoint.transform.position += new Vector3(0.2f, 0, 0.08f);
            index = Random.Range(0, playerPrefabs.Length);
            GameObject otherPlayer = Instantiate(playerPrefabs[index], spawnPoint.transform.position, Quaternion.identity);
            otherPlayer.GetComponent<NameScript>().SetName(nameArray[Random.Range(0, nameArray.Length)]);
        }
    }

    string[] ReadLinesFromFile(string fileName)
    {
        TextAsset textAsset = Resources.Load<TextAsset>(fileName);

        if(textAsset != null)
        {
            return textAsset.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        }
        else
        {
            Debug.LogWarning("File not found: " + fileName);
            return new string[0];
        }
    }

}
