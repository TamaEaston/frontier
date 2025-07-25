using UnityEngine;
using Helpers;

/// <summary>
/// Biome colour view mode - shows solid biome colors without sprites
/// Key: 1 - Shows biome colors only for clear terrain visibility
/// </summary>
public class BiomeColourView : IHexagonView
{
    private HexGrid hexGrid;
    private HexGridColours colorHelper;
    
    public string ViewName => "BiomeColour";
    
    public void Initialize(HexGrid hexGrid)
    {
        this.hexGrid = hexGrid;
        this.colorHelper = new HexGridColours();
    }
    
    public void RenderHexagon(Hexagon hexagon, HexagonRenderer renderer)
    {
        if (hexagon == null || renderer == null) return;
        
        // Check for all water features first (oceans, lakes, glaciers) using same logic as other views
        Color waterColor = colorHelper.GetStandardWaterColour(hexagon.HeightAboveSeaLevel, hexagon.SurfaceWater, hexagon.Temperature);
        if (waterColor != Color.clear)
        {
            // Use standardized water color (includes ocean depth gradient)
            renderer.RenderSolidColor(hexagon, waterColor);
            return;
        }
        
        // Land tiles only - use biome colors (no sprites in this view)
        if (hexagon.Biome != null)
        {
            // Parse biome color from hex string for land biomes
            Color biomeColor = Color.white;
            if (ColorUtility.TryParseHtmlString(hexagon.Biome.Colour, out biomeColor))
            {
                // Always render as solid color (no sprites in this view)
                renderer.RenderSolidColor(hexagon, biomeColor);
            }
            else
            {
                Debug.LogError($"Invalid biome color string: {hexagon.Biome.Colour}");
                // Use fallback color calculation for land
                Color fallbackColor = GetLandBiomeColor(hexagon);
                renderer.RenderSolidColor(hexagon, fallbackColor);
            }
        }
        else
        {
            // No biome assigned to land tile - use calculated land color
            Color defaultColor = GetLandBiomeColor(hexagon);
            renderer.RenderSolidColor(hexagon, defaultColor);
        }
    }
    
    public void RenderOverlay(Hexagon hexagon, OverlayRenderer overlayRenderer)
    {
        // BiomeColour view typically doesn't show overlays
        // Clear any existing overlays
        overlayRenderer.ClearOverlay(hexagon);
        
        // Render rivers in biome colour view
        RenderRivers(hexagon);
    }
    
    public void OnViewActivated()
    {
        Debug.Log("BiomeColour view activated - showing solid biome colors");
    }
    
    public void OnViewDeactivated()
    {
        Debug.Log("BiomeColour view deactivated");
    }
    
    public Color GetHexagonColor(Hexagon hexagon)
    {
        if (hexagon == null) return Color.white;
        
        // Check for all water features first (oceans, lakes, glaciers) using same logic as other views
        Color waterColor = colorHelper.GetStandardWaterColour(hexagon.HeightAboveSeaLevel, hexagon.SurfaceWater, hexagon.Temperature);
        if (waterColor != Color.clear)
        {
            return waterColor;
        }
        
        // Return land biome color
        return GetLandBiomeColor(hexagon);
    }
    
    /// <summary>
    /// Get color for land tiles only (excludes water features)
    /// </summary>
    private Color GetLandBiomeColor(Hexagon hexagon)
    {
        if (hexagon == null) return Color.white;
        
        // For land tiles, use the original biome color calculation but exclude water
        // This handles cases where no biome is assigned to land
        if (hexagon.Temperature < -5)
        {
            // Glacial/ice areas on land
            float t = Mathf.InverseLerp(0, 5000, hexagon.HeightAboveSeaLevel);
            return Color.Lerp(new Color(0.9f, 0.9f, 0.9f), Color.white, t);
        }
        else if (hexagon.HeightAboveSeaLevel <= 50)
        {
            return new Color(0.93f, 0.79f, 0.69f); // Yellow Sand
        }
        else if (hexagon.HeightAboveSeaLevel <= 2500)
        {
            float t = Mathf.InverseLerp(51, 2500, hexagon.HeightAboveSeaLevel);
            return Color.Lerp(new Color(0.5f, 0.8f, 0.2f), new Color(0.13f, 0.55f, 0.13f), t); // Grass Green to Forest Green
        }
        else
        {
            float t = Mathf.InverseLerp(2501, 5000, hexagon.HeightAboveSeaLevel);
            return Color.Lerp(new Color(0.5f, 0.5f, 0.5f), new Color(0.8f, 0.8f, 0.8f), t); // Rock Grey to Light Rock Grey
        }
    }
    
    public bool ShouldShowOverlay(Hexagon hexagon)
    {
        // BiomeColour view shows rivers as overlays
        return hexagon.RiverWidth > 0 && hexagon.LowestNeighbour != null && hexagon.AltitudeVsSeaLevel > 0;
    }
    
    /// <summary>
    /// Render rivers using LineRenderer components
    /// </summary>
    /// <param name="hexagon">The hexagon to render rivers for</param>
    private void RenderRivers(Hexagon hexagon)
    {
        if (hexagon.RiverWidth > 0 && hexagon.LowestNeighbour != null && hexagon.AltitudeVsSeaLevel > 0)
        {
            Debug.Log($"Creating river: RiverWidth={hexagon.RiverWidth}, AltitudeVsSeaLevel={hexagon.AltitudeVsSeaLevel}, Position=({hexagon.PositionX},{hexagon.PositionY})");
            
            Color riverColour = colorHelper.GetStandardRiverColour(hexagon.Temperature);
            
            Vector3 startPos = new Vector3(hexagon.transform.position.x, hexagon.transform.position.y, -1);
            Vector3 endPos = new Vector3(hexagon.LowestNeighbour.transform.position.x, hexagon.LowestNeighbour.transform.position.y, -1);
            
            // Calculate the direction from the center of each hex to the center of the other hex
            Vector3 directionToNeighbour = (endPos - startPos).normalized;
            Vector3 directionToHex = -directionToNeighbour;
            
            // Calculate the points on the edges of the hexes where the lines should start and end
            float hexRadius = hexagon.GetComponent<Renderer>().bounds.size.x / 2;
            Vector3 edgePos1 = startPos + directionToNeighbour * hexRadius;
            Vector3 edgePos2 = endPos + directionToHex * hexRadius;
            
            float riverWidth = Mathf.Min(0.3f, hexagon.RiverWidth / 2000f);
            
            // Create the first line from the center of the hex to the edge of the hex
            GameObject lineObject1 = new GameObject("RiverLine1");
            LineRenderer line1 = lineObject1.AddComponent<LineRenderer>();
            line1.startWidth = riverWidth;
            line1.endWidth = riverWidth;
            line1.material = new Material(Shader.Find("Unlit/Color")) { color = riverColour };
            line1.sortingOrder = 1; // Render in front of hexagons
            line1.SetPosition(0, startPos);
            line1.SetPosition(1, edgePos1);
            line1.tag = "RiverLine";
            
            if (hexagon.LowestNeighbour.Altitude > hexGrid.SeaLevel)
            {
                // Create the second line from the center of the lowest neighbour to the edge of the hex
                GameObject lineObject2 = new GameObject("RiverLine2");
                LineRenderer line2 = lineObject2.AddComponent<LineRenderer>();
                line2.startWidth = riverWidth;
                line2.endWidth = riverWidth;
                line2.material = new Material(Shader.Find("Unlit/Color")) { color = riverColour };
                line2.sortingOrder = 1; // Render in front of hexagons
                line2.SetPosition(0, endPos);
                line2.SetPosition(1, edgePos2);
                line2.tag = "RiverLine";
            }
        }
    }
}