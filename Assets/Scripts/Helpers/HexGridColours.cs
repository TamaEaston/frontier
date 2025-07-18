using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEditorInternal;

namespace Helpers
{
    public class HexGridColours
    {
        public Color GetAltitudeColour(float heightAboveSeaLevel)
        {
            if (heightAboveSeaLevel < 0)
            {
                float t = Mathf.InverseLerp(-10000, 0, heightAboveSeaLevel);
                return Color.Lerp(new Color(0, 0, 0.5f), new Color(0.4f, 0.7f, 0.8f), t); // Dark Blue to Light Sea Blue
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
            else if (heightAboveSeaLevel <= 5000)
            {
                float t = Mathf.InverseLerp(2501, 5000, heightAboveSeaLevel);
                return Color.Lerp(new Color(0.5f, 0.5f, 0.5f), Color.white, t); // Rock Grey to Snow White
            }
            else
            {
                float t = Mathf.InverseLerp(5001, 10000, heightAboveSeaLevel);
                return Color.Lerp(Color.white, new Color(0.9f, 0.9f, 1f), t); // Snow White to White with Blue Tint
            }
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
    }
}