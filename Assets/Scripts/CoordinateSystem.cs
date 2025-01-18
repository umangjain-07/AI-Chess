using System;
using System.Collections.Generic;
using UnityEngine;

public static class CoordinateSystem
{
    /// <summary>
    /// Convert board coordinates (0-7, 0-7) to chess notation (e.g., e4, d2).
    /// </summary>
    public static string ConvertToChessNotation(int x, int y, string color)
    {
        char file = (char)('a' + x); // File (column: a-h)
        int rank = y + 1; // Rank (row: 1-8)
        return $"{file}{rank}";
    }

    /// <summary>
    /// Convert Standard Algebraic Notation (SAN) back to board coordinates.
    /// </summary>
    /// <param name="san">The SAN notation of the move.</param>
    /// <param name="currentPlayer">The current player ("white" or "black").</param>
    /// <param name="boardState">Current board state as a dictionary of positions and piece names.</param>
    /// <returns>A tuple containing the starting and ending board coordinates.</returns>
    public static (int startX, int startY, int endX, int endY, string promotion) ConvertSANToBoard(
        string san, 
        string currentPlayer, 
        Dictionary<(int, int), string> boardState)
    {
        san = san.Replace("x", ""); // Remove capture symbol
        string promotion = null;

        // Handle castling
        if (san == "O-O")
        {
            return currentPlayer == "white" ? (4, 0, 6, 0, null) : (4, 7, 6, 7, null);
        }
        if (san == "O-O-O")
        {
            return currentPlayer == "white" ? (4, 0, 2, 0, null) : (4, 7, 2, 7, null);
        }

        // Extract promotion
        if (san.Contains("="))
        {
            promotion = san[^1].ToString();
            san = san.Substring(0, san.IndexOf("="));
        }

        // Extract end position
        string endSquare = san.Length > 2 ? san[^2..] : san;
        int endX = endSquare[0] - 'a'; // File
        int endY = int.Parse(endSquare[1].ToString()) - 1; // Rank

        // Determine the piece type
        char piece = char.IsUpper(san[0]) ? san[0] : 'P'; // Default to pawn if lowercase
        List<(int, int)> possibleStarts = FindPossibleStarts(piece, endX, endY, boardState, currentPlayer);

        // Disambiguate if needed
        if (san.Length > 2 && !char.IsUpper(san[0]))
        {
            if (char.IsDigit(san[0])) // Rank disambiguation
            {
                possibleStarts.RemoveAll(start => start.Item2 != san[0] - '1');
            }
            else if (char.IsLetter(san[0])) // File disambiguation
            {
                possibleStarts.RemoveAll(start => start.Item1 != san[0] - 'a');
            }
        }

        if (possibleStarts.Count == 1)
        {
            return (possibleStarts[0].Item1, possibleStarts[0].Item2, endX, endY, promotion);
        }

        throw new ArgumentException($"Unable to determine start position for SAN: {san}");
    }

    /// <summary>
    /// Generate Standard Algebraic Notation (SAN) for a move.
    /// </summary>
    public static string GenerateSAN(string pieceName, string startCoord, string endCoord, bool isCapture, bool isCastling, bool isKingside, string promotedPiece = null)
    {
        // Handle castling moves
        if (isCastling)
        {
            return isKingside ? "O-O" : "O-O-O"; // Kingside or Queenside castling
        }

        string pieceLetter = GetPieceLetter(pieceName);
        string captureSymbol = isCapture ? "x" : ""; // Include 'x' for captures
        string promotion = promotedPiece != null ? $"={promotedPiece}" : ""; // Include promotion notation

        // For pawn captures, prefix the file of the starting square
        if (pieceLetter == "" && isCapture)
        {
            pieceLetter = startCoord[0].ToString();
        }

        return $"{pieceLetter}{captureSymbol}{endCoord}{promotion}";
    }

    /// <summary>
    /// Helper method to get the SAN letter for a piece.
    /// </summary>
    private static string GetPieceLetter(string pieceName)
    {
        if (pieceName.Contains("pawn")) return ""; // Pawns have no letter
        if (pieceName.Contains("knight")) return "N"; // Knight -> N
        if (pieceName.Contains("bishop")) return "B";
        if (pieceName.Contains("rook")) return "R";
        if (pieceName.Contains("queen")) return "Q";
        if (pieceName.Contains("king")) return "K";
        return "?"; // Unknown piece
    }

    /// <summary>
    /// Find possible start positions for a piece given the target position.
    /// </summary>
    private static List<(int, int)> FindPossibleStarts(char piece, int endX, int endY, Dictionary<(int, int), string> boardState, string currentPlayer)
    {
        List<(int, int)> possibleStarts = new List<(int, int)>();

        foreach (var kvp in boardState)
        {
            (int x, int y) = kvp.Key;
            string pieceOnBoard = kvp.Value;

            // Skip if not the right piece or color
            if (!pieceOnBoard.Contains(piece.ToString(), StringComparison.OrdinalIgnoreCase) || !pieceOnBoard.StartsWith(currentPlayer))
            {
                continue;
            }

            // Validate if this piece can legally move to the end position
            if (IsValidMove(piece, x, y, endX, endY, boardState, currentPlayer))
            {
                possibleStarts.Add((x, y));
            }
        }

        return possibleStarts;
    }

    /// <summary>
    /// Check if a move is valid for a piece.
    /// </summary>
    private static bool IsValidMove(char piece, int startX, int startY, int endX, int endY, Dictionary<(int, int), string> boardState, string currentPlayer)
    {
        // Simplified move validation for common pieces
        int dx = Math.Abs(startX - endX);
        int dy = Math.Abs(startY - endY);

        switch (piece)
        {
            case 'P': // Pawn
                return (currentPlayer == "white" && endY - startY == 1 && dx == 0) ||
                       (currentPlayer == "black" && startY - endY == 1 && dx == 0);
            case 'N': // Knight
                return (dx == 2 && dy == 1) || (dx == 1 && dy == 2);
            case 'B': // Bishop
                return dx == dy && !IsPathBlocked(startX, startY, endX, endY, boardState);
            case 'R': // Rook
                return (dx == 0 || dy == 0) && !IsPathBlocked(startX, startY, endX, endY, boardState);
            case 'Q': // Queen
                return (dx == dy || dx == 0 || dy == 0) && !IsPathBlocked(startX, startY, endX, endY, boardState);
            case 'K': // King
                return dx <= 1 && dy <= 1;
        }

        return false;
    }

    /// <summary>
    /// Check if a path is blocked for sliding pieces (bishop, rook, queen).
    /// </summary>
    private static bool IsPathBlocked(int startX, int startY, int endX, int endY, Dictionary<(int, int), string> boardState)
    {
        int dx = Math.Sign(endX - startX);
        int dy = Math.Sign(endY - startY);

        int x = startX + dx;
        int y = startY + dy;

        while (x != endX || y != endY)
        {
            if (boardState.ContainsKey((x, y)))
            {
                return true;
            }

            x += dx;
            y += dy;
        }

        return false;
    }
}
