using UnityEngine;
using Helpers;

/// <summary>
/// Magma view mode - shows tectonic plate activity with directional arrows
/// Key: 1 - Displays magma intensity and flow direction
/// </summary>
public class MagmaView : IHexagonView
    {
        private HexGrid hexGrid;
        private HexGridColours colorHelper;
        
        public string ViewName => "Magma";
        
        public void Initialize(HexGrid hexGrid)
        {
            this.hexGrid = hexGrid;
            this.colorHelper = new HexGridColours();
        }
        
        public void RenderHexagon(Hexagon hexagon, HexagonRenderer renderer)
        {
            if (hexagon == null || renderer == null) return;
            
            // Get base terrain color for background
            Color baseColor = GetHexagonColor(hexagon);
            renderer.RenderSolidColor(hexagon, baseColor);
        }
        
        public void RenderOverlay(Hexagon hexagon, OverlayRenderer overlayRenderer)
        {
            if (hexagon == null || overlayRenderer == null) return;
            
            // Show magma flow arrows for areas with significant magma activity
            if (hexagon.MagmaIntensity > 0)
            {
                overlayRenderer.RenderMagmaArrow(hexagon);
            }
            else
            {
                overlayRenderer.ClearOverlay(hexagon);
            }
        }
        
        public void OnViewActivated()
        {
            Debug.Log("Magma view activated - showing tectonic plate activity");
        }
        
        public void OnViewDeactivated()
        {
            Debug.Log("Magma view deactivated");
        }
        
        public Color GetHexagonColor(Hexagon hexagon)
        {
            if (hexagon == null) return Color.white;
            
            // For magma view, we want to show the underlying terrain
            // This provides context for where the magma activity is occurring
            return colorHelper.GetBiomeColour(
                hexagon.AltitudeVsSeaLevel, 
                hexagon.SurfaceWater, 
                hexagon.Temperature
            );
        }
        
        public bool ShouldShowOverlay(Hexagon hexagon)
        {
            // Show overlay for hexagons with any magma activity
            return hexagon != null && hexagon.MagmaIntensity > 0;
        }
    }
