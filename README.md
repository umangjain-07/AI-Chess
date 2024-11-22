# Unity Chess Game

A fully functional chess game built with Unity, designed to provide an engaging and interactive chess-playing experience. The project features core chess mechanics, basic AI capabilities, and an intuitive user interface.

## Features

- **Playable Chess Game**: Complete chess game with all standard rules and mechanics, including castling, en passant, and pawn promotion.
- **Player vs AI Mode**: A basic AI player that selects moves randomly from the available legal moves.
- **Interactive UI**: Highlighted move options, clear winner display, and a restart feature.
- **Customizable Chess Pieces**: Easily replaceable sprites for chess pieces to suit your design style.
- **Pawn Promotion**: Automatic promotion of pawns to queens upon reaching the last rank.
- **Castling**: Properly implemented kingside and queenside castling mechanics.

## Gameplay Overview

1. **Player Turns**: The game alternates turns between the white and black players.
2. **Legal Moves Highlighted**: Click a chess piece to see its possible moves highlighted on the board.
3. **AI Player**: The AI moves randomly but adheres to chess rules.
4. **Victory Conditions**: The game detects checkmate and displays the winner.

## How to Play

1. **White Moves First**: By default, the player is assigned the white pieces and plays the first move.
2. **Piece Movement**: Click a piece to highlight its possible moves. Click a highlighted square to move the piece.
3. **Restart Game**: At the end of the game, click the board to restart.

## Project Structure

- **Scripts**:
  - `Game.cs`: Manages the chessboard, game state, and turn transitions.
  - `Chessman.cs`: Represents individual chess pieces and their behaviors.
  - `MovePlate.cs`: Handles the visual representation and logic of possible moves.
  - `AIPlayer.cs`: Contains the logic for the AI's random move generation.
- **Prefabs**:
  - `ChessPiece`: Prefab used to instantiate the chess pieces.
  - `MovePlate`: Prefab for highlighting legal moves on the board.
- **Sprites**: Sprites for the chess pieces (white and black variants).

## Requirements

- Unity 2020.3 LTS or later.
- Basic understanding of Unity's GameObject and Component systems.


