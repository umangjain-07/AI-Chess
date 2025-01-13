using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlate : MonoBehaviour
{
    public GameObject controller;
    private GameObject reference = null;

    public int matrixX;
    public int matrixY;
    public bool attack = false;
    public bool castling = false; // Flag for castling moves

    public void Start()
    {
        if (attack)
        {
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        }
    }

    public void OnMouseUp()
    {
        controller = GameObject.FindGameObjectWithTag("GameController");
        Game gameController = controller.GetComponent<Game>();

        Chessman chessman = reference.GetComponent<Chessman>();
        string pieceColor = chessman.player; // Piece's player color
        string pieceName = reference.name.Replace("white_", "").Replace("black_", "");

        int startX = chessman.GetXBoard();
        int startY = chessman.GetYBoard();
        int endX = matrixX;
        int endY = matrixY;

        // Handle castling moves
        if (castling && chessman.name.Contains("king"))
        {
            HandleCastlingMove(gameController, chessman);
            return; // Exit after handling castling to avoid regular move logic
        }

        // Use the new CoordinateSystem for SAN and board updates
        string startCoord = CoordinateSystem.ConvertToChessNotation(startX, startY, pieceColor);
        string endCoord = CoordinateSystem.ConvertToChessNotation(endX, endY, pieceColor);
        string moveDescription = CoordinateSystem.GenerateSAN(
            pieceName,
            startCoord,
            endCoord,
            attack,
            false, // Not castling
            false // Not kingside (irrelevant here)
        );

        // Handle attacks
        if (attack)
        {
            GameObject capturedPiece = gameController.GetPosition(matrixX, matrixY);
            string capturedPieceName = capturedPiece.name.Replace("white_", "").Replace("black_", "");
            moveDescription += $" (captures {capturedPieceName})";

            // Check if a king is captured, ending the game
            if (capturedPiece.name == "white_king") gameController.Winner("black");
            if (capturedPiece.name == "black_king") gameController.Winner("white");

            Destroy(capturedPiece); // Remove the captured piece
        }

        

        // Record the move in the game's history
        gameController.RecordMove(moveDescription);

        // Update the board state for the moved piece
        gameController.SetPositionEmpty(startX, startY);
        chessman.SetXBoard(matrixX);
        chessman.SetYBoard(matrixY);
        chessman.SetCoords();
        gameController.SetPosition(reference);

        // Mark the piece as having moved
        chessman.hasMoved = true;
        chessman.CheckPawnPromotion();

        // Switch to the next player's turn
        gameController.NextTurn();

        // Destroy all move plates after the move
        chessman.DestroyMovePlates();
    }

    private void HandleCastlingMove(Game gameController, Chessman king)
    {
        // Clear the original position of the king
        gameController.SetPositionEmpty(king.GetXBoard(), king.GetYBoard());

        bool isKingside = matrixX == king.GetXBoard() + 2; // Determine if kingside castling

        // Handle rook movement during castling
        if (isKingside) // Kingside castling
        {
            Debug.Log("Kingside castling executed for " + king.name);

            GameObject rook = gameController.GetPosition(king.GetXBoard() + 3, king.GetYBoard());
            if (rook != null)
            {
                MoveRookDuringCastling(rook, king.GetXBoard() + 1, king.GetYBoard());
            }
        }
        else // Queenside castling
        {
            Debug.Log("Queenside castling executed for " + king.name);

            GameObject rook = gameController.GetPosition(king.GetXBoard() - 4, king.GetYBoard());
            if (rook != null)
            {
                MoveRookDuringCastling(rook, king.GetXBoard() - 1, king.GetYBoard());
            }
        }

        // Move the king to the new position
        king.SetXBoard(matrixX);
        king.SetYBoard(matrixY);
        king.SetCoords();
        gameController.SetPosition(reference); // Update the king's new position on the board
        king.hasMoved = true;

        // Record castling move
        string castlingSAN = CoordinateSystem.GenerateSAN(
            "king",
            "",
            "",
            false,
            true,
            isKingside
        );
        Debug.Log($"Move: {castlingSAN}");
        gameController.RecordMove(castlingSAN);

        // Switch to the next player's turn
        gameController.NextTurn();
        king.DestroyMovePlates();
    }

    private void MoveRookDuringCastling(GameObject rook, int newX, int newY)
    {
        Chessman rookChessman = rook.GetComponent<Chessman>();
        Game gameController = controller.GetComponent<Game>();

        gameController.SetPositionEmpty(rookChessman.GetXBoard(), rookChessman.GetYBoard());
        rookChessman.SetXBoard(newX);
        rookChessman.SetYBoard(newY);
        rookChessman.SetCoords();
        gameController.SetPosition(rook);
        rookChessman.hasMoved = true;
    }

    public void SetCoords(int x, int y)
    {
        matrixX = x;
        matrixY = y;
    }

    public void SetReference(GameObject obj)
    {
        reference = obj;
    }

    public GameObject GetReference()
    {
        return reference;
    }
}
