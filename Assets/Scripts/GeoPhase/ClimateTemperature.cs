using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class ClimateTemperature
{
    private HexGrid hexGrid;

    public ClimateTemperature(HexGrid hexGrid)
    {
        this.hexGrid = hexGrid;
    }

    public void Execute()
    {
        float TemperatureChange = 0.0f;

        switch (GameSettings.ClimateMode)
        {
            case "Cooling":
                TemperatureChange = -0.5f;
                break;
            case "Warming":
                TemperatureChange = 0.5f;
                break;
            default:
                TemperatureChange = 0f; // Default value
                break;
        }
        hexGrid.EquatorSolarIntensity = hexGrid.EquatorSolarIntensity + TemperatureChange;

        var allHexagons = hexGrid.GetHexagons().Cast<Hexagon>().ToList();

        foreach (var hex in allHexagons)
        {
            // Calculate solar intensity
            float latitude = Mathf.Clamp(2.0f * hex.PositionY / (hexGrid.Height - 1) - 1.0f, -1.0f, 1.0f);
            float power = 0.6f;
            float sinValue = Mathf.Sin((latitude + 1) * Mathf.PI / 2.0f);
            hex.SolarIntensity = (hexGrid.EquatorSolarIntensity - hexGrid.EquatorToPolarSolarIntensityDifference) + hexGrid.EquatorToPolarSolarIntensityDifference * Mathf.Pow(Mathf.Abs(sinValue), power);
        }
    }
}