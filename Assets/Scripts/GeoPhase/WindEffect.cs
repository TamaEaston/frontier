using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
public class WindEffect
{
    private HexGrid hexGrid;
    private float seaLevel;

    public WindEffect(HexGrid hexGrid, float seaLevel)
    {
        this.hexGrid = hexGrid;
        this.seaLevel = seaLevel;
    }

    public void Execute()
    {
        // Debug.Log("WindEffect has been triggered");

        var windHexagons = hexGrid.GetHexagons().Cast<Hexagon>().ToList();

        //Reset all WindSources & Direction/ Intensity defaults
        foreach (Hexagon hex in windHexagons)
        {
            Array.Clear(hex.WindSources, 0, hex.WindSources.Length);
            hex.WindDirection = 180;
            hex.WindIntensity = 25f;
            
            // Boost wind intensity for eastern ocean (strong maritime winds)
            if (hex.Altitude <= seaLevel && hex.PositionX >= hexGrid.Width - 3) 
            {
                hex.WindIntensity = 75f; // Strong eastern ocean winds
            }
            
            if (hex.Altitude <= seaLevel) 
            {
                // Check if this is one of the 3 easternmost ocean columns
                if (hex.PositionX >= hexGrid.Width - 3) // Eastern edge columns
                {
                    hex.Evaporation = 60f; // 6x normal ocean evaporation for strong maritime winds
                }
                else 
                {
                    hex.Evaporation = 10f; // Normal ocean evaporation
                }
            }
            else 
            {
                hex.Evaporation = 2f; // Land evaporation unchanged
            }
        }

        //Calculate WindDirection & TemperatureNoWind
        foreach (Hexagon hex in windHexagons)
        {
            hex.WindDirection = 180;

            int directionIndex = 3;
            Hexagon westNeighbour = hex.Neighbours[directionIndex];
            Hexagon directionNeighbour = westNeighbour;

            if (directionNeighbour != null && directionNeighbour.Altitude > seaLevel && (directionNeighbour.HeightAboveSeaLevel - hex.HeightAboveSeaLevel) > 1000f)
            {
                int[] indices = { 2, 3, 4 }; // indices for South-West, West & North-West neighbours
                Hexagon lowestNeighbour = directionNeighbour;
                int lowestIndex = directionIndex;

                foreach (int index in indices)
                {
                    Hexagon neighbour = hex.Neighbours[index];
                    if (neighbour != null && neighbour.Altitude < lowestNeighbour.Altitude)
                    {
                        lowestNeighbour = neighbour;
                        lowestIndex = index;
                    }
                }
                directionIndex = lowestIndex;
            }
            if (directionIndex < 2 || directionIndex > 4)
            {
                directionIndex = 3;
            }
            hex.WindDirection = directionIndex * 60;

            //Add hex to WindSources of WindDirection Neighbour

            Hexagon windNeighbour = hex.Neighbours[directionIndex];

            if (windNeighbour != null)
            {
                for (int i = 0; i < windNeighbour.WindSources.Length; i++)
                {
                    if (windNeighbour.WindSources[i] == null)
                    {
                        windNeighbour.WindSources[i] = hex;
                        break;
                    }
                }
            }

            Hexagon awayNeighbour = hex.Neighbours[(directionIndex + 3) % 6];

            //Calculate Wind Change & TemperatureNoWind
            if (directionNeighbour != null && directionNeighbour.Altitude <= seaLevel)
            {
                hex.WindChange = 2;
                // Keep continental temperature for water/coastal areas (TemperatureNoWind was set by ClimateTemperature.cs)
            }
            else
            {
                float AltitudeChange = (awayNeighbour != null) ? (awayNeighbour.HeightAboveSeaLevel - hex.HeightAboveSeaLevel) : 0;
                hex.WindChange = (AltitudeChange > 0) ? (AltitudeChange / 100f) : (AltitudeChange / 50f);
                // Apply altitude cooling to continental temperature (preserve the linear arctic-desert gradient)
                hex.TemperatureNoWind = hex.TemperatureNoWind - (4 * Mathf.Pow(2, hex.HeightAboveSeaLevel / 1000f - 1));
            }
            hex.Temperature = Math.Max(-50, Math.Min(50, hex.TemperatureNoWind));
        }

        // Calculate Wind Intensity and Water Vapour
        foreach (Hexagon hex in windHexagons)
        {
            hex.WindIntensity = 0f;
            hex.WaterVapour = 0f;
            int yStart = hex.PositionY;
            int yLimit = Math.Min(hex.PositionY + 5, hexGrid.Height - 1);
            (float windIntensity, float waterVapour) = CalculateWindIntensityAndWaterVapour(hex, yStart, yLimit, new HashSet<Hexagon>());
            hex.WindIntensity += windIntensity;
            hex.WindIntensity = Math.Max(0f, Math.Min(100f, hex.WindIntensity));
            hex.WaterVapour += waterVapour;
            hex.WaterVapour = Math.Max(0f, Math.Min(100f, hex.WaterVapour));
            
            // Boost WindIntensity and WaterVapour for eastern ocean edge (fierce ocean)
            if (hex.Altitude <= seaLevel && hex.PositionX >= hexGrid.Width - 3) 
            {
                hex.WindIntensity = 100f; // Maximum wind intensity for fierce eastern ocean
                hex.WaterVapour = 100f;   // Maximum water vapour from large ocean
            }
        }

        hexGrid.AverageGlobalTemperature = Mathf.Round(windHexagons.Average(h => h.Temperature) * 10) / 10;

        (float, float) CalculateWindIntensityAndWaterVapour(Hexagon newHex, int yStart, int yLimit, HashSet<Hexagon> visited)
        {
            float totalWindChange = 0f;
            float totalWaterVapour = 0f;

            foreach (Hexagon windSource in newHex.WindSources)
            {
                if (windSource == null)
                    break;

                if ((yStart <= yLimit && (windSource.PositionY > yLimit || windSource.PositionY < yStart)) ||
                    (yStart > yLimit && (windSource.PositionY > yLimit && windSource.PositionY < yStart)) ||
                    visited.Contains(windSource))
                    continue;

                visited.Add(windSource);
                totalWindChange += windSource.WindChange;
                totalWaterVapour += windSource.Evaporation;
                var (windIntensity, waterVapour) = CalculateWindIntensityAndWaterVapour(windSource, yStart, yLimit, visited);
                totalWindChange += windIntensity;
                totalWaterVapour += waterVapour;
            }

            return (totalWindChange, totalWaterVapour);
        }
    }
}