using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TerrainAnalysis
{
    private HexGrid hexGrid;

    public TerrainAnalysis(HexGrid hexGrid)
    {
        this.hexGrid = hexGrid;
    }

    public void Execute()
    {
        Debug.Log("TerrainAnalysis has been triggered");
        var allHexagons = hexGrid.GetHexagons().Cast<Hexagon>().ToList();

        // Step 1: Calculate terrain roughness for each hexagon
        foreach (var hex in allHexagons)
        {
            hex.CalculateTerrainRoughness();
        }

        // Step 2: Determine quartiles and assign terrain quartile values
        AssignTerrainQuartiles(allHexagons);
    }

    private void AssignTerrainQuartiles(List<Hexagon> hexagons)
    {
        // Get all terrain roughness values for quartile calculation
        var terrainRoughnessValues = hexagons
            .Select(hex => hex.TerrainRoughness)
            .OrderBy(value => value)
            .ToList();

        if (terrainRoughnessValues.Count == 0)
        {
            Debug.LogWarning("No hexagons found for terrain quartile calculation");
            return;
        }

        // Calculate quartile thresholds
        int count = terrainRoughnessValues.Count;
        float q1Threshold = terrainRoughnessValues[count / 4];
        float q2Threshold = terrainRoughnessValues[count / 2];
        float q3Threshold = terrainRoughnessValues[(3 * count) / 4];

        Debug.Log($"Terrain Quartile Thresholds - Q1: {q1Threshold:F2}, Q2: {q2Threshold:F2}, Q3: {q3Threshold:F2}");

        // Assign quartiles to each hexagon
        foreach (var hex in hexagons)
        {
            int quartile;
            if (hex.TerrainRoughness <= q1Threshold)
                quartile = 1; // Flat terrain
            else if (hex.TerrainRoughness <= q2Threshold)
                quartile = 2; // Rolling terrain
            else if (hex.TerrainRoughness <= q3Threshold)
                quartile = 3; // Hilly terrain
            else
                quartile = 4; // Mountainous terrain

            hex.SetTerrainQuartile(quartile);
        }

        // Log quartile distribution for debugging
        var quartileCounts = hexagons.GroupBy(h => h.TerrainQuartile)
            .ToDictionary(g => g.Key, g => g.Count());

        Debug.Log($"Terrain Quartile Distribution - Q1 (Flat): {quartileCounts.GetValueOrDefault(1, 0)}, " +
                  $"Q2 (Rolling): {quartileCounts.GetValueOrDefault(2, 0)}, " +
                  $"Q3 (Hilly): {quartileCounts.GetValueOrDefault(3, 0)}, " +
                  $"Q4 (Mountainous): {quartileCounts.GetValueOrDefault(4, 0)}");
    }
}