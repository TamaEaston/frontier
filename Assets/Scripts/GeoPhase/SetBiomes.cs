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
            hex.Biome = SelectBiomeWithPriority(hex);
        }

        LogBiomeDistribution(allHexagons);
    }

    /// <summary>
    /// Select biome using priority order system
    /// </summary>
    private Biome SelectBiomeWithPriority(Hexagon hex)
    {
        // Priority 1: Deep Ocean
        if (hex.HeightAboveSeaLevel < -30)
        {
            return GetBiomeByName("Ocean") ?? GetFallbackBiome(hex);
        }

        // Priority 2: Dense Kelp Forest (Rocky underwater areas near coast)
        if (hex.AltitudeVsSeaLevel >= -30 && hex.AltitudeVsSeaLevel <= -5 && 
            IsNearOcean(hex) && hex.TerrainQuartile >= 3)
        {
            var kelpBiome = GetBiomeByName("Coastal Kelp Forest");
            if (kelpBiome != null && MatchesBiomeConditions(hex, kelpBiome))
                return kelpBiome;
        }

        // Priority 3: Shallow Coastal Kelp (Rocky coastal waters)
        if (hex.AltitudeVsSeaLevel >= -30 && hex.AltitudeVsSeaLevel <= -2 && IsNearOcean(hex))
        {
            var coastalKelp = GetBiomeByName("Coastal Kelp Forest");
            if (coastalKelp != null && MatchesBiomeConditions(hex, coastalKelp))
                return coastalKelp;
        }

        // Priority 4: Ice Sheet (Temperature override)
        if (hex.Temperature < -10)
        {
            var iceSheet = GetBiomeByName("Ice Sheet");  
            if (iceSheet != null && MatchesBiomeConditions(hex, iceSheet))
                return iceSheet;
        }

        // Priority 5: Volcanic (Activity override)
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

        // Priority 6: Swamp (High surface water on low land)
        if (hex.SurfaceWater > 150 && hex.HeightAboveSeaLevel < 50)
        {
            var swamp = GetBiomeByName("Swamp");
            if (swamp != null && MatchesBiomeConditions(hex, swamp))
                return swamp;
        }

        // Priority 7: High altitude + mountainous terrain
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

        // Priority 8: Desert conditions (Low rainfall)
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

        // Priority 9: Coastal areas
        if (IsNearOcean(hex))
        {
            var coastalBiome = GetBiomeByName("Rocky Shore");
            if (coastalBiome != null && MatchesBiomeConditions(hex, coastalBiome))
                return coastalBiome;
        }

        // Priority 10: High wind areas
        if (hex.WindIntensity > 70)
        {
            var stormyBiome = GetBiomeByName("Stormy Temperate");
            if (stormyBiome != null && MatchesBiomeConditions(hex, stormyBiome))
                return stormyBiome;
        }

        // Priority 11: General terrain-climate combination
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