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
            string activeView = GameSettings.ActiveOverlay == "None" ? "Biome" : GameSettings.ActiveOverlay;
            populationText.text = "View: " + activeView + " | Era: " + hexGrid.Era * 10 + " AG";
        }

    }


}