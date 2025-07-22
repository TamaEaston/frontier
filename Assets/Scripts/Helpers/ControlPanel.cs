using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class ControlPanel : MonoBehaviour
{
    public TextMeshProUGUI infoPanel;
    public TextMeshProUGUI averageTemperatureText;
    public HexGrid hexGrid;
    public List<Biome> biomes;
    public Texture2D plateSelector;
    void Start()
    {
        // Initialization if needed
    }

    void Update()
    {
        if (infoPanel != null && hexGrid != null)
        {
            string activeView = GameSettings.ActiveOverlay == "None" ? "Biome" : GameSettings.ActiveOverlay;
            
            // Calculate averages for land hexes only
            float totalHeight = 0f;
            float totalRainfall = 0f;
            float totalTemperature = 0f;
            float totalFertility = 0f;
            float totalWindIntensity = 0f;
            int landHexCount = 0;
            var hexagons = hexGrid.GetHexagons();
            
            for (int i = 0; i < hexagons.GetLength(0); i++)
            {
                for (int j = 0; j < hexagons.GetLength(1); j++)
                {
                    if (hexagons[i, j] != null && hexagons[i, j].AltitudeVsSeaLevel > 0) // Land only
                    {
                        totalHeight += hexagons[i, j].HeightAboveSeaLevel;
                        totalRainfall += hexagons[i, j].Rainfall;
                        totalTemperature += hexagons[i, j].Temperature;
                        totalFertility += hexagons[i, j].Fertility;
                        totalWindIntensity += hexagons[i, j].WindIntensity;
                        landHexCount++;
                    }
                }
            }
            
            float averageHeight = landHexCount > 0 ? totalHeight / landHexCount : 0f;
            float averageRainfall = landHexCount > 0 ? totalRainfall / landHexCount : 0f;
            float averageTemperature = landHexCount > 0 ? totalTemperature / landHexCount : 0f;
            float averageFertility = landHexCount > 0 ? totalFertility / landHexCount : 0f;
            float averageWindIntensity = landHexCount > 0 ? totalWindIntensity / landHexCount : 0f;
            
            infoPanel.text = activeView + " | Era " + hexGrid.Era + " | Height " + averageHeight.ToString("F0") + "m | Rainfall " + averageRainfall.ToString("F0") + " | Temp " + averageTemperature.ToString("F0") + "°C | Fertility " + averageFertility.ToString("F1") + " | Wind " + averageWindIntensity.ToString("F0");
        }

    }


}