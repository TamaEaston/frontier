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
        var allHexagons = hexGrid.GetHexagons()
            .Cast<Hexagon>()
            .ToList();
        float averageAltitude = allHexagons.Average(hex => hex.Altitude);

        hexGrid.SeaLevel = averageAltitude + (hexGrid.SeaPerHex + hexGrid.AverageGlobalTemperature * 10);

    }
}