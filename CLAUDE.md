# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Unity-based geological simulation called "hexageo" that simulates tectonic plate movement and geological processes on a hexagonal grid. The project models geological phenomena like magma flow, altitude changes, and surface slumping.

## Core Architecture

### Main Components

- **HexGrid.cs**: The core grid system that manages the hexagonal world grid, handles tectonic plate generation, and processes geological simulation steps via space key input
- **Hexagon.cs**: Individual hexagon cell data structure containing altitude, magma properties, wind properties, and plate assignments
- **GeoPhase/**: Geological simulation phases including:
  - `MagmaImpact.cs`: Simulates magma movement affecting altitude based on intensity and direction
  - `Slump.cs`: Handles altitude redistribution where higher cells transfer 10% of height difference to lower neighbors
  - `RefreshHexGridDisplay.cs`: Updates visual representation after geological changes

### Key Systems

- **Tectonic Plates**: Randomly generated plates with direction, intensity, and size properties that affect magma flow
- **Neighbor System**: Each hexagon has 6 neighbors with special handling for polar regions (top/bottom rows)
- **Geological Processes**: Sequential execution of magma impact → slump → display refresh

## Unity Project Structure

- Unity Version: 2022.3.15f1
- Main Scene: Assets/Scenes/SampleScene.unity
- Scripts in Assets/Scripts/ with organized subfolders
- Prefabs: Hexagon and MagmaOverlay for grid visualization
- Color system managed through HexGridColours helper

## Development Workflow

### Building & Running
- Open project in Unity Editor 2022.3.15f1
- Use Unity's standard build process (File → Build Settings)
- Test in Play Mode within Unity Editor

### Simulation Controls
- **Space Key**: Triggers one geological simulation step (magma impact → slump → refresh display)
- Performance timing logged to Unity Console

### Key Parameters
- Grid size controlled by Width/Height in HexGrid
- SeaLevel set to 10000 (affects color rendering)
- NumberOfPlates limited to max 24
- Altitude range: 9500-10501 (initial random values)

## Important Implementation Details

### Grid Wrapping
- Grid wraps horizontally (east-west neighbors use modulo arithmetic)
- Polar regions (top/bottom rows) have special neighbor assignments to central cells

### Geological Algorithm Order
1. **MagmaImpact**: Uses MagmaIntensity and MagmaDirection to modify AltitudeNew
2. **Slump**: Transfers 10% of height difference from higher to lower neighbors
3. **RefreshHexGridDisplay**: Updates visual colors based on height above sea level

### Plate Generation
- Plates grow from random seed points with 50% chance for neighbors to join
- Each plate has unique direction (30°, 60°, 90°, 120°, 150°, 180°), intensity (25-100), and size limits