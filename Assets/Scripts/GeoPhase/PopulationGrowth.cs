using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class PopulationGrowth
{
    private HexGrid hexGrid;

    public PopulationGrowth(HexGrid hexGrid)
    {
        this.hexGrid = hexGrid;
    }

    public void Execute()
    {
        // Flatten the 2D array into a 1D list and order hexagons by altitude from highest to lowest
        var populatedHexagons = hexGrid.GetHexagons()
            .OfType<Hexagon>()
            .Where(hex => hex.HumanPopulation > 0)
            .OrderByDescending(n => n.HumanPopulation)
            .ToList();

        foreach (var hex in populatedHexagons)
        {
            hex.HumanPopulationNew = hex.HumanPopulation;
        }

        foreach (var hex in populatedHexagons)
        {
            // 1. If HumanComfortIndex is larger than HumanPopulation then add 0.25 to HumanPopulationNew.
            if (hex.HumanComfortIndex > hex.HumanPopulation)
            {
                hex.HumanPopulationNew += (hex.HumanComfortIndex / 20);
            }

            // 2. If rounded (HumanPopulationNew / 10) higher than rounded (HumanPopulation / 10) then make a 50% chance check to see if a new hex is going to be settled.
            if (Mathf.Round(hex.HumanPopulationNew / 10) > Mathf.Round(hex.HumanPopulation / 10))
            {
                if (new System.Random().NextDouble() < ((hex.HumanPopulationNew + 10) / 100)) // 50% chance
                {
                    // 3. If a new hex is going to be settled then check the HumanComfortIndex of all of the hexes within 3 hexes with zero HumanPopulation - using the Hexagon.Neighbours relationship. 
                    // Randomly select one of the neighbouring hexes with the highest HumanComfortIndex. Add 10 to the HumanPopulationNew of the the new settled hex & remove 10 from the HumanPopulationNew of the existing hex.
                    var eligibleNeighbours = hex.Neighbours
                        .Concat(hex.Neighbours.SelectMany(n => n.Neighbours)) // Include the neighbours of neighbours
                        .Where(n => n.HumanPopulation == 0 && n.HumanComfortIndex >= 10)
                        .OrderByDescending(n => n.HumanComfortIndex)
                        .Distinct() // Remove duplicates
                        .ToList();

                    if (eligibleNeighbours.Any())
                    {
                        var newHex = eligibleNeighbours.FirstOrDefault();
                        newHex.HumanPopulationNew += 10;
                        newHex.Civilisation = hex.Civilisation;
                        hex.HumanPopulationNew -= 10;
                    }
                }
            }

            if (hex.HumanPopulationNew > hex.HumanPopulation && Mathf.Round(hex.HumanPopulationNew / 10) > 4)
            {
                foreach (var otherHex in populatedHexagons)
                {
                    if (Mathf.Round(otherHex.HumanPopulationNew / 10) > 4)
                    {
                        // Calculate the distance between hex and otherHex
                        float distance = Mathf.Abs(hex.PositionX - otherHex.PositionX);

                        // Check if otherHex is within ((hex.HumanPopulation / 10) - 4) hexes
                        if (distance <= ((hex.HumanPopulation / 10) - 4) && hex.HumanPopulationNew < otherHex.HumanPopulation)
                        {
                            // Calculate the cap on hex.HumanPopulationNew
                            float cap = otherHex.HumanPopulation - 20;

                            // If the cap is less than hex.HumanPopulationNew, set hex.HumanPopulationNew to the cap
                            if (cap < hex.HumanPopulationNew)
                            {
                                // Debug.Log($"hex.HumanPopulationNew: {hex.HumanPopulationNew}, cap: {cap}");
                                hex.HumanPopulationNew = cap;
                            }
                        }
                    }
                }
            }

            // 4. If the HumanComfortIndex is smaller than HumanPopulation then remove 0.5 from HumanPopulationNew.
            hex.HumanPopulationNew = hex.HumanComfortIndex < 10 ? 0 : (hex.HumanComfortIndex < hex.HumanPopulation ? hex.HumanPopulationNew - 10f : hex.HumanPopulationNew);
        }
        var allHexagons = hexGrid.GetHexagons()
            .OfType<Hexagon>()
            .ToList();

        foreach (var hex in allHexagons)
        {
            hex.HumanPopulation = Mathf.Max(0, Mathf.Min(100, hex.HumanPopulationNew));
        }

    }
}