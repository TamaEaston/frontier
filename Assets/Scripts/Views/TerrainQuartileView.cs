using UnityEngine;
using Helpers;

/// <summary>
/// Terrain Quartile view mode - shows terrain roughness quartiles with river systems
/// Key: 7 - Displays terrain types from flat plains to mountainous regions with river networks
/// </summary>
public class TerrainQuartileView : IHexagonView
{
    private HexGrid hexGrid;
    private HexGridColours colorHelper;
    
    public string ViewName => "Terrain";
    
    public void Initialize(HexGrid hexGrid)
    {
        this.hexGrid = hexGrid;
        this.colorHelper = new HexGridColours();
    }
    
    public void RenderHexagon(Hexagon hexagon, HexagonRenderer renderer)
    {
        if (hexagon == null || renderer == null) return;
        
        // Get terrain quartile-based color
        Color terrainColor = GetHexagonColor(hexagon);
        renderer.RenderSolidColor(hexagon, terrainColor);
    }
    
    public void RenderOverlay(Hexagon hexagon, OverlayRenderer overlayRenderer)
    {
        if (hexagon == null || overlayRenderer == null) return;
        
        // Clear overlay arrows for clean terrain view
        overlayRenderer.ClearOverlay(hexagon);
        
        // Render rivers in terrain quartile view for enhanced visibility
        RenderRivers(hexagon);
    }
    
    public void OnViewActivated()
    {
        Debug.Log("Terrain view activated - showing terrain roughness, lakes, and river systems");
    }
    
    public void OnViewDeactivated()
    {
        Debug.Log("Terrain view deactivated");
    }
    
    public Color GetHexagonColor(Hexagon hexagon)
    {
        if (hexagon == null) return Color.white;
        
        // Use dedicated terrain quartile color calculation with lake support
        return colorHelper.GetTerrainQuartileColour(
            hexagon.TerrainQuartile, 
            hexagon.AltitudeVsSeaLevel,
            hexagon.SurfaceWater,
            hexagon.Temperature
        );
    }
    
    public bool ShouldShowOverlay(Hexagon hexagon)
    {
        // Show overlay for hexagons with rivers or lakes
        return hexagon != null && (hexagon.RiverWidth > 0 || hexagon.SurfaceWater >= 100);
    }
    
    /// <summary>
    /// Render rivers using LineRenderer components with enhanced visibility for terrain view
    /// </summary>
    /// <param name="hexagon">The hexagon to render rivers for</param>
    private void RenderRivers(Hexagon hexagon)
    {
        // Only render rivers on land with significant width
        if (hexagon.RiverWidth > 0 && hexagon.LowestNeighbour != null && hexagon.AltitudeVsSeaLevel > 0)
        {
            // Use standardized river colors consistent with other views
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
            
            // Enhanced river width for better visibility on terrain view
            float riverWidth = Mathf.Max(0.05f, Mathf.Min(0.4f, hexagon.RiverWidth / 1500f));
            
            // Create the first line from the center of the hex to the edge of the hex
            GameObject lineObject1 = new GameObject("RiverLine1");
            LineRenderer line1 = lineObject1.AddComponent<LineRenderer>();
            line1.startWidth = riverWidth;
            line1.endWidth = riverWidth;
            line1.material = new Material(Shader.Find("Unlit/Color")) { color = riverColour };
            line1.sortingOrder = 2; // Render above hexagons and other overlays
            line1.SetPosition(0, startPos);
            line1.SetPosition(1, edgePos1);
            line1.tag = "RiverLine";
            
            // Only create second line if neighbor is above sea level
            if (hexagon.LowestNeighbour.Altitude > hexGrid.SeaLevel)
            {
                // Create the second line from the center of the lowest neighbour to the edge of the hex
                GameObject lineObject2 = new GameObject("RiverLine2");
                LineRenderer line2 = lineObject2.AddComponent<LineRenderer>();
                line2.startWidth = riverWidth;
                line2.endWidth = riverWidth;
                line2.material = new Material(Shader.Find("Unlit/Color")) { color = riverColour };
                line2.sortingOrder = 2; // Render above hexagons and other overlays
                line2.SetPosition(0, endPos);
                line2.SetPosition(1, edgePos2);
                line2.tag = "RiverLine";
            }
        }
    }
}