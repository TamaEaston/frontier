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
            
            // Calculate average height above sea level
            float totalHeight = 0f;
            int hexCount = 0;
            var hexagons = hexGrid.GetHexagons();
            
            for (int i = 0; i < hexagons.GetLength(0); i++)
            {
                for (int j = 0; j < hexagons.GetLength(1); j++)
                {
                    if (hexagons[i, j] != null)
                    {
                        totalHeight += hexagons[i, j].HeightAboveSeaLevel;
                        hexCount++;
                    }
                }
            }
            
            float averageHeight = hexCount > 0 ? totalHeight / hexCount : 0f;
            
            infoPanel.text = activeView + " | Era " + hexGrid.Era + " | Height " + averageHeight.ToString("F0") + "m";
        }

    }


}