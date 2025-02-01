# Heroes Battle - turn-based combat game with server simulation

## Overview

**Heroes Battle** is a turn-based combat game where two characters fight using bows and arrows, along with special skills like healing, shield protection, and fireball attacks. The game imitates server communication using asynchronous requests, giving players the experience of how the game would function when working with a real server.

## Features

- **Turn-Based Combat:** Players take turns attacking the enemy and responding to the enemy's actions.
- **Character Abilities:** Includes attacks with bows and arrows, healing, shield protection, and fireball spells.
- **Server Simulation:** The game mimics server interactions using a mock server.
- **Asynchronous Requests:** Simulates server communication with async requests, providing a realistic feel for the game’s server interaction.

## Server Simulation

The game uses an interface `IServerAdapter` to connect to the server. In the current setup, a `MockServerAdapter` class simulates the server connection and communication. This mock class is ideal for testing and demonstrating server communication without needing an actual server.

### Configuring Server Delay

Server delay can be configured in the Unity editor:

1. Navigate to the project scene in the Hierarchy window.
2. Find the `MockServerAdapter` object at the path `Context/MockServerAdapter`.
3. Adjust the `Delay Ms` parameter to set the desired delay time in milliseconds for the server response.
4. Play the game in the Unity editor or make a build to experience the server communication simulation.

This allows for simulation of different server latencies and shows how the game responds.

## Usage

**Heroes Battle** demonstrates how turn-based games can interact with a server, showcasing server delays and asynchronous actions. It is a useful tool for understanding client-server interactions in game development.

## License

This project is licensed under the [Creative Commons Attribution-NonCommercial-NoDerivatives 4.0 International License (CC BY-NC-ND 4.0)](https://creativecommons.org/licenses/by-nc-nd/4.0/). It is intended for demonstration purposes and may not be used for commercial purposes, nor may it be modified or built upon.
