using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : MonoBehaviour
{
    private Game gameController;
    private string aiPlayerColor = "black"; // Set AI to play as "black"
    private bool hasMovedThisTurn = false; // Flag to prevent AI from acting more than once per turn

    private void Start()
    {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();
    }

    private void Update()
    {
        // Check if it's the AI's turn, the game is not over, and AI hasn't moved yet this turn
        if (!gameController.IsGameOver() && gameController.GetCurrentPlayer() == aiPlayerColor && !hasMovedThisTurn)
        {
            hasMovedThisTurn = true; // Set flag to indicate the AI is about to move
            StartCoroutine(MakeMoveAfterDelay());
        }
        else if (gameController.GetCurrentPlayer() != aiPlayerColor)
        {
            hasMovedThisTurn = false; // Reset flag on player's turn
        }
    }

    private IEnumerator MakeMoveAfterDelay()
    {
        yield return new WaitForSeconds(1.0f); // Small delay for realism

        List<MovePlate> legalMovePlates = GetLegalMovePlates();
        
        if (legalMovePlates.Count > 0)
        {
            // Randomly select a MovePlate
            MovePlate selectedMovePlate = legalMovePlates[Random.Range(0, legalMovePlates.Count)];

            // Execute the move on the selected MovePlate's coordinates
            Chessman piece = selectedMovePlate.GetReference().GetComponent<Chessman>();
            ExecuteMove(piece, selectedMovePlate);
        }
    }

    private List<MovePlate> GetLegalMovePlates()
    {
        List<MovePlate> legalMovePlates = new List<MovePlate>();
        GameObject[] playerPieces = aiPlayerColor == "white" ? gameController.GetWhitePieces() : gameController.GetBlackPieces();

        foreach (var pieceObj in playerPieces)
        {
            if (pieceObj == null) continue; // Ensure piece hasn't been destroyed
            Chessman piece = pieceObj.GetComponent<Chessman>();
            if (piece != null)
            {
                // Destroy any existing MovePlates and generate new ones
                piece.DestroyMovePlates();

                // Check for castling if the piece is a king
                if (piece.name.Contains("king") && !piece.hasMoved)
                {
                    piece.CheckCastlingOptions(); // Generate castling options if the king hasn’t moved
                }
                else
                {
                    piece.InitiateMovePlates();
                }

                // Collect all generated MovePlates for this piece
                GameObject[] movePlates = GameObject.FindGameObjectsWithTag("MovePlate");
                foreach (var movePlateObj in movePlates)
                {
                    MovePlate movePlate = movePlateObj.GetComponent<MovePlate>();
                    if (movePlate != null && movePlate.GetReference() == pieceObj)
                    {
                        legalMovePlates.Add(movePlate);
                    }
                }

                // Clean up MovePlates after collecting possible moves
                piece.DestroyMovePlates();
            }
        }

        return legalMovePlates;
    }

    private void ExecuteMove(Chessman piece, MovePlate movePlate)
    {
        if (piece == null || movePlate == null) return;

        int x = movePlate.matrixX;
        int y = movePlate.matrixY;

        // Check if this is a castling move (king moves two squares horizontally)
        bool isCastlingMove = piece.name.Contains("king") && Mathf.Abs(piece.GetXBoard() - x) == 2;

        // Convert old and new coordinates to chess notation using the player's color
        string oldPosition = CoordinateSystem.ConvertToChessNotation(piece.GetXBoard(), piece.GetYBoard(), aiPlayerColor);
        string newPosition = CoordinateSystem.ConvertToChessNotation(x, y, aiPlayerColor);

        // Log the move in standard chess notation
        Debug.Log($"{piece.name} moved from {oldPosition} to {newPosition}");

        GameObject target = gameController.GetPosition(x, y);

        if (target != null)
        {
            if (target.name == "white_king" || target.name == "black_king")
            {
                gameController.Winner(aiPlayerColor);
            }
            Destroy(target);
        }

        // If castling, move the rook to its designated castling position
        if (isCastlingMove)
        {
            int rookStartX = x == 6 ? 7 : 0; // Rook starts at 7 for kingside castling, 0 for queenside
            int rookEndX = x == 6 ? 5 : 3;   // Rook ends at 5 for kingside, 3 for queenside
            int rookY = y; // Rook is on the same row as the king

            GameObject rookObj = gameController.GetPosition(rookStartX, rookY);
            if (rookObj != null && rookObj.GetComponent<Chessman>().name.Contains("rook"))
            {
                Chessman rook = rookObj.GetComponent<Chessman>();

                gameController.SetPositionEmpty(rook.GetXBoard(), rook.GetYBoard());
                rook.SetXBoard(rookEndX);
                rook.SetYBoard(rookY);
                rook.SetCoords();
                gameController.SetPosition(rook.gameObject);
            }
        }

        // Move the king or other piece to the MovePlate position
        gameController.SetPositionEmpty(piece.GetXBoard(), piece.GetYBoard());
        piece.SetXBoard(x);
        piece.SetYBoard(y);
        piece.SetCoords();
        gameController.SetPosition(piece.gameObject);

        // Check for pawn promotion if the piece is a pawn
        piece.CheckPawnPromotion();

        // Switch turns after the move
        gameController.NextTurn();
    }
}
