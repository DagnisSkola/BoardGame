# 🎪 Circus Board Game (Unity)

## Overview

**Circus Board Game** is a turn-based board game developed in **Unity**, where the player competes against a **bot opponent** in a colorful circus-themed environment. The goal is simple: be the first to reach the final field by advancing across **120 fields** using a **six-sided dice**—but the path is full of surprises.

Unexpected **skip** and **setback** fields keep every match unpredictable and strategic.

---

## Game Objective

- Reach the **final (120th) field** before the bot player.
- Progress is determined by dice rolls and special board fields.

---

## Core Gameplay Mechanics

### 🎲 Dice System

- Players roll a **6-sided dice** on their turn.
- The rolled number determines how many fields the player advances.

### 👤 Players

- **Human Player** – controlled by the user.
- **Bot Player** – AI-controlled opponent that follows the same rules.

Turns alternate between the player and the bot.

---

## Board Design

- The board consists of **120 connected fields**.
- Each field may have a special effect or be neutral.

### Special Fields

- ⏭️ **Skip Fields** – cause the player to skip their next turn.
- ⬅️ **Setback Fields** – move the player backward by a certain number of fields.

These fields introduce risk and strategy, making each game different.

---

## Winning Conditions

- The first player to land on or pass the **120th field** wins the game.
- Dice rolls and special field effects can significantly change the outcome near the end.

---

## Features

- 🎪 Circus-themed board and atmosphere
- 🤖 Intelligent bot opponent
- 🎲 Random dice-based movement
- 🔄 Turn-based gameplay
- ⚠️ Dynamic skip and setback mechanics

---

## Technology Used

- **Game Engine:** Unity
- **Programming Language:** C#

---

## Notes

This project focuses on:

- Turn-based game logic
- Randomization (dice rolls)
- Basic AI behavior
- Board-based movement systems

## Images

![Main Menu](https://imgur.com/5p0a2H2)
![Player pick](https://imgur.com/zafRi1R)
![Zoomed in](https://imgur.com/ltLbKB9)
![Full view](https://imgur.com/OL610xQ)

## ToDo

- [x] Main menu buttons (start, quit, settings, leaderboard)
- [x] Character selection screen with animation
- [x] Settings scene ;
- [x] Board scene with throwable dice
- [x] Game logic with multiple players ;
- [x] Game camera ;
- [x] Board scene with throwable dice
- [x] Game logic with multiple players ;
- [x] Game camera ;
- [x] Leaderboard scene ;
- [x] Circus game functionality ;
- [x] Player bounces back if throw is higher than needed for finish;
- [x] Add text to notify which players turn it is;
- [x] Winning logic
---

Enjoy the show, and may luck be on your side! 🎭🎉

