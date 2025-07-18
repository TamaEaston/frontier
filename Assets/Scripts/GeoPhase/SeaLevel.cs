using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class SeaLevel
{
    private HexGrid hexGrid;

    public SeaLevel(HexGrid hexGrid)
    {
        this.hexGrid = hexGrid;
    }

    public void Execute()
    {
        // Fixed sea level for Frontier continental system
        // Sea level is always maintained at 10000m for consistent continental boundaries
        hexGrid.SeaLevel = 10000f;

        // Note: Original dynamic calculation disabled for Frontier
        // var allHexagons = hexGrid.GetHexagons()
        //     .Cast<Hexagon>()
        //     .ToList();
        // float averageAltitude = allHexagons.Average(hex => hex.Altitude);
        // hexGrid.SeaLevel = averageAltitude + (hexGrid.SeaPerHex + hexGrid.AverageGlobalTemperature * 10);
    }
}