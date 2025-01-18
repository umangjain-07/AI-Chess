using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : MonoBehaviour
{
    private Game gameController;
    private string aiPlayerColor; // AI's assigned color ("white" or "black")
    private bool hasMovedThisTurn = false; // Prevent AI from acting more than once per turn
    private const int MaxDepth =2; // Depth for Minimax algorithm
    private const float MaxComputeTime = 10.0f; // Max AI thinking time in seconds
    private HashSet<string> previousMoves = new HashSet<string>(); // Track previous AI moves

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
            hasMovedThisTurn = true;
            StartCoroutine(MakeMoveAfterDelay());
        }
        else if (gameController.GetCurrentPlayer() != aiPlayerColor)
        {
            hasMovedThisTurn = false; // Reset flag when it's not AI's turn
        }
    }

    private IEnumerator MakeMoveAfterDelay()
{
    yield return new WaitForSeconds(0.0f); // Add delay for realism

    float startTime = Time.time;
    var bestMove = GetBestMove(MaxDepth, startTime);

    if (bestMove != null)
    {
        ExecuteMove(bestMove.Piece, bestMove.TargetMovePlate, true); // Real move

        // Hand over control to the other player after the move
        gameController.NextTurn();
    }
    else
    {
        Debug.LogWarning("[AIPlayer] No valid moves found.");
    }
}


    private Move GetBestMove(int depth, float startTime)
{
    List<Move> allMoves = GetAllPossibleMoves();
    // Debug.Log($"[AIPlayer] Total legal moves available: {allMoves.Count}");
    float bestValue = float.NegativeInfinity;
    Move bestMove = null;

    foreach (var move in allMoves)
    {
        if (Time.time - startTime > MaxComputeTime)
        {
            Debug.LogWarning("[AIPlayer] Exceeded computation time. Stopping search.");
            break;
        }

        // Simulate the move (isRealMove = false)
        ExecuteMove(move.Piece, move.TargetMovePlate, false);
        float moveValue = Minimax(depth - 1, float.NegativeInfinity, float.PositiveInfinity, false, startTime);
        UndoMove(move.Piece, move.PreviousPosition, move.CapturedPiece);

        if (moveValue > bestValue && !previousMoves.Contains(move.ToString()))
        {
            bestValue = moveValue;
            bestMove = move;
        }
    }

    if (bestMove != null)
    {
        previousMoves.Add(bestMove.ToString()); // Track the move to avoid repetition

        // Generate SAN for the best move
        string startCoord = CoordinateSystem.ConvertToChessNotation(bestMove.PreviousPosition.Item1, bestMove.PreviousPosition.Item2, aiPlayerColor);
        string endCoord = CoordinateSystem.ConvertToChessNotation(bestMove.TargetMovePlate.matrixX, bestMove.TargetMovePlate.matrixY, aiPlayerColor);
        bool isCapture = bestMove.CapturedPiece != null;
        string san = CoordinateSystem.GenerateSAN(
            bestMove.Piece.name,
            startCoord,
            endCoord,
            isCapture,
            isCastling: false,
            isKingside: false
        );

        Debug.Log($"[AIPlayer] Move: {san}");
    }

    return bestMove;
}



    private float Minimax(int depth, float alpha, float beta, bool isMaximizing, float startTime)
{
    if (Time.time - startTime > MaxComputeTime)
    {
        Debug.LogWarning("[AIPlayer] Minimax exceeded computation time.");
        return 0; // Stop computation if time limit exceeded
    }

    if (depth == 0 || gameController.IsGameOver())
    {
        return EvaluateBoard();
    }

    List<Move> allMoves = GetAllPossibleMoves(isMaximizing ? aiPlayerColor : GetOpponentColor());
    

    if (isMaximizing)
    {
        float maxEval = float.NegativeInfinity;

        foreach (var move in allMoves)
        {
            // Simulate the move (isRealMove = false)
            ExecuteMove(move.Piece, move.TargetMovePlate, false);
            float eval = Minimax(depth - 1, alpha, beta, false, startTime);
            UndoMove(move.Piece, move.PreviousPosition, move.CapturedPiece);

            maxEval = Mathf.Max(maxEval, eval);
            alpha = Mathf.Max(alpha, eval);
            if (beta <= alpha)
            {
                Debug.Log("[AIPlayer] Alpha-Beta Pruning triggered in maximizing.");
                break;
            }
        }

        return maxEval;
    }
    else
    {
        float minEval = float.PositiveInfinity;

        foreach (var move in allMoves)
        {
            // Simulate the move (isRealMove = false)
            ExecuteMove(move.Piece, move.TargetMovePlate, false);
            float eval = Minimax(depth - 1, alpha, beta, true, startTime);
            UndoMove(move.Piece, move.PreviousPosition, move.CapturedPiece);

            minEval = Mathf.Min(minEval, eval);
            beta = Mathf.Min(beta, eval);
            if (beta <= alpha)
            {
                Debug.Log("[AIPlayer] Alpha-Beta Pruning triggered in minimizing.");
                break;
            }
        }

        return minEval;
    }
}


    private float EvaluateBoard()
{
    float score = 0;

    foreach (var piece in gameController.GetAllPieces())
    {
        Chessman chessman = piece.GetComponent<Chessman>();
        if (chessman != null)
        {
            float value = GetPieceValue(chessman.name);
            score += chessman.player == aiPlayerColor ? value : -value;
        }
    }

    return score;
}

private bool IsUnderThreat(Chessman piece, string attackerColor)
{
    GameObject[] opponentPieces = attackerColor == "white"
        ? gameController.GetWhitePieces()
        : gameController.GetBlackPieces();

    foreach (var opponentPieceObj in opponentPieces)
    {
        if (opponentPieceObj == null) continue;

        Chessman opponentPiece = opponentPieceObj.GetComponent<Chessman>();
        if (opponentPiece != null && CanCapture(piece, opponentPiece))
        {
            return true;
        }
    }

    return false;
}

// Checks if an opponent piece can directly attack the target piece
private bool CanCapture(Chessman target, Chessman attacker)
{
    attacker.DestroyMovePlates(); // Clear previous plates
    attacker.InitiateMovePlates(); // Generate valid move plates

    foreach (var movePlateObj in GameObject.FindGameObjectsWithTag("MovePlate"))
    {
        MovePlate movePlate = movePlateObj.GetComponent<MovePlate>();
        if (movePlate != null &&
            movePlate.matrixX == target.GetXBoard() &&
            movePlate.matrixY == target.GetYBoard())
        {
            attacker.DestroyMovePlates(); // Clean up move plates
            return true;
        }
    }

    attacker.DestroyMovePlates(); // Clean up move plates
    return false;
}


    private float GetPieceValue(string pieceName)
    {
        if (pieceName.Contains("pawn")) return 1;
        if (pieceName.Contains("knight")) return 3;
        if (pieceName.Contains("bishop")) return 3.25f;
        if (pieceName.Contains("rook")) return 5;
        if (pieceName.Contains("queen")) return 9;
        if (pieceName.Contains("king")) return 1000; // High value for the king
        return 0;
    }

    private List<Move> GetAllPossibleMoves(string playerColor = null)
{
    List<Move> moves = new List<Move>();
    List<Move> capturingMoves = new List<Move>();
    playerColor ??= aiPlayerColor;

    GameObject[] playerPieces = playerColor == "white"
        ? gameController.GetWhitePieces()
        : gameController.GetBlackPieces();

    foreach (var pieceObj in playerPieces)
    {
        if (pieceObj == null) continue;

        Chessman piece = pieceObj.GetComponent<Chessman>();
        if (piece != null)
        {
            piece.DestroyMovePlates(); // Clear previous plates
            piece.InitiateMovePlates(); // Generate valid move plates

            foreach (var movePlate in GameObject.FindGameObjectsWithTag("MovePlate"))
            {
                MovePlate plate = movePlate.GetComponent<MovePlate>();
                if (plate != null && plate.GetReference() == pieceObj)
                {
                    var capturedPiece = gameController.GetPosition(plate.matrixX, plate.matrixY);
                    var move = new Move(piece, plate, (piece.GetXBoard(), piece.GetYBoard()), capturedPiece);

                    if (capturedPiece != null)
                    {
                        capturingMoves.Add(move); // Prioritize capturing moves
                    }
                    else
                    {
                        moves.Add(move);
                    }
                }
            }

            piece.DestroyMovePlates(); // Clean up after collecting moves
        }
    }

    // Combine capturing moves first, followed by non-capturing moves
    capturingMoves.AddRange(moves);
    return capturingMoves;
}



    private void ExecuteMove(Chessman piece, MovePlate movePlate, bool isRealMove)
{
    string startCoord = CoordinateSystem.ConvertToChessNotation(piece.GetXBoard(), piece.GetYBoard(), aiPlayerColor);
    string endCoord = CoordinateSystem.ConvertToChessNotation(movePlate.matrixX, movePlate.matrixY, aiPlayerColor);

    bool isCapture = gameController.GetPosition(movePlate.matrixX, movePlate.matrixY) != null;
    string san = CoordinateSystem.GenerateSAN(
        piece.name,
        startCoord,
        endCoord,
        isCapture,
        isCastling: false,
        isKingside: false
    );

    
    int x = movePlate.matrixX;
    int y = movePlate.matrixY;

    // Update the game state
    gameController.SetPositionEmpty(piece.GetXBoard(), piece.GetYBoard());
    var capturedPiece = gameController.GetPosition(x, y);

    if (capturedPiece != null)
    {
        if (isRealMove)
        {
            Destroy(capturedPiece); // Destroy the piece only for real moves
        }
        else
        {
            capturedPiece.SetActive(false); // Deactivate for simulation
        }
    }

    // Update piece's position
    piece.SetXBoard(x);
    piece.SetYBoard(y);
    piece.SetCoords();
    gameController.SetPosition(piece.gameObject);
}



private void UndoMove(Chessman piece, (int x, int y) previousPosition, GameObject capturedPiece)
{
    string currentPos = CoordinateSystem.ConvertToChessNotation(piece.GetXBoard(), piece.GetYBoard(), aiPlayerColor);
    string oldPos = CoordinateSystem.ConvertToChessNotation(previousPosition.x, previousPosition.y, aiPlayerColor);

    
    gameController.SetPositionEmpty(piece.GetXBoard(), piece.GetYBoard());
    piece.SetXBoard(previousPosition.x);
    piece.SetYBoard(previousPosition.y);
    piece.SetCoords();
    gameController.SetPosition(piece.gameObject);

    if (capturedPiece != null)
    {
        string capturedPos = CoordinateSystem.ConvertToChessNotation(previousPosition.x, previousPosition.y, aiPlayerColor);
        capturedPiece.SetActive(true); // Reactivate the captured piece
        Chessman capturedChessman = capturedPiece.GetComponent<Chessman>();
        gameController.SetPosition(capturedPiece);
    }
}


    private string GetOpponentColor()
    {
        return aiPlayerColor == "white" ? "black" : "white";
    }

    private class Move
    {
        public Chessman Piece { get; }
        public MovePlate TargetMovePlate { get; }
        public (int x, int y) PreviousPosition { get; }
        public GameObject CapturedPiece { get; }

        public Move(Chessman piece, MovePlate targetMovePlate, (int x, int y) previousPosition, GameObject capturedPiece)
        {
            Piece = piece;
            TargetMovePlate = targetMovePlate;
            PreviousPosition = previousPosition;
            CapturedPiece = capturedPiece;
        }

        public override string ToString()
        {
            return $"{Piece.name} to {TargetMovePlate.matrixX},{TargetMovePlate.matrixY}";
        }
    }
}
