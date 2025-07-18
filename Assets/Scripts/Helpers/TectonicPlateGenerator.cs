using UnityEngine;
using System.Collections.Generic;

public class TectonicPlateGenerator
{
    public void GenerateTectonicPlate(Hexagon[,] hexagons, int Width, int Height, List<TectonicPlate> tectonicPlates, int MinHexagonsPerPlate, Hexagon startHex)
    {
        {
            bool OverRide = false;
            if (startHex == null)
            {
                do
                {
                    int x = Random.Range(0, Width);
                    int y = Random.Range(0, Height);
                    startHex = hexagons[x, y];
                } while (startHex.BelongsToPlate != '\0');

            }
            else
            {
                OverRide = true;
            }
            TectonicPlate plate = new TectonicPlate
            {
                PlateDirection = 60 * Random.Range(1, 7),
                PlateIntensity = Random.Range(25, 101),
                PlateMaxSize = Random.Range(MinHexagonsPerPlate, MinHexagonsPerPlate * 3),
                PlateID = (char)('A' + tectonicPlates.Count)
            };
            tectonicPlates.Add(plate);

            List<Hexagon> toProcess = new List<Hexagon> { startHex };
            int size = 0;

            while (toProcess.Count > 0 && size < plate.PlateMaxSize)
            {
                Hexagon hex = toProcess[Random.Range(0, toProcess.Count)];
                toProcess.Remove(hex);

                hex.BelongsToPlate = plate.PlateID;
                hex.MagmaDirection = plate.PlateDirection;
                hex.MagmaIntensity = plate.PlateIntensity;
                hex.PlateSelected = (OverRide) ? true : false;
                //            hex.SpriteRenderer.color = colours.GetPlateColour(plate.PlateID);
                size++;

                foreach (Hexagon neighbour in hex.Neighbours)
                {
                    if (neighbour != null && (OverRide || neighbour.BelongsToPlate == '\0') && Random.value < 0.5f)
                    {
                        toProcess.Add(neighbour);
                    }
                }
            }
            //UnityEngine.Debug.Log("Tectonic Plate " + plate.PlateID + " has " + size + " hexagons.");
        }
    }
}