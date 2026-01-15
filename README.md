# Laundromat Simulator – Phase 1

## Overview
This project is a Unity-based laundromat simulation focusing on the core gameplay loop for Phase 1.  
Phase 1 expands the daily workflow, customer behavior, and operational systems.

---

## Unity Version

- **Unity Editor:** 2022.3.62f3 (LTS)
- **link : https://unity.com/releases/editor/whats-new/2022.3.62f3
⚠️ Important:
This project was developed and tested on Unity 2022 LTS.  
It is **not compatible with Unity 6** at this time due to dependency and API changes introduced in newer versions.

Please open the project using **Unity 2022.3.62f3** (or the same 2022 LTS series) to ensure correct behavior.


---

## Project Setup
1. Clone the repository:
2. Open Unity Hub
3. Click **Open Project**
4. Select the project root folder
5. Allow Unity to import assets (Library folder is generated automatically)

---

## How to Play (Phase 1)
- Start scene: **GamePlay** (in Assets go to Scene folder and there open GamePlay Scene)
- Player manages a laundromat during business hours
- Two customer types:
- **Self-Service (SS)**: Uses washer → dryer → exits
- **Full-Service (FS)**: Drop-off and pickup handled at the main counter
- End the day manually to view the daily summary

---

## Implemented Systems (Phase 1)

### Time-of-Day System
- In-game clock with AM/PM
- Customers only spawn during business hours
- End Day action resets daily counters

### Unified Full-Service Workflow
- Single main counter for:
- Order intake
- Order pickup
- Upgrade desk removed
- Upgrade system accessed via office laptop
- Player processes orders through washer, dryer, and folding interaction

### Self-Service Customers
- Washer → dryer → exit behavior
- Basic waiting animations
- Minor pathing polish in progress

### Daily Summary
- Displays:
- Self-service earnings
- Full-service earnings
- Total revenue
- Orders completed
- Resets each day

---

## Build
A playable Windows build is provided separately with this milestone.


---

## Git Repository Notes
- Repository reflects the full development history available
- Unity-generated folders (Library, Temp, etc.) are excluded via .gitignore
- All gameplay-related changes are committed

---

## Next Steps
- The game is currently in an active **QA and playtesting phase** for Phase 1
- Minor edge cases are being identified and refined regularly
- General bug fixing and stability improvements are ongoing based on testing
