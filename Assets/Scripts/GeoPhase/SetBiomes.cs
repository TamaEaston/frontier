using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class SetBiomes
{
    private HexGrid hexGrid;
    private List<Biome> biomes;

    public SetBiomes(HexGrid hexGrid, List<Biome> biomes)
    {
        this.hexGrid = hexGrid;
        this.biomes = biomes;
    }

    public void Execute()
    {

        if (biomes == null)
        {
            UnityEngine.Debug.LogError("Biomes is null in Execute");
        }
        else
        {
            UnityEngine.Debug.Log("Number of biomes in Executre: " + biomes.Count);
        }

        var allHexagons = hexGrid.GetHexagons()
            .Cast<Hexagon>()
            .ToList();

        // Output the structured data of biomes
        string json = System.IO.File.ReadAllText(Application.dataPath + "/Data/biomes.json");
        BiomeList biomeList = JsonUtility.FromJson<BiomeList>(json);

        if (biomeList.biomes == null || biomeList.biomes.Count == 0)
        {
            Debug.LogError("No biomes loaded");
        }
        else
        {
            string biomesJson = JsonUtility.ToJson(biomeList);
            Debug.Log(biomesJson);
        }

        foreach (var hex in allHexagons)
        {
            foreach (var biome in biomes)
            {
                if (biome.HeightAboveSeaLevel.Min <= hex.HeightAboveSeaLevel &&
                    hex.HeightAboveSeaLevel <= biome.HeightAboveSeaLevel.Max &&
                    biome.Temperature.Min <= hex.Temperature &&
                    hex.Temperature <= biome.Temperature.Max &&
                    biome.Rainfall.Min <= hex.Rainfall &&
                    hex.Rainfall <= biome.Rainfall.Max)
                {
                    hex.Biome = biome;
                    // UnityEngine.Debug.Log("Biome set: " + biome.Name);
                    break;
                }
            }
        }

    }
}