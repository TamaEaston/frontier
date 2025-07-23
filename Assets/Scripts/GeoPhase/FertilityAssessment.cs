using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FertilityAssessment
{
    private HexGrid hexGrid;
    int temperatureSweetSpotMin = 5;
    int temperatureSweetSpotMax = 30;
    int altitudeSweetSpotMin = 0;
    int altitudeSweetSpotMax = 800;
    int rainfallSweetSpotMin = 20;
    int rainfallSweetSpotMax = 40;
    int riverWidthSweetSpotMin = 50;
    int riverWidthSweetSpotMax = 300;

    public FertilityAssessment(HexGrid hexGrid)
    {
        this.hexGrid = hexGrid;
    }

    public void Execute()
    {
        Debug.Log("FertilityAssessment has been triggered");
        var allHexagons = hexGrid.GetHexagons().Cast<Hexagon>().ToList();

        foreach (var hex in allHexagons)
        {
            // Exclusion criteria: only underwater hexes
            if (hex.HeightAboveSeaLevel < 0)
            {
                hex.Fertility = 0;
            }
            else
            {
                // Calculate individual scores (0-10 each)
                float temperatureScore = CalculateScore(hex.Temperature, temperatureSweetSpotMin, temperatureSweetSpotMax, 2);
                float altitudeScore = CalculateScore(hex.HeightAboveSeaLevel, altitudeSweetSpotMin, altitudeSweetSpotMax, 2);
                float rainfallScore = CalculateScore(hex.Rainfall, rainfallSweetSpotMin, rainfallSweetSpotMax);
                float riverWidthScore = CalculateScore(hex.RiverWidth, riverWidthSweetSpotMin, riverWidthSweetSpotMax);
                
                // Water: Take the BETTER of rainfall or rivers (not both)
                float waterScore = Mathf.Max(rainfallScore, riverWidthScore);
                
                // Multiplicative calculation: ALL factors must be decent
                // Convert 0-10 scores to 0-1 multipliers
                float tempMultiplier = temperatureScore / 10f;
                float altitudeMultiplier = altitudeScore / 10f;
                float waterMultiplier = waterScore / 10f;
                
                // Final fertility: product of all factors, scaled back to 0-10
                hex.Fertility = tempMultiplier * altitudeMultiplier * waterMultiplier * 10f;
            }
        }
    }

    private float CalculateScore(float value, float sweetSpotMin, float sweetSpotMax, float baseLog = 2)
    {
        // If the value is within the sweet spot range, return the maximum score
        if (value >= sweetSpotMin && value <= sweetSpotMax)
        {
            return 10;
        }

        // Calculate the distance to the nearest boundary of the sweet spot range
        float distance = Mathf.Min(Mathf.Abs(value - sweetSpotMin), Mathf.Abs(value - sweetSpotMax));

        // Calculate the score as a reverse logarithm of the distance
        float score = Mathf.Min(10, 10 / Mathf.Log(distance + 1, baseLog));

        // Return the score
        return score;
    }
}