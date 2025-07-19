using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FertilityAssessment
{
    private HexGrid hexGrid;
    int temperatureSweetSpotMin = 10;
    int temperatureSweetSpotMax = 30;
    int altitudeSweetSpotMin = 0;
    int altitudeSweetSpotMax = 2000;
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
            // Calculate individual scores (0-10 each)
            float temperatureScore = CalculateScore(hex.Temperature, temperatureSweetSpotMin, temperatureSweetSpotMax, 10);
            float altitudeScore = CalculateScore(hex.HeightAboveSeaLevel, altitudeSweetSpotMin, altitudeSweetSpotMax, 10);
            float rainfallScore = CalculateScore(hex.Rainfall, rainfallSweetSpotMin, rainfallSweetSpotMax);
            float riverWidthScore = CalculateScore(hex.RiverWidth, riverWidthSweetSpotMin, riverWidthSweetSpotMax);

            // Exclusion criteria: only underwater hexes
            if (hex.HeightAboveSeaLevel < 0)
            {
                hex.Fertility = 0;
            }
            else
            {
                // Weighted calculation: Rainfall×4 + Temperature×3 + RiverWidth×2 + Altitude×1
                // Scale to 0-10 range
                float totalScore = (rainfallScore * 4f) + (temperatureScore * 3f) + (riverWidthScore * 2f) + (altitudeScore * 1f);
                float maxPossibleScore = (10 * 4f) + (10 * 3f) + (10 * 2f) + (10 * 1f); // = 100
                hex.Fertility = Mathf.Clamp(totalScore / maxPossibleScore * 10f, 0f, 10f);
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