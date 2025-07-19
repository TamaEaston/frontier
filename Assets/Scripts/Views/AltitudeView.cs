using UnityEngine;
using Helpers;

/// <summary>
/// Altitude view mode - shows elevation with rise/sink indicators
/// Key: 2 - Displays altitude gradient and elevation changes
/// </summary>
public class AltitudeView : IHexagonView
    {
        private HexGrid hexGrid;
        private HexGridColours colorHelper;
        
        public string ViewName => "Altitude";
        
        public void Initialize(HexGrid hexGrid)
        {
            this.hexGrid = hexGrid;
            this.colorHelper = new HexGridColours();
        }
        
        public void RenderHexagon(Hexagon hexagon, HexagonRenderer renderer)
        {
            if (hexagon == null || renderer == null) return;
            
            // Get altitude-based color
            Color altitudeColor = GetHexagonColor(hexagon);
            renderer.RenderSolidColor(hexagon, altitudeColor);
        }
        
        public void RenderOverlay(Hexagon hexagon, OverlayRenderer overlayRenderer)
        {
            if (hexagon == null || overlayRenderer == null) return;
            
            // Show altitude change arrows for significant elevation changes
            if (Mathf.Abs(hexagon.AltitudeChange) > 1f) // Only show for meaningful changes
            {
                overlayRenderer.RenderAltitudeArrow(hexagon, hexagon.AltitudeChange);
            }
            else
            {
                overlayRenderer.ClearOverlay(hexagon);
            }
        }
        
        public void OnViewActivated()
        {
            Debug.Log("Altitude view activated - showing elevation data");
        }
        
        public void OnViewDeactivated()
        {
            Debug.Log("Altitude view deactivated");
        }
        
        public Color GetHexagonColor(Hexagon hexagon)
        {
            if (hexagon == null) return Color.white;
            
            // Use dedicated altitude color calculation
            return colorHelper.GetAltitudeColour(
                hexagon.AltitudeVsSeaLevel, 
                hexagon.SurfaceWater, 
                hexagon.Temperature
            );
        }
        
        public bool ShouldShowOverlay(Hexagon hexagon)
        {
            // Show overlay for hexagons with significant altitude changes
            return hexagon != null && Mathf.Abs(hexagon.AltitudeChange) > 1f;
        }
    }
