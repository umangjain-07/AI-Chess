using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : MonoBehaviour
{
    private Game gameController;
    private string aiPlayerColor; // AI's assigned color ("white" or "black")
    private bool hasMovedThisTurn = false; // Prevent AI from acting more than once per turn

    private void Start()
    {
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<Game>();

        // Randomly assign the AI player color
        aiPlayerColor = Random.value > 0.5f ? "black" : "white";
        Debug.Log($"[AIPlayer] AI assigned color: {aiPlayerColor}");
    }

    private void Update()
    {
        // Check if it's the AI's turn, the game is not over, and AI hasn't moved yet this turn
        if (!gameController.IsGameOver() && gameController.GetCurrentPlayer() == aiPlayerColor && !hasMovedThisTurn)
        {
            hasMovedThisTurn = true; // Prevent multiple moves in the same turn
            StartCoroutine(MakeMoveAfterDelay());
        }
        else if (gameController.GetCurrentPlayer() != aiPlayerColor)
        {
            hasMovedThisTurn = false; // Reset flag when it's not AI's turn
        }
    }

    private IEnumerator MakeMoveAfterDelay()
    {
        yield return new WaitForSeconds(1.0f); // Small delay for realism

        List<MovePlate> legalMovePlates = GetLegalMovePlates();
        if (legalMovePlates.Count > 0)
        {
            // Randomly select a move
            MovePlate selectedMovePlate = legalMovePlates[Random.Range(0, legalMovePlates.Count)];
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
            if (pieceObj == null) continue;

            Chessman piece = pieceObj.GetComponent<Chessman>();
            if (piece != null)
            {
                // Generate all legal move plates for the piece
                piece.DestroyMovePlates();
                if (piece.name.Contains("king") && !piece.hasMoved)
                {
                    piece.CheckCastlingOptions();
                }
                else
                {
                    piece.InitiateMovePlates();
                }

                // Collect move plates
                foreach (var movePlateObj in GameObject.FindGameObjectsWithTag("MovePlate"))
                {
                    MovePlate movePlate = movePlateObj.GetComponent<MovePlate>();
                    if (movePlate != null && movePlate.GetReference() == pieceObj)
                    {
                        legalMovePlates.Add(movePlate);
                    }
                }

                piece.DestroyMovePlates(); // Clean up move plates after collection
            }
        }

        Debug.Log($"[AIPlayer] Analyzed moves: {legalMovePlates.Count} ");
        return legalMovePlates;
    }

    private void ExecuteMove(Chessman piece, MovePlate movePlate)
    {
        if (piece == null || movePlate == null) return;

        int x = movePlate.matrixX;
        int y = movePlate.matrixY;

        // Determine castling
        bool isCastlingMove = piece.name.Contains("king") && Mathf.Abs(piece.GetXBoard() - x) == 2;

        // Generate move in Standard Algebraic Notation (SAN)
        string oldPosition = CoordinateSystem.ConvertToChessNotation(piece.GetXBoard(), piece.GetYBoard(), aiPlayerColor);
        string newPosition = CoordinateSystem.ConvertToChessNotation(x, y, aiPlayerColor);

        GameObject target = gameController.GetPosition(x, y);
        bool isCapture = target != null;

        string moveDescription;
        if (isCastlingMove)
        {
            moveDescription = x > piece.GetXBoard() ? "O-O" : "O-O-O"; // Kingside or Queenside castling
        }
        else
        {
            string promotedPiece = null;
            if (piece.name.Contains("pawn") && (y == 0 || y == 7))
            {
                promotedPiece = "Q"; // Default promotion to queen
            }

            moveDescription = CoordinateSystem.GenerateSAN(piece.name, oldPosition, newPosition, isCapture, false, false, promotedPiece);
        }

        

        // Handle captures
        if (isCapture)
        {
            Destroy(target);
        }

        // Handle castling-specific logic
        if (isCastlingMove)
        {
            HandleCastlingMove(piece, x, y);
        }

        // Move the piece
        gameController.SetPositionEmpty(piece.GetXBoard(), piece.GetYBoard());
        piece.SetXBoard(x);
        piece.SetYBoard(y);
        piece.SetCoords();
        gameController.SetPosition(piece.gameObject);

        // Handle promotion
        piece.CheckPawnPromotion();

        // Record move and switch turns
        gameController.RecordMove(moveDescription);
        gameController.NextTurn();
    }

    private void HandleCastlingMove(Chessman king, int kingX, int kingY)
    {
        int rookStartX = kingX == 6 ? 7 : 0; // Kingside or Queenside
        int rookEndX = kingX == 6 ? 5 : 3;

        GameObject rookObj = gameController.GetPosition(rookStartX, kingY);
        if (rookObj != null)
        {
            Chessman rook = rookObj.GetComponent<Chessman>();

            // Move the rook to the appropriate position
            gameController.SetPositionEmpty(rook.GetXBoard(), rook.GetYBoard());
            rook.SetXBoard(rookEndX);
            rook.SetYBoard(kingY);
            rook.SetCoords();
            gameController.SetPosition(rook.gameObject);
        }
    }
}
