using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;

public class Piece
{
    //References
    public Position game;
    public Move[] pseudoLegalMoves;
    public Move[] legalMoves;
    public string name;
    public bool enPassantVulnerable = false;
    public bool hasMoved = false;
    public int value;

    //Board positions
    private int boardX = -1;
    private int boardY = -1;
    
    private string playerColor;

    // Assign player color based on the piece's name
    public void AssignPlayerColor()
    {
        if (name.StartsWith("white"))
            playerColor = "white";
        else
            playerColor = "black";
    }

    public void AssignValue(){
    switch (name)
        {
            case "black_queen":
                value = -900;
                break;
            case "white_queen":
                value = 900;
                break;
            case "black_knight":
                value = -300;
                break;
            case "white_knight":
                value = 300; 
                break;
            case "black_bishop":
                value = -320;
                break;
            case "white_bishop":
                value = 320;
                break;
            case "black_rook":
                value = -500;
                break;
            case "white_rook":
                value = 500;
                break;
            case "black_pawn":
                value = -100;
                break;
            case "white_pawn":
                value = 100;
                break;
            case "black_king":
                value = -20000;
                break;
            case "white_king":
                value = 20000;
                break;
        }
    }
    public void SetPieceMoveArray()
    {
        switch (name)
        {
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

    // Getters and Setters for board coordinates
    public int GetBoardX() => boardX;
    public int GetBoardY() => boardY;
    public void SetBoardX(int x) => boardX = x;
    public void SetBoardY(int y) => boardY = y;

    public string GetPlayerColor() => playerColor;

    public bool IsEnPassantVulnerable() => enPassantVulnerable;
    public void SetEnPassantVulnerable(bool value) => enPassantVulnerable = value;

    public void ClearPsuedoMoves(){
      for (int i = 0; i < pseudoLegalMoves.Length; i++){
        if (pseudoLegalMoves[i] != null){
          pseudoLegalMoves[i] = null; 
        }
      }
    }
    public void ClearLegalMoves(){
      for (int i = 0; i < legalMoves.Length; i++){
        if (legalMoves[i] != null){
          legalMoves[i] = null; 
        }
      }
    }

    public void GeneratePsuedoMoves(){
      ClearPsuedoMoves();
      switch (this.name){
        case "black_queen":
        case "white_queen":
          PossibleQueenMove();
          break;
        case "black_knight":
        case "white_knight":
          PossibleKnightMove();
          break;
        case "black_bishop":
        case "white_bishop":
          PossibleBishopMove();
          break;
        case "black_king":
        case "white_king":
          PossibleKingMove();
          break;
        case "black_rook":
        case "white_rook":
          PossibleRookMove();
          break;
        case "black_pawn":
          BlackPawnMove(boardX, boardY-1);
          break;
        case "white_pawn":
          WhitePawnMove(boardX, boardY + 1);
          break;
      }
    }

    public Move CreateMove(int x, int y) {
        Move move = new Move();
        move.SetPosition(x,y);
        move.myPiece = this;
        move.startingX = move.myPiece.GetBoardX();
        move.startingY = move.myPiece.GetBoardY();
        move.wasMovedBefore = move.myPiece.hasMoved;
        if (game.GetPosition(x,y)!=null){
          move.isAttack = true;
          move.SetAttackedPiece(game.GetPosition(x,y));
        }
        if (game.enPassantVulnerablePiece != null){
          move.LastEnPassantPiece = game.enPassantVulnerablePiece;
        }
        return move;
    }
    public Move CreateEnPassantMove(int x, int y) {
        Move move = new Move();
        move.SetPosition(x,y);
        move.myPiece = this;
        move.isAttack = true;
        move.isEnPassant = true;
        move.startingX = move.myPiece.GetBoardX();
        move.startingY = move.myPiece.GetBoardY();
        move.wasMovedBefore = move.myPiece.hasMoved;
        return move;
    }
    public Move CreatePromotingMove(int x, int y, string pieceName) {
        Move move = new Move();
        move.promoteTo = pieceName;
        move.SetPosition(x,y);
        move.myPiece = this;
        move.startingX = move.myPiece.GetBoardX();
        move.startingY = move.myPiece.GetBoardY();
        move.wasMovedBefore = move.myPiece.hasMoved;
        if (game.GetPosition(x,y)!=null){
          move.isAttack = true;
          move.SetAttackedPiece(game.GetPosition(x,y));
        }
        UnityEngine.Debug.Log(move.promoteTo);
        return move;
    }
    public void AddToPsuedoMoveArray(Move move){
      for (int i = 0; i < pseudoLegalMoves.Length; i++){
        if (pseudoLegalMoves[i] == null){ // Check for null to add to end of pseudo move array
          pseudoLegalMoves[i] = move; 
          return;
        }
      }
    }
    public void AddToLegalMoveArray(Move move){
      for (int i = 0; i < legalMoves.Length; i++){
        if (legalMoves[i] == null){ // Check for null to add to end of pseudo move array
          legalMoves[i] = move;
          return;
        }
    }
    }
    public void CreateLegalMoves(){
      ClearLegalMoves();
      foreach (Move move in pseudoLegalMoves){
        if (move!=null){
        if (game.isLegal(move)){
          AddToLegalMoveArray(move);
          }
        }
      }
      }
    public void LineMove(int xIncrement, int yIncrement){
      int x = boardX + xIncrement;
      int y = boardY + yIncrement;

      while (game.PositionOnBoard(x, y) && game.GetPosition(x,y) == null){
        Move move = CreateMove(x,y);
        if (game.PositionOnBoard(x,y)){
        game.isSquareAttacked[x,y] = true;
        }
        AddToPsuedoMoveArray(move);
        x += xIncrement;
        y += yIncrement;
      }
      if (game.PositionOnBoard(x, y) && game.GetPosition(x,y).GetPlayerColor() != playerColor){
        Move move = CreateMove(x,y);
        if (game.PositionOnBoard(x,y)){
        game.isSquareAttacked[x,y] = true;
        }
        AddToPsuedoMoveArray(move);
      }
    }
    public void PossibleQueenMove(){
      LineMove(0, 1);
      LineMove(1, 0);
      LineMove(0, -1);
      LineMove(-1, 0);
      LineMove(1, 1);
      LineMove(-1, -1);
      LineMove(-1, 1);
      LineMove(1, -1);
    }
    public void PossibleRookMove(){
      LineMove(0, 1);
      LineMove(1, 0);
      LineMove(0, -1);
      LineMove(-1, 0);
    }
    public void PossibleBishopMove(){
      LineMove(1, 1);
      LineMove(-1, -1);
      LineMove(-1, 1);
      LineMove(1, -1);
    }
    public void PossibleKnightMove(){
      PointMove(boardX + 1, boardY + 2);
      PointMove(boardX - 1, boardY + 2);
      PointMove(boardX + 2, boardY + 1);
      PointMove(boardX + 2, boardY - 1);
      PointMove(boardX + 1, boardY - 2);
      PointMove(boardX - 1, boardY - 2);
      PointMove(boardX - 2, boardY + 1);
      PointMove(boardX - 2, boardY - 1);
    }
    public void PossibleKingMove(){
      PointMove(boardX, boardY + 1);
      PointMove(boardX, boardY - 1);
      PointMove(boardX + 1, boardY - 1);
      PointMove(boardX + 1, boardY);
      PointMove(boardX + 1, boardY + 1);
      PointMove(boardX - 1, boardY - 1);
      PointMove(boardX - 1, boardY);
      PointMove(boardX - 1, boardY + 1);
      if (!hasMoved) {
        // Check for kingside castling
        if (CanCastleKingside()) {
            GenerateCastlingMove("kingside");
        }

        // Check for queenside castling
        if (CanCastleQueenside()) {
            GenerateCastlingMove("queenside");
        }
      }
    }

    public void PointMove(int x, int y){
      if (game.PositionOnBoard(x,y)){
        if (game.GetPosition(x,y) == null) {
          Move move = CreateMove(x,y);
          if (game.PositionOnBoard(x,y)){
        game.isSquareAttacked[x,y] = true;
        }
          AddToPsuedoMoveArray(move);
        }
        else if (game.GetPosition(x,y).GetPlayerColor() != this.playerColor) {
          Move move = CreateMove(x,y);
          if (game.PositionOnBoard(x,y)){
        game.isSquareAttacked[x,y] = true;
        }
          AddToPsuedoMoveArray(move);
        }
    }
    }

    private void GenerateCastlingMove(string side) { //We know the king has not moved
    int boardY = (GetPlayerColor() == "white") ? 0 : 7;  // Determine row based on the color

    if (side == "kingside") {
        // Kingside castling: King moves to column 6
        Move castlingMove = CreateMove(6, boardY);
        castlingMove.isCastling = true;
        AddToPsuedoMoveArray(castlingMove);
    } else if (side == "queenside") {
        // Queenside castling: King moves to column 2
        Move castlingMove = CreateMove(2, boardY);
        castlingMove.isCastling = true;
        AddToPsuedoMoveArray(castlingMove);
    }
}


  public bool CanCastleKingside() {
    int boardY = (GetPlayerColor() == "white") ? 0 : 7;  // Row depends on whether it's white or black

    // Check if the squares between the king and kingside rook are empty
    if (game.GetPosition(5, boardY) == null && game.GetPosition(6, boardY) == null) {
        Piece rookPiece = game.GetPosition(7, boardY);  // Kingside rook position
        if (rookPiece != null && !rookPiece.hasMoved) {
            // The rook hasn't moved, and the squares are empty, so kingside castling is possible
            return true;
        }
    }
    return false;
}

public bool CanCastleQueenside() {
    int boardY = (GetPlayerColor() == "white") ? 0 : 7;  // Row depends on whether it's white or black

    // Check if the squares between the king and queenside rook are empty
    if (game.GetPosition(1, boardY) == null && game.GetPosition(2, boardY) == null && game.GetPosition(3, boardY) == null) {
        Piece rookPiece = game.GetPosition(0, boardY);  // Queenside rook position
        if (rookPiece != null && !rookPiece.hasMoved) {
            // The rook hasn't moved, and the squares are empty, so queenside castling is possible
            return true;
        }
    }
    return false;
}

public bool CanCastleKingsideFEN() {
    int boardY = (GetPlayerColor() == "white") ? 0 : 7; 

    // Check if the squares between the king and kingside rook are empty
    if (!hasMoved) {
        Piece rookPiece = game.GetPosition(7, boardY);  // Kingside rook position
        if (rookPiece != null && !rookPiece.hasMoved) {
            // The rook hasn't moved, and the squares are empty, so kingside castling is possible
            return true;
        }
    }
    return false;
}

public bool CanCastleQueensideFEN() {
    int boardY = (GetPlayerColor() == "white") ? 0 : 7;
    if (!hasMoved) { 
        Piece rookPiece = game.GetPosition(0, boardY);  // Queenside rook position
        if (rookPiece != null && !rookPiece.hasMoved) {
            // The rook hasn't moved, and the squares are empty, so queenside castling is possible
            return true;
        }
    }
    return false;
}


    public void WhitePawnMove(int x, int y){
      Move move = null;
      if (game.PositionOnBoard(x,y)){

        if (game.GetPosition(x,y) == null){
          move = CreateMove(x,y);
            AddToPsuedoMoveArray(move);
        }

      }

      if (boardY == 1){

        if (game.GetPosition(x,y+1) == null && game.GetPosition(x, y)==null){
          move = CreateMove(x,y+1);
          move.isEnPassantVulnerable = true;
            AddToPsuedoMoveArray(move);
        }

      }
        if (game.PositionOnBoard(x+1,y)){
          game.isSquareAttacked[x+1,y] = true;
        }
          if (game.PositionOnBoard(x+1,y) && game.GetPosition(x+1, y)!=null && 
          game.GetPosition(x+1,y).GetPlayerColor() != playerColor){
            move = CreateMove(x+1,y);
            AddToPsuedoMoveArray(move);
      }
      if (game.PositionOnBoard(x-1,y)){
        game.isSquareAttacked[x-1,y] = true;
        }
         if (game.PositionOnBoard(x-1,y) && game.GetPosition(x-1, y)!=null && 
          game.GetPosition(x-1,y).GetPlayerColor() != playerColor){
            move = CreateMove(x-1,y);
            AddToPsuedoMoveArray(move);
      }
        // En passant capture to the left
          if (boardY == 4 && game.PositionOnBoard(boardX - 1, boardY) && game.GetPosition(boardX - 1, boardY) != null && 
          game.GetPosition(boardX - 1, boardY).IsEnPassantVulnerable()) {
          Move enPassantMove = CreateEnPassantMove(boardX - 1, boardY + 1);
          enPassantMove.attackedPiece = game.GetPosition(boardX - 1, boardY); // The pawn to be captured
          AddToPsuedoMoveArray(enPassantMove);
      }

        // En passant capture to the right
          if (boardY == 4 && game.PositionOnBoard(boardX + 1, boardY) && game.GetPosition(boardX + 1, boardY) != null && 
          game.GetPosition(boardX + 1, boardY).IsEnPassantVulnerable()) {
          Move enPassantMove = CreateEnPassantMove(boardX + 1, boardY + 1);
          enPassantMove.attackedPiece = game.GetPosition(boardX + 1, boardY); // The pawn to be captured
          AddToPsuedoMoveArray(enPassantMove);
      }
      if (move != null){
        if (move.GetBoardY() == 7){ 
          move.isPromoting = true;
      }
      }
    }

    public void BlackPawnMove(int x, int y){
    Move move = null;
    if (game.PositionOnBoard(x,y)){

        if (game.GetPosition(x,y) == null){
            move = CreateMove(x,y);
            AddToPsuedoMoveArray(move);
        }
    }

        if (boardY == 6){ //2 Square move at the beginning pawn rank
        if (game.GetPosition(x,y-1) == null && game.GetPosition(x, y)==null){
            move = CreateMove(x,y-1);
            move.isEnPassantVulnerable = true;
            AddToPsuedoMoveArray(move);
        }
      }
        if (game.PositionOnBoard(x-1,y)){
          game.isSquareAttacked[x-1,y] = true;
        }
          if (game.PositionOnBoard(x-1,y) && game.GetPosition(x-1, y)!=null && 
          game.GetPosition(x-1,y).GetPlayerColor() != playerColor){
            move = CreateMove(x-1,y);
            AddToPsuedoMoveArray(move);
          }
        if (game.PositionOnBoard(x+1,y)){
          game.isSquareAttacked[x+1,y] = true;
        }
          if (game.PositionOnBoard(x+1,y) && game.GetPosition(x+1, y)!=null && 
          game.GetPosition(x+1,y).GetPlayerColor() != playerColor){
            move = CreateMove(x+1,y);
            AddToPsuedoMoveArray(move);
      }
        // En passant capture to the left
          if (boardY == 3 && game.PositionOnBoard(boardX - 1, boardY) && game.GetPosition(boardX - 1, boardY) != null && 
          game.GetPosition(boardX - 1, boardY).IsEnPassantVulnerable()) {
          Move enPassantMove = CreateEnPassantMove(boardX - 1, boardY - 1);
          enPassantMove.attackedPiece = game.GetPosition(boardX - 1, boardY); // The pawn to be captured
          AddToPsuedoMoveArray(enPassantMove);
      }

        // En passant capture to the right
          if (boardY == 3 && game.PositionOnBoard(boardX + 1, boardY) && game.GetPosition(boardX + 1, boardY) != null && 
          game.GetPosition(boardX + 1, boardY).IsEnPassantVulnerable()) {
          Move enPassantMove = CreateEnPassantMove(boardX + 1, boardY - 1);
          enPassantMove.attackedPiece = game.GetPosition(boardX + 1, boardY); // The pawn to be captured
          AddToPsuedoMoveArray(enPassantMove);
      }
      if (move != null){
        if (move.GetBoardY() == 7){ 
          move.isPromoting = true;
      }
    }
    }

  


  public Move[] GetPseudoLegalMoves(){
    return pseudoLegalMoves;
  }

  public Move[] GetLegalMoves(){
    return legalMoves;
  }

  public string GetName(){
    return this.name;
  }  

  public void PrintLegalMoves(){
        for (int i = 0; i<legalMoves.Length; i++){
          //UnityEngine.Debug.Log(pseudoLegalMoves[i]);
          if (legalMoves[i]!=null){
             legalMoves[i].PrintMove();
        }
        }
  }
}