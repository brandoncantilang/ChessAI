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
    public bool wasMovedBefore = false;
    public bool isEnPassant = false;
    public bool isEnPassantVulnerable = false;
    public bool isCastling = false;
    public bool isPromoting = false;
    public string promoteTo;
    public Piece LastEnPassantPiece;
    public Move previousMove;
    public int score;

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

    public override string ToString()
{
    string startSquare = ConvertToChessNotation(startingX, startingY);
    string endSquare = ConvertToChessNotation(boardX, boardY);

    string pieceName = myPiece != null ? myPiece.name : "Unknown Piece";
    string moveDescription = $"{pieceName} {startSquare} -> ";

    if (isPromoting)
    {
        moveDescription += $"{promoteTo} {endSquare} (promotion)";
    }
    else if (isCastling)
    {
        moveDescription = "Castling";
    }
    else if (isEnPassant)
    {
        moveDescription += $"{pieceName} {endSquare} (en passant)";
    }
    else if (isAttack && attackedPiece != null)
    {
        string attackedPieceName = attackedPiece.name;
        moveDescription += $"{attackedPieceName} {endSquare} (capture)";
    }
    else
    {
        moveDescription += $"{pieceName} {endSquare}";
    }

    return moveDescription;
}


    private string ConvertToChessNotation(int x, int y)
    {
        char file = (char)('a' + x);
        int rank = y + 1;
        return $"{file}{rank}";
    }
}
