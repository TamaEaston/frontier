using UnityEngine;
using System;

/// <summary>
/// Handles rendering of overlay elements like arrows for different view modes
/// Manages the Arrow objects that display directional and intensity information
/// </summary>
public class OverlayRenderer
    {
        private HexGrid hexGrid;
        
        public OverlayRenderer(HexGrid hexGrid)
        {
            this.hexGrid = hexGrid;
        }
        
        /// <summary>
        /// Render a directional arrow overlay
        /// </summary>
        /// <param name="hexagon">The hexagon to render overlay for</param>
        /// <param name="direction">Direction in degrees (0-360)</param>
        /// <param name="intensity">Intensity value (0-100)</param>
        /// <param name="color">Color of the arrow</param>
        /// <param name="maxScale">Maximum scale of the arrow</param>
        public void RenderDirectionalArrow(Hexagon hexagon, float direction, float intensity, Color color, float maxScale = 0.5f)
        {
            if (hexagon == null || hexGrid?.GetArrows() == null) return;
            
            Arrow arrow = hexGrid.GetArrows()[hexagon.PositionX, hexagon.PositionY];
            if (arrow == null) return;
            
            // Set rotation based on direction
            arrow.transform.rotation = Quaternion.Euler(0, 0, direction);
            
            // Set color with transparency based on intensity
            Color arrowColor = color;
            arrowColor.a = Mathf.Clamp01(intensity / 100f);
            arrow.SpriteRenderer.color = arrowColor;
            
            // Set scale based on intensity
            float scale = Mathf.Min(maxScale, intensity / 200f);
            arrow.transform.localScale = new Vector3(scale, scale, 1f);
        }
        
        /// <summary>
        /// Render an altitude change arrow (up or down)
        /// </summary>
        /// <param name="hexagon">The hexagon to render for</param>
        /// <param name="altitudeChange">The altitude change value</param>
        public void RenderAltitudeArrow(Hexagon hexagon, float altitudeChange)
        {
            if (hexagon == null || hexGrid?.GetArrows() == null) return;
            
            Arrow arrow = hexGrid.GetArrows()[hexagon.PositionX, hexagon.PositionY];
            if (arrow == null) return;
            
            // Determine direction: up (90°) for rising, down (270°) for sinking
            int direction = (altitudeChange > 0) ? 90 : 270;
            arrow.transform.rotation = Quaternion.Euler(0, 0, direction);
            
            // Color: white for sinking, black for rising
            Color arrowColor = (altitudeChange < 0) ? Color.white : Color.black;
            arrowColor.a = 0.75f;
            arrow.SpriteRenderer.color = arrowColor;
            
            // Scale based on magnitude of change
            float scale = Math.Min(0.5f, Math.Abs(altitudeChange) / 100f);
            arrow.transform.localScale = new Vector3(scale, scale, 1f);
        }
        
        /// <summary>
        /// Render a magma flow arrow
        /// </summary>
        /// <param name="hexagon">The hexagon to render for</param>
        public void RenderMagmaArrow(Hexagon hexagon)
        {
            if (hexagon == null) return;
            
            Color magmaColor = GetMagmaColor(hexagon.MagmaIntensity);
            magmaColor.a = 0.75f;
            
            // Handle plate selection overlay
            if (hexagon.PlateSelected)
            {
                magmaColor = Color.black;
                magmaColor.a = hexagon.MagmaIntensity / 100f;
                hexagon.PlateSelected = false;
            }
            
            RenderDirectionalArrow(hexagon, hexagon.MagmaDirection, hexagon.MagmaIntensity, magmaColor, 0.3f);
        }
        
        /// <summary>
        /// Render a wind flow arrow
        /// </summary>
        /// <param name="hexagon">The hexagon to render for</param>
        public void RenderWindArrow(Hexagon hexagon)
        {
            if (hexagon == null) return;
            
            Color windColor = Color.white;
            windColor.a = 0.25f;
            
            RenderDirectionalArrow(hexagon, hexagon.WindDirection, hexagon.WindIntensity, windColor, 0.5f);
        }
        
        /// <summary>
        /// Clear/hide the overlay for a hexagon
        /// </summary>
        /// <param name="hexagon">The hexagon to clear overlay for</param>
        public void ClearOverlay(Hexagon hexagon)
        {
            if (hexagon == null || hexGrid?.GetArrows() == null) return;
            
            Arrow arrow = hexGrid.GetArrows()[hexagon.PositionX, hexagon.PositionY];
            if (arrow == null) return;
            
            arrow.transform.rotation = Quaternion.Euler(0, 0, 0);
            arrow.transform.localScale = new Vector3(0f, 0f, 1f);
        }
        
        /// <summary>
        /// Get magma color based on intensity (yellow to red gradient)
        /// </summary>
        /// <param name="magmaIntensity">Magma intensity (0-100)</param>
        /// <returns>Color representing magma intensity</returns>
        private Color GetMagmaColor(float magmaIntensity)
        {
            float t = Mathf.InverseLerp(0, 100, magmaIntensity);
            return Color.Lerp(Color.yellow, Color.red, t);
        }
    }