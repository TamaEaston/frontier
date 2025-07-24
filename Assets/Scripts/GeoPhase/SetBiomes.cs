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
            // Clear existing biome assignment
            hex.Biome = null;
            
            // Find matching biome with terrain quartile consideration
            List<Biome> matchingBiomes = new List<Biome>();
            
            foreach (var biome in biomes)
            {
                if (biome.HeightAboveSeaLevel.Min <= hex.HeightAboveSeaLevel &&
                    hex.HeightAboveSeaLevel <= biome.HeightAboveSeaLevel.Max &&
                    biome.Temperature.Min <= hex.Temperature &&
                    hex.Temperature <= biome.Temperature.Max &&
                    biome.Rainfall.Min <= hex.Rainfall &&
                    hex.Rainfall <= biome.Rainfall.Max &&
                    biome.SurfaceWater.Min <= hex.SurfaceWater &&
                    hex.SurfaceWater <= biome.SurfaceWater.Max)
                {
                    matchingBiomes.Add(biome);
                }
            }
            
            // Select best biome based on terrain quartile preferences
            if (matchingBiomes.Count > 0)
            {
                hex.Biome = SelectBiomeByTerrain(matchingBiomes, hex);
            }
        }

    }

    private Biome SelectBiomeByTerrain(List<Biome> matchingBiomes, Hexagon hex)
    {
        // Define terrain preferences for different biome types
        Dictionary<string, int[]> terrainPreferences = new Dictionary<string, int[]>()
        {
            // Flat terrain preferred (Q1-Q2)
            {"Sandy Desert", new int[] {1, 2}},
            {"Grassland", new int[] {1, 2}},
            {"Prairie", new int[] {1, 2}},
            {"Steppe", new int[] {1, 2}},
            {"Plains", new int[] {1, 2}},
            {"Savanna", new int[] {1, 2}},
            {"Farmland", new int[] {1, 2}},
            {"Agricultural", new int[] {1, 2}},
            
            // Rolling terrain preferred (Q2-Q3)
            {"Forest", new int[] {2, 3}},
            {"Deciduous Forest", new int[] {2, 3}},
            {"Boreal Forest", new int[] {2, 3}},
            {"Woodland", new int[] {2, 3}},
            {"Hills", new int[] {2, 3}},
            {"Rolling Hills", new int[] {2, 3}},
            
            // Mountainous terrain preferred (Q3-Q4)
            {"Rocky Desert", new int[] {3, 4}},
            {"Badlands", new int[] {3, 4}},
            {"Highland", new int[] {3, 4}},
            {"Mountain", new int[] {3, 4}},
            {"Alpine", new int[] {4}},
            {"Cliff", new int[] {4}},
            {"Peak", new int[] {4}},
            
            // Flat wetlands (Q1)
            {"Swamp", new int[] {1}},
            {"Marsh", new int[] {1}},
            {"Wetland", new int[] {1}},
            {"Floodplain", new int[] {1}}
        };
        
        // Find biomes that match terrain preferences
        var preferredBiomes = matchingBiomes.Where(biome => 
        {
            if (terrainPreferences.ContainsKey(biome.Name))
            {
                return terrainPreferences[biome.Name].Contains(hex.TerrainQuartile);
            }
            return true; // If no preference defined, include all biomes
        }).ToList();
        
        // Return preferred biome if available, otherwise fallback to first matching biome
        return preferredBiomes.Count > 0 ? preferredBiomes[0] : matchingBiomes[0];
    }
}