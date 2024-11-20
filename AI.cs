using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour
{
    public GameObject controller;
    public ChessGame game;
    string color;
    private System.Random random;
    private ulong[,] zobristTable; // Zobrist hash table
    private ulong currentHash;     // Current hash for the position
    
    // Transposition table to track unique positions
    private HashSet<ulong> transpositionTable;

    public void Activate(){
        controller = GameObject.FindGameObjectWithTag("GameController");
        game = controller.GetComponent<ChessGame>();
        color = game.aiPlayer;
    }
    public int Evaluate(Position positionToEvaluate)
    {
        int score = 0;

        // Add the total value of pieces for white (positive score for white pieces)
        foreach (Piece piece in positionToEvaluate.GetPlayerWhite())
        {
            if (piece != null)
            {
                score += piece.value;
            }
        }

        // Subtract the total value of pieces for black (negative score for black pieces)
        foreach (Piece piece in positionToEvaluate.GetPlayerBlack())
        {
            if (piece != null)
            {
                score += piece.value;
            }
        }

        // Return the evaluation score (positive means white is better, negative means black is better)
        return score;
    }

    public List<Move> GetAllPossibleMoves(Position position, string playerColor)
    {
        List<Move> allMoves = new List<Move>(); // List to store all legal moves

        // Get the correct player's pieces based on the color
        Piece[] playerPieces = (playerColor == "white") ? position.GetPlayerWhite() : position.GetPlayerBlack();

        // Iterate through all the pieces of the player
        foreach (Piece piece in playerPieces)
        {
            if (piece != null) // Ensure the piece is not null
            {
                // Generate legal moves for the current piece
                piece.GeneratePsuedoMoves();
                piece.CreateLegalMoves();
                Move[] legalMoves = piece.GetLegalMoves();
                
                // Add each legal move to the allMoves list
                foreach (Move move in legalMoves)
                {
                    if (move != null) // Ensure the move is not null
                    {
                        allMoves.Add(move);
                    }
                }
            }
        }

        return allMoves; // Return the list of all legal moves
    }

    // Function to make a random move from the list of possible moves
    public void MakeRandomMove()
    {
        random = new System.Random();
        // Get all possible moves for the current player
        List<Move> allMoves = GetAllPossibleMoves(game.gamePosition, color);

        // If there are no legal moves, return null (e.g., checkmate or stalemate)
        if (allMoves.Count == 0)
        {
            return;
        }

        // Pick a random move from the list
        int randomIndex = random.Next(allMoves.Count);
        Move selectedMove = allMoves[randomIndex];

        PieceScript pscript = game.GetPosition(selectedMove.myPiece.GetBoardX(), selectedMove.myPiece.GetBoardY()).GetComponent<PieceScript>();
        pscript.HandleMoveOnBoardAndLogic(selectedMove);
    }
    
}
