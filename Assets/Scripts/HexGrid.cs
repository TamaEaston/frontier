using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEditorInternal;
using Helpers;
using TMPro;

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
    public Arrow ArrowPrefab;
    public Settlement SettlementPrefab;
    public TextMeshProUGUI SeaLevelTextPrefab;
    public int Width;
    public int Height;
    public int NumberOfPlates;
    private int MinHexagonsPerPlate;
    public float EquatorSolarIntensity;
    public int EquatorToPolarSolarIntensityDifference;

    public int Era = 0;
    public float SeaLevel = 10000;
    public float GenesisSeaLevel = 10000;
    public float SeaPerHex = 0;
    public float AverageGlobalTemperature = 15;

    private Hexagon[,] hexagons;
    public Hexagon[,] GetHexagons()
    {
        return hexagons;
    }

    private Arrow[,] arrows;
    public Arrow[,] GetArrows()
    {
        return arrows;
    }
    private Settlement[,] settlements;
    public Settlement[,] GetSettlements()
    {
        return settlements;
    }

    private RiverOverlay[,] riverLines;
    public RiverOverlay[,] getRiverLines()
    {
        return riverLines;
    }


    public HexGridColours colours = new HexGridColours();
    List<TectonicPlate> tectonicPlates = new List<TectonicPlate>();
    public BiomeLoader biomeLoader;
    public List<Biome> biomes;

    void Start()
    {

        NumberOfPlates = Mathf.Min(NumberOfPlates, 24); // Apply the restriction
        hexagons = new Hexagon[Width, Height];
        arrows = new Arrow[Width, Height];
        settlements = new Settlement[Width, Height];

        float xDistance = 1.2f; // Adjust this value as necessary
        float yDistance = Mathf.Sqrt(3) * 0.6f; // Adjust this value as necessary

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                float xOffset = (j % 2 == 0) ? 0 : xDistance / 2;
                float centerOffsetX = (Width * xDistance) / 2;
                float centerOffsetY = (Height * yDistance) / 2;
                float positionX = i * xDistance + xOffset - centerOffsetX;
                float positionY = j * yDistance - centerOffsetY;

                // Assuming this is line 80
                Hexagon hex = Instantiate(HexagonPrefab, new Vector3(positionX, positionY, 0), Quaternion.identity);
                Arrow arrow = Instantiate(ArrowPrefab, new Vector3(positionX, positionY, -5), Quaternion.identity);
                Settlement settlement = Instantiate(SettlementPrefab, new Vector3(positionX, positionY, -4), Quaternion.identity);

                hex.hexGrid = this;

                // Set the ID & position of the hexagon
                hex.HexagonID = i + "-" + j;
                hex.PositionX = i;
                hex.PositionY = j;

                // Assign a random altitude
                hex.Altitude = Random.Range(9000, 11000);
                hex.AltitudeOld = hex.Altitude;

                // Assign magma intensity & direction as East
                hex.MagmaIntensity = 25;
                hex.MagmaDirection = 90;

                // Assign Rainfall for testing
                hex.Rainfall = 25;

                // Calculate solar intensity
                float latitude = Mathf.Clamp(2.0f * j / (Height - 1) - 1.0f, -1.0f, 1.0f);
                float power = 0.6f;
                float sinValue = Mathf.Sin((latitude + 1) * Mathf.PI / 2.0f);
                hex.SolarIntensity = (EquatorSolarIntensity - EquatorToPolarSolarIntensityDifference) + EquatorToPolarSolarIntensityDifference * Mathf.Pow(Mathf.Abs(sinValue), power);

                hexagons[i, j] = hex;
                arrows[i, j] = arrow;
                settlements[i, j] = settlement;
            }
        }

        // Set Neighbours...
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                if (j % 2 == 0) // Even row
                {
                    hexagons[i, j].Neighbours[0] = i < Width - 1 ? hexagons[i + 1, j] : null; // East
                    hexagons[i, j].Neighbours[1] = j < Height - 1 ? hexagons[i, j + 1] : null; // South-East
                    hexagons[i, j].Neighbours[2] = j < Height - 1 && i > 0 ? hexagons[i - 1, j + 1] : null; // South-West
                    hexagons[i, j].Neighbours[3] = i > 0 ? hexagons[i - 1, j] : null; // West
                    hexagons[i, j].Neighbours[4] = j > 0 && i > 0 ? hexagons[i - 1, j - 1] : null; // North-West
                    hexagons[i, j].Neighbours[5] = j > 0 ? hexagons[i, j - 1] : null; // North-East
                }
                else // Odd row
                {
                    hexagons[i, j].Neighbours[0] = i < Width - 1 ? hexagons[i + 1, j] : null; // East
                    hexagons[i, j].Neighbours[1] = j < Height - 1 && i < Width - 1 ? hexagons[i + 1, j + 1] : null; // South-East
                    hexagons[i, j].Neighbours[2] = j < Height - 1 ? hexagons[i, j + 1] : null; // South-West
                    hexagons[i, j].Neighbours[3] = i > 0 ? hexagons[i - 1, j] : null; // West
                    hexagons[i, j].Neighbours[4] = j > 0 ? hexagons[i, j - 1] : null; // North-West
                    hexagons[i, j].Neighbours[5] = j > 0 && i < Width - 1 ? hexagons[i + 1, j - 1] : null; // North-East
                }
            }
        }

        // Generate TectonicPlates
        int NumbersOfHexagons = Width * Height;
        MinHexagonsPerPlate = NumbersOfHexagons / NumberOfPlates;
        for (int i = 0; i < NumberOfPlates; i++)
        {
            TectonicPlateGenerator generator = new TectonicPlateGenerator();
            generator.GenerateTectonicPlate(hexagons, Width, Height, tectonicPlates, MinHexagonsPerPlate, null);
        }

        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        GeoPhase phase = new GeoPhase(this, biomes);
        int NumberOfGeoPhases = 25;
        for (int i = 0; i < NumberOfGeoPhases; i++)
        {
            phase.ExecuteMagmaImpact();
            phase.ExecuteSlump();
            phase.ExecuteSeaLevel();
            phase.ExecuteWindEffect();
            phase.ExecuteRiverFlow();
        }
        UnityEngine.Debug.Log("Time taken for " + NumberOfGeoPhases + " GeoPhases : " + stopwatch.ElapsedMilliseconds + " ms. Average: " + stopwatch.ElapsedMilliseconds / NumberOfGeoPhases + " ms");
        phase.ExecuteHumanComfortAssessment();
        GenesisSeaLevel = SeaLevel;
        AddHumanPopulation();
        phase.ExecuteSetBiomes();
        phase.ExecuteRefreshHexDisplay();

    }

    void Update()
    {

        // if (biomes == null)
        // {
        //     UnityEngine.Debug.LogError("Biomes is null in Update");
        // }
        // else
        // {
        //     UnityEngine.Debug.Log("Number of biomes in Update: " + biomes.Count);
        // }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            Era += 1;
            GeoPhase phase = new GeoPhase(this, biomes);
            phase.ExecuteClimateTemperature();
            phase.ExecuteMagmaImpact();
            phase.ExecuteSlump();
            phase.ExecuteSeaLevel();
            phase.ExecuteWindEffect();
            phase.ExecuteRiverFlow();
            phase.ExecuteHumanComfortAssessment();
            phase.ExecutePopulationGrowth();
            phase.ExecuteSetBiomes();
            phase.ExecuteRefreshHexDisplay();

            stopwatch.Stop();
            UnityEngine.Debug.Log("Time taken: " + stopwatch.ElapsedMilliseconds + " ms");
        }

        //Toggle Overlays
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            toggleOverlay("None");
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            toggleOverlay("Magma");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            toggleOverlay("Altitude");
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            toggleOverlay("Weather");
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            toggleOverlay("Temperature");
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            toggleOverlay("HumanComfort");
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            toggleOverlay("Settlement");
        }
    }

    private void Awake()
    {
        // Check if BiomeLoader is not null
        if (biomeLoader == null)
        {
            UnityEngine.Debug.LogError("BiomeLoader is not set. Please assign it in the Unity editor.");
            return;
        }

        // Now you can access the biomes loaded in BiomeLoader
        biomes = biomeLoader.Biomes;

        // Check if biomes are not null
        if (biomes == null)
        {
            UnityEngine.Debug.LogError("Biomes not loaded in BiomeLoader.");
            return;
        }

        // Output the number of loaded biomes
        UnityEngine.Debug.Log("Number of loaded biomes: " + biomes.Count);
    }

    void toggleOverlay(string Overlay)
    {
        if (GameSettings.ActiveOverlay != Overlay)
        {
            GameSettings.ActiveOverlay = Overlay;
        }
        else if (GameSettings.ActiveOverlay == Overlay)
        {
            GameSettings.ActiveOverlay = "None";
        }
        UnityEngine.Debug.Log("Active Overlay: " + GameSettings.ActiveOverlay);
        GeoPhase phase = new GeoPhase(this, biomes);
        phase.ExecuteRefreshHexDisplay();
    }

    void AddHumanPopulation()
    {
        List<Hexagon> eligibleHexagons = this.GetHexagons()
            .OfType<Hexagon>()
            .Where(hex => hex.HumanComfortIndex >= 90 && hex.AltitudeVsSeaLevel > 100)
            .ToList();

        List<Hexagon> centersOfPopulation = new List<Hexagon>();

        // Randomly shuffle the eligible hexagons
        System.Random rng = new System.Random();
        int n = eligibleHexagons.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Hexagon value = eligibleHexagons[k];
            eligibleHexagons[k] = eligibleHexagons[n];
            eligibleHexagons[n] = value;
        }

        foreach (Hexagon hex in eligibleHexagons)
        {
            if (centersOfPopulation.Count == 5)
            {
                break;
            }

            bool isFarEnough = true;
            foreach (Hexagon center in centersOfPopulation)
            {
                if (HexDistance(hex, center) < 6)
                {
                    isFarEnough = false;
                    break;
                }
            }

            if (isFarEnough)
            {
                hex.HumanPopulation = 50; // Set the population of the hex to 50

                // Assign the hex to a Civilisation
                hex.Civilisation = Civilisation.civilisations[centersOfPopulation.Count];

                centersOfPopulation.Add(hex);
            }
        }

    }

    int HexDistance(Hexagon a, Hexagon b)
    {
        // Convert offset coordinates to cube coordinates
        int ax = a.PositionX - (a.PositionY - (a.PositionY & 1)) / 2;
        int az = a.PositionY;
        int ay = -ax - az;

        int bx = b.PositionX - (b.PositionY - (b.PositionY & 1)) / 2;
        int bz = b.PositionY;
        int by = -bx - bz;

        // Calculate the distance between the two hexes
        return (Mathf.Abs(ax - bx) + Mathf.Abs(ay - by) + Mathf.Abs(az - bz)) / 2;
    }

    public void GeneratePlateAtHexagon(Hexagon hex)
    {
        TectonicPlateGenerator generator = new TectonicPlateGenerator();
        generator.GenerateTectonicPlate(hexagons, Width, Height, tectonicPlates, MinHexagonsPerPlate, hex);
    }

    public void RefreshDisplay(bool DisplayOnly = true)
    {
        GeoPhase phase = new GeoPhase(this, biomes);
        if (!DisplayOnly)
        {
            phase.ExecuteMagmaImpact();
            phase.ExecuteSlump();
            phase.ExecuteWindEffect();
            phase.ExecuteRiverFlow();
            phase.ExecuteHumanComfortAssessment();
            phase.ExecutePopulationGrowth();
            phase.ExecuteSetBiomes();
        }

        phase.ExecuteRefreshHexDisplay();
    }

}