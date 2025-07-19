using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class ControlPanel : MonoBehaviour
{
    public TextMeshProUGUI populationText;
    public TextMeshProUGUI averageTemperatureText;
    public HexGrid hexGrid;
    public List<Biome> biomes;
    public Texture2D plateSelector;
    void Start()
    {
    }

    void Update()
    {
        if (populationText != null)
        {
            populationText.text = "Era: " + hexGrid.Era * 10 + " AG";
        }

        if (averageTemperatureText != null && hexGrid != null)
        {
            averageTemperatureText.text = "Avg. Temp: " + Mathf.Round((hexGrid.AverageGlobalTemperature * 10)) / 10 + "°C";
            averageTemperatureText.text += "\nSea Level: " + Mathf.Round(hexGrid.SeaLevel - hexGrid.GenesisSeaLevel) + "m\n";
        }

    }


}