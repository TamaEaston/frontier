using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEditorInternal;
using Helpers;

public class TectonicPlate
{
    public int PlateDirection { get; set; }
    public int PlateIntensity { get; set; }
    public int PlateMaxSize { get; set; }
    public char PlateID { get; set; }
}

public class HexGrid : MonoBehaviour
{
    public Hexagon HexagonPrefab;
    public MagmaOverlay MagmaOverlayPrefab;
    public int Width;
    public int Height;
    public int NumberOfPlates;
    private int MinHexagonsPerPlate;
    public float SeaLevel = 10000;

    private Hexagon[,] hexagons;

    public Hexagon[,] GetHexagons()
    {
        return hexagons;
    }
    public HexGridColours colours = new HexGridColours();
    public List<TectonicPlate> tectonicPlates = new List<TectonicPlate>();
    public List<DisplayOverlay> Overlays { get; set; }


    void Start()
    {
        NumberOfPlates = Mathf.Min(NumberOfPlates, 24); // Apply the restriction
        hexagons = new Hexagon[Width, Height];

        float xDistance = 0.9f; // Adjust this value as necessary
        float yDistance = Mathf.Sqrt(3) * 0.45f; // Adjust this value as necessary


        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                float xOffset = (y % 2 == 0) ? 0 : xDistance / 2;
                float centerOffsetX = (Width * xDistance) / 2;
                float centerOffsetY = (Height * yDistance) / 2;

                float positionX = x * xDistance + xOffset - centerOffsetX;
                float positionY = y * yDistance - centerOffsetY;

                Hexagon hex = Instantiate(HexagonPrefab, new Vector3(positionX, positionY, 0), Quaternion.Euler(0, 0, 90));

                hex.PosX = positionX;
                hex.PosY = positionY;

                DisplayOverlay magmaOverlay = Instantiate(MagmaOverlayPrefab, new Vector3(positionX, positionY, 0), Quaternion.Euler(0, 0, 90));

                // Assign a random altitude
                hex.Altitude = Random.Range(9500, 10501);
                float heightAboveSeaLevel = hex.Altitude - SeaLevel;
                hex.SpriteRenderer.color = colours.GetAltitudeColour(heightAboveSeaLevel);

                // Assign magma intensity & direction as West
                hex.MagmaIntensity = 25;
                hex.MagmaDirection = 270;

                // assign wind intensity & direction as East
                hex.WindIntensity = 25;
                hex.WindDirection = 90;

                hexagons[x, y] = hex;
            }
        }

        // Set Neighbours...
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                hexagons[i, j].Neighbours[0] = hexagons[(i + 1) % Width, j]; // East
                hexagons[i, j].Neighbours[1] = j < Height - 1 ? hexagons[(i + 1) % Width, j + 1] : null; // South-East
                hexagons[i, j].Neighbours[2] = j < Height - 1 ? hexagons[i, j + 1] : null; // South-West
                hexagons[i, j].Neighbours[3] = hexagons[(i - 1 + Width) % Width, j]; // West
                hexagons[i, j].Neighbours[4] = j > 0 ? hexagons[(i - 1 + Width) % Width, j - 1] : null; // North-West
                hexagons[i, j].Neighbours[5] = j > 0 ? hexagons[i, j - 1] : null; // North-East   

                // North-West and North-East neighbours for top row
                if (j == 0)
                {
                    hexagons[i, j].Neighbours[4] = hexagons[Width / 2, 0]; // North-West
                    hexagons[i, j].Neighbours[5] = hexagons[Width / 2, 0]; // North-East
                }
                else if (j == Height - 1)
                // South-East and South-West neighbours for bottom row
                {
                    hexagons[i, j].Neighbours[1] = hexagons[Width / 2, Height - 1]; // South-East
                    hexagons[i, j].Neighbours[2] = hexagons[Width / 2, Height - 1]; // South-West
                }
            }
        }

        // Generate TectonicPlates
        int NumbersOfHexagons = Width * Height;
        MinHexagonsPerPlate = NumbersOfHexagons / NumberOfPlates;
        for (int i = 0; i < NumberOfPlates; i++)
        {
            GenerateTectonicPlate();
        }


    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            GeoPhase phase = new GeoPhase(this, SeaLevel); // Replace SeaLevel with the actual sea level value
            phase.ExecuteMagmaImpact();
            phase.ExecuteSlump();
            phase.ExecuteRefreshHexDisplay();

            stopwatch.Stop();
            UnityEngine.Debug.Log("Time taken: " + stopwatch.ElapsedMilliseconds + " ms");
        }
    }


    void GenerateTectonicPlate()
    {
        Hexagon startHex = null;
        do
        {
            int x = Random.Range(0, Width);
            int y = Random.Range(0, Height);
            startHex = hexagons[x, y];
        } while (startHex.BelongsToPlate != '\0');

        TectonicPlate plate = new TectonicPlate
        {
            PlateDirection = 30 * Random.Range(1, 7),
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
            hex.SpriteRenderer.color = colours.GetPlateColour(plate.PlateID);
            size++;

            foreach (Hexagon neighbour in hex.Neighbours)
            {
                if (neighbour != null && neighbour.BelongsToPlate == '\0' && Random.value < 0.5f)
                {
                    toProcess.Add(neighbour);
                }
            }
        }
    }


}