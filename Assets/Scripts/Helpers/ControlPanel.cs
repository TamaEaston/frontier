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
        Dictionary<Civilisation, float> populationDict = new Dictionary<Civilisation, float>();

        foreach (Civilisation civilisation in Civilisation.civilisations) // Access the static civilisations list directly
        {
            float totalPopulation = 0;
            foreach (Hexagon hex in hexGrid.GetHexagons()) // Use the GetHexagons method
            {
                if (hex != null && hex.Civilisation == civilisation)
                {
                    totalPopulation += hex.HumanPopulation;
                }
            }
            populationDict[civilisation] = totalPopulation;
        }
        if (populationText != null)
        {
            populationText.text = "";
            foreach (var item in populationDict.OrderByDescending(i => i.Value)) // Order by totalPopulation descending
            {
                populationText.text += item.Key.Name + ": " + Mathf.Round(item.Value) + "\n"; // Round totalPopulation
            }
            populationText.text += "\n=============\nEra: " + hexGrid.Era * 10 + " AG";
        }

        if (averageTemperatureText != null && hexGrid != null)
        {
            averageTemperatureText.text = "Avg. Temp: " + Mathf.Round((hexGrid.AverageGlobalTemperature * 10)) / 10 + "°C";
            averageTemperatureText.text += "\nSea Level: " + Mathf.Round(hexGrid.SeaLevel - hexGrid.GenesisSeaLevel) + "m\n";
        }

    }


}