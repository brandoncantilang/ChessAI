using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;

public class PieceScript : MonoBehaviour
{
    //References
    public GameObject controller;
    public ChessGame game;
    public Piece myPiece;
    public MovePlateScript movePlate;
    public Move[] pseudoLegalMoves;
    public Move[] legalMoves;
    Vector3 offset;
    public string name;
    public float yOffset = 0.7f; // Positive y value to make the piece appear higher
    private bool enPassantVulnerable = false;
    public bool hasMoved = false;

    //Piece refernces
    public Sprite white_king, white_queen, white_rook,
     white_bishop, white_knight, white_pawn;

     public Sprite black_king, black_queen, black_rook,
      black_bishop, black_knight, black_pawn;

    //Board positions
    private int boardX = -1;
    private int boardY = -1;
    private int boardZ = -1; //z is used for layering in 2D
    

    //"white" or "black" player's turn
    private string playerColor;

    public void Activate() {
      //Giving access to controller object
      controller = GameObject.FindGameObjectWithTag("GameController");
      game = controller.GetComponent<ChessGame>();
      //Adjust transform of origin and space between pieces, see method comment
      SetCoordinates(game.GetColorInFront(game.boardFlips));

      //Get sprite reference for each piece name
      switch (myPiece.name) {
        case "white_king": this.GetComponent<SpriteRenderer>().sprite = white_king; playerColor = "white";
        break;
        case "white_queen": this.GetComponent<SpriteRenderer>().sprite = white_queen; playerColor = "white";
        break;
        case "white_rook": this.GetComponent<SpriteRenderer>().sprite = white_rook; playerColor = "white";
        break;
        case "white_bishop": this.GetComponent<SpriteRenderer>().sprite = white_bishop; playerColor = "white";
        break;
        case "white_knight": this.GetComponent<SpriteRenderer>().sprite = white_knight; playerColor = "white";
        break;
        case "white_pawn": this.GetComponent<SpriteRenderer>().sprite = white_pawn; playerColor = "white";
        break;
        case "black_king": this.GetComponent<SpriteRenderer>().sprite = black_king; playerColor = "black";
        break;
        case "black_queen": this.GetComponent<SpriteRenderer>().sprite = black_queen; playerColor = "black";
        break;
        case "black_rook": this.GetComponent<SpriteRenderer>().sprite = black_rook; playerColor = "black";
        break;
        case "black_bishop": this.GetComponent<SpriteRenderer>().sprite = black_bishop; playerColor = "black";
        break;
        case "black_knight": this.GetComponent<SpriteRenderer>().sprite = black_knight; playerColor = "black";
        break;
        case "black_pawn": this.GetComponent<SpriteRenderer>().sprite = black_pawn; playerColor = "black";
        break;
      }
    }

    public void SetPieceMoveArray() {
      switch (this.name){
        case "black_queen":
        case "white_queen":
          pseudoLegalMoves = new Move[27];
          legalMoves = new Move[27];
          break;
        case "black_knight":
        case "white_knight":
          pseudoLegalMoves = new Move[8];
          legalMoves = new Move[8];
          break;
        case "black_bishop":
        case "white_bishop":
          pseudoLegalMoves = new Move[13];
          legalMoves = new Move[13];
          break;
        case "black_king":
        case "white_king":
          pseudoLegalMoves = new Move[8];
          legalMoves = new Move[8];
          break;
        case "black_rook":
        case "white_rook":
          pseudoLegalMoves = new Move[14];
          legalMoves = new Move[14];
          break;
        case "black_pawn":
        case "white_pawn":
          pseudoLegalMoves = new Move[4];
          legalMoves = new Move[4];
          break;
      }
    }

    /*Simplify coords system, translates easy to understand board coordinates like (0,0) or a1 in chess
      to a meaningful place on the board*/
    public void SetCoordinates(string colorInFront) {
      if (myPiece != null) {
      float x = myPiece.GetBoardX();
      float y = myPiece.GetBoardY();
      SetBoardZ(myPiece.GetBoardY());

      if (colorInFront == "black") {
      x = 7-myPiece.GetBoardX();
      y = 7-myPiece.GetBoardY();
      SetBoardZ(7-myPiece.GetBoardY());
      }

      x *= 1.92f; //Numbers to transform board coords to world coords
      y *= 1.92f;
      x += -6.7f;
      y += -5.2f;
      this.transform.position = new Vector3(x, y, boardZ);
      }
    }

    //Getters and setters
    public int GetBoardX(){
      return boardX;
    }
    public int GetBoardY(){
      return boardY;
    }
    public int GetBoardZ(){
      return boardZ;
    }
    public void SetBoardX(int x){
      boardX = x;
    }
    public void SetBoardY(int y){
      boardY = y;
    }
    public void SetBoardZ(int z){
      boardZ = z;
    }

void OnMouseDown()
{
    if (!game.gamePosition.gameOver && !game.promotionInProgress && game.isPlayerTurn())
    {
        offset = transform.position - MouseWorldPosition();
        if (game.gamePosition.GetPlayer() == myPiece.GetPlayerColor())
        {
            myPiece.GeneratePsuedoMoves();  // Generate all potential moves
            myPiece.CreateLegalMoves();      // Filter legal moves
            game.SpawnLegalMoves(myPiece);   // Show legal moves on the board
        }
    }
}

    void OnMouseDrag(){
    Vector3 mousePosition = MouseWorldPosition();
    transform.position = new Vector3(mousePosition.x, mousePosition.y + yOffset, -1);
    }

    void OnMouseUp()
{
    game = controller.GetComponent<ChessGame>();
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

    // Check if the piece was released on a valid move plate
    MovePlateScript movePlate = GetMovePlateAtMousePosition(ray);
    // Destroy move plates after the move attempt
    game.DestroyMovePlates();
    if (movePlate != null)
    {
        Move move = movePlate.myMove;
        // Handle the move
        HandleMoveOnBoardAndLogic(move);
    }
    else{
      game.SetAllCoordinates(game.GetColorInFront(game.boardFlips));
      }
      if (game.gamePosition.GetPlayer() == game.aiPlayer){
        game.gameAI.MakeRandomMove();
      }
}


private MovePlateScript GetMovePlateAtMousePosition(Ray ray)
{
    RaycastHit2D[] raycastHits = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity);
    foreach (RaycastHit2D raycastHit in raycastHits)
    {
        if (raycastHit.collider.name == "MovePlate(Clone)")
        {
            return raycastHit.collider.GetComponent<MovePlateScript>();
        }
    }
    return null;
}

public void HandleMoveOnBoardAndLogic(Move moveMade){
    game.gamePosition.HandleMove(moveMade);
    HandleMove(moveMade);
}

public void MovePiece(int targetX, int targetY)
{
    game.SetPositionEmpty(targetX, targetY); // Clear old position

    SetBoardX(targetX);
    SetBoardY(targetY);
    game.SetPosition(this.gameObject);
    game.SetGamePositionFromObject(this);
}

public void MovePiece(PieceScript piece, int targetX, int targetY)
{
    game.SetPositionEmpty(targetX, targetY); // Clear old position

    piece.SetBoardX(targetX);
    piece.SetBoardY(targetY);
    piece.SetCoordinates(game.GetColorInFront(game.boardFlips));
    game.SetPosition(piece.gameObject);
}

private void HandleMove(Move moveMade)
{
    if (moveMade.isAttack)
    {
        HandleAttack(moveMade);
    }
    if (moveMade.isCastling){
        if (moveMade.GetBoardX() == 6){
          // Kingside castling: Move rook from 7 to 5
            MovePiece(game.GetPosition(7, moveMade.GetBoardY()).GetComponent<PieceScript>(), 5, moveMade.GetBoardY());
        }
        if (moveMade.GetBoardX() == 2){
          // Kingside castling: Move rook from 0 to 3
            MovePiece(game.GetPosition(0, moveMade.GetBoardY()).GetComponent<PieceScript>(), 3, moveMade.GetBoardY());
        }
    }
    if ((myPiece.name == "white_pawn" && moveMade.GetBoardY() == 7) || (myPiece.name == "black_pawn" && moveMade.GetBoardY() == 0)) {
      UnityEngine.Debug.Log("Pawn reached back rank");
      HandlePromotion(moveMade);
      MovePiece(moveMade.GetBoardX(), moveMade.GetBoardY());
    }
    else {
    MovePiece(moveMade.GetBoardX(), moveMade.GetBoardY());
    }
    game.CheckDrawOrCheckMate();
    if (game.gamePosition.gameOver) {
        game.SetAllCoordinates(game.GetColorInFront(game.boardFlips));
        game.SetWinner(game.gamePosition.winner);      
    }
    else {
         game.SetAllCoordinates(game.GetColorInFront(game.boardFlips));
    }
    
}

private void HandleAttack(Move moveMade)
{
    int targetX = moveMade.GetBoardX();
    int targetY = moveMade.GetBoardY();
    PieceScript attackedPieceScript = null;

    if (moveMade.isEnPassant && this.playerColor == "white"){
      attackedPieceScript = game.GetPosition(targetX, targetY-1).gameObject.GetComponent<PieceScript>();
    }
    else if (moveMade.isEnPassant && this.playerColor == "black"){
      attackedPieceScript = game.GetPosition(targetX, targetY+1).gameObject.GetComponent<PieceScript>();
    }
    else
    {
      attackedPieceScript = game.GetPosition(targetX, targetY).gameObject.GetComponent<PieceScript>();
    }

    if (attackedPieceScript != null)
    {
        // Remove the internal piece reference
        attackedPieceScript.myPiece = null;
        game.gamePosition.SetPlayerArrayEmpty(attackedPieceScript.myPiece);
        game.gamePosition.SetPositionEmpty(targetX, targetY);
        // Destroy the visual representation
        Destroy(attackedPieceScript.gameObject);
    }

}

public void HandlePromotion(Move moveMade)
{
    // Check if the move involves a pawn reaching the back rank for promotion
    if ((myPiece.GetPlayerColor() == "white" && moveMade.GetBoardY() == 7) || (myPiece.GetPlayerColor() == "black" && moveMade.GetBoardY() == 0))
    {
        game.promotionInProgress = true;  // Pause the game

        if (myPiece.GetPlayerColor() == "white")
        {
            game.EnableWhitePromotion();  // Enable white promotion UI
        }
        else if (myPiece.GetPlayerColor() == "black")
        {
            game.EnableBlackPromotion();  // Enable black promotion UI
        }
      StartCoroutine(WaitForPromotionThenMove(moveMade));
        // Wait for promotion, then move the piece
    }
}
private IEnumerator WaitForPromotionThenMove(Move moveMade)
{
    // Wait until the promotion is completed (i.e., promotionInProgress becomes false)
    yield return new WaitUntil(() => game.promotionInProgress == false);
    game.CompletePromotion(moveMade.myPiece);
}

    Vector3 MouseWorldPosition(){
        var mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(mouseScreenPos);
    }
}