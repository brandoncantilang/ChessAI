using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Move{
    public Piece attackedPiece;
    //References 
    int boardX;
    int boardY;
    public bool isLegal = false;
    public bool isAttack = false;
    public Piece myPiece;
    public int startingX;
    public int startingY;
    public bool isEnPassant = false;
    public bool isEnPassantVulnerable = false;
    public bool isCastling = false;
    public bool isPromoting = false;

    public void SetPosition(int x, int y){
        boardX = x;
        boardY = y;
    }
    public int GetBoardX(){
        return boardX;
    }
    public int GetBoardY(){
        return boardY;
    }
    public void SetIsLegal(bool legalBool){
        isLegal = legalBool;
    }
    public void SetAttackedPiece(Piece pieceName){
        attackedPiece = pieceName;
    }
    public void PrintMove(){
        UnityEngine.Debug.Log("(" + GetBoardX() + ", " + GetBoardY() + " ) ");
    }
}
