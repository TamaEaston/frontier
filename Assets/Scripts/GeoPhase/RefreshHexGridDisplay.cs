using System.Collections.Generic;
using UnityEngine;
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
        Debug.Log("RefreshHexGridDisplay has been triggered");
        for (int x = 0; x < hexGrid.GetHexagons().GetLength(0); x++)
        {
            for (int y = 0; y < hexGrid.GetHexagons().GetLength(1); y++)
            {
                Hexagon hex = hexGrid.GetHexagons()[x, y];
                float heightAboveSeaLevel = hex.Altitude - seaLevel;
                hex.SpriteRenderer.color = hexGrid.colours.GetAltitudeColour(heightAboveSeaLevel);
                //                Debug.Log("Hexagon" + i + "," + j + " has been updated with Altitude of " + hex.Altitude + " and colour " + hex.SpriteRenderer.color);
            }
        }
    }

}