using UnityEngine;
using Helpers;

/// <summary>
/// Biome view mode - shows detailed biome sprites with biome colors
/// Key: 0 - Default view showing the natural terrain appearance
/// </summary>
public class BiomeView : IHexagonView
    {
        private HexGrid hexGrid;
        private HexGridColours colorHelper;
        
        public string ViewName => "Biome";
        
        public void Initialize(HexGrid hexGrid)
        {
            this.hexGrid = hexGrid;
            this.colorHelper = new HexGridColours();
        }
        
        public void RenderHexagon(Hexagon hexagon, HexagonRenderer renderer)
        {
            if (hexagon == null || renderer == null) return;
            
            if (hexagon.Biome != null)
            {
                // Parse biome color from hex string
                Color biomeColor = Color.white;
                if (ColorUtility.TryParseHtmlString(hexagon.Biome.Colour, out biomeColor))
                {
                    // Load and apply biome sprite
                    Sprite biomeSprite = renderer.LoadBiomeSprite(hexagon.Biome.Image);
                    if (biomeSprite != null)
                    {
                        renderer.RenderWithSprite(hexagon, biomeSprite, biomeColor);
                    }
                    else
                    {
                        // Fallback to solid color if sprite not found
                        renderer.RenderSolidColor(hexagon, biomeColor);
                    }
                }
                else
                {
                    Debug.LogError($"Invalid biome color string: {hexagon.Biome.Colour}");
                    // Use fallback color calculation
                    Color fallbackColor = GetHexagonColor(hexagon);
                    renderer.RenderSolidColor(hexagon, fallbackColor);
                }
            }
            else
            {
                // No biome assigned - use calculated color
                Color defaultColor = GetHexagonColor(hexagon);
                renderer.RenderSolidColor(hexagon, defaultColor);
            }
        }
        
        public void RenderOverlay(Hexagon hexagon, OverlayRenderer overlayRenderer)
        {
            // Biome view typically doesn't show overlays
            // Clear any existing overlays
            overlayRenderer.ClearOverlay(hexagon);
        }
        
        public void OnViewActivated()
        {
            Debug.Log("Biome view activated - showing detailed terrain sprites");
        }
        
        public void OnViewDeactivated()
        {
            Debug.Log("Biome view deactivated");
        }
        
        public Color GetHexagonColor(Hexagon hexagon)
        {
            if (hexagon == null) return Color.white;
            
            // Use the existing biome color calculation as fallback
            return colorHelper.GetBiomeColour(
                hexagon.AltitudeVsSeaLevel, 
                hexagon.SurfaceWater, 
                hexagon.Temperature
            );
        }
        
        public bool ShouldShowOverlay(Hexagon hexagon)
        {
            // Biome view shows no overlays - just the terrain sprites
            return false;
        }
    }