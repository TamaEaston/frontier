# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**HexageonUnity** is a sophisticated Unity-based geological simulation that models tectonic plate movement, climate systems, and environmental processes on a hexagonal grid. The project simulates complex interactions between geology, weather, water systems, human settlements, and biomes.

## Core Architecture

### Central Controller System
- **HexGrid.cs**: Main simulation orchestrator managing grid, user input, overlays, and phase execution
- **Hexagon.cs**: Individual cell containing 30+ properties (altitude, temperature, wind, rainfall, magma, settlements, etc.)
- **GeoPhase.cs**: Simulation phase sequencer ensuring correct execution order and data dependencies
- **GameSettings.cs**: Global state management for overlays, edit modes, and climate parameters

### Complete Geological Simulation Pipeline
```
MagmaImpact → Slump → SeaLevel → WindEffect → RiverFlow → 
ClimateTemperature → HumanComfortAssessment → PopulationGrowth → SetBiomes → RefreshDisplay
```

### Key Subsystems

**Environmental Systems:**
- **WindEffect.cs**: Complex wind pattern calculations with altitude, ocean, and land effects
- **RiverFlow.cs**: Dynamic river formation and water routing across terrain
- **ClimateTemperature.cs**: Solar intensity, latitude, and altitude-based temperature modeling
- **SeaLevel.cs**: Dynamic sea level management affecting biomes and land visibility

**Civilization Systems:**
- **PopulationGrowth.cs**: Human settlement expansion based on comfort and resources
- **HumanComfortAssessment.cs**: Multi-factor habitability scoring system
- **Civilisation.cs**: 5 distinct civilizations with unique expansion patterns and colors

**Visualization Systems:**
- **BiomeLoader.cs**: JSON-driven biome system with environmental constraints
- **HexGridColours.cs**: Advanced color mapping for multiple data visualization modes
- **Arrow.cs/Settlement.cs/RiverOverlay.cs**: Specialized overlay rendering components

## Unity Project Structure

- **Unity Version**: 6000.1.12f1 (Unity 6)
- **Project Name**: HexageonUnity
- **Main Scene**: Assets/Scenes/SampleScene.unity
- **Core Data**: Assets/Data/biomes.json (40+ biome definitions with environmental parameters)
- **Visual Assets**: Assets/Resources/Tiles/ (200+ biome sprites)

## Development Workflow

### Building & Running
- Open project in Unity Editor 6000.1.12f1
- Use Unity's standard build process (File → Build Settings)
- Test in Play Mode within Unity Editor for simulation development

### Simulation Controls
- **Space Key**: Execute complete geological simulation step (full pipeline)
- **Number Keys 0-6**: Toggle visualization overlays:
  - 0: None, 1: Magma, 2: Altitude, 3: Weather, 4: Temperature, 5: HumanComfort, 6: Settlement
- **Mouse Interaction**: Hexagon tooltips and edit mode functionality

### Performance Monitoring
- Built-in stopwatch timing for each geological phase
- Performance data logged to Unity Console
- Visual feedback for long-running operations

## Critical Implementation Details

### Hexagonal Grid System
- **Grid Wrapping**: Horizontal wrapping with modulo arithmetic for world-like topology
- **Polar Handling**: Special neighbor assignments for top/bottom rows to prevent edge artifacts
- **Neighbor Calculation**: Sophisticated 6-neighbor system with offset coordinate conversion

### Advanced Geological Processes
1. **MagmaImpact**: Directional tectonic movement affecting neighbors based on plate intensity/direction
2. **Slump**: Height redistribution (2.5% transfer) processed from highest to lowest altitude
3. **WindEffect**: Multi-factor wind calculations (ocean boost, altitude resistance, land friction)
4. **RiverFlow**: Water routing creating realistic river networks with width-based rendering

### Environmental Modeling
- **Temperature**: Latitude-based solar intensity + altitude effects + wind transport
- **Rainfall**: Wind-carried moisture with evaporation and precipitation cycles
- **Biome Assignment**: Multi-parameter matching (altitude, temperature, rainfall, surface water)
- **Water Cycle**: Evaporation, wind transport, rainfall, and river formation

### Data-Driven Configuration
- **Biomes**: JSON configuration with Name, Image, Color, and environmental constraints
- **Tectonic Plates**: Dynamic generation with random size, direction (60° increments), and intensity
- **Global Parameters**: Sea level (default 10000), max plates (24), grid dimensions

### Key Dependencies
- **Unity Packages**: 2D features, Test Framework, Visual Scripting, Timeline
- **Missing Dependencies**: TextMeshPro package needed (causes current compilation errors)

## Known Issues

### Current Compilation Errors
- **TMPro Namespace**: Missing TextMeshPro package causes multiple compilation errors
- **Ambiguous Method Calls**: Duplicate GeoPhase method signatures need resolution
- **Method Naming**: Several methods violate C# naming conventions (camelCase vs PascalCase)

### Architecture Debt
- Multiple GeoPhase classes with identical method signatures creating ambiguity
- Some unused using directives in HexGrid.cs
- Performance optimization opportunities in visualization rendering