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
            // Temperature view shows no overlays - pure color visualization
            if (overlayRenderer != null)
            {
                overlayRenderer.ClearOverlay(hexagon);
            }
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
            // Temperature view shows no overlays - clean color-only visualization
            return false;
        }
    }
