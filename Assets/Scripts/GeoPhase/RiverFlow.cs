using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RiverFlow
{
    private HexGrid hexGrid;
    private float seaLevel;
    
    // Geomorphological parameters
    private const float MIN_RIVER_WIDTH_FOR_CONFLUENCE = 15f;
    private const float EROSION_RATE_NORMAL = 0.5f;
    private const float EROSION_RATE_GLACIAL = 1.0f;
    private const float SEDIMENT_DEPOSITION_RATE = 0.3f;
    private const int MAX_ITERATIONS = 3;

    public RiverFlow(HexGrid hexGrid, float seaLevel)
    {
        this.hexGrid = hexGrid;
        this.seaLevel = seaLevel;
    }

    public void Execute()
    {
        // Iterative geomorphological simulation
        for (int iteration = 0; iteration < MAX_ITERATIONS; iteration++)
        {
            // Phase 1: Map natural drainage patterns (highest to lowest)
            MapInitialDrainageNetwork();
            
            // Phase 2: Process confluence from bottom-up (lowest to highest)  
            ProcessConfluenceFromBottom();
            
            // Phase 3: Simulate erosion and sediment transport
            SimulateErosionAndSedimentation();
            
            // Debug.Log($"RiverFlow iteration {iteration + 1} completed");
        }
    }

    /// <summary>
    /// Phase 1: Map initial drainage patterns from highest to lowest terrain
    /// Sets up the basic flow network based on gravity and topography
    /// </summary>
    private void MapInitialDrainageNetwork()
    {
        var orderedHexagons = hexGrid.GetHexagons()
            .Cast<Hexagon>()
            .OrderByDescending(hex => hex.Altitude)
            .ToList();

        // Initialize all hexes
        foreach (var hex in orderedHexagons)
        {
            hex.AltitudeNew = hex.Altitude;
            hex.SurfaceWaterNew = 0;
            hex.RiverWidth = 0;
        }

        // Calculate rainfall and initial flow directions
        foreach (var hex in orderedHexagons)
        {
            CalculateRainfall(hex);
            hex.SurfaceWaterNew += hex.Rainfall;
            
            // Find natural drainage direction (steepest descent)
            var lowestNeighbour = FindSteepestDescentNeighbour(hex);
            hex.LowestNeighbour = lowestNeighbour;
            
            // Set initial river width based on rainfall
            hex.RiverWidth = hex.SurfaceWaterNew;
        }
    }

    /// <summary>
    /// Phase 2: Process confluence from lowest to highest altitude
    /// This simulates how water naturally accumulates in drainage basins
    /// </summary>
    private void ProcessConfluenceFromBottom()
    {
        var orderedHexagons = hexGrid.GetHexagons()
            .Cast<Hexagon>()
            .Where(hex => hex.RiverWidth > 0)
            .OrderBy(hex => hex.Altitude) // Process from LOWEST to HIGHEST
            .ToList();

        var processedHexes = new HashSet<Hexagon>();

        foreach (var currentHex in orderedHexagons)
        {
            if (processedHexes.Contains(currentHex)) continue;

            // Find all higher neighbors with significant river flow
            var tributaries = FindTributaryRivers(currentHex, processedHexes);
            
            if (tributaries.Count > 0)
            {
                // Merge tributary flows into current hex
                MergeTributaries(currentHex, tributaries);
                
                // Mark processed to prevent double-processing
                processedHexes.Add(currentHex);
                foreach (var tributary in tributaries)
                {
                    processedHexes.Add(tributary);
                }
            }
        }
    }

    /// <summary>
    /// Phase 3: Simulate erosion and sediment transport
    /// This reshapes the terrain based on water flow patterns
    /// </summary>
    private void SimulateErosionAndSedimentation()
    {
        var allHexagons = hexGrid.GetHexagons().Cast<Hexagon>().ToList();

        foreach (var hex in allHexagons)
        {
            if (hex.LowestNeighbour != null && hex.RiverWidth > 0)
            {
                // Calculate erosion based on flow volume and gradient
                float altitudeDifference = hex.Altitude - hex.LowestNeighbour.Altitude;
                float baseErosion = hex.RiverWidth * EROSION_RATE_NORMAL;
                
                // Glacial erosion bonus
                if (hex.Temperature < -5f)
                {
                    baseErosion = hex.RiverWidth * EROSION_RATE_GLACIAL;
                }
                
                // Gradient factor (steeper = more erosion)
                float gradientFactor = Mathf.Clamp(altitudeDifference / 100f, 0.1f, 2.0f);
                float erosionAmount = baseErosion * gradientFactor;
                
                // Apply erosion with limits to prevent extreme changes
                erosionAmount = Mathf.Min(erosionAmount, hex.Altitude * 0.05f); // Max 5% of hex altitude
                
                // Erode source
                hex.AltitudeNew -= erosionAmount;
                
                // Deposit sediment downstream (partial deposition)
                float sedimentDeposited = erosionAmount * SEDIMENT_DEPOSITION_RATE;
                hex.LowestNeighbour.AltitudeNew += sedimentDeposited;
                
                // Flow water downstream and calculate final river width
                float waterFlow = Mathf.Min(hex.SurfaceWaterNew, hex.LowestNeighbour.Altitude <= seaLevel ? 999999 : 300);
                hex.LowestNeighbour.SurfaceWaterNew += waterFlow;
                hex.SurfaceWaterNew -= waterFlow;
                
                // River width represents water flowing through this hex
                hex.RiverWidth = waterFlow;
            }
        }

        // Apply terrain changes
        foreach (var hex in allHexagons)
        {
            hex.Altitude = hex.AltitudeNew;
            hex.SurfaceWater = hex.SurfaceWaterNew;
        }
    }

    /// <summary>
    /// Calculate rainfall for a hex based on wind and weather patterns
    /// </summary>
    private void CalculateRainfall(Hexagon hex)
    {
        // Wind direction analysis
        float oppositeWindDirection = (hex.WindDirection + 180) % 360;
        Hexagon windComesFrom = hex.Neighbours[(int)Mathf.Round(oppositeWindDirection / 60f) % 6];
        float windAltitudeChange = (windComesFrom != null) ? hex.HeightAboveSeaLevel - windComesFrom.HeightAboveSeaLevel : 0;

        // Eastern neighbor moisture contribution
        float eastNeighbourWindVapour = (hex.Neighbours[0] != null) ? 
            (hex.Neighbours[0].WaterVapour + hex.Neighbours[0].WindIntensity) / 2 : 0;
            
        // Depth below sea level (negative for underwater)
        float depthBelowSeaLevel = Math.Min(0, hex.Altitude - seaLevel);
        
        // Calculate rainfall from multiple weather factors
        hex.Rainfall = (hex.WaterVapour + hex.WindIntensity + 
                       (Math.Max(0, windAltitudeChange) / 10) + 
                       eastNeighbourWindVapour + 
                       (depthBelowSeaLevel / 10)) / 8;
    }

    /// <summary>
    /// Find the steepest descent neighbor for natural water flow
    /// </summary>
    private Hexagon FindSteepestDescentNeighbour(Hexagon hex)
    {
        return hex.Neighbours
            .Where(neighbour => neighbour != null && neighbour.Altitude < hex.Altitude)
            .OrderBy(neighbour => neighbour.AltitudeWithRiverWidth)
            .FirstOrDefault();
    }

    /// <summary>
    /// Find tributary rivers that should flow into the current hex
    /// Only considers direct neighbors with parallel flow directions
    /// </summary>
    private List<Hexagon> FindTributaryRivers(Hexagon currentHex, HashSet<Hexagon> processedHexes)
    {
        var tributaries = new List<Hexagon>();
        var currentFlowDirection = GetFlowDirection(currentHex);

        // Check all neighbors for potential tributaries
        for (int i = 0; i < 6; i++)
        {
            var neighbor = currentHex.Neighbours[i];
            
            if (neighbor == null || processedHexes.Contains(neighbor)) continue;
            if (neighbor.Altitude <= currentHex.Altitude) continue; // Must be higher
            if (neighbor.RiverWidth < MIN_RIVER_WIDTH_FOR_CONFLUENCE) continue;

            var neighborFlowDirection = GetFlowDirection(neighbor);
            
            // Check if neighbor flows in parallel direction to current hex
            if (AreFlowsParallel(currentFlowDirection, neighborFlowDirection))
            {
                tributaries.Add(neighbor);
            }
            
            // Also check if neighbor flows directly into current hex
            if (neighbor.LowestNeighbour == currentHex)
            {
                tributaries.Add(neighbor);
            }
        }

        return tributaries.Distinct().ToList();
    }

    /// <summary>
    /// Merge tributary flows into the main river
    /// </summary>
    private void MergeTributaries(Hexagon mainRiver, List<Hexagon> tributaries)
    {
        foreach (var tributary in tributaries)
        {
            // Redirect tributary to flow into main river (if adjacent)
            if (mainRiver.Neighbours.Contains(tributary))
            {
                tributary.LowestNeighbour = mainRiver;
                
                // Combine flow volumes (80% transfer to simulate realistic confluence)
                float transferredFlow = tributary.RiverWidth * 0.8f;
                mainRiver.RiverWidth += transferredFlow;
                tributary.RiverWidth *= 0.2f; // Tributary keeps 20% of original flow
                
                // Debug.Log($"Merged tributary at {tributary.HexagonID} into main river at {mainRiver.HexagonID}");
            }
        }
    }

    /// <summary>
    /// Get the flow direction index for a hex (which neighbor it flows to)
    /// </summary>
    private int GetFlowDirection(Hexagon hex)
    {
        if (hex.LowestNeighbour == null) return -1;

        for (int i = 0; i < hex.Neighbours.Length; i++)
        {
            if (hex.Neighbours[i] == hex.LowestNeighbour)
                return i;
        }
        return -1;
    }

    /// <summary>
    /// Check if two flow directions are parallel (same or adjacent directions)
    /// </summary>
    private bool AreFlowsParallel(int direction1, int direction2)
    {
        if (direction1 == -1 || direction2 == -1) return false;
        if (direction1 == direction2) return true; // Same direction

        // Adjacent directions in hexagonal grid (including wrap-around)
        int diff = Mathf.Abs(direction1 - direction2);
        return diff == 1 || diff == 5;
    }
}