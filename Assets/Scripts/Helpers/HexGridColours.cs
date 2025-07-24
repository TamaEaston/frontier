using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEditorInternal;

namespace Helpers
{
    public class HexGridColours
    {
        public Color GetBiomeColour(float heightAboveSeaLevel, float SurfaceWater, float Temperature)
        {
            if (heightAboveSeaLevel < 0)
            {
                if (Temperature > -10)
                {
                    float t = Mathf.InverseLerp(-10000, 0, heightAboveSeaLevel);
                    return Color.Lerp(new Color(0, 0, 0.5f), new Color(0.4f, 0.7f, 0.8f), t); // Dark Blue to Light Sea Blue
                }
                else
                {
                    return new Color(0.9f, 0.9f, 1.0f); // Very Light Blue
                }
            }
            else if (SurfaceWater >= 100)
            {
                if (Temperature > -5)
                {
                    return new Color(0.0f, 0.5f, 0.5f);
                }
                else
                {
                    return new Color(0.6f, 0.8f, 1.0f); // Light Blue
                }
            }
            else if (Temperature < -5)
            {
                float t = Mathf.InverseLerp(0, 5000, heightAboveSeaLevel);
                return Color.Lerp(new Color(0.9f, 0.9f, 0.9f), Color.white, t);
            }
            else if (heightAboveSeaLevel <= 50)
            {
                return new Color(0.93f, 0.79f, 0.69f); // Yellow Sand
            }
            else if (heightAboveSeaLevel <= 2500)
            {
                float t = Mathf.InverseLerp(51, 2500, heightAboveSeaLevel);
                return Color.Lerp(new Color(0.5f, 0.8f, 0.2f), new Color(0.13f, 0.55f, 0.13f), t); // Grass Green to Forest Green
            }
            else
            {
                float t = Mathf.InverseLerp(2501, 5000, heightAboveSeaLevel);
                return Color.Lerp(new Color(0.5f, 0.5f, 0.5f), new Color(0.8f, 0.8f, 0.8f), t); // Rock Grey to Light Rock Grey
            }
        }

        public Color GetAltitudeColour(float heightAboveSeaLevel, float SurfaceWater, float Temperature)
        {
            if (heightAboveSeaLevel < 0)
            {
                float t = Mathf.InverseLerp(-10000, 0, heightAboveSeaLevel);
                return Color.Lerp(Color.black, new Color(0.4f, 0.7f, 0.8f), t); // Black to Light Sea Blue
            }
            else if (SurfaceWater >= 100)
            {
                if (Temperature > -5)
                {
                    return new Color(0.0f, 0.5f, 0.5f);
                }
                else
                {
                    return new Color(0.6f, 0.8f, 1.0f); // Light Blue
                }
            }
            else
            {
                float t = Mathf.InverseLerp(0, 10000, heightAboveSeaLevel);
                return Color.Lerp(new Color(0.13f, 0.55f, 0.13f), Color.white, t); // Forest Green to White
            }
        }

        public Color GetRainfallColour(float heightAboveSeaLevel, float SurfaceWater, float Rainfall)
        {
            if (heightAboveSeaLevel < 0)
            {
                float t = Mathf.InverseLerp(-10000, 0, heightAboveSeaLevel);
                return Color.Lerp(new Color(0, 0, 0.5f), new Color(0.4f, 0.7f, 0.8f), t); // Dark Blue to Light Sea Blue
            }
            else if (SurfaceWater >= 100)
            {
                return new Color(0.0f, 0.5f, 0.5f);
            }
            else
            {
                float t = Mathf.InverseLerp(0, 50, Rainfall);
                return Color.Lerp(Color.yellow, new Color(0, 0.39f, 0), t); // Yellow to Dark Green
            }
        }

        public Color GetTemperatureColour(float heightAboveSeaLevel, float SurfaceWater, float Temperature)
        {
            if (heightAboveSeaLevel < 0)
            {
                float t = Mathf.InverseLerp(-40, 40, Temperature);
                return Color.Lerp(new Color(0.9f, 0.9f, 1f), new Color(0, 0, 0.5f), t); // Icy Blue Tint to Dark Blue
            }
            else if (SurfaceWater >= 100)
            {
                if (Temperature > -5)
                {
                    return new Color(0.0f, 0.5f, 0.5f);
                }
                else
                {
                    return new Color(0.6f, 0.8f, 1.0f); // Light Blue
                }
            }
            else
            {
                float t;
                Color startColour, endColour;

                if (Temperature >= 0 && Temperature <= 20)
                {
                    // For temperatures between 0 and 20, range from light green to dark green
                    t = Mathf.InverseLerp(0, 20, Temperature);
                    startColour = new Color(0.56f, 0.93f, 0.56f); // Light Green
                    endColour = new Color(0.0f, 0.5f, 0.0f); // Dark Green
                }
                else if (Temperature > 20)
                {
                    t = Mathf.InverseLerp(20, 35, Temperature);
                    startColour = new Color(0.0f, 0.5f, 0.0f); // Dark Green
                    endColour = new Color(1.0f, 0.5f, 0.0f); // Orange
                }
                else
                {
                    // For other temperatures, range from white to red
                    t = Mathf.InverseLerp(-35, 0, Temperature);
                    startColour = Color.white;
                    endColour = new Color(0.56f, 0.93f, 0.56f); // Light Green
                }

                return Color.Lerp(startColour, endColour, t);
            }
        }

        public Color GetMagmaColour(float magmaIntensity)
        {
            float t = Mathf.InverseLerp(0, 100, magmaIntensity); // Normalize magmaIntensity to the range [0, 1]
            return Color.Lerp(Color.yellow, Color.red, t); // Interpolate from Yellow to Red
        }

        public Color GetPlateColour(char plateId)
        {
            // Define the hue values for red and orange in the HSV color space.
            float hueRed = 0.0f; // Hue value for red
            float hueYellow = 0.167f; // Hue value for yellow

            // Calculate the index of the plateId letter in the alphabet (0 for 'A', 1 for 'B', etc.).
            int index = plateId - 'A';

            // Calculate the hue for this letter by interpolating between red and orange.
            float hue = hueRed + (hueYellow - hueRed) * index / 25;

            // Convert the HSV color to RGB and return it.
            return Color.HSVToRGB(hue, 1, 1);
        }

        public Color GetFertilityColour(float fertility, float heightAboveSeaLevel, float riverWidth)
        {
            // This method should only be called for land tiles
            // Water tiles should use GetBiomeColour instead
            
            // Land hexes: Grey (fertility 0) to Bright Green (fertility 10)
            float t = Mathf.InverseLerp(0, 10, fertility);
            Color fertilityColor = Color.Lerp(Color.grey, Color.green, t);
            
            // If there's a river, blend with blue to show water presence
            if (riverWidth > 0)
            {
                float riverIntensity = Mathf.Clamp01(riverWidth / 300f); // Scale based on max river width
                fertilityColor = Color.Lerp(fertilityColor, new Color(0.2f, 0.6f, 1f), riverIntensity * 0.3f); // Subtle blue tint
            }
            
            return fertilityColor;
        }

        public Color GetTerrainQuartileColour(int terrainQuartile, float altitudeVsSeaLevel)
        {
            // Ocean: Keep existing depth gradient
            if (altitudeVsSeaLevel <= 0)
            {
                float t = Mathf.InverseLerp(-10000, 0, altitudeVsSeaLevel);
                return Color.Lerp(Color.black, new Color(0.4f, 0.7f, 0.8f), t); // Black to Light Sea Blue
            }
            
            // Land: Four distinct terrain quartile colors
            switch (terrainQuartile)
            {
                case 1: return new Color(0.8f, 0.9f, 0.6f, 1f); // Light green (Q1 - Flat/Plains)
                case 2: return new Color(0.7f, 0.8f, 0.4f, 1f); // Medium green (Q2 - Rolling)  
                case 3: return new Color(0.6f, 0.5f, 0.3f, 1f); // Brown (Q3 - Hilly)
                case 4: return new Color(0.4f, 0.3f, 0.2f, 1f); // Dark brown (Q4 - Mountainous)
                default: return Color.gray; // Fallback
            }
        }
    }
}