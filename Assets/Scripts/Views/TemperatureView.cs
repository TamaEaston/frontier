using UnityEngine;
using Helpers;

/// <summary>
/// Temperature view mode - shows thermal data with color gradient
/// Key: 4 - Pure temperature visualization without overlays
/// </summary>
public class TemperatureView : IHexagonView
    {
        private HexGrid hexGrid;
        private HexGridColours colorHelper;
        
        public string ViewName => "Temperature";
        
        public void Initialize(HexGrid hexGrid)
        {
            this.hexGrid = hexGrid;
            this.colorHelper = new HexGridColours();
        }
        
        public void RenderHexagon(Hexagon hexagon, HexagonRenderer renderer)
        {
            if (hexagon == null || renderer == null) return;
            
            // Get temperature-based color
            Color temperatureColor = GetHexagonColor(hexagon);
            renderer.RenderSolidColor(hexagon, temperatureColor);
        }
        
        public void RenderOverlay(Hexagon hexagon, OverlayRenderer overlayRenderer)
        {
            // Temperature view shows rivers but no other overlays
            if (overlayRenderer != null)
            {
                overlayRenderer.ClearOverlay(hexagon);
            }
            
            // Render rivers in temperature view
            RenderRivers(hexagon);
        }
        
        public void OnViewActivated()
        {
            Debug.Log("Temperature view activated - showing thermal data");
        }
        
        public void OnViewDeactivated()
        {
            Debug.Log("Temperature view deactivated");
        }
        
        public Color GetHexagonColor(Hexagon hexagon)
        {
            if (hexagon == null) return Color.white;
            
            // Use dedicated temperature color calculation
            return colorHelper.GetTemperatureColour(
                hexagon.AltitudeVsSeaLevel, 
                hexagon.SurfaceWater, 
                hexagon.Temperature
            );
        }
        
        public bool ShouldShowOverlay(Hexagon hexagon)
        {
            // Temperature view shows rivers as overlays
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
                Color riverColour = (hexagon.Temperature < -5) ? new Color(0.6f, 0.8f, 1.0f) : new Color(0.0f, 0.5f, 0.5f);
                
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
