using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class Slump
{
    private HexGrid hexGrid;

    public Slump(HexGrid hexGrid)
    {
        this.hexGrid = hexGrid;
    }

    public void Execute()
    {
        // Flatten the 2D array into a 1D list and order hexagons by altitude from highest to lowest
        var orderedHexagons = hexGrid.GetHexagons()
            .Cast<Hexagon>()
            .OrderByDescending(hex => hex.Altitude)
            .ToList();

        foreach (var hex in orderedHexagons)
        {
            hex.AltitudeNew = hex.Altitude;
        }

        foreach (var hex in orderedHexagons)
        {
            // Order neighbouring hexagons by altitude from highest to lowest
            var orderedNeighbours = hex.Neighbours
                .Where(neighbour => neighbour != null)
                .OrderByDescending(neighbour => neighbour.Altitude)
                .ToList();

            foreach (var neighbour in orderedNeighbours)
            {
                // Calculate the difference in altitude
                float altitudeDifference = hex.Altitude - neighbour.Altitude;

                if (altitudeDifference > 0)
                {
                    // Take 2.5% of the difference off the higher hexagon and add it to the lower one
                    float transfer = altitudeDifference * 0.025f;
                    hex.AltitudeNew -= transfer;
                    neighbour.AltitudeNew += transfer;
                    //                    Debug.Log(hex.Altitude + " " + altitudeDifference + " - " + transfer);
                }
            }
        }

        foreach (var hex in orderedHexagons)
        {
            hex.Altitude = hex.AltitudeNew;
        }

    }
}