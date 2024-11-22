using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game : MonoBehaviour
{
    // Reference from Unity IDE
    public GameObject[] GetWhitePieces() => playerWhite;
    public GameObject[] GetBlackPieces() => playerBlack;

    public GameObject chesspiece;

    // Board positions and player arrays
    private GameObject[,] positions = new GameObject[8, 8];
    private GameObject[] playerBlack = new GameObject[16];
    private GameObject[] playerWhite = new GameObject[16];

    // Current turn and sprites for promotion
    private string currentPlayer = "white";
    public Sprite white_queen;
    public Sprite black_queen;

    // Game state
    private bool gameOver = false;

    // Move history
    private List<string> moveHistory = new List<string>();

    // Unity calls this when the game starts
    public void Start()
    {
        playerWhite = new GameObject[] {
            Create("white_rook", 0, 0), Create("white_knight", 1, 0),
            Create("white_bishop", 2, 0), Create("white_queen", 3, 0),
            Create("white_king", 4, 0), Create("white_bishop", 5, 0),
            Create("white_knight", 6, 0), Create("white_rook", 7, 0),
            Create("white_pawn", 0, 1), Create("white_pawn", 1, 1),
            Create("white_pawn", 2, 1), Create("white_pawn", 3, 1),
            Create("white_pawn", 4, 1), Create("white_pawn", 5, 1),
            Create("white_pawn", 6, 1), Create("white_pawn", 7, 1)
        };

        playerBlack = new GameObject[] {
            Create("black_rook", 0, 7), Create("black_knight", 1, 7),
            Create("black_bishop", 2, 7), Create("black_queen", 3, 7),
            Create("black_king", 4, 7), Create("black_bishop", 5, 7),
            Create("black_knight", 6, 7), Create("black_rook", 7, 7),
            Create("black_pawn", 0, 6), Create("black_pawn", 1, 6),
            Create("black_pawn", 2, 6), Create("black_pawn", 3, 6),
            Create("black_pawn", 4, 6), Create("black_pawn", 5, 6),
            Create("black_pawn", 6, 6), Create("black_pawn", 7, 6)
        };

        // Set all piece positions on the board
        for (int i = 0; i < playerBlack.Length; i++)
        {
            SetPosition(playerBlack[i]);
            SetPosition(playerWhite[i]);
        }
    }

    // Method to create a chess piece
    public GameObject Create(string name, int x, int y)
    {
        GameObject obj = Instantiate(chesspiece, new Vector3(0, 0, -1), Quaternion.identity);
        Chessman cm = obj.GetComponent<Chessman>();
        cm.name = name;
        cm.SetXBoard(x);
        cm.SetYBoard(y);
        cm.Activate();
        return obj;
    }

    // Set a piece on the board
    public void SetPosition(GameObject obj)
    {
        Chessman cm = obj.GetComponent<Chessman>();
        positions[cm.GetXBoard(), cm.GetYBoard()] = obj;
    }

    // Mark a board position as empty
    public void SetPositionEmpty(int x, int y)
    {
        positions[x, y] = null;
    }

    // Get a piece at a specific board position
    public GameObject GetPosition(int x, int y)
    {
        return positions[x, y];
    }

    // Check if a position is on the board
    public bool PositionOnBoard(int x, int y)
    {
        return (x >= 0 && y >= 0 && x < positions.GetLength(0) && y < positions.GetLength(1));
    }

    // Get the current player
    public string GetCurrentPlayer()
    {
        return currentPlayer;
    }

    // Check if the game is over
    public bool IsGameOver()
    {
        return gameOver;
    }

    // Switch to the next turn
    public void NextTurn()
    {
        currentPlayer = (currentPlayer == "white") ? "black" : "white";
    }

    // Unity update method for game state
    public void Update()
    {
        if (gameOver && Input.GetMouseButtonDown(0))
        {
            gameOver = false;
            SceneManager.LoadScene("Game"); // Restart the game
        }
    }

    // Handle game over and winner display
    public void Winner(string playerWinner)
    {
        gameOver = true;
        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<Text>().enabled = true;
        GameObject.FindGameObjectWithTag("WinnerText").GetComponent<Text>().text = playerWinner + " WON";
        GameObject.FindGameObjectWithTag("RestartText").GetComponent<Text>().enabled = true;
        GameObject.FindGameObjectWithTag("RestartText").GetComponent<Text>().text = "Checkmate";
    }

    // Record a move in history
    public void RecordMove(string move)
    {
        moveHistory.Add(move);
        Debug.Log($"Move Recorded: {move}");
    }

    // Get the move history
    public List<string> GetMoveHistory()
    {
        return moveHistory;
    }
}
