using System;
using UnityEngine;
using UnityEngine.UI;

public class Hexagon : MonoBehaviour
{
    public string HexagonID;
    public int PositionX;
    public int PositionY;

    public SpriteRenderer SpriteRenderer;
    public float Altitude;
    public float AltitudeNew;
    public float AltitudeOld;
    public float AltitudeChange;
    public float MagmaIntensity;
    public float MagmaDirection;
    public bool PlateSelected;
    public float VolcanicActivity;
    public char BelongsToPlate;
    public float WindIntensity;
    public float WindChange;
    public float WindDirection;
    public float Evaporation;
    public float WaterVapour;
    public float Rainfall;
    public float SurfaceWater;
    public float SurfaceWaterNew;
    public float RiverWidth;
    public float SolarIntensity;
    public float TemperatureNoWind;
    public float Temperature;

    public Hexagon[] Neighbours = new Hexagon[6];
    public Hexagon[] WindSources = new Hexagon[6];

    public Hexagon LowestNeighbour = null;

    public HexGrid hexGrid { get; set; }
    public Biome Biome { get; set; }
    public float HeightAboveSeaLevel
    {
        get
        {
            return hexGrid != null ? Math.Max(0, Altitude - hexGrid.SeaLevel) : 0;
        }
    }

    public float AltitudeVsSeaLevel
    {
        get
        {
            return hexGrid != null ? Altitude - hexGrid.SeaLevel : 0;
        }
    }

    public float AltitudeWithRiverWidth
    {
        get
        {
            return hexGrid != null ? Altitude - (RiverWidth / 10) : 0;
        }
    }

    private bool isClicked = false;

    private void OnMouseDown()
    {
        isClicked = true;
    }

    private void OnMouseUp()
    {
        isClicked = false;
    }

    private void OnGUI()
    {
        if (isClicked)
        {
            if (GameSettings.EditMode == "CreateSinkHole")
            {
                Altitude = (Altitude > 0) ? 0 : Altitude - 2000;
                GameSettings.EditMode = "None";
                UnityEngine.Debug.Log("Sink Hole created");
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                GameSettings.ActiveOverlay = "None";
                hexGrid.RefreshDisplay(false);
            }
            else if (GameSettings.EditMode == "CreateVolcano")
            {
                VolcanicActivity = 100;
                GameSettings.EditMode = "None";
                UnityEngine.Debug.Log("Volcano created");
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                GameSettings.ActiveOverlay = "None";
                hexGrid.RefreshDisplay(false);
            }
            else if (GameSettings.EditMode == "CreateRandomPlate")
            {
                hexGrid.GeneratePlateAtHexagon(this); // Call the GeneratePlateAtHexagon method
                GameSettings.EditMode = "None";
                UnityEngine.Debug.Log("Random Plate created");
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                hexGrid.RefreshDisplay(true);
            }
            else
            {
                // Create a tooltip window
                int x = (int)Input.mousePosition.x;
                int y = (int)(Screen.height - Input.mousePosition.y); // Convert to GUI coordinates
                string tooltipText = $"HexagonID: {HexagonID}\nPositionX: {PositionX}\nPositionY: {PositionY}\nAltitude: {Altitude}\nAltitudeChange: {AltitudeChange}\nHeightAboveSeaLevel: {HeightAboveSeaLevel}\nAltitudeVsSeaLevel: {AltitudeVsSeaLevel}\nMagmaIntensity: {MagmaIntensity}\nMagmaDirection: {MagmaDirection}\nWindIntensity: {WindIntensity}\nWindChange: {WindChange}\nWindDirection: {WindDirection}\nEvaporation: {Evaporation}\nWaterVapour: {WaterVapour}\nRainfall: {Rainfall}\nSurfaceWater: {SurfaceWater}\nSurfaceWaterNew: {SurfaceWaterNew}\nRiverWidth: {RiverWidth}\nSolarIntensity: {SolarIntensity}\nTemperatureNoWind: {TemperatureNoWind}\nTemperature: {Temperature}";

                // Create a GUIStyle for the tooltip
                GUIStyle tooltipStyle = new GUIStyle(GUI.skin.box);
                tooltipStyle.alignment = TextAnchor.UpperLeft;
                tooltipStyle.fontSize = 10; // Change the font size

                // Calculate the size of the tooltip box
                GUIContent content = new GUIContent(tooltipText);
                Vector2 size = tooltipStyle.CalcSize(content);

                GUI.Box(new Rect(x - (size.x / 2), y - (size.y / 2), size.x, size.y), tooltipText, tooltipStyle);
            }
        }
    }

}