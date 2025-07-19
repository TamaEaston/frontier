using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class ClimateTemperature
{
    private HexGrid hexGrid;
    private readonly bool isNorthArctic;

    public ClimateTemperature(HexGrid hexGrid)
    {
        this.hexGrid = hexGrid;
        // Use the fixed arctic assignment from HexGrid (set once at map generation)
        isNorthArctic = hexGrid.IsNorthArctic;
    }

    public void Execute()
    {
        float temperatureChange = GameSettings.ClimateMode switch
        {
            "Cooling" => -0.5f,
            "Warming" => 0.5f,
            _ => 0f
        };
        hexGrid.EquatorSolarIntensity += temperatureChange;

        var allHexagons = hexGrid.GetHexagons().Cast<Hexagon>().ToList();

        // Continental temperature system
        const float arcticBaseTemp = -5f;
        const float desertBaseTemp = 40f;
        
        foreach (var hex in allHexagons)
        {
            // Calculate north-south position (0.0 = north edge, 1.0 = south edge)
            float northSouthPosition = (float)hex.PositionY / (hexGrid.Height - 1);
            
            // Determine temperature based on which end is arctic
            float baseTemp;
            if (isNorthArctic)
            {
                // North is arctic, South is desert
                baseTemp = Mathf.Lerp(arcticBaseTemp, desertBaseTemp, northSouthPosition);
            }
            else
            {
                // South is arctic, North is desert
                baseTemp = Mathf.Lerp(desertBaseTemp, arcticBaseTemp, northSouthPosition);
            }
            
            // Apply global climate modifier (EquatorSolarIntensity change)
            float finalTemp = baseTemp + temperatureChange;
            
            // Set both SolarIntensity (for backward compatibility) and direct temperature
            hex.SolarIntensity = finalTemp;
            hex.TemperatureNoWind = finalTemp;
        }
    }
}