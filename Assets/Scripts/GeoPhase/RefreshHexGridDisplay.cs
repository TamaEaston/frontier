using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UI;
public class RefreshHexGridDisplay
{
    private HexGrid hexGrid;
    private float seaLevel;

    public RefreshHexGridDisplay(HexGrid hexGrid, float seaLevel)
    {
        this.hexGrid = hexGrid;
        this.seaLevel = seaLevel;
    }

    public void Execute()
    {
        //Clear All Lines
        foreach (GameObject line in GameObject.FindGameObjectsWithTag("RiverLine"))
        {
            GameObject.Destroy(line);
        }

        int mapHeight = hexGrid.GetHexagons().GetLength(1);
        int mapWidth = hexGrid.GetHexagons().GetLength(0);

        // Debug.Log("RefreshHexGridDisplay has been triggered");
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {

                Hexagon hex = hexGrid.GetHexagons()[i, j];

                Color altitudeColour = Color.white;
                if (GameSettings.ActiveOverlay == "Altitude")
                {
                    altitudeColour = hexGrid.colours.GetAltitudeColour(hex.AltitudeVsSeaLevel, (float)hex.SurfaceWater, (float)hex.Temperature);
                }
                else if (GameSettings.ActiveOverlay == "Weather")
                {
                    altitudeColour = hexGrid.colours.GetRainfallColour(hex.AltitudeVsSeaLevel, (float)hex.SurfaceWater, (float)hex.Rainfall);
                }
                else if (GameSettings.ActiveOverlay == "Temperature")
                {
                    altitudeColour = hexGrid.colours.GetTemperatureColour(hex.AltitudeVsSeaLevel, (float)hex.SurfaceWater, (float)hex.Temperature);
                }
                else
                {
                    if (hex.Biome != null)
                    {
                        Color color;
                        if (ColorUtility.TryParseHtmlString(hex.Biome.Colour, out color))
                        {
                            altitudeColour = color;
                        }
                        else
                        {
                            Debug.LogError("Invalid color string: " + hex.Biome.Colour);
                        }

                        // Remove the leading slash and file extension from the image path
                        string imagePath = hex.Biome.Image.TrimStart('/').Replace(".png", "");

                        // Load the sprite
                        Sprite sprite = Resources.Load<Sprite>(imagePath);

                        if (sprite != null)
                        {
                            // Apply the sprite to the SpriteRenderer on your hex object
                            hex.SpriteRenderer.sprite = sprite;
                        }
                        else
                        {
                            Debug.LogError("Failed to load sprite: " + imagePath);
                        }
                    }
                    else
                    {
                        altitudeColour = hexGrid.colours.GetBiomeColour(hex.AltitudeVsSeaLevel, (float)hex.SurfaceWater, (float)hex.Temperature);
                    }

                }

                altitudeColour.a = 1f;
                hex.SpriteRenderer.color = altitudeColour;

                //River Lines
                if (hex.LowestNeighbour != null && hex.AltitudeVsSeaLevel > 0)
                {
                    Color riverColour = (hex.Temperature < -5) ? new Color(0.6f, 0.8f, 1.0f) : new Color(0.0f, 0.5f, 0.5f);

                    Vector3 startPos = new Vector3(hex.transform.position.x, hex.transform.position.y, -1);
                    Vector3 endPos = new Vector3(hex.LowestNeighbour.transform.position.x, hex.LowestNeighbour.transform.position.y, -1);

                    // Calculate the direction from the center of each hex to the center of the other hex
                    Vector3 directionToNeighbour = (endPos - startPos).normalized;
                    Vector3 directionToHex = -directionToNeighbour;

                    // Calculate the points on the edges of the hexes where the lines should start and end
                    // Replace 'hexRadius' with the actual radius of your hexes
                    float hexRadius = hex.GetComponent<Renderer>().bounds.size.x / 2;
                    Vector3 edgePos1 = startPos + directionToNeighbour * hexRadius;
                    Vector3 edgePos2 = endPos + directionToHex * hexRadius;

                    float riverWidth = Math.Min(0.3f, hex.RiverWidth / 2000f);
                    // Debug.Log("River Width: " + riverWidth + " Surface Water: " + hex.SurfaceWater);

                    // Create the first line from the center of the hex to the edge of the hex
                    GameObject lineObject1 = new GameObject("Line1");
                    LineRenderer line1 = lineObject1.AddComponent<LineRenderer>();
                    line1.startWidth = riverWidth;
                    line1.endWidth = riverWidth;
                    line1.material = new Material(Shader.Find("Unlit/Color")) { color = riverColour };
                    line1.SetPosition(0, startPos);
                    line1.SetPosition(1, edgePos1);
                    line1.tag = "RiverLine";

                    if (hex.LowestNeighbour.Altitude > seaLevel)
                    {
                        // Create the second line from the center of the lowest neighbour to the edge of the hex
                        GameObject lineObject2 = new GameObject("Line2");
                        LineRenderer line2 = lineObject2.AddComponent<LineRenderer>();
                        line2.startWidth = riverWidth;
                        line2.endWidth = riverWidth;
                        line2.material = new Material(Shader.Find("Unlit/Color")) { color = riverColour };
                        line2.SetPosition(0, endPos);
                        line2.SetPosition(1, edgePos2);
                        line2.tag = "RiverLine";
                    }
                    //                    UnityEngine.Debug.Log("River Lines drawn from " + startPos + " to " + endPos);
                }



                if (GameSettings.ActiveOverlay == "Magma")
                {
                    //Magma Overlay
                    Arrow magmaHex = hexGrid.GetArrows()[i, j];
                    magmaHex.transform.rotation = Quaternion.Euler(0, 0, hex.MagmaDirection);
                    Color magmaColor = hexGrid.colours.GetMagmaColour(hex.MagmaIntensity);
                    magmaColor.a = 0.75f;
                    if (hex.PlateSelected)
                    {
                        magmaColor = Color.black;
                        magmaColor.a = hex.MagmaIntensity / 100;
                        hex.PlateSelected = false;
                    }

                    magmaHex.SpriteRenderer.color = magmaColor;
                    magmaHex.transform.localScale = new Vector3(0.3f, 0.3f, 1f);
                }
                else if (GameSettings.ActiveOverlay == "Altitude")
                {
                    //Altitude Overlay
                    Arrow altitudeArrow = hexGrid.GetArrows()[i, j];
                    int arrowDirection = (hex.AltitudeChange > 0) ? 90 : 270;
                    altitudeArrow.transform.rotation = Quaternion.Euler(0, 0, arrowDirection);

                    Color altitudeArrowColour = (hex.AltitudeChange < 0) ? Color.white : Color.black;
                    altitudeArrowColour.a = 0.75f;
                    altitudeArrow.SpriteRenderer.color = altitudeArrowColour;
                    float scale = Math.Min(0.5f, Math.Abs(hex.AltitudeChange) / 100f);
                    altitudeArrow.transform.localScale = new Vector3(scale, scale, 1f);
                }
                else if (GameSettings.ActiveOverlay == "Weather")
                {
                    //Wind Overlay
                    Arrow windHex = hexGrid.GetArrows()[i, j];
                    windHex.transform.rotation = Quaternion.Euler(0, 0, hex.WindDirection);
                    Color windColor = Color.white;
                    windColor.a = 0.25f;
                    windHex.SpriteRenderer.color = windColor;
                    float scale = Math.Min(0.5f, hex.WindIntensity / 200f);
                    windHex.transform.localScale = new Vector3(scale, scale, 1f);
                }
                else
                {
                    //Clear Arrows
                    Arrow windHex = hexGrid.GetArrows()[i, j];
                    windHex.transform.rotation = Quaternion.Euler(0, 0, 0);
                    windHex.transform.localScale = new Vector3(0f, 0f, 1f);
                }

            }
        }
    }
}