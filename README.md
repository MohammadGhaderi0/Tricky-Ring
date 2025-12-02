# Tricky Ring 2D 🔴

> A fast-paced 2D hyper-casual arcade game built with Unity.

![Unity](https://img.shields.io/badge/Unity-6.2%2B-black?style=flat&logo=unity) ![UI Toolkit](https://img.shields.io/badge/UI-UI%20Toolkit-blue?style=flat) ![License](https://img.shields.io/badge/License-MIT-green?style=flat)

## 📖 About The Project

**Tricky Ring** is a fast paced, 2D hyper casual game created as a technical challenge. The goal was to implement specific arcade mechanics while utilizing modern Unity features, specifically transitioning away from the legacy uGUI system in favor of the new **UI Toolkit**.

The player controls a rotating orb that must switch between an inner and outer orbital ring to dodge obstacles and collect points. As the player progresses, the rotation speed increases, testing reflexes and timing.

## 🛠 Technical Implementation

### 1. UI Toolkit (Runtime UI)
This project implements the entire user interface using **UI Toolkit**. This provides a CSS-like styling approach (`.uss`) and XML-based layouts (`.uxml`), separating the UI logic from its design.

### 2. Polar Coordinate Movement
Instead of standard vector movement, the player's position is calculated using polar coordinates converted to Cartesian coordinates for circular motion. This ensures smooth orbital mechanics without complex physics calculations.

The core movement logic in `PlayerController.cs`:

$$
x = \cos(\theta) \cdot r
$$
$$
y = \sin(\theta) \cdot r
$$

Where $\theta$ is the current angle and $r$ is the current radius (interpolated between `innerRadius` and `outerRadius`).

### 3. Editor Tooling
The project includes helper scripts like `CircleObstaclePlacer.cs` which utilize `[ContextMenu]` attributes to procedurally generate and align obstacles in a perfect circle within the Editor. This significantly speeds up the level design process.


## 🎮 How to Play

* **Tap or Click** to jump between the **Outer Ring** and the **Inner Ring**.
* Avoid the **spiked obstacles**.
* Collect the **white dots** to score points.
* Maintain a collection streak to change the background color and increase your score multiplier.

## 📦 Unity Version & Setup

* This project uses **Unity 6.2** (or later).
* Ensure the **Universal Render Pipeline (URP)** package is installed via the Package Manager to ensure materials render correctly.

## 🎨 Assets & Credits

* **Visual Assets:** [Kenney Assets](https://kenney.nl/) (Particle Pack, Game Icons).
* **Fonts:** NotoSans & Metropolis.
