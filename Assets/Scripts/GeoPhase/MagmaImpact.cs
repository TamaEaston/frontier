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
        // Debug.Log("MagmaImpact has been triggered");

        for (int i = 0; i < hexGrid.GetHexagons().GetLength(0); i++)
        {
            for (int j = 0; j < hexGrid.GetHexagons().GetLength(1); j++)
            {
                Hexagon hex = hexGrid.GetHexagons()[i, j];
                hex.AltitudeChange = hex.Altitude - hex.AltitudeOld;
                hex.AltitudeOld = hex.Altitude;

                // Convert MagmaDirection to an index for the Neighbours array
                int hexDirectionRef = (int)Mathf.Round(hex.MagmaDirection / 60f) % 6;

                // Determine which neighbour the MagmaDirection points towards and away from
                Hexagon towardsNeighbour = hex.Neighbours[hexDirectionRef];
                Hexagon awayNeighbour = hex.Neighbours[(hexDirectionRef + 3) % 6];

                // Adjust the Altitude of the two identified neighbouring hexagons based on the MagmaIntensity
                if (towardsNeighbour != null)
                {
                    towardsNeighbour.Altitude += hex.MagmaIntensity;
                }
                if (awayNeighbour != null)
                {
                    awayNeighbour.Altitude -= hex.MagmaIntensity;
                }

                //                Debug.Log("Hexagon: " + hex + " is pointing towards " + towardsNeighbour + " and away from " + awayNeighbour + " with a MagmaIntensity of " + hex.MagmaIntensity + " and a MagmaDirection of " + hex.MagmaDirection + " degrees.");

                if (hex.VolcanicActivity > 0)
                {
                    hex.Altitude += (hex.VolcanicActivity * 10);
                    hex.VolcanicActivity = Mathf.Max(0, hex.VolcanicActivity - 10);
                }
            }
        }
    }
}