// Assets/Scripts/CoordinateSystem.cs

using UnityEngine;

public static class CoordinateSystem
{
    // Convert board coordinates (0-7, 0-7) to chess notation (e.g., e4, d2)
    public static string ConvertToChessNotation(int x, int y, string playerColor)
    {
        char file = (char)('a' + x); // 'a' to 'h' for the file

        // Adjust rank based on player color perspective
        int rank;
        if (playerColor == "white")
        {
            rank = 8 - y; // For White, the ranks are from 8 (top) to 1 (bottom)
        }
        else // playerColor == "black"
        {
            rank = y + 1; // For Black, the ranks are from 1 (bottom) to 8 (top)
        }

        return $"{file}{rank}";
    }
}
