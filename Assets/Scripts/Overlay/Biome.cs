using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Biome
{
    public string Name;
    public string Image;
    public string Colour;
    public Range HeightAboveSeaLevel;
    public Range Temperature;
    public Range Rainfall;
    public Range SurfaceWater;
}

[System.Serializable]
public class Range
{
    public int Min;
    public int Max;
}

[System.Serializable]
public class BiomeList
{
    public List<Biome> biomes;
}