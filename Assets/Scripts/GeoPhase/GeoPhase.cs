// GeoPhase.cs
using System.Collections.Generic;
using UnityEngine;
public class GeoPhase
{
    private HexGrid hexGrid;
    private float seaLevel;

    public GeoPhase(HexGrid hexGrid, float seaLevel)
    {
        Debug.Log("GeoPhase has been triggered");
        this.hexGrid = hexGrid;
        this.seaLevel = seaLevel;
    }

    public void ExecuteMagmaImpact()
    {
        MagmaImpact magmaImpact = new MagmaImpact(hexGrid);
        magmaImpact.Execute();
    }

    public void ExecuteSlump()
    {
        Slump slump = new Slump(hexGrid);
        slump.Execute();
    }


    public void ExecuteRefreshHexDisplay()
    {
        RefreshHexGridDisplay refreshHexDisplay = new RefreshHexGridDisplay(hexGrid, seaLevel);
        refreshHexDisplay.Execute();
    }
}

