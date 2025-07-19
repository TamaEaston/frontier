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
    public RiverOverlay RiverOverlayPrefab;
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
    public bool IsNorthArctic; // Fixed climate assignment for entire map lifecycle

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
    
    private ViewManager viewManager;

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
        // Set fixed climate assignment for entire map lifecycle (randomized once)
        IsNorthArctic = UnityEngine.Random.value > 0.5f;

        NumberOfPlates = Mathf.Min(NumberOfPlates, 24); // Apply the restriction
        hexagons = new Hexagon[Width, Height];
        arrows = new Arrow[Width, Height];
        riverLines = new RiverOverlay[Width, Height];

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
                RiverOverlay riverOverlay = null;
                if (RiverOverlayPrefab != null)
                {
                    riverOverlay = Instantiate(RiverOverlayPrefab, new Vector3(positionX, positionY, -2), Quaternion.identity);
                }

                hex.hexGrid = this;

                // Set the ID & position of the hexagon
                hex.HexagonID = i + "-" + j;
                hex.PositionX = i;
                hex.PositionY = j;

                // Continental terrain generation will be done after grid creation
                hex.Altitude = 9500; // Temporary deep ocean level
                hex.AltitudeOld = hex.Altitude;

                // Assign magma intensity & direction as East
                hex.MagmaIntensity = 25;
                hex.MagmaDirection = 90;

                // Assign Rainfall for testing
                hex.Rainfall = 25;

                // SolarIntensity will be set by ClimateTemperature.cs during simulation
                hex.SolarIntensity = 0f;

                hexagons[i, j] = hex;
                arrows[i, j] = arrow;
                riverLines[i, j] = riverOverlay;
            }
        }

        // Generate Continental Terrain
        GenerateContinentalTerrain();

        // Apply EdgeGuard immediately after terrain generation to ensure ocean boundaries
        GeoPhase edgePhase = new GeoPhase(this, biomes);
        edgePhase.ExecuteEdgeGuard();

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
        int NumberOfGeoPhases = 5;
        for (int i = 0; i < NumberOfGeoPhases; i++)
        {
            Era += 1; // Count initial generation cycles
            phase.ExecuteMagmaImpact();
            phase.ExecuteSlump();
            phase.ExecuteSeaLevel();
            phase.ExecuteClimateTemperature();
            phase.ExecuteFertilityAssessment();
            phase.ExecuteWindEffect();
            phase.ExecuteRiverFlow();
        }
        UnityEngine.Debug.Log("Time taken for " + NumberOfGeoPhases + " GeoPhases : " + stopwatch.ElapsedMilliseconds + " ms. Average: " + stopwatch.ElapsedMilliseconds / NumberOfGeoPhases + " ms");
        GenesisSeaLevel = SeaLevel;
        phase.ExecuteSetBiomes();
        
        // Initialize the new view system
        viewManager = new ViewManager(this);
        viewManager.RefreshDisplay();

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
            
            // Run 5 geological cycles per Space press
            for (int cycle = 0; cycle < 5; cycle++)
            {
                Era += 1;
                GeoPhase phase = new GeoPhase(this, biomes);
                phase.ExecuteClimateTemperature();
                phase.ExecuteFertilityAssessment();
                phase.ExecuteMagmaImpact();
                phase.ExecuteSlump();
                phase.ExecuteSeaLevel();
                phase.ExecuteWindEffect();
                phase.ExecuteRiverFlow();
                phase.ExecuteSetBiomes();
                phase.ExecuteEdgeGuard();
                
                // Only refresh display on the final cycle for performance
                if (cycle == 4)
                {
                    if (viewManager != null)
                    {
                        viewManager.RefreshDisplay();
                    }
                    else
                    {
                        phase.ExecuteRefreshHexDisplay();
                    }
                }
            }

            stopwatch.Stop();
            UnityEngine.Debug.Log($"5 geological cycles completed in {stopwatch.ElapsedMilliseconds} ms (Era {Era})");
        }

        // Regenerate map when R is pressed
        if (Input.GetKeyDown(KeyCode.R))
        {
            UnityEngine.Debug.Log("Regenerating map...");
            RegenerateMap();
        }

        //Toggle Views using ViewManager
        if (viewManager != null)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                viewManager.HandleViewInput(KeyCode.Alpha0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                viewManager.HandleViewInput(KeyCode.Alpha1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                viewManager.HandleViewInput(KeyCode.Alpha2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                viewManager.HandleViewInput(KeyCode.Alpha3);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                viewManager.HandleViewInput(KeyCode.Alpha4);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                viewManager.HandleViewInput(KeyCode.Alpha5);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                viewManager.HandleViewInput(KeyCode.Alpha6);
            }

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
        
        // Use ViewManager for display refresh
        if (viewManager != null)
        {
            viewManager.RefreshDisplay();
        }
        else
        {
            GeoPhase phase = new GeoPhase(this, biomes);
            phase.ExecuteRefreshHexDisplay();
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
        if (!DisplayOnly)
        {
            GeoPhase phase = new GeoPhase(this, biomes);
            phase.ExecuteMagmaImpact();
            phase.ExecuteSlump();
            phase.ExecuteClimateTemperature();
            phase.ExecuteFertilityAssessment();
            phase.ExecuteWindEffect();
            phase.ExecuteRiverFlow();
            phase.ExecuteSetBiomes();
            phase.ExecuteEdgeGuard();
        }

        // Use ViewManager for display refresh
        if (viewManager != null)
        {
            viewManager.RefreshDisplay();
        }
        else
        {
            // Fallback to old system if ViewManager not initialized
            GeoPhase phase = new GeoPhase(this, biomes);
            phase.ExecuteRefreshHexDisplay();
        }
    }

    private void GenerateContinentalTerrain()
    {
        // Realistic continental terrain optimized for human settlement
        // Sea level = 10000m (fixed), Ocean = 7500m (-2500m relative to sea level)
        // Target: 80%+ of landmass at 100-800m elevation for optimal settlement
        
        // 1. Define 7-zone geographic layout with realistic altitudes
        
        // Western Coast (3-8% of width) - with coastal noise
        float westernCoastBaseNorthX = Random.Range(Width * 0.03f, Width * 0.08f);
        float westernCoastBaseSouthX = Random.Range(Width * 0.03f, Width * 0.08f);
        float coastalAltitude = Random.Range(10100f, 10300f); // 100-300m above sea level
        
        // Western Coastal Plains (8-20% of width) - prime agricultural land
        float westernPlainsNorthX = Random.Range(Width * 0.08f, Width * 0.20f);
        float westernPlainsSouthX = Random.Range(Width * 0.08f, Width * 0.20f);
        float coastalPlainsAltitude = Random.Range(10100f, 10300f); // 100-300m above sea level
        
        // Western Mountains (20-30% of width) - NARROWER but dramatic
        float westernMountainsNorthX = Random.Range(Width * 0.20f, Width * 0.30f);
        float westernMountainsSouthX = Random.Range(Width * 0.20f, Width * 0.30f);
        float westernMountainsAltitude = Random.Range(11000f, 14000f); // 1000-4000m above sea level
        
        // Central Plains (30-70% of width) - fertile continental interior
        float centralPlainsNorthX = (westernMountainsNorthX + Random.Range(Width * 0.70f, Width * 0.80f)) / 2f;
        float centralPlainsSouthX = (westernMountainsSouthX + Random.Range(Width * 0.70f, Width * 0.80f)) / 2f;
        float centralPlainsAltitude = Random.Range(10200f, 10600f); // 200-600m above sea level - OPTIMAL
        
        // Eastern Mountains (70-80% of width) - NARROWER but dramatic
        float easternMountainsNorthX = Random.Range(Width * 0.70f, Width * 0.80f);
        float easternMountainsSouthX = Random.Range(Width * 0.70f, Width * 0.80f);
        float easternMountainsAltitude = Random.Range(11000f, 14000f); // 1000-4000m above sea level
        
        // Eastern Coastal Plains (80-92% of width) - prime agricultural land
        float easternPlainsNorthX = Random.Range(Width * 0.80f, Width * 0.92f);
        float easternPlainsSouthX = Random.Range(Width * 0.80f, Width * 0.92f);
        float easternCoastalPlainsAltitude = Random.Range(10100f, 10300f); // 100-300m above sea level
        
        // Eastern Coast (92-97% of width) - with coastal noise
        float easternCoastBaseNorthX = Random.Range(Width * 0.92f, Width * 0.97f);
        float easternCoastBaseSouthX = Random.Range(Width * 0.92f, Width * 0.97f);
        float easternCoastalAltitude = Random.Range(10100f, 10300f); // 100-300m above sea level
        
        // 2. Create natural coastlines with Perlin noise and 7-zone interpolation
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                Hexagon hex = hexagons[i, j];
                float x = i;
                
                // Calculate position along north-south axis (0 = north, 1 = south)
                float northSouthRatio = (float)j / (Height - 1);
                
                // Apply coastal noise for natural bays and inlets
                float largeBayNoise = Mathf.PerlinNoise(j * 0.1f, 0) * 0.05f * Width; // Large bays
                float smallInletNoise = Mathf.PerlinNoise(j * 0.3f, 1000) * 0.02f * Width; // Small inlets
                float coastalNoise = largeBayNoise + smallInletNoise;
                
                // Interpolate geographic zones with coastal noise
                float westernCoastX = Mathf.Lerp(westernCoastBaseNorthX, westernCoastBaseSouthX, northSouthRatio) + coastalNoise;
                float westernPlainsX = Mathf.Lerp(westernPlainsNorthX, westernPlainsSouthX, northSouthRatio);
                float westernMountainsX = Mathf.Lerp(westernMountainsNorthX, westernMountainsSouthX, northSouthRatio);
                float centralPlainsX = Mathf.Lerp(centralPlainsNorthX, centralPlainsSouthX, northSouthRatio);
                float easternMountainsX = Mathf.Lerp(easternMountainsNorthX, easternMountainsSouthX, northSouthRatio);
                float easternPlainsX = Mathf.Lerp(easternPlainsNorthX, easternPlainsSouthX, northSouthRatio);
                float easternCoastX = Mathf.Lerp(easternCoastBaseNorthX, easternCoastBaseSouthX, northSouthRatio) + coastalNoise;
                
                // Constrain coastlines to stay within bounds (3% to 97%)
                westernCoastX = Mathf.Clamp(westernCoastX, Width * 0.03f, Width * 0.08f);
                easternCoastX = Mathf.Clamp(easternCoastX, Width * 0.92f, Width * 0.97f);
                
                float finalAltitude;
                
                // 7-zone altitude interpolation with realistic human settlement elevations
                if (x <= westernCoastX)
                {
                    // West Ocean to Western Coast transition
                    float t = x / westernCoastX;
                    finalAltitude = Mathf.Lerp(7500f, coastalAltitude, t);
                }
                else if (x <= westernPlainsX)
                {
                    // Western Coast to Western Coastal Plains transition
                    float t = (x - westernCoastX) / (westernPlainsX - westernCoastX);
                    finalAltitude = Mathf.Lerp(coastalAltitude, coastalPlainsAltitude, t);
                }
                else if (x <= westernMountainsX)
                {
                    // Western Coastal Plains to Western Mountains transition
                    float t = (x - westernPlainsX) / (westernMountainsX - westernPlainsX);
                    finalAltitude = Mathf.Lerp(coastalPlainsAltitude, westernMountainsAltitude, t);
                }
                else if (x <= centralPlainsX)
                {
                    // Western Mountains to Central Plains transition
                    float t = (x - westernMountainsX) / (centralPlainsX - westernMountainsX);
                    finalAltitude = Mathf.Lerp(westernMountainsAltitude, centralPlainsAltitude, t);
                }
                else if (x <= easternMountainsX)
                {
                    // Central Plains to Eastern Mountains transition
                    float t = (x - centralPlainsX) / (easternMountainsX - centralPlainsX);
                    finalAltitude = Mathf.Lerp(centralPlainsAltitude, easternMountainsAltitude, t);
                }
                else if (x <= easternPlainsX)
                {
                    // Eastern Mountains to Eastern Coastal Plains transition
                    float t = (x - easternMountainsX) / (easternPlainsX - easternMountainsX);
                    finalAltitude = Mathf.Lerp(easternMountainsAltitude, easternCoastalPlainsAltitude, t);
                }
                else if (x <= easternCoastX)
                {
                    // Eastern Coastal Plains to Eastern Coast transition
                    float t = (x - easternPlainsX) / (easternCoastX - easternPlainsX);
                    finalAltitude = Mathf.Lerp(easternCoastalPlainsAltitude, easternCoastalAltitude, t);
                }
                else
                {
                    // Eastern Coast to East Ocean transition
                    float t = (x - easternCoastX) / ((Width - 1) - easternCoastX);
                    finalAltitude = Mathf.Lerp(easternCoastalAltitude, 7500f, t);
                }
                
                // Enforce minimum continental altitude (prevent inland seas)
                if (x > westernCoastX && x < easternCoastX)
                {
                    finalAltitude = Mathf.Max(finalAltitude, 10100f); // Minimum 100m above sea level
                }
                
                // Add zone-appropriate terrain variation
                float terrainNoise;
                if (x <= westernCoastX || x >= easternCoastX)
                {
                    // Coastal areas: Moderate variation for beaches, cliffs, dunes
                    terrainNoise = Random.Range(-50f, 50f);
                }
                else if (x <= westernPlainsX || x >= easternPlainsX)
                {
                    // Coastal plains: Rolling hills and valleys
                    terrainNoise = Random.Range(-100f, 100f);
                }
                else if (x <= westernMountainsX || x >= easternMountainsX)
                {
                    // Mountains: High variation for peaks, valleys, ridges
                    terrainNoise = Random.Range(-300f, 300f);
                }
                else
                {
                    // Central plains: Gentle rolling terrain
                    terrainNoise = Random.Range(-75f, 75f);
                }
                
                finalAltitude += terrainNoise;
                
                hex.Altitude = finalAltitude;
                hex.AltitudeOld = hex.Altitude;
            }
        }
        
        UnityEngine.Debug.Log($"Realistic continental terrain generated: WCoast({westernCoastBaseNorthX:F0}-{westernCoastBaseSouthX:F0}) WPlains({westernPlainsNorthX:F0}-{westernPlainsSouthX:F0}) WMtns({westernMountainsNorthX:F0}-{westernMountainsSouthX:F0}) CPlains({centralPlainsNorthX:F0}-{centralPlainsSouthX:F0}) EMtns({easternMountainsNorthX:F0}-{easternMountainsSouthX:F0}) EPlains({easternPlainsNorthX:F0}-{easternPlainsSouthX:F0}) ECoast({easternCoastBaseNorthX:F0}-{easternCoastBaseSouthX:F0})");
    }

    /// <summary>
    /// Regenerate the entire map with new climate assignment and terrain
    /// </summary>
    private void RegenerateMap()
    {
        // Reset Era to 0
        Era = 0;
        
        // Set new random climate assignment
        IsNorthArctic = UnityEngine.Random.value > 0.5f;
        
        // Regenerate continental terrain
        GenerateContinentalTerrain();
        
        // Apply EdgeGuard to ensure ocean boundaries
        GeoPhase edgePhase = new GeoPhase(this, biomes);
        edgePhase.ExecuteEdgeGuard();
        
        // Generate new tectonic plates
        tectonicPlates.Clear();
        for (int i = 0; i < NumberOfPlates; i++)
        {
            TectonicPlateGenerator generator = new TectonicPlateGenerator();
            generator.GenerateTectonicPlate(hexagons, Width, Height, tectonicPlates, MinHexagonsPerPlate, null);
        }
        
        // Run initial geological phases like in Start()
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        GeoPhase phase = new GeoPhase(this, biomes);
        int NumberOfGeoPhases = 25;
        for (int i = 0; i < NumberOfGeoPhases; i++)
        {
            Era += 1; // Count initial generation cycles
            phase.ExecuteClimateTemperature();
            phase.ExecuteFertilityAssessment();
            phase.ExecuteMagmaImpact();
            phase.ExecuteSlump();
            phase.ExecuteSeaLevel();
            phase.ExecuteWindEffect();
            phase.ExecuteRiverFlow();
        }
        stopwatch.Stop();
        UnityEngine.Debug.Log($"Map regenerated with {NumberOfGeoPhases} initial cycles in {stopwatch.ElapsedMilliseconds} ms");
        
        // Set biomes and refresh display
        GenesisSeaLevel = SeaLevel;
        phase.ExecuteSetBiomes();
        
        // Refresh the view
        if (viewManager != null)
        {
            viewManager.RefreshDisplay();
        }
        else
        {
            phase.ExecuteRefreshHexDisplay();
        }
        
        UnityEngine.Debug.Log($"New map generated! Climate: {(IsNorthArctic ? "North Arctic" : "South Arctic")}");
    }

}