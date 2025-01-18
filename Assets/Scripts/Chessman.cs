using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chessman : MonoBehaviour
{
    //References to objects in our Unity Scene
    public GameObject controller;
    public GameObject movePlate;

    //Position for this Chesspiece on the Board
    //The correct position will be set later
    private int xBoard = -1;
    private int yBoard = -1;

    //Variable for keeping track of the player it belongs to "black" or "white"
    public string player; // Make this public so it can be accessed by other scripts

    //References to all the possible Sprites that this Chesspiece could be
    public Sprite black_queen, black_knight, black_bishop, black_king, black_rook, black_pawn;
    public Sprite white_queen, white_knight, white_bishop, white_king, white_rook, white_pawn;

    public bool hasMoved; // Add this variable to keep track of whether the king or rook has moved 

    public void Activate()
    {
        //Get the game controller
        controller = GameObject.FindGameObjectWithTag("GameController");

        //Take the instantiated location and adjust transform
        SetCoords();

        //Choose correct sprite based on piece's name
        switch (this.name)
        {
            case "black_queen": this.GetComponent<SpriteRenderer>().sprite = black_queen; player = "black"; break;
            case "black_knight": this.GetComponent<SpriteRenderer>().sprite = black_knight; player = "black"; break;
            case "black_bishop": this.GetComponent<SpriteRenderer>().sprite = black_bishop; player = "black"; break;
            case "black_king": this.GetComponent<SpriteRenderer>().sprite = black_king; player = "black"; break;
            case "black_rook": this.GetComponent<SpriteRenderer>().sprite = black_rook; player = "black"; break;
            case "black_pawn": this.GetComponent<SpriteRenderer>().sprite = black_pawn; player = "black"; break;
            case "white_queen": this.GetComponent<SpriteRenderer>().sprite = white_queen; player = "white"; break;
            case "white_knight": this.GetComponent<SpriteRenderer>().sprite = white_knight; player = "white"; break;
            case "white_bishop": this.GetComponent<SpriteRenderer>().sprite = white_bishop; player = "white"; break;
            case "white_king": this.GetComponent<SpriteRenderer>().sprite = white_king; player = "white"; break;
            case "white_rook": this.GetComponent<SpriteRenderer>().sprite = white_rook; player = "white"; break;
            case "white_pawn": this.GetComponent<SpriteRenderer>().sprite = white_pawn; player = "white"; break;
        }
    }

    public void SetCoords()
    {
        //Get the board value in order to convert to xy coords
        float x = xBoard;
        float y = yBoard;

        //Adjust by variable offset
        x *= 0.66f;
        y *= 0.66f;

        //Add constants (pos 0,0)
        x += -2.3f;
        y += -2.3f;

        //Set actual unity values
        this.transform.position = new Vector3(x, y, -1.0f);
    }

    public int GetXBoard()
    {
        return xBoard;
    }

    public int GetYBoard()
    {
        return yBoard;
    }

    public void SetXBoard(int x)
    {
        xBoard = x;
    }

    public void SetYBoard(int y)
    {
        yBoard = y;
    }

   private void OnMouseUp()
{
    if (!controller.GetComponent<Game>().IsGameOver() && controller.GetComponent<Game>().GetCurrentPlayer() == player)
    {
        // Allow interaction only if it's the player's turn
        DestroyMovePlates();
        InitiateMovePlates();

        CheckPawnPromotion();
    }
}

    public void DestroyMovePlates()
    {
        GameObject[] movePlates = GameObject.FindGameObjectsWithTag("MovePlate");
        for (int i = 0; i < movePlates.Length; i++)
        {
            if (movePlates[i] != null)
            {
                Destroy(movePlates[i]); // Ensure each MovePlate is properly destroyed
            }
        }
    }

    public void InitiateMovePlates()
    {
        switch (this.name)
        {
            case "black_queen":
            case "white_queen":
                LineMovePlate(1, 0);
                LineMovePlate(0, 1);
                LineMovePlate(1, 1);
                LineMovePlate(-1, 0);
                LineMovePlate(0, -1);
                LineMovePlate(-1, -1);
                LineMovePlate(-1, 1);
                LineMovePlate(1, -1);
                break;
            case "black_knight":
            case "white_knight":
                LMovePlate();
                break;
            case "black_bishop":
            case "white_bishop":
                LineMovePlate(1, 1);
                LineMovePlate(1, -1);
                LineMovePlate(-1, 1);
                LineMovePlate(-1, -1);
                break;
            case "black_king":
            case "white_king":
                SurroundMovePlate();
                CheckCastlingOptions();
                break;
            case "black_rook":
            case "white_rook":
                LineMovePlate(1, 0);
                LineMovePlate(0, 1);
                LineMovePlate(-1, 0);
                LineMovePlate(0, -1);
                break;
            case "black_pawn":
                PawnMovePlate(xBoard, yBoard - 1);
                break;
            case "white_pawn":
                PawnMovePlate(xBoard, yBoard + 1);
                break;
        }
    }

    public void LineMovePlate(int xIncrement, int yIncrement)
    {
        Game sc = controller.GetComponent<Game>();

        int x = xBoard + xIncrement;
        int y = yBoard + yIncrement;

        while (sc.PositionOnBoard(x, y) && sc.GetPosition(x, y) == null)
        {
            MovePlateSpawn(x, y);
            x += xIncrement;
            y += yIncrement;
        }

        if (sc.PositionOnBoard(x, y) && sc.GetPosition(x, y).GetComponent<Chessman>().player != player)
        {
            MovePlateAttackSpawn(x, y);
        }
    }

    public void LMovePlate()
    {
        PointMovePlate(xBoard + 1, yBoard + 2);
        PointMovePlate(xBoard - 1, yBoard + 2);
        PointMovePlate(xBoard + 2, yBoard + 1);
        PointMovePlate(xBoard + 2, yBoard - 1);
        PointMovePlate(xBoard + 1, yBoard - 2);
        PointMovePlate(xBoard - 1, yBoard - 2);
        PointMovePlate(xBoard - 2, yBoard + 1);
        PointMovePlate(xBoard - 2, yBoard - 1);
    }

    public void SurroundMovePlate()
    {
        PointMovePlate(xBoard, yBoard + 1);
        PointMovePlate(xBoard, yBoard - 1);
        PointMovePlate(xBoard - 1, yBoard + 0);
        PointMovePlate(xBoard - 1, yBoard - 1);
        PointMovePlate(xBoard - 1, yBoard + 1);
        PointMovePlate(xBoard + 1, yBoard + 0);
        PointMovePlate(xBoard + 1, yBoard - 1);
        PointMovePlate(xBoard + 1, yBoard + 1);
    }

    public void PointMovePlate(int x, int y)
    {
        Game sc = controller.GetComponent<Game>();
        if (sc.PositionOnBoard(x, y))
        {
            GameObject cp = sc.GetPosition(x, y);

            if (cp == null)
            {
                MovePlateSpawn(x, y);
            }
            else if (cp.GetComponent<Chessman>().player != player)
            {
                MovePlateAttackSpawn(x, y);
            }
        }
    }

    public void PawnMovePlate(int x, int y)
{
    Game sc = controller.GetComponent<Game>();

    // Check if the pawn is within the board
    if (sc.PositionOnBoard(x, y))
    {
        // Normal 1-tile forward move
        if (sc.GetPosition(x, y) == null)
        {
            MovePlateSpawn(x, y);
        }

        // 2-tile forward move if the pawn is at its starting position
        if (player == "white" && yBoard == 1 && sc.GetPosition(x, y + 1) == null)
        {
            MovePlateSpawn(x, y + 1);
        }
        else if (player == "black" && yBoard == 6 && sc.GetPosition(x, y - 1) == null)
        {
            MovePlateSpawn(x, y - 1);
        }

        // Check for diagonal attacks
        if (sc.PositionOnBoard(x + 1, y) && sc.GetPosition(x + 1, y) != null && sc.GetPosition(x + 1, y).GetComponent<Chessman>().player != player)
        {
            MovePlateAttackSpawn(x + 1, y);
        }

        if (sc.PositionOnBoard(x - 1, y) && sc.GetPosition(x - 1, y) != null && sc.GetPosition(x - 1, y).GetComponent<Chessman>().player != player)
        {
            MovePlateAttackSpawn(x - 1, y);
        }
    }
}

    public void MovePlateSpawn(int matrixX, int matrixY)
    {
        GameObject mp = Instantiate(movePlate, new Vector3(matrixX * 0.66f - 2.3f, matrixY * 0.66f - 2.3f, -3.0f), Quaternion.identity);

        MovePlate mpScript = mp.GetComponent<MovePlate>();
        mpScript.SetReference(gameObject);
        mpScript.SetCoords(matrixX, matrixY);
    }

    public void MovePlateAttackSpawn(int matrixX, int matrixY)
    {
        //Get the board value in order to convert to xy coords
        float x = matrixX;
        float y = matrixY;

        //Adjust by variable offset
        x *= 0.66f;
        y *= 0.66f;

        //Add constants (pos 0,0)
        x += -2.3f;
        y += -2.3f;

        //Set actual unity values
        GameObject mp = Instantiate(movePlate, new Vector3(x, y, -3.0f), Quaternion.identity);

        MovePlate mpScript = mp.GetComponent<MovePlate>();
        mpScript.attack = true;
        mpScript.SetReference(gameObject);
        mpScript.SetCoords(matrixX, matrixY);
    }
public void CheckPawnPromotion()
{
    // Ensure this check only applies to pawns
    if (this.name.Contains("pawn"))
    {
        // Check if the pawn reached the opposite end of the board
        if ((player == "white" && yBoard == 7) || (player == "black" && yBoard == 0))
        {
            PromotePawn();
        }
    }
}

public void PromotePawn()
{
    // Set the new piece type and sprite for promotion to a queen
    string newPieceName = player == "white" ? "white_queen" : "black_queen";
    Sprite newSprite = player == "white" ? white_queen : black_queen;

    // Update the name and sprite to reflect the promoted piece
    this.name = newPieceName;
    SpriteRenderer spriteRenderer = this.GetComponent<SpriteRenderer>();
    spriteRenderer.sprite = newSprite;

    // Update the board position in the Game script to reflect the promoted piece
    Game gameController = controller.GetComponent<Game>();
    gameController.SetPosition(this.gameObject);  // Update the board array in Game with the new piece

    // Optional: Reset the pawn's `hasMoved` status, as it's now a new piece
    hasMoved = false;
}
public void CheckCastlingOptions()
{
    if (this.name.Contains("king") && !hasMoved)
    {
        // Kingside castling
        if (IsKingsideCastlingPossible())
        {
            MovePlateSpawn(xBoard + 2, yBoard, true); // Pass `true` to indicate castling
        }

        // Queenside castling
        if (IsQueensideCastlingPossible())
        {
            MovePlateSpawn(xBoard - 2, yBoard, true); // Pass `true` to indicate castling
        }
    }
}

// Update `MovePlateSpawn` to accept a `castling` flag
public void MovePlateSpawn(int matrixX, int matrixY, bool isCastling = false)
{
    GameObject mp = Instantiate(movePlate, new Vector3(matrixX * 0.66f - 2.3f, matrixY * 0.66f - 2.3f, -3.0f), Quaternion.identity);
    MovePlate mpScript = mp.GetComponent<MovePlate>();
    mpScript.SetReference(gameObject);
    mpScript.SetCoords(matrixX, matrixY);
    mpScript.castling = isCastling; // Set castling flag
}

private bool IsKingsideCastlingPossible()
{
    Game sc = controller.GetComponent<Game>();

    // Ensure the rook and all squares between the king and rook are within board bounds
    if (!sc.PositionOnBoard(xBoard + 1, yBoard) || !sc.PositionOnBoard(xBoard + 2, yBoard) || !sc.PositionOnBoard(xBoard + 3, yBoard))
        return false;

    // Ensure squares between king and rook are empty and the rook hasn't moved
    return sc.GetPosition(xBoard + 1, yBoard) == null
           && sc.GetPosition(xBoard + 2, yBoard) == null
           && sc.GetPosition(xBoard + 3, yBoard) != null
           && sc.GetPosition(xBoard + 3, yBoard).name.Contains("rook")
           && !sc.GetPosition(xBoard + 3, yBoard).GetComponent<Chessman>().hasMoved;
}

private bool IsQueensideCastlingPossible()
{
    Game sc = controller.GetComponent<Game>();

    // Ensure the rook and all squares between the king and rook are within board bounds
    if (!sc.PositionOnBoard(xBoard - 1, yBoard) || !sc.PositionOnBoard(xBoard - 2, yBoard) || !sc.PositionOnBoard(xBoard - 3, yBoard) || !sc.PositionOnBoard(xBoard - 4, yBoard))
        return false;

    // Ensure squares between king and rook are empty and the rook hasn't moved
    return sc.GetPosition(xBoard - 1, yBoard) == null
           && sc.GetPosition(xBoard - 2, yBoard) == null
           && sc.GetPosition(xBoard - 3, yBoard) == null
           && sc.GetPosition(xBoard - 4, yBoard) != null
           && sc.GetPosition(xBoard - 4, yBoard).name.Contains("rook")
           && !sc.GetPosition(xBoard - 4, yBoard).GetComponent<Chessman>().hasMoved;
}

// Call this method in your king's InitiateMovePlates method if you want to add castling options for the king
}
