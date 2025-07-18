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
    public Button sinkHoleButton;
    public Button volcanoButton;
    public Button randomPlateButton;
    public Button coolingClimateButton;
    public Button stableClimateButton;
    public Button warmingClimateButton;
    public Texture2D plateSelector;
    void Start()
    {
        sinkHoleButton.onClick.AddListener(OnSinkHoleButtonClicked);
        volcanoButton.onClick.AddListener(OnVolcanoButtonClicked);
        randomPlateButton.onClick.AddListener(OnRandomPlateButtonClicked);
        coolingClimateButton.onClick.AddListener(() => { GameSettings.ClimateMode = "Cooling"; });
        stableClimateButton.onClick.AddListener(() => { GameSettings.ClimateMode = "Stable"; });
        warmingClimateButton.onClick.AddListener(() => { GameSettings.ClimateMode = "Warming"; });
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

        if (coolingClimateButton?.image != null || stableClimateButton?.image != null || warmingClimateButton?.image != null)
        {

            switch (GameSettings.ClimateMode)
            {
                case "Cooling":
                    coolingClimateButton.image.color = Color.cyan; // light blue
                    stableClimateButton.image.color = Color.white; // reset other buttons
                    warmingClimateButton.image.color = Color.white; // reset other buttons
                    break;
                case "Stable":
                    stableClimateButton.image.color = Color.green; // light green
                    coolingClimateButton.image.color = Color.white; // reset other buttons
                    warmingClimateButton.image.color = Color.white; // reset other buttons
                    break;
                case "Warming":
                    warmingClimateButton.image.color = Color.red; // light red
                    coolingClimateButton.image.color = Color.white; // reset other buttons
                    stableClimateButton.image.color = Color.white; // reset other buttons
                    break;
                default:
                    // Reset all buttons if no match
                    coolingClimateButton.image.color = Color.white;
                    stableClimateButton.image.color = Color.white;
                    warmingClimateButton.image.color = Color.white;
                    break;
            }
        }
    }

    void OnRandomPlateButtonClicked()
    {
        UnityEngine.Debug.Log("Magma button clicked");
        GameSettings.ActiveOverlay = "Magma";
        GameSettings.EditMode = "CreateRandomPlate";
        Vector2 hotspot = new Vector2(plateSelector.width / 2, plateSelector.height / 2);
        Cursor.SetCursor(plateSelector, hotspot, CursorMode.Auto);
        GeoPhase phase = new GeoPhase(hexGrid, biomes);
        phase.ExecuteRefreshHexDisplay();
    }

    void OnSinkHoleButtonClicked()
    {
        UnityEngine.Debug.Log("Sink Hole button clicked");
        GameSettings.ActiveOverlay = "Altitude";
        GameSettings.EditMode = "CreateSinkHole";
        Vector2 hotspot = new Vector2(plateSelector.width / 2, plateSelector.height / 2);
        Cursor.SetCursor(plateSelector, hotspot, CursorMode.Auto);
        GeoPhase phase = new GeoPhase(hexGrid, biomes);
        phase.ExecuteRefreshHexDisplay();
    }

    void OnVolcanoButtonClicked()
    {
        UnityEngine.Debug.Log("Volcano button clicked");
        GameSettings.ActiveOverlay = "Altitude";
        GameSettings.EditMode = "CreateVolcano";
        Vector2 hotspot = new Vector2(plateSelector.width / 2, plateSelector.height / 2);
        Cursor.SetCursor(plateSelector, hotspot, CursorMode.Auto);
        GeoPhase phase = new GeoPhase(hexGrid, biomes);
        phase.ExecuteRefreshHexDisplay();
    }

}