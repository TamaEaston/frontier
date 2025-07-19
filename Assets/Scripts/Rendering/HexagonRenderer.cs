using UnityEngine;

/// <summary>
/// Handles rendering of individual hexagon colors and sprites
/// Provides a clean interface for view classes to render hexagons
/// </summary>
public class HexagonRenderer
    {
        private static Sprite defaultHexSprite;
        
        /// <summary>
        /// Get or cache the default hexagon sprite for color-only rendering
        /// </summary>
        private static Sprite DefaultHexSprite
        {
            get
            {
                if (defaultHexSprite == null)
                {
                    // Try to load a simple hex sprite, fallback to any available sprite
                    defaultHexSprite = Resources.Load<Sprite>("Tiles/farmland");
                    if (defaultHexSprite == null)
                    {
                        Debug.LogWarning("No default hex sprite found. Views may not render correctly.");
                    }
                }
                return defaultHexSprite;
            }
        }
        
        /// <summary>
        /// Render a hexagon with solid color only (no sprite texture)
        /// </summary>
        /// <param name="hexagon">The hexagon to render</param>
        /// <param name="color">The color to apply</param>
        public void RenderSolidColor(Hexagon hexagon, Color color)
        {
            if (hexagon?.SpriteRenderer == null) return;
            
            // Store original sprite if not already stored
            if (hexagon.originalSprite == null && hexagon.SpriteRenderer.sprite != null)
            {
                hexagon.originalSprite = hexagon.SpriteRenderer.sprite;
            }
            
            // Use original sprite for shape, apply solid color
            hexagon.SpriteRenderer.sprite = hexagon.originalSprite ?? DefaultHexSprite;
            hexagon.SpriteRenderer.color = color;
        }
        
        /// <summary>
        /// Render a hexagon with both sprite texture and color tint
        /// </summary>
        /// <param name="hexagon">The hexagon to render</param>
        /// <param name="sprite">The sprite texture to use</param>
        /// <param name="color">The color tint to apply</param>
        public void RenderWithSprite(Hexagon hexagon, Sprite sprite, Color color)
        {
            if (hexagon?.SpriteRenderer == null) return;
            
            // Store original sprite if not already stored
            if (hexagon.originalSprite == null && hexagon.SpriteRenderer.sprite != null)
            {
                hexagon.originalSprite = hexagon.SpriteRenderer.sprite;
            }
            
            hexagon.SpriteRenderer.sprite = sprite ?? hexagon.originalSprite ?? DefaultHexSprite;
            hexagon.SpriteRenderer.color = color;
        }
        
        /// <summary>
        /// Load a biome sprite from the Resources/Tiles directory
        /// </summary>
        /// <param name="imagePath">The image path from biome data</param>
        /// <returns>The loaded sprite, or null if not found</returns>
        public Sprite LoadBiomeSprite(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath)) return null;
            
            // Remove leading slash and file extension from the image path
            string cleanPath = imagePath.TrimStart('/').Replace(".png", "");
            
            // Load the sprite from Resources
            Sprite sprite = Resources.Load<Sprite>(cleanPath);
            
            if (sprite == null)
            {
                Debug.LogWarning($"Failed to load biome sprite: {cleanPath}");
            }
            
            return sprite;
        }
        
        /// <summary>
        /// Reset a hexagon to its original state
        /// </summary>
        /// <param name="hexagon">The hexagon to reset</param>
        public void ResetToOriginal(Hexagon hexagon)
        {
            if (hexagon?.SpriteRenderer == null) return;
            
            hexagon.SpriteRenderer.sprite = hexagon.originalSprite ?? DefaultHexSprite;
            hexagon.SpriteRenderer.color = Color.white;
        }
    }