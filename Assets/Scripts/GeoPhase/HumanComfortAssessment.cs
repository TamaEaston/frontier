using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class HumanComfortAssessment
{
    private HexGrid hexGrid;
    int temperatureSweetSpotMin = 15;
    int temperatureSweetSpotMax = 25;
    int altitudeSweetSpotMin = 5;
    int altitudeSweetSpotMax = 1000;
    int rainfallSweetSpotMin = 15;
    int rainfallSweetSpotMax = 30;
    int riverflowSweetSpotMin = 100;
    int riverflowSweetSpotMax = 250;
    int windIntensitySweetSpotMin = 0;
    int windIntensitySweetSpotMax = 50;


    public HumanComfortAssessment(HexGrid hexGrid)
    {
        this.hexGrid = hexGrid;
    }

    public void Execute()
    {
        Debug.Log("HumanComfortAssessment has been triggered");
        // Flatten the 2D array into a 1D list 
        var allHexagons = hexGrid.GetHexagons()
            .Cast<Hexagon>()
            .ToList();

        PrintStat("Temperature", allHexagons.Select(hex => hex.Temperature));
        PrintStat("HeightAboveSeaLevel", allHexagons.Where(hex => hex.HeightAboveSeaLevel != 0).Select(hex => hex.HeightAboveSeaLevel));
        PrintStat("Rainfall", allHexagons.Select(hex => hex.Rainfall));
        PrintStat("RiverWidth", allHexagons.Select(hex => hex.RiverWidth));
        PrintStat("WindIntensity", allHexagons.Select(hex => hex.WindIntensity));


        foreach (var hex in allHexagons)
        {
            float temperatureScore = CalculateScore(hex.Temperature, temperatureSweetSpotMin, temperatureSweetSpotMax, 10);
            float altitudeScore = CalculateScore(hex.HeightAboveSeaLevel, altitudeSweetSpotMin, altitudeSweetSpotMax, 10);
            float rainfallScore = CalculateScore(hex.Rainfall, rainfallSweetSpotMin, rainfallSweetSpotMax);
            float riverflowScore = CalculateScore(hex.RiverWidth, riverflowSweetSpotMin, riverflowSweetSpotMax);
            float windIntensityScore = CalculateScore(hex.WindIntensity, windIntensitySweetSpotMin, windIntensitySweetSpotMax);

            // Use the maximum score from rainfall and riverflow
            float waterScore = (hex.Temperature < -5) ? 0 : Mathf.Max(rainfallScore, riverflowScore);

            if (hex.HeightAboveSeaLevel > 0 && hex.HeightAboveSeaLevel < 4000 && hex.SurfaceWater < 100 && hex.VolcanicActivity == 0 && hex.Temperature > -15 && hex.Temperature < 35)
            {
                // Calculate the overall score and clamp it between 0 and 11
                hex.HumanComfortIndex = Mathf.Max(0, Mathf.Round(Mathf.Clamp((temperatureScore * 5) + (altitudeScore * 3f) + (waterScore * 3f) + (windIntensityScore * 1), 0, 120)) - 20);
            }
            else
            {
                hex.HumanComfortIndex = 0;
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

    private void PrintStat(string name, IEnumerable<float> values)
    {
        float min = values.Min();
        float max = values.Max();
        float mean = values.Average();
        float median = values.OrderBy(x => x).Skip((values.Count() - 1) / 2).First();

        Debug.Log($"{name}: Min = {min}, Max = {max}, Mean = {mean}, Median = {median}");
    }
}