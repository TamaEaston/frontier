// GeoPhase.cs
using System.Collections.Generic;
using UnityEngine;
public class GeoPhase
{
    private HexGrid hexGrid;
    private List<Biome> biomes;
    private float seaLevel;

    public GeoPhase(HexGrid hexGrid, List<Biome> biomes)
    {
        Debug.Log("GeoPhase has been triggered");
        this.hexGrid = hexGrid;
        this.seaLevel = hexGrid.SeaLevel;
        this.biomes = biomes; // Add this line
    }
    public void ExecuteClimateTemperature()
    {
        ClimateTemperature climateTemperature = new ClimateTemperature(hexGrid);
        climateTemperature.Execute();
    }

    public void ExecuteFertilityAssessment()
    {
        FertilityAssessment fertilityAssessment = new FertilityAssessment(hexGrid);
        fertilityAssessment.Execute();
    }

    public void ExecuteTerrainAnalysis()
    {
        TerrainAnalysis terrainAnalysis = new TerrainAnalysis(hexGrid);
        terrainAnalysis.Execute();
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

    public void ExecuteSeaLevel()
    {
        SeaLevel seaLevel = new SeaLevel(hexGrid);
        seaLevel.Execute();
    }

    public void ExecuteWindEffect()
    {
        this.seaLevel = hexGrid.SeaLevel;
        WindEffect windEffect = new WindEffect(hexGrid, seaLevel);
        windEffect.Execute();
    }

    public void ExecuteRiverFlow()
    {
        this.seaLevel = hexGrid.SeaLevel;
        RiverFlow riverFlow = new RiverFlow(hexGrid, seaLevel);
        riverFlow.Execute();
    }


    public void ExecuteSetBiomes()
    {
        SetBiomes setBiomes = new SetBiomes(hexGrid, biomes);
        setBiomes.Execute();
    }

    public void ExecuteEdgeGuard()
    {
        EdgeGuard edgeGuard = new EdgeGuard(hexGrid);
        edgeGuard.Execute();
    }

    public void ExecuteRefreshHexDisplay()
    {
        this.seaLevel = hexGrid.SeaLevel;
        RefreshHexGridDisplay refreshHexDisplay = new RefreshHexGridDisplay(hexGrid, seaLevel);
        refreshHexDisplay.Execute();
    }
}

