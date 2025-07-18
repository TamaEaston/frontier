using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class RiverFlow
{
    private HexGrid hexGrid;
    private float seaLevel;

    public RiverFlow(HexGrid hexGrid, float seaLevel)
    {
        this.hexGrid = hexGrid;
        this.seaLevel = seaLevel;
    }

    public void Execute()
    {
        // Flatten the 2D array into a 1D list and order hexagons by altitude from highest to lowest
        var orderedHexagons = hexGrid.GetHexagons()
            .Cast<Hexagon>()
            // .Where(hex => hex.Altitude > seaLevel)
            .OrderByDescending(hex => hex.Altitude)
            .ToList();

        foreach (var hex in orderedHexagons)
        {
            hex.AltitudeNew = hex.Altitude;
            hex.SurfaceWaterNew = 0;
            hex.RiverWidth = 0;
        }

        foreach (var hex in orderedHexagons)
        {
            //Wind Direction
            float oppositeWindDirection = (hex.WindDirection + 180) % 360;
            Hexagon windComesFrom = hex.Neighbours[(int)Mathf.Round(oppositeWindDirection / 60f) % 6];
            float windAltitudeChange = (windComesFrom != null) ? hex.HeightAboveSeaLevel - windComesFrom.HeightAboveSeaLevel : 0;

            //Calculate Rainfall
            float eastNeighbourWindVapour = (hex.Neighbours[0] != null) ? (hex.Neighbours[0].WaterVapour + hex.Neighbours[0].WindIntensity) / 2 : 0;
            float heightAboveSeaLevel = Math.Min(0, hex.Altitude - seaLevel);
            hex.Rainfall = (hex.WaterVapour + hex.WindIntensity + (Math.Max(0, windAltitudeChange) / 10) + eastNeighbourWindVapour + (heightAboveSeaLevel / 10)) / 8;

            hex.SurfaceWaterNew += hex.Rainfall;
            hex.RiverWidth += hex.SurfaceWaterNew;

            // Order neighbouring hexagons by altitude from lowest to highest
            var lowestNeighbour = hex.Neighbours
                .Where(neighbour => neighbour != null)
                .OrderBy(neighbour => neighbour.AltitudeWithRiverWidth)
                .FirstOrDefault();

            // If lowestNeighbour is not null and its altitude is less than the altitude of the hex
            if (lowestNeighbour != null && lowestNeighbour.Altitude < hex.Altitude)
            {
                // Calculate the remaining capacity of hex.lowestNeighbour.SurfaceWaterNew
                float remainingCapacity = (lowestNeighbour.Altitude <= seaLevel) ? 999999 : 300 - lowestNeighbour.SurfaceWaterNew;

                // Calculate DownwardFlow as the minimum of hex.SurfaceWaterNew and remainingCapacity
                float DownwardFlow = Mathf.Min(hex.SurfaceWaterNew, remainingCapacity);

                // Debug.Log("Remaining Capacity: " + remainingCapacity + " Downward Flow: " + DownwardFlow + " River Width: " + hex.RiverWidth);

                //Glaciers double the erosion rate.
                float DownwardErosion = (hex.Temperature < -5) ? hex.SurfaceWaterNew : (hex.SurfaceWaterNew / 2);

                lowestNeighbour.SurfaceWaterNew += DownwardFlow;
                lowestNeighbour.Altitude += DownwardErosion;

                hex.SurfaceWaterNew -= DownwardFlow;
                hex.Altitude -= DownwardErosion;
                hex.RiverWidth = DownwardFlow + lowestNeighbour.SurfaceWaterNew;
            }
            else
            {
                lowestNeighbour = null;
            }

            hex.LowestNeighbour = lowestNeighbour;
        }

        foreach (var hex in orderedHexagons)
        {
            hex.Altitude = hex.AltitudeNew;
            hex.SurfaceWater = hex.SurfaceWaterNew;
        }

    }
}