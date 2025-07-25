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
            return;
        }

        UnityEngine.Debug.Log($"SetBiomes executing with {biomes.Count} biomes and terrain system");

        var allHexagons = hexGrid.GetHexagons().Cast<Hexagon>().ToList();

        // Process each hexagon with priority-based biome assignment
        foreach (var hex in allHexagons)
        {
            // Skip biome assignment for lakes only - they should be handled by water color system
            // Ocean tiles (HeightAboveSeaLevel <= 0) should get Ocean biome assignment
            if (hex.HeightAboveSeaLevel > 0 && hex.SurfaceWater >= 100)
            {
                hex.Biome = null; // No biome for lakes - will be handled by standardized water colors
            }
            else
            {
                hex.Biome = SelectBiomeWithPriority(hex);
            }
        }

        LogBiomeDistribution(allHexagons);
    }

    /// <summary>
    /// Select biome using priority order system
    /// </summary>
    private Biome SelectBiomeWithPriority(Hexagon hex)
    {
        // Priority 1: Ocean (EXCLUSIVE - any tile at or below sea level is ocean)
        if (hex.HeightAboveSeaLevel <= 0)
        {
            var oceanBiome = GetBiomeByName("Ocean");
            if (oceanBiome != null) return oceanBiome;
            
            // If Ocean biome not found, create a fallback ocean-appropriate biome
            UnityEngine.Debug.LogWarning("Ocean biome not found in biomes.json!");
            return GetFallbackBiome(hex);
        }

        // Note: Since HeightAboveSeaLevel <= 0 is always Ocean, kelp forests and other underwater biomes
        // are now handled exclusively through the Ocean biome. This simplifies the system.

        // Priority 2: Ice Sheet (Temperature override on land)
        if (hex.Temperature < -10)
        {
            var iceSheet = GetBiomeByName("Ice Sheet");  
            if (iceSheet != null && MatchesBiomeConditions(hex, iceSheet))
                return iceSheet;
        }

        // Priority 3: Volcanic (Activity override on land)
        if (hex.VolcanicActivity > 70)
        {
            var volcanicActive = GetBiomeByName("Volcanic Active");
            if (volcanicActive != null && MatchesBiomeConditions(hex, volcanicActive))
                return volcanicActive;
        }
        else if (hex.VolcanicActivity > 20)
        {
            var volcanicDormant = GetBiomeByName("Volcanic Dormant");
            if (volcanicDormant != null && MatchesBiomeConditions(hex, volcanicDormant))
                return volcanicDormant;
        }

        // Priority 4: Swamp (High surface water on low land)
        if (hex.SurfaceWater > 150 && hex.HeightAboveSeaLevel < 50)
        {
            var swamp = GetBiomeByName("Swamp");
            if (swamp != null && MatchesBiomeConditions(hex, swamp))
                return swamp;
        }

        // Priority 5: High altitude + mountainous terrain
        if (hex.HeightAboveSeaLevel > 2000 && hex.TerrainQuartile == 4)
        {
            var mountainBiomes = new[] { "Alpine Tundra", "Snowy Mountains", "Rocky Mountains", "Forested Mountains" };
            foreach (var biomeName in mountainBiomes)
            {
                var biome = GetBiomeByName(biomeName);
                if (biome != null && MatchesBiomeConditions(hex, biome))
                    return biome;
            }
        }

        // Priority 6: Desert conditions (Low rainfall)
        if (hex.Rainfall < 15)
        {
            var desertBiomes = new[] { "Sandy Desert", "Cold Desert", "Cactus Scrubland" };
            foreach (var biomeName in desertBiomes)
            {
                var biome = GetBiomeByName(biomeName);
                if (biome != null && MatchesBiomeConditions(hex, biome))
                    return biome;
            }
        }

        // Priority 7: Coastal areas (land near ocean)
        if (IsNearOcean(hex))
        {
            var coastalBiome = GetBiomeByName("Rocky Shore");
            if (coastalBiome != null && MatchesBiomeConditions(hex, coastalBiome))
                return coastalBiome;
        }

        // Priority 8: General terrain-climate combination
        return GetBestBiomeMatch(hex);
    }

    /// <summary>
    /// Check if all biome conditions are met
    /// </summary>
    private bool MatchesBiomeConditions(Hexagon hex, Biome biome)
    {
        // Check core environmental parameters
        if (!InRange(hex.HeightAboveSeaLevel, biome.HeightAboveSeaLevel)) return false;
        if (!InRange(hex.Temperature, biome.Temperature)) return false;
        if (!InRange(hex.Rainfall, biome.Rainfall)) return false;
        if (!InRange(hex.SurfaceWater, biome.SurfaceWater)) return false;
        
        // Check terrain quartile
        if (biome.Terrain != null && !InRange(hex.TerrainQuartile, biome.Terrain)) return false;
        
        // Check volcanic activity
        if (biome.VolcanicActivity != null && !InRange(hex.VolcanicActivity, biome.VolcanicActivity)) return false;
        
        // Check wind intensity  
        if (biome.WindIntensity != null && !InRange(hex.WindIntensity, biome.WindIntensity)) return false;
        
        // Check ocean proximity
        if (biome.NearOcean && !IsNearOcean(hex)) return false;

        return true;
    }

    /// <summary>
    /// Find best matching biome using scoring system
    /// </summary>
    private Biome GetBestBiomeMatch(Hexagon hex)
    {
        float bestScore = -1f;
        Biome bestBiome = null;

        foreach (var biome in biomes)
        {
            // Skip Ocean biome in general matching (should be handled by priority system)
            if (biome.Name == "Ocean") continue;
            
            // Skip biomes that are completely incompatible with basic hex properties
            if (!IsBasicallyCompatible(hex, biome)) continue;
            
            float score = CalculateBiomeScore(hex, biome);
            if (score > bestScore)
            {
                bestScore = score;
                bestBiome = biome;
            }
        }

        return bestBiome ?? GetFallbackBiome(hex);
    }

    /// <summary>
    /// Check if hex is basically compatible with biome (prevents major mismatches)
    /// </summary>
    private bool IsBasicallyCompatible(Hexagon hex, Biome biome)
    {
        // Don't assign land biomes to ocean (HeightAboveSeaLevel <= 0 is always ocean)
        if (hex.HeightAboveSeaLevel <= 0 && biome.HeightAboveSeaLevel != null && biome.HeightAboveSeaLevel.Min > 0)
            return false;
            
        // Don't assign ocean biomes to land
        if (hex.HeightAboveSeaLevel > 0 && biome.HeightAboveSeaLevel != null && biome.HeightAboveSeaLevel.Max <= 0)
            return false;
            
        return true;
    }

    /// <summary>
    /// Calculate how well a hex matches a biome (0-1 score)
    /// </summary>
    private float CalculateBiomeScore(Hexagon hex, Biome biome)
    {
        float score = 0f;
        int parameterCount = 0;

        // Core parameters (always present)
        score += GetRangeScore(hex.HeightAboveSeaLevel, biome.HeightAboveSeaLevel);
        score += GetRangeScore(hex.Temperature, biome.Temperature);
        score += GetRangeScore(hex.Rainfall, biome.Rainfall);
        score += GetRangeScore(hex.SurfaceWater, biome.SurfaceWater);
        parameterCount += 4;

        // Optional parameters
        if (biome.Terrain != null)
        {
            score += GetRangeScore(hex.TerrainQuartile, biome.Terrain);
            parameterCount++;
        }

        if (biome.VolcanicActivity != null)
        {
            score += GetRangeScore(hex.VolcanicActivity, biome.VolcanicActivity);
            parameterCount++;
        }

        if (biome.WindIntensity != null)
        {
            score += GetRangeScore(hex.WindIntensity, biome.WindIntensity);
            parameterCount++;
        }

        if (biome.NearOcean)
        {
            score += IsNearOcean(hex) ? 1f : 0f;
            parameterCount++;
        }

        return parameterCount > 0 ? score / parameterCount : 0f;
    }

    /// <summary>
    /// Calculate how well a value fits within a range (0-1 score)
    /// </summary>
    private float GetRangeScore(float value, Range range)
    {
        if (range == null) return 1f; // No constraint = perfect match
        
        if (value >= range.Min && value <= range.Max) return 1f; // Perfect match
        
        // Calculate distance from range
        float distanceFromRange;
        if (value < range.Min)
            distanceFromRange = range.Min - value;
        else
            distanceFromRange = value - range.Max;
        
        // Convert distance to score (closer = higher score)
        float rangeSize = range.Max - range.Min;
        if (rangeSize <= 0) return value == range.Min ? 1f : 0f;
        
        return Mathf.Max(0f, 1f - (distanceFromRange / rangeSize));
    }

    /// <summary>
    /// Check if value is within range
    /// </summary>
    private bool InRange(float value, Range range)
    {
        return range == null || (value >= range.Min && value <= range.Max);
    }

    /// <summary>
    /// Detect if hex is near ocean for coastal biomes
    /// </summary>
    private bool IsNearOcean(Hexagon hex)
    {
        if (hex.Neighbours == null) return false;
        
        // Check if any neighbor is underwater (ocean)
        foreach (var neighbor in hex.Neighbours)
        {
            if (neighbor != null && neighbor.AltitudeVsSeaLevel < 0)
                return true;
        }
        
        return false;
    }

    /// <summary>
    /// Get biome by name
    /// </summary>
    private Biome GetBiomeByName(string name)
    {
        return biomes?.FirstOrDefault(b => b.Name == name);
    }

    /// <summary>
    /// Get fallback biome (Shrubland)
    /// </summary>
    private Biome GetFallbackBiome(Hexagon hex)
    {
        return GetBiomeByName("Shrubland") ?? biomes?.FirstOrDefault();
    }

    /// <summary>
    /// Log biome distribution for debugging
    /// </summary>
    private void LogBiomeDistribution(List<Hexagon> hexagons)
    {
        var distribution = hexagons
            .Where(h => h.Biome != null)
            .GroupBy(h => h.Biome.Name)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .ToList();

        UnityEngine.Debug.Log($"Top 10 Biome Distribution (Total: {hexagons.Count} hexes):");
        foreach (var group in distribution)
        {
            float percentage = (group.Count() / (float)hexagons.Count) * 100f;
            UnityEngine.Debug.Log($"  {group.Key}: {group.Count()} hexes ({percentage:F1}%)");
        }
    }
}