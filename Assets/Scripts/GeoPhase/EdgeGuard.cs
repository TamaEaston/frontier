using System;
using UnityEngine;

public class EdgeGuard
{
    private HexGrid hexGrid;

    public EdgeGuard(HexGrid grid)
    {
        hexGrid = grid;
    }

    public void Execute()
    {
        // EdgeGuard ensures ocean boundaries after all geological simulation
        // Applies stepped depth profile to guarantee navigable coastal waters
        
        ApplyWestEdgeDepths();
        ApplyEastEdgeDepths();
        
        UnityEngine.Debug.Log("EdgeGuard applied: Guaranteed ocean depths at map boundaries");
    }

    private void ApplyWestEdgeDepths()
    {
        // Apply stepped depth profile to western edge columns
        Hexagon[,] hexagons = hexGrid.GetHexagons();
        
        for (int y = 0; y < hexGrid.Height; y++)
        {
            // Column 0: Deep shipping lane (400m below sea level)
            if (hexagons[0, y] != null)
            {
                hexagons[0, y].Altitude = Math.Min(hexagons[0, y].Altitude, 9600f);
            }
            
            // Column 1: Harbor depth (200m below sea level)
            if (hexagons[1, y] != null)
            {
                hexagons[1, y].Altitude = Math.Min(hexagons[1, y].Altitude, 9800f);
            }
            
            // Column 2: Shallow coastal waters (100m below sea level)
            if (hexagons[2, y] != null)
            {
                hexagons[2, y].Altitude = Math.Min(hexagons[2, y].Altitude, 9900f);
            }
        }
    }

    private void ApplyEastEdgeDepths()
    {
        // Apply stepped depth profile to eastern edge columns
        Hexagon[,] hexagons = hexGrid.GetHexagons();
        
        for (int y = 0; y < hexGrid.Height; y++)
        {
            // Column Width-1: Deep shipping lane (400m below sea level)
            if (hexagons[hexGrid.Width - 1, y] != null)
            {
                hexagons[hexGrid.Width - 1, y].Altitude = Math.Min(hexagons[hexGrid.Width - 1, y].Altitude, 9600f);
            }
            
            // Column Width-2: Harbor depth (200m below sea level)
            if (hexagons[hexGrid.Width - 2, y] != null)
            {
                hexagons[hexGrid.Width - 2, y].Altitude = Math.Min(hexagons[hexGrid.Width - 2, y].Altitude, 9800f);
            }
            
            // Column Width-3: Shallow coastal waters (100m below sea level)
            if (hexagons[hexGrid.Width - 3, y] != null)
            {
                hexagons[hexGrid.Width - 3, y].Altitude = Math.Min(hexagons[hexGrid.Width - 3, y].Altitude, 9900f);
            }
        }
    }
}