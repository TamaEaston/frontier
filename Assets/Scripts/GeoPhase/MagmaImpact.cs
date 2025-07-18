using System.Collections.Generic;
using UnityEngine;
public class MagmaImpact
{
    private HexGrid hexGrid;

    public MagmaImpact(HexGrid hexGrid)
    {
        this.hexGrid = hexGrid;
    }

    public void Execute()
    {
        Debug.Log("MagmaImpact has been triggered");

        for (int i = 0; i < hexGrid.GetHexagons().GetLength(0); i++)
        {
            for (int j = 0; j < hexGrid.GetHexagons().GetLength(1); j++)
            {
                Hexagon hex = hexGrid.GetHexagons()[i, j];

                // Convert MagmaDirection to an index for the Neighbours array
                int hexDirectionRef = (int)Mathf.Round(hex.MagmaDirection / 60f) % 6;

                // Determine which neighbor the MagmaDirection points towards and away from
                Hexagon towardsNeighbor = hex.Neighbours[hexDirectionRef];
                Hexagon awayNeighbor = hex.Neighbours[(hexDirectionRef + 3) % 6];

                // Adjust the Altitude of the two identified neighboring hexagons based on the MagmaIntensity
                if (towardsNeighbor != null)
                {
                    towardsNeighbor.Altitude += hex.MagmaIntensity;
                }
                if (awayNeighbor != null)
                {
                    awayNeighbor.Altitude -= hex.MagmaIntensity;
                }

                //                Debug.Log("Hexagon: " + hex + " is pointing towards " + towardsNeighbor + " and away from " + awayNeighbor + " with a MagmaIntensity of " + hex.MagmaIntensity + " and a MagmaDirection of " + hex.MagmaDirection + " degrees.");
            }
        }
    }
}