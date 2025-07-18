using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BiomeLoader : MonoBehaviour
{
    public List<Biome> Biomes { get; private set; }

    private void Awake()
    {
        try
        {
            string json = System.IO.File.ReadAllText("Assets/Data/biomes.json");
            BiomeList biomeList = JsonUtility.FromJson<BiomeList>(json);
            Biomes = biomeList.biomes;

            // Output the structured data of biomes
            string biomesJson = JsonUtility.ToJson(Biomes, true);
            UnityEngine.Debug.Log("Biomes Loaded: " + biomesJson);
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError("Failed to load biomes: " + e.Message);
        }
    }
}