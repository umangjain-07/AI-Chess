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
    string pieceColor = chessman.player;
    string pieceName = reference.name.Replace("white_", "").Replace("black_", "");

    int startX = chessman.GetXBoard();
    int startY = chessman.GetYBoard();
    int endX = matrixX;
    int endY = matrixY;

    // Check if this move is a castling move
    if (castling && chessman.name.Contains("king"))
    {
        HandleCastlingMove(gameController, chessman);
        return; // Exit after handling castling to avoid regular move logic
    }

    string startCoord = ConvertToChessNotation(startX, startY, pieceColor);
    string endCoord = ConvertToChessNotation(endX, endY, pieceColor);

    string moveDescription = $"{pieceColor} {pieceName} moves from {startCoord} to {endCoord}";

    if (attack)
    {
        GameObject cp = gameController.GetPosition(matrixX, matrixY);
        string capturedPiece = cp.name.Replace("white_", "").Replace("black_", "");
        moveDescription += $" and captures {capturedPiece}";

        if (cp.name == "white_king") gameController.Winner("black");
        if (cp.name == "black_king") gameController.Winner("white");

        Destroy(cp);
    }

    gameController.RecordMove(moveDescription);

    // Update the board state for the regular move
    gameController.SetPositionEmpty(startX, startY);
    chessman.SetXBoard(matrixX);
    chessman.SetYBoard(matrixY);
    chessman.SetCoords();
    gameController.SetPosition(reference);

    chessman.hasMoved = true; // Mark the piece as having moved
    chessman.CheckPawnPromotion();

    gameController.NextTurn();
    chessman.DestroyMovePlates();
}

private void HandleCastlingMove(Game gameController, Chessman king)
{
    // Clear the original position of the king
    gameController.SetPositionEmpty(king.GetXBoard(), king.GetYBoard());

    if (matrixX == king.GetXBoard() + 2) // Kingside castling
    {
        Debug.Log("Kingside castling executed for " + king.name);

        // Move the Kingside rook
        GameObject rook = gameController.GetPosition(king.GetXBoard() + 3, king.GetYBoard());
        if (rook != null)
        {
            Chessman rookChessman = rook.GetComponent<Chessman>();
            gameController.SetPositionEmpty(rookChessman.GetXBoard(), rookChessman.GetYBoard()); // Clear the old rook position
            rookChessman.SetXBoard(king.GetXBoard() + 1); // Move the rook
            rookChessman.SetYBoard(king.GetYBoard());
            rookChessman.SetCoords();
            gameController.SetPosition(rook); // Update the board array
            rookChessman.hasMoved = true;
        }
    }
    else if (matrixX == king.GetXBoard() - 2) // Queenside castling
    {
        Debug.Log("Queenside castling executed for " + king.name);

        // Move the Queenside rook
        GameObject rook = gameController.GetPosition(king.GetXBoard() - 4, king.GetYBoard());
        if (rook != null)
        {
            Chessman rookChessman = rook.GetComponent<Chessman>();
            gameController.SetPositionEmpty(rookChessman.GetXBoard(), rookChessman.GetYBoard()); // Clear the old rook position
            rookChessman.SetXBoard(king.GetXBoard() - 1); // Move the rook
            rookChessman.SetYBoard(king.GetYBoard());
            rookChessman.SetCoords();
            gameController.SetPosition(rook); // Update the board array
            rookChessman.hasMoved = true;
        }
    }

    // Move the king to the new position
    king.SetXBoard(matrixX);
    king.SetYBoard(matrixY);
    king.SetCoords();
    gameController.SetPosition(reference); // Update the king's new position on the board
    king.hasMoved = true;

    // Record the move and switch turns
    gameController.RecordMove($"{king.player} king castles");
    gameController.NextTurn();
    king.DestroyMovePlates();
}

    private string ConvertToChessNotation(int x, int y, string playerColor)
    {
        char file = (char)('a' + x);
        int rank = playerColor == "white" ? y + 1 : 8 - y;
        return $"{file}{rank}";
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
