using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AI : MonoBehaviour
{
    public GameObject controller;
    public ChessGame game;
    string color;
    private System.Random random;
    private ulong[,] zobristTable; // Zobrist hash table
    private ulong currentHash;     // Current hash for the position
    public ValueMaps valueMaps = new ValueMaps();
    
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

    // Evaluate white pieces
    foreach (Piece piece in positionToEvaluate.GetPlayerWhite())
    {
        if (piece != null)
        {
            score += piece.value;

            // Add position-specific score using the piece-square tables
            int row = piece.GetBoardY();
            int col = piece.GetBoardX();

            if (piece.name.Contains("pawn"))
            {
                score += ValueMaps.pawns[row, col];
            }
            else if (piece.name.Contains("knight"))
            {
                score += ValueMaps.knights[row, col];
            }
            else if (piece.name.Contains("bishop"))
            {
                score += ValueMaps.bishops[row, col];
            }
            else if (piece.name.Contains("rook"))
            {
                score += ValueMaps.rooks[row, col];
            }
            else if (piece.name.Contains("queen"))
            {
                score += ValueMaps.queens[row, col];
            }
            else if (piece.name.Contains("king"))
            {
                score += ValueMaps.kingMiddleGame[row, col]; // Assume middle game for now
            }
            if (!piece.name.Contains("pawn") &&
                positionToEvaluate.isSquareAttackedByBlackPawns[piece.GetBoardY(), piece.GetBoardX()])
            {
                score -= piece.value / 3; // Reduce score as a penalty for being attacked by a pawn
            }
        }
    }

    // Evaluate black pieces similarly (subtracting values since black is bad for white)
    foreach (Piece piece in positionToEvaluate.GetPlayerBlack())
    {
        if (piece != null)
        {
            score += piece.value;

            // Add position-specific score using the piece-square tables
            int row = 7 - piece.GetBoardY();
            int col = piece.GetBoardX();

            if (piece.name.Contains("pawn"))
            {
                score -= ValueMaps.pawns[row, col];
            }
            else if (piece.name.Contains("knight"))
            {
                score -= ValueMaps.knights[row, col];
            }
            else if (piece.name.Contains("bishop"))
            {
                score -= ValueMaps.bishops[row, col];
            }
            else if (piece.name.Contains("rook"))
            {
                score -= ValueMaps.rooks[row, col];
            }
            else if (piece.name.Contains("queen"))
            {
                score -= ValueMaps.queens[row, col];
            }
            else if (piece.name.Contains("king"))
            {
                score -= ValueMaps.kingMiddleGame[row, col]; // Assume middle game for now
            }
            // Penalize non-pawn pieces if they are attacked by a black pawn
            if (!piece.name.Contains("pawn") &&
                positionToEvaluate.isSquareAttackedByWhitePawns[piece.GetBoardY(), piece.GetBoardX()])
            {
                score += piece.value / 3; // Reduce score as a penalty for being attacked by a pawn
            }
        }
    }

    return score;
}

public void OrderMoves(List<Move> moves, Position currentPosition)
{
    foreach (Move move in moves)
    {
        int moveScoreGuess = 0;

        // Retrieve the current piece being moved and the target piece being captured
        Piece movePiece = move.myPiece;
        Piece targetPiece = move.attackedPiece;

        // Determine the piece type and potential captured piece type
        int movePieceValue = movePiece.value;
        int capturePieceValue = (targetPiece != null) ? targetPiece.value : 0;

        // Prioritize capturing opponent's valuable pieces with our least valuable pieces
        if (targetPiece != null)
        {
            moveScoreGuess = 10 * capturePieceValue - movePieceValue;
        }

        // Penalize moving our pieces to a square attacked by an opponent pawn
        if (currentPosition.IsSquareAttackedByEnemyPawns(move))
        {
            moveScoreGuess -= movePieceValue;
        }

        // Store the estimated score in the move (assuming move has a field or property to hold this)
        move.score = moveScoreGuess;
    }

    // Sort the moves by their score in descending order (highest score moves first)
    moves.Sort((x, y) => y.score.CompareTo(x.score));
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
                        // Add the move as is if it's not a promotion
                        allMoves.Add(move);
                }
            }
        }
    }

    // Log the number of promoting moves
    int promotingMovesCount = allMoves.Count(m => m.promoteTo != null);
    
    // Ensure the method always returns a value
    return allMoves;
}

    // Function to make a random move from the list of possible moves
    public Move MakeRandomMove()
    {
        random = new System.Random();
        // Get all possible moves for the current player
        List<Move> allMoves = GetAllPossibleMoves(game.gamePosition, color);

        // If there are no legal moves, return null (e.g., checkmate or stalemate)
        if (allMoves.Count == 0)
        {
            return null;
        }

        // Pick a random move from the list
        int randomIndex = random.Next(allMoves.Count);
        Move selectedMove = allMoves[randomIndex];

        return selectedMove;
    }
    

    public void PrintAllAIMoves()
{
    List<Move> aiMoves = GetAllPossibleMoves(game.gamePosition, game.aiPlayer);
    foreach (Move move in aiMoves)
    {
        if (move != null)
        {
            UnityEngine.Debug.Log(move.ToString()); // Assuming Move class has a ToString method for printing
        }
    }
}

public int AlphaBeta(Position position, int depth, int alpha, int beta, bool isMaximizingPlayer)
{
    // Base case: if depth is zero or the game is over, evaluate the board
    if (depth == 0)
    {
        int eval = Evaluate(position);
        UnityEngine.Debug.Log($"Evaluating position at depth {depth}, Score: {eval}");
        return eval;
    }

    // Generate and sort all possible moves for better pruning
    string currentPlayer = isMaximizingPlayer ? "white" : "black";
    List<Move> allMoves = position.GetAllPossibleMoves(currentPlayer);
    OrderMoves(allMoves, position);
    UnityEngine.Debug.Log($"Generating moves for {currentPlayer} at depth {depth}. Total moves: {allMoves.Count}");

    // Initialize evaluation variables
    if (isMaximizingPlayer)
    {
        int maxEval = int.MinValue;

        // Iterate through all generated moves
        foreach (Move move in allMoves)
        {
            UnityEngine.Debug.Log($"Maximizing Player ({currentPlayer}) handling move: {move}");

            // Make the move
            position.HandleMove(move);

            // Recursively call AlphaBeta for minimizer
            int eval = AlphaBeta(position, depth - 1, alpha, beta, false);
            maxEval = Math.Max(maxEval, eval);

            // Undo the move
            position.UndoMove(move);

            // Update alpha and check for pruning
            alpha = Math.Max(alpha, eval);
            UnityEngine.Debug.Log($"Maximizing Player ({currentPlayer}) updated alpha: {alpha}");

            // Prune branches where alpha exceeds or equals beta
            if (beta <= alpha)
            {
                UnityEngine.Debug.Log($"Pruning for Maximizing Player at depth {depth} with alpha: {alpha}, beta: {beta}");
                break; // Beta cutoff
            }
        }
        return maxEval;
    }
    else
    {
        int minEval = int.MaxValue;

        // Iterate through all generated moves
        foreach (Move move in allMoves)
        {
            UnityEngine.Debug.Log($"Minimizing Player ({currentPlayer}) handling move: {move}");

            // Make the move
            position.HandleMove(move);

            // Recursively call AlphaBeta for maximizer
            int eval = AlphaBeta(position, depth - 1, alpha, beta, true);
            minEval = Math.Min(minEval, eval);

            // Undo the move
            position.UndoMove(move);

            // Update beta and check for pruning
            beta = Math.Min(beta, eval);
            UnityEngine.Debug.Log($"Minimizing Player ({currentPlayer}) updated beta: {beta}");

            // Prune branches where alpha exceeds or equals beta
            if (beta <= alpha)
            {
                UnityEngine.Debug.Log($"Pruning for Minimizing Player at depth {depth} with alpha: {alpha}, beta: {beta}");
                break; // Alpha cutoff
            }
        }
        return minEval;
    }
}





    public Move GetBestMove(Position position, int depth)
{
    // Determine if the AI is maximizing or minimizing based on color
    bool isMaximizing = (color == "white");
    int bestValue = isMaximizing ? int.MinValue : int.MaxValue;
    Move bestMove = null;

    // Generate all possible moves for the current player
    List<Move> allMoves = position.GetAllPossibleMoves(color);
    OrderMoves(allMoves, position);
    UnityEngine.Debug.Log($"Finding best move for AI ({color}) at depth {depth}. Total moves: {allMoves.Count}");

    foreach (Move move in allMoves)
    {
        UnityEngine.Debug.Log($"Evaluating move: {move}");
        position.HandleMove(move);

        // Call AlphaBeta for each move
        int moveValue = AlphaBeta(position, depth - 1, int.MinValue, int.MaxValue, !isMaximizing);
        UnityEngine.Debug.Log($"Move: {move}, Value: {moveValue}");

        position.UndoMove(move);

        // Update the best move if needed
        if (isMaximizing && moveValue > bestValue)
        {
            bestValue = moveValue;
            bestMove = move;
        }
        else if (!isMaximizing && moveValue < bestValue)
        {
            bestValue = moveValue;
            bestMove = move;
        }
    }

    if (bestMove != null)
    {
        UnityEngine.Debug.Log($"Best move found: {bestMove} with value: {bestValue}");
    }
    else
    {
        return MakeRandomMove();
        UnityEngine.Debug.Log("No valid moves available!");
    }

    return bestMove;
}



    // Method for the AI to make a move on the board
    public void MakeAIMove()
    {
        Position currentPosition = game.gamePosition;

        // Get the best move using alpha-beta pruning
        Move bestMove = GetBestMove(currentPosition, 3); // Depth of 3

        if (bestMove != null)
        {
            // Apply the move on the board
            PieceScript ps = game.GetPosition(bestMove.myPiece.GetBoardX(), bestMove.myPiece.GetBoardY()).GetComponent<PieceScript>();
            ps.HandleMoveOnBoardAndLogic(bestMove);
            UnityEngine.Debug.Log($"AI played move: {bestMove}");
        }
        else
        {
            UnityEngine.Debug.Log("No valid moves available!");
        }
    }

}
