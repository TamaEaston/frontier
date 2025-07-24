using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Central manager for all hexagon view modes
/// Handles switching between views and coordinating rendering
/// </summary>
public class ViewManager
    {
        private HexGrid hexGrid;
        private HexagonRenderer hexagonRenderer;
        private OverlayRenderer overlayRenderer;
        
        // All available view modes
        private Dictionary<string, IHexagonView> views;
        private IHexagonView currentView;
        
        public ViewManager(HexGrid hexGrid)
        {
            this.hexGrid = hexGrid;
            this.hexagonRenderer = new HexagonRenderer();
            this.overlayRenderer = new OverlayRenderer(hexGrid);
            
            InitializeViews();
        }
        
        /// <summary>
        /// Initialize all view mode implementations
        /// </summary>
        private void InitializeViews()
        {
            views = new Dictionary<string, IHexagonView>();
            
            // Create and register all view modes
            RegisterView("None", new BiomeView());
            RegisterView("Magma", new MagmaView());
            RegisterView("Altitude", new AltitudeView());
            RegisterView("Weather", new WeatherView());
            RegisterView("Temperature", new TemperatureView());
            RegisterView("Fertility", new FertilityView());
            RegisterView("Terrain", new TerrainQuartileView());
            
            // Set default view
            SetActiveView("None");
        }
        
        /// <summary>
        /// Register a view mode with the manager
        /// </summary>
        /// <param name="viewName">The name/key for this view</param>
        /// <param name="view">The view implementation</param>
        private void RegisterView(string viewName, IHexagonView view)
        {
            view.Initialize(hexGrid);
            views[viewName] = view;
        }
        
        /// <summary>
        /// Switch to a specific view mode
        /// </summary>
        /// <param name="viewName">The name of the view to activate</param>
        public void SetActiveView(string viewName)
        {
            if (!views.ContainsKey(viewName))
            {
                Debug.LogWarning($"View '{viewName}' not found. Available views: {string.Join(", ", views.Keys)}");
                return;
            }
            
            // Deactivate current view
            currentView?.OnViewDeactivated();
            
            // Activate new view
            currentView = views[viewName];
            currentView.OnViewActivated();
            
            Debug.Log($"Switched to view: {currentView.ViewName}");
        }
        
        /// <summary>
        /// Get the name of the currently active view
        /// </summary>
        /// <returns>Current view name</returns>
        public string GetActiveViewName()
        {
            return currentView?.ViewName ?? "None";
        }
        
        /// <summary>
        /// Refresh the display for all hexagons using the current view
        /// </summary>
        public void RefreshDisplay()
        {
            if (currentView == null || hexGrid?.GetHexagons() == null) return;
            
            // Clear all existing river lines
            foreach (GameObject line in GameObject.FindGameObjectsWithTag("RiverLine"))
            {
                GameObject.Destroy(line);
            }
            
            var hexagons = hexGrid.GetHexagons();
            int mapWidth = hexagons.GetLength(0);
            int mapHeight = hexagons.GetLength(1);
            
            for (int i = 0; i < mapWidth; i++)
            {
                for (int j = 0; j < mapHeight; j++)
                {
                    Hexagon hex = hexagons[i, j];
                    if (hex == null) continue;
                    
                    // Render the hexagon using the current view
                    currentView.RenderHexagon(hex, hexagonRenderer);
                    
                    // Render overlay if needed
                    if (currentView.ShouldShowOverlay(hex))
                    {
                        currentView.RenderOverlay(hex, overlayRenderer);
                    }
                    else
                    {
                        overlayRenderer.ClearOverlay(hex);
                    }
                }
            }
        }
        
        /// <summary>
        /// Handle input for view switching
        /// </summary>
        /// <param name="keyCode">The key that was pressed</param>
        public void HandleViewInput(KeyCode keyCode)
        {
            string targetView = null;
            
            switch (keyCode)
            {
                case KeyCode.Alpha1:
                    targetView = "None";
                    break;
                case KeyCode.Alpha2:
                    targetView = "Magma";
                    break;
                case KeyCode.Alpha3:
                    targetView = "Altitude";
                    break;
                case KeyCode.Alpha4:
                    targetView = "Weather";
                    break;
                case KeyCode.Alpha5:
                    targetView = "Temperature";
                    break;
                case KeyCode.Alpha6:
                    targetView = "Fertility";
                    break;
                case KeyCode.Alpha7:
                    targetView = "Terrain";
                    break;
            }
            
            if (targetView != null)
            {
                // Toggle view: if already active, switch to None; otherwise switch to target
                string currentViewName = GetActiveViewName();
                if (currentViewName == targetView && targetView != "None")
                {
                    SetActiveView("None");
                    GameSettings.ActiveOverlay = "None";
                }
                else
                {
                    SetActiveView(targetView);
                    GameSettings.ActiveOverlay = targetView;
                }
                
                // Refresh display immediately
                RefreshDisplay();
            }
        }
        
        /// <summary>
        /// Get all available view names
        /// </summary>
        /// <returns>List of available view names</returns>
        public List<string> GetAvailableViews()
        {
            return new List<string>(views.Keys);
        }
    }