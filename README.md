# 🐒 Jungle Law: Animal War Game 🐊

**Jungle Law** is a 2D Turn-Based Strategy (TBS) game built with the Unity engine. Lead an army of unique animals (Capybaras, Monkeys, Frogs) to battle for dominance over the jungle. Capture strategic control points, manage your economy, utilize terrain height advantages, and adapt to dynamic weather conditions to destroy the enemy base!

---

## 🎮 Core Features

- **Classic Turn-Based Gameplay:** Tactical 1v1 turn loop (Player 1 vs Player 2) powered by two distinct resources: Gold and Mana.
- **Persistent Control Point System:** \* Standard strategic nodes (1x1) providing a steady turn-based income.
    - **Large Area Headquarters (2x2):** A massive high-value zone in the center of the map that requires smart positioning.
    - _State Retention:_ Capture points permanently remember their owner, continuing to generate resources even after your units move away.
- **Dynamic Weather Grid:** Random rain events affect the battlefield in real-time. Downpours reduce both the movement range and the vision radius (_Fog of War_) of all units.
- **Tactical Height Advantage (Hills):** Standing on high ground grants units a powerful combat buff (+10 Attack) and significantly extends their radar sight lines (+2 Vision).
- **Advanced Fog of War:** Real-time tile visibility calculations based entirely on your active units' positions and sight ranges.
- **Reactive Unit Expressions:** Animals react visually to battlefield events with custom animations for attacking, surprise expressions upon taking damage, and a smooth tombstone transformation sequence upon death.

---

## 🕹️ Controls

- **Camera Movement:** Use `WASD` or `Arrow Keys` to pan seamlessly across the map. The camera movement completely ignores UI panels, allowing effortless edge-panning.
- **Camera Zoom:** Use the `Mouse Scrollwheel` for smooth orthographic zooming, strictly bounded within the map's native limits.
- **Selection & Movement:** `Left Mouse Button (LPM)` to select a unit, and click on any highlighted green tile to execute a move command.
- **Unit Recruitment:** Click on animal icons in the lower command panel (`CommandPanel`) to draft new units into your squad.
- **End Turn:** Click the `End Turn` button on the top bar to pass control to the opposing player.

---

## 🛠️ Technical Stack

- **Engine:** Unity (Version 6 LTS / 6000.3.11f1)
- **Language:** C# (Object-Oriented Architecture)
- **Render Pipeline:** Universal Render Pipeline (URP 2D)
- **Core Architecture Components:**
    - `TurnManager.cs` – Drives the main turn state machine, economy ticks, and handles safe asynchronous initialization via `Coroutines` (`WaitForEndOfFrame`).
    - `Animal.cs` – Parent unit class executing stat calculation matrices (`RecalculateStats`) alongside combat and damage feedback states.
    - `CapturePoint.cs` – Controls multi-tile spatial tracking over a deterministic integer grid matrix (`Vector3Int`).
    - `CameraController.cs` – Handles orthographic projection matrix clamping via `Mathf.Clamp`.

---

## 📦 Installation & Setup

1. Clone the repository to your local machine:
    ```bash
    git clone [https://github.com/YourUsername/animal-war-game.git](https://github.com/YourUsername/animal-war-game.git)
    ```
