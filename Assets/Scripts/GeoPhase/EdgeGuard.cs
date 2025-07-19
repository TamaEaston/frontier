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
        
        int westChanges = ApplyWestEdgeDepths();
        int eastChanges = ApplyEastEdgeDepths();
        
        UnityEngine.Debug.Log($"EdgeGuard applied: {westChanges} west edge changes, {eastChanges} east edge changes - Ocean boundaries secured");
    }

    private int ApplyWestEdgeDepths()
    {
        // Apply stepped depth profile to western edge columns
        Hexagon[,] hexagons = hexGrid.GetHexagons();
        int changes = 0;
        
        for (int y = 0; y < hexGrid.Height; y++)
        {
            // Column 0: Deep shipping lane (400m below sea level)
            if (hexagons[0, y] != null)
            {
                float originalAltitude = hexagons[0, y].Altitude;
                hexagons[0, y].Altitude = Math.Min(hexagons[0, y].Altitude, 9600f);
                if (hexagons[0, y].Altitude < originalAltitude) changes++;
            }
            
            // Column 1: Harbor depth (200m below sea level)
            if (hexagons[1, y] != null)
            {
                float originalAltitude = hexagons[1, y].Altitude;
                hexagons[1, y].Altitude = Math.Min(hexagons[1, y].Altitude, 9800f);
                if (hexagons[1, y].Altitude < originalAltitude) changes++;
            }
            
            // Column 2: Shallow coastal waters (100m below sea level)
            if (hexagons[2, y] != null)
            {
                float originalAltitude = hexagons[2, y].Altitude;
                hexagons[2, y].Altitude = Math.Min(hexagons[2, y].Altitude, 9900f);
                if (hexagons[2, y].Altitude < originalAltitude) changes++;
            }
        }
        
        return changes;
    }

    private int ApplyEastEdgeDepths()
    {
        // Apply stepped depth profile to eastern edge columns
        Hexagon[,] hexagons = hexGrid.GetHexagons();
        int changes = 0;
        
        for (int y = 0; y < hexGrid.Height; y++)
        {
            // Column Width-1: Deep shipping lane (400m below sea level)
            if (hexagons[hexGrid.Width - 1, y] != null)
            {
                float originalAltitude = hexagons[hexGrid.Width - 1, y].Altitude;
                hexagons[hexGrid.Width - 1, y].Altitude = Math.Min(hexagons[hexGrid.Width - 1, y].Altitude, 9600f);
                if (hexagons[hexGrid.Width - 1, y].Altitude < originalAltitude) changes++;
            }
            
            // Column Width-2: Harbor depth (200m below sea level)
            if (hexagons[hexGrid.Width - 2, y] != null)
            {
                float originalAltitude = hexagons[hexGrid.Width - 2, y].Altitude;
                hexagons[hexGrid.Width - 2, y].Altitude = Math.Min(hexagons[hexGrid.Width - 2, y].Altitude, 9800f);
                if (hexagons[hexGrid.Width - 2, y].Altitude < originalAltitude) changes++;
            }
            
            // Column Width-3: Shallow coastal waters (100m below sea level)
            if (hexagons[hexGrid.Width - 3, y] != null)
            {
                float originalAltitude = hexagons[hexGrid.Width - 3, y].Altitude;
                hexagons[hexGrid.Width - 3, y].Altitude = Math.Min(hexagons[hexGrid.Width - 3, y].Altitude, 9900f);
                if (hexagons[hexGrid.Width - 3, y].Altitude < originalAltitude) changes++;
            }
        }
        
        return changes;
    }
}