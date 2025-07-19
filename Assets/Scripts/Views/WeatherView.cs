using UnityEngine;
using Helpers;

/// <summary>
/// Weather view mode - shows rainfall with wind direction indicators
/// Key: 3 - Displays precipitation patterns and wind flow
/// </summary>
public class WeatherView : IHexagonView
    {
        private HexGrid hexGrid;
        private HexGridColours colorHelper;
        
        public string ViewName => "Weather";
        
        public void Initialize(HexGrid hexGrid)
        {
            this.hexGrid = hexGrid;
            this.colorHelper = new HexGridColours();
        }
        
        public void RenderHexagon(Hexagon hexagon, HexagonRenderer renderer)
        {
            if (hexagon == null || renderer == null) return;
            
            // Get rainfall-based color
            Color rainfallColor = GetHexagonColor(hexagon);
            renderer.RenderSolidColor(hexagon, rainfallColor);
        }
        
        public void RenderOverlay(Hexagon hexagon, OverlayRenderer overlayRenderer)
        {
            if (hexagon == null || overlayRenderer == null) return;
            
            // Show wind arrows for areas with wind activity
            if (hexagon.WindIntensity > 10f) // Only show for meaningful wind
            {
                overlayRenderer.RenderWindArrow(hexagon);
            }
            else
            {
                overlayRenderer.ClearOverlay(hexagon);
            }
        }
        
        public void OnViewActivated()
        {
            Debug.Log("Weather view activated - showing rainfall and wind patterns");
        }
        
        public void OnViewDeactivated()
        {
            Debug.Log("Weather view deactivated");
        }
        
        public Color GetHexagonColor(Hexagon hexagon)
        {
            if (hexagon == null) return Color.white;
            
            // Use dedicated rainfall color calculation
            return colorHelper.GetRainfallColour(
                hexagon.AltitudeVsSeaLevel, 
                hexagon.SurfaceWater, 
                hexagon.Rainfall
            );
        }
        
        public bool ShouldShowOverlay(Hexagon hexagon)
        {
            // Show overlay for hexagons with significant wind activity
            return hexagon != null && hexagon.WindIntensity > 10f;
        }
    }
