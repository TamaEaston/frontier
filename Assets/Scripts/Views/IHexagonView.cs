using UnityEngine;

/// <summary>
/// Interface for all hexagon view modes (Biome, Magma, Altitude, Weather, Temperature)
/// Ensures consistent behavior across all visualization modes
/// </summary>
public interface IHexagonView
    {
        /// <summary>
        /// The name of this view mode (for debugging and UI display)
        /// </summary>
        string ViewName { get; }
        
        /// <summary>
        /// Initialize the view with required dependencies
        /// </summary>
        /// <param name="hexGrid">The main hex grid reference</param>
        void Initialize(HexGrid hexGrid);
        
        /// <summary>
        /// Render a single hexagon for this view mode
        /// </summary>
        /// <param name="hexagon">The hexagon to render</param>
        /// <param name="renderer">The renderer to use for this hexagon</param>
        void RenderHexagon(Hexagon hexagon, HexagonRenderer renderer);
        
        /// <summary>
        /// Render overlay elements (arrows, etc.) for this view mode
        /// </summary>
        /// <param name="hexagon">The hexagon to render overlays for</param>
        /// <param name="overlayRenderer">The overlay renderer to use</param>
        void RenderOverlay(Hexagon hexagon, OverlayRenderer overlayRenderer);
        
        /// <summary>
        /// Called when this view becomes active
        /// </summary>
        void OnViewActivated();
        
        /// <summary>
        /// Called when this view becomes inactive
        /// </summary>
        void OnViewDeactivated();
        
        /// <summary>
        /// Get the base color for a hexagon in this view mode
        /// </summary>
        /// <param name="hexagon">The hexagon to get color for</param>
        /// <returns>The color to apply to this hexagon</returns>
        Color GetHexagonColor(Hexagon hexagon);
        
        /// <summary>
        /// Determine if this hexagon should show overlay elements
        /// </summary>
        /// <param name="hexagon">The hexagon to check</param>
        /// <returns>True if overlay should be shown</returns>
        bool ShouldShowOverlay(Hexagon hexagon);
    }