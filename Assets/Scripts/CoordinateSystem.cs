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
    /// Generate Standard Algebraic Notation (SAN) for a move.
    /// </summary>
    /// <param name="pieceName">The name of the piece (e.g., "pawn", "queen").</param>
    /// <param name="startCoord">Starting coordinate in chess notation (e.g., e2).</param>
    /// <param name="endCoord">Ending coordinate in chess notation (e.g., e4).</param>
    /// <param name="isCapture">Whether the move involves capturing another piece.</param>
    /// <param name="isCastling">Whether the move is castling.</param>
    /// <param name="isKingside">If castling, whether it's kingside (true) or queenside (false).</param>
    /// <param name="promotedPiece">The piece the pawn promotes to (e.g., "Q" for queen, null otherwise).</param>
    /// <returns>The move in SAN format.</returns>
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
    /// <param name="pieceName">The name of the piece (e.g., "pawn", "knight").</param>
    /// <returns>The corresponding SAN letter (e.g., "", "N", "B").</returns>
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
}
