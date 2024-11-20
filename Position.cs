using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class Position{
public GameObject chesspiece;
    public GameObject movePlate;
    public Piece[,] positions = new Piece[8, 8];
    public Piece[] playerWhite = new Piece [16]; 
    public Piece[] playerBlack = new Piece [16];
    public int moveCounter = 1; // Full move counter, increments after Black's move
    public int halfMoveCounter = 0; // Half-move counter for the 50-move rule
    private Dictionary<string, int> fenHistory = new Dictionary<string, int>(); // Dictionary to store FEN strings and count occurrences
    public Move lastMove;
    public string winner;
    public bool gameOver = false;
    private string currentPlayer;
    private string enPassantSquare = "-"; // Default value indicating no en passant available

public Position(string fen)
    {
        LoadPositionFromFEN(fen); // Use your existing method to load the position from FEN
    }


    // Logic-only creation of a Piece
    public Piece Create(string name, int x, int y)
    {
        // Create a logical-only piece (no GameObject)
        Piece piece = new Piece();
        piece.name = name;
        piece.SetBoardX(x);
        piece.SetBoardY(y);
        piece.SetPieceMoveArray();
        piece.AssignPlayerColor();
        piece.AssignValue();
        piece.game = this;
        SetPosition(piece); // Set the piece's position
        return piece; // Return logic piece (not instantiated in the world)
    }

    public void SetPlayer(string color) {
        currentPlayer = color;
    }
    public string GetPlayer() {
        return currentPlayer;
    }
    
    public Dictionary<string, int> GetFENHistory() {
        return fenHistory;
    }

    public Piece GetKing(string color) {
        string kingName = color == "white" ? "white_king" : "black_king";
        Piece[] pieceArr = color == "white" ? playerWhite : playerBlack;
        foreach (Piece piece in pieceArr) {
            if (piece != null){
            if (piece.name == kingName) {
                return piece; // Return the king piece for the specified color
            }
            }
        }
        return null; // Return null if king not found
    }

    public void SetPosition(Piece piece){ //Puts board coordinates for each piece into the position array
        positions[piece.GetBoardX(), piece.GetBoardY()] = piece;
    }

    public void SetPosition(Piece piece, int x, int y){ //Puts board coordinates for each piece into the position array
        positions[piece.GetBoardX(), piece.GetBoardY()] = piece;
    }

    public void SetPositionEmpty(int x, int y) { //Clear position when piece is taken
        positions[x, y] = null;
    }
    public Piece GetPosition(int x, int y) {
        return positions[x, y];
    }
    public Piece[,] GetAllPositions() {
        return positions;
    }
    public Piece[] GetPlayerWhite() {
        return playerWhite;
    }
    public Piece[] GetPlayerBlack() {
        return playerBlack;
    }

    public bool PositionOnBoard(int x, int y) {  //Check if board position exists, [0,8)
        if (x<0 || y<0 || x >= positions.GetLength(0) || y >= positions.GetLength(1)){
            return false;
        }
        return true;
    }

    public void SetPlayerArrayEmpty(Piece piece) { //Clear position when piece is taken
        if (piece != null && piece.GetPlayerColor() == "white") {
        for (int i=0; i<playerWhite.Length; i++) {
            if (playerWhite[i] == piece) playerWhite[i] = null;
        }
        } else {
        for (int i=0; i<playerBlack.Length; i++) {
            if (playerBlack[i] == piece) playerBlack[i] = null;
        }
        }

    }

    public List<Move> GetAllPossibleMoves(string playerColor)
    {
        List<Move> allMoves = new List<Move>(); // List to store all legal moves

        // Get the correct player's pieces based on the color
        Piece[] playerPieces = (playerColor == "white") ? GetPlayerWhite() : GetPlayerBlack();

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

    public void ConsoleLogPosition() {
        Piece[,] arr = GetAllPositions();
        StringBuilder sb = new StringBuilder();
        for (int i = 7; i>=0; i--){
            for (int j = 0; j<=7; j++){
                if (arr[j,i]==null){
                sb.Append("Empty");
                sb.Append(' ');
                }
                else{
                string name = (arr[j,i].name);
                sb.Append(name);
                sb.Append(' ');
                }
            }
            sb.Append("\n");
        }
        UnityEngine.Debug.Log(sb.ToString());
    }

    public void MovePiece(int targetX, int targetY, Piece piece)
{
    if (piece != null) {
    SetPositionEmpty(piece.GetBoardX(), piece.GetBoardY()); // Clear old position
    piece.SetBoardX(targetX);
    piece.SetBoardY(targetY);
    SetPosition(piece, targetX, targetY);
    }
}

public void HandleMove(Move moveMade)
{
    bool captureMade = false;
    lastMove = moveMade;
    HandleEnPassantMove(moveMade.myPiece, moveMade);
    if (moveMade.isAttack)
    {
        if (moveMade.isEnPassant && moveMade.myPiece.GetPlayerColor() == "white"){
            SetPositionEmpty(moveMade.GetBoardX(), moveMade.GetBoardY() - 1);
            SetPlayerArrayEmpty(moveMade.attackedPiece);
        }
        else if (moveMade.isEnPassant && moveMade.myPiece.GetPlayerColor() == "black"){
            SetPositionEmpty(moveMade.GetBoardX(), moveMade.GetBoardY() + 1);
            SetPlayerArrayEmpty(moveMade.attackedPiece);
        }
        else {
            SetPositionEmpty(moveMade.GetBoardX(), moveMade.GetBoardY());
            SetPlayerArrayEmpty(moveMade.attackedPiece);
        }
        moveMade.attackedPiece.name = null;
        captureMade = true;
    }
    if (moveMade.isCastling){
        if (moveMade.GetBoardX() == 6){
          // Kingside castling: Move rook from 7 to 5
            MovePiece(5, moveMade.GetBoardY(), GetPosition(7, moveMade.GetBoardY()));
        }
        if (moveMade.GetBoardX() == 2){
          // Kingside castling: Move rook from 0 to 3
            MovePiece(3, moveMade.GetBoardY(), GetPosition(0, moveMade.GetBoardY()));
        }
    }
    MovePiece(moveMade.GetBoardX(), moveMade.GetBoardY(), moveMade.myPiece);
    moveMade.myPiece.hasMoved = true;
    EndTurn(moveMade.myPiece, captureMade);
    string fen = GenerateFENForRepetitionTracking();
    AddFENToHistory(fen);
    CheckDrawOrCheckMate();
}

public void UndoMove(Move moveMade)
{
    // Step 1: Reverse the main move
    MovePiece(moveMade.startX, moveMade.startY, moveMade.myPiece); // Move the piece back to its original square
    moveMade.myPiece.hasMoved = moveMade.wasMovedBefore; // Reset the hasMoved status

    // Step 2: Restore captured pieces if an attack was made
    if (moveMade.isAttack)
    {
        if (moveMade.isEnPassant)
        {
            // Restore en passant capture by placing the captured pawn back
            int captureY = moveMade.GetBoardY() + (moveMade.myPiece.GetPlayerColor() == "white" ? -1 : 1);
            SetPosition(moveMade.GetBoardX(), captureY, moveMade.attackedPiece);
            SetPlayerArray(moveMade.attackedPiece); // Re-add the captured piece to the player's array
        }
        else
        {
            // Place the captured piece back on the target square
            SetPosition(moveMade.GetBoardX(), moveMade.GetBoardY(), moveMade.attackedPiece);
            SetPlayerArray(moveMade.attackedPiece); // Re-add the captured piece to the player's array
        }
    }

    // Step 3: Undo castling, if castling was performed
    if (moveMade.isCastling)
    {
        if (moveMade.GetBoardX() == 6) // Kingside castling
        {
            MovePiece(7, moveMade.GetBoardY(), GetPosition(5, moveMade.GetBoardY())); // Move rook back from 5 to 7
        }
        else if (moveMade.GetBoardX() == 2) // Queenside castling
        {
            MovePiece(0, moveMade.GetBoardY(), GetPosition(3, moveMade.GetBoardY())); // Move rook back from 3 to 0
        }
    }

    // Step 5: Reset additional flags or states if needed, depending on game logic
}


public void HandlePromotion(Move moveMade, string pieceToPromoteTo)
{
    // Check if the move involves a pawn reaching the back rank for promotion
    ApplyPromotion(moveMade.myPiece, pieceToPromoteTo);
}

public void ApplyPromotion(Piece piece, string pieceType)
{
    // Replace the pawn with the selected piece
    switch (pieceType)
    {
        case "white_queen":
            piece.name = "white_queen";
            break;
        case "white_rook":
            piece.name = "white_rook";
            break;
        case "white_bishop":
            piece.name = "white_bishop";
            break;
        case "white_knight":
            piece.name = "white_knight";
            break;
        case "black_queen":
            piece.name = "black_queen";
            break;
        case "black_rook":
            piece.name = "black_rook";
            break;
        case "black_bishop":
            piece.name = "black_bishop";
            break;
        case "black_knight":
            piece.name = "black_knight";
            break;
    }
    piece.SetPieceMoveArray();
    piece.AssignValue();
}
    public bool isLegal(Move pseudoMove){
        Move move = pseudoMove;
        int originalX = move.myPiece.GetBoardX(); //Save old piece position to reset after check
        int originalY = move.myPiece.GetBoardY();
        Piece tempPiece = null;
        // Check if the move is a castling move
        if (move.isCastling) {
        // If it's a castling move, use the IsCastlingLegal method to validate it
        if (!IsCastlingLegal(move, originalX, originalY)) {
            return false; // Castling is not legal
        }
        // Castling is legal
        return true;
        }


        if (move.isAttack){
            SetPositionEmpty(move.GetBoardX(), move.GetBoardY());
            SetPlayerArrayEmpty(tempPiece);
            tempPiece = pseudoMove.attackedPiece;
        }

        MovePiece(move.GetBoardX(), move.GetBoardY(), move.myPiece); //Move piece to simulate board if it was moved
        if (move.myPiece.GetPlayerColor() == "white"){
        foreach (Piece piece in playerBlack){
        if (piece != null){
          piece.GeneratePsuedoMoves();

        if (piece == tempPiece) {
            tempPiece.ClearPsuedoMoves(); // Clear the pseudo-moves of the captured piece
            }

          Move[] responses = piece.GetPseudoLegalMoves();
          foreach (Move blackMove in responses){
            if (blackMove!=null){
            if (blackMove.attackedPiece != null){
            blackMove.SetAttackedPiece(GetPosition(blackMove.GetBoardX(), blackMove.GetBoardY()));
            UnityEngine.Debug.Log(blackMove.attackedPiece.name);
            if (blackMove.attackedPiece.name == "white_king"){
              MovePiece(originalX, originalY, move.myPiece); //Put board back after legality check is complete
              if (tempPiece != null && PositionOnBoard(tempPiece.GetBoardX(), tempPiece.GetBoardY())){
                SetPosition(tempPiece);
              }
              return false;
            }
            }
            }
          }
        }
        }
        }
        if (move.myPiece.GetPlayerColor() == "black"){
        foreach (Piece piece in playerWhite){
        if (piece != null){
          piece.GeneratePsuedoMoves();

        if (piece == tempPiece) {
            tempPiece.ClearPsuedoMoves(); // Clear the pseudo-moves of the captured piece
            }

          Move[] responses = piece.GetPseudoLegalMoves();
          foreach (Move whiteMove in responses){
            if (whiteMove!=null){
            if (whiteMove.attackedPiece != null){
            whiteMove.SetAttackedPiece(GetPosition(whiteMove.GetBoardX(), whiteMove.GetBoardY()));
            UnityEngine.Debug.Log(whiteMove.attackedPiece.name);
            if (whiteMove.attackedPiece.name == "black_king"){
              MovePiece(originalX, originalY, move.myPiece); //Put board back after legality check is complete
              if (tempPiece != null && PositionOnBoard(tempPiece.GetBoardX(), tempPiece.GetBoardY())){
                SetPosition(tempPiece);
              }
              return false;
            }
            }
            }
          }
        }
        }
        }
        MovePiece(originalX, originalY, move.myPiece); //Put board back after legality check is complete
              if (tempPiece != null && PositionOnBoard(tempPiece.GetBoardX(), tempPiece.GetBoardY())){
                SetPosition(tempPiece);
              }
        return true;
    }


    private bool IsCastlingLegal(Move castlingMove, int originalX, int originalY) {
    Piece kingScript = castlingMove.myPiece;
    int targetX = castlingMove.GetBoardX();
    int boardY = kingScript.GetBoardY();  // Row where the king is (0 for white, 7 for black)

    // Check if the king is currently in check by examining the opponent's pseudo-legal moves
    foreach (Piece piece in (kingScript.GetPlayerColor() == "white" ? playerBlack : playerWhite)) {
        if (piece != null) {
            piece.GeneratePsuedoMoves();
            Move[] responses = piece.GetPseudoLegalMoves();
            foreach (Move move in responses) {
                if (move != null && move.GetBoardX() == originalX && move.GetBoardY() == originalY) {
                    // If the king is currently in check, castling is not allowed
                    return false;
                }
            }
        }
    }

    // Determine the squares the king moves through (two squares for kingside, two squares for queenside)
    int[] kingPathX = (targetX == 6) ? new int[] {5, 6} : new int[] {3, 2};  // Kingside or queenside

    // Simulate the king moving through the path
    foreach (int x in kingPathX) {
        MovePiece(x, boardY, kingScript);
        
        // Check if any opponent piece can attack the square the king moves through
        foreach (Piece piece in (kingScript.GetPlayerColor() == "white" ? playerBlack : playerWhite)) {
            if (piece != null) {
                piece.GeneratePsuedoMoves();
                Move[] responses = piece.GetPseudoLegalMoves();
                foreach (Move move in responses) {
                    if (move != null && move.attackedPiece != null && (move.attackedPiece.name == "white_king" || move.attackedPiece.name == "black_king")) {
                        // If the king would be in check on this square, castling is illegal
                        MovePiece(originalX, originalY, kingScript);  // Reset king's position
                        return false;
                    }
                }
            }
        }
    }

    // If the castling path is safe, castling is legal
    MovePiece(originalX, originalY, kingScript);  // Reset king's position
    return true;
}

    // Function to handle move execution and en passant vulnerability
    public void HandleEnPassantMove(Piece piece, Move moveMade) {
        // Check if the moved piece is a pawn and moved two squares forward
        if (piece.GetName() != null && piece.GetName().Contains("pawn") && Mathf.Abs(moveMade.GetBoardY() - moveMade.myPiece.GetBoardY()) == 2) {
            piece.SetEnPassantVulnerable(true);  // Mark this pawn as en passant vulnerable
        }
        // Reset en passant vulnerability for all other pawns
        ResetEnPassantVulnerability(moveMade.myPiece);
    }

    // Reset en passant vulnerability for all other pawns
    public void ResetEnPassantVulnerability(Piece movedPiece) {
        foreach (Piece piece in playerWhite) {
            if (piece != null && piece != movedPiece && piece.GetName().Contains("pawn")) {
                piece.SetEnPassantVulnerable(false);
            }
        }

        foreach (Piece piece in playerBlack) {
            if (piece != null && piece != movedPiece && piece.GetName().Contains("pawn")) {
                piece.SetEnPassantVulnerable(false);
            }
        }
    }

    public bool BlackHasMoves(){
        foreach (Piece piece in playerBlack){
        if (piece != null){
          piece.GeneratePsuedoMoves();
          piece.CreateLegalMoves();
          Move[] responses = piece.GetLegalMoves();
          foreach (Move blackMove in responses){
            if (blackMove!=null){
              return true;
            }
            }
          }
        }
        return false;
    }
    public bool WhiteHasMoves(){
        foreach (Piece piece in playerWhite){
        if (piece != null){
          piece.GeneratePsuedoMoves();
          piece.CreateLegalMoves();
          Move[] responses = piece.GetLegalMoves();
          foreach (Move whiteMove in responses){

            if (whiteMove!=null){
              return true;
            }
            }
          }
        }
        return false;
    }

    public bool IsKingInCheck(string playerColor)
{
    // Determine the opponent's pieces based on the current player's color
    Piece[] opponentPieces = playerColor == "white" ? playerBlack : playerWhite;
    string kingName = playerColor == "white" ? "white_king" : "black_king";


    // Loop through each of the opponent's pieces
    foreach (Piece piece in opponentPieces)
    {
        if (piece != null)
        {
            piece.GeneratePsuedoMoves(); // Generate pseudo-legal moves for this piece

            Move[] opponentMoves = piece.GetPseudoLegalMoves();
            foreach (Move move in opponentMoves)
            {
                if (move != null && move.attackedPiece != null && move.attackedPiece.name == kingName)
                {
                    // If any move attacks the current player's king, return true (king is in check)
                    return true;
                }
            }
        }
    }

    // If no opponent moves target the king, return false (king is not in check)
    return false;
}


    private bool IsThreefoldRepetition()
    {
    // Iterate over the FEN history and check if any position has occurred 3 times
    foreach (var entry in fenHistory)
    {
        if (entry.Value >= 3)  // If any position has been repeated 3 times
        {
            return true;
        }
    }
    return false;
    }



    

public string GenerateFENForRepetitionTracking()
{
    StringBuilder fen = new StringBuilder();

    // 1. Piece placement (ranks 8 to 1)
    for (int rank = 7; rank >= 0; rank--)  // Start from rank 8 to 1
    {
        int emptySquares = 0;

        for (int file = 0; file < 8; file++)  // Files a to h
        {
            Piece piece = positions[file, rank];
            if (piece != null)
            {
                if (emptySquares > 0)
                {
                    fen.Append(emptySquares);  // Add number of empty squares
                    emptySquares = 0;
                }

                // Append piece symbol (uppercase for white, lowercase for black)
                string pieceName = piece.GetName();
                fen.Append(GetFENPieceSymbol(pieceName));
            }
            else
            {
                emptySquares++;  // Count empty squares
            }
        }

        if (emptySquares > 0)
        {
            fen.Append(emptySquares);  // Append remaining empty squares in the rank
        }

        if (rank > 0) fen.Append("/");  // Separate ranks with a '/'
    }

    // 2. Active color
    fen.Append(" ");
    fen.Append(currentPlayer == "white" ? "w" : "b");

    // 3. Castling availability
    fen.Append(" ");
    string castlingRights = GetCastlingRightsUsingIsCastlingLegal();
    fen.Append(string.IsNullOrEmpty(castlingRights) ? "-" : castlingRights);

    // 4. En passant target square (if en passant move was made)
    fen.Append(" ");
    enPassantSquare = GetEnPassantSquare();
    if (enPassantSquare!= null) fen.Append(enPassantSquare);

    // 5. Half-move clock (for the 50-move rule) **Commented out for repetition tracking**
    fen.Append(" ");
    fen.Append(halfMoveCounter.ToString());

    // 6. Full-move number (increments after black's move)
    fen.Append(" ");
    fen.Append(moveCounter.ToString());

    string fenString = fen.ToString();  // Store the final FEN string

    UnityEngine.Debug.Log(fenString);

    return fenString;  // Return the FEN string for repetition tracking
}

// Convert piece name to FEN symbol
private string GetFENPieceSymbol(string pieceName)
{
    switch (pieceName)
    {
        case "white_king": return "K";
        case "white_queen": return "Q";
        case "white_rook": return "R";
        case "white_bishop": return "B";
        case "white_knight": return "N";
        case "white_pawn": return "P";
        case "black_king": return "k";
        case "black_queen": return "q";
        case "black_rook": return "r";
        case "black_bishop": return "b";
        case "black_knight": return "n";
        case "black_pawn": return "p";
        default: return "";  // In case of an unknown piece name
    }
}

// En passant square logic: return the en passant target square if enPassantMoveMade is set
private string GetEnPassantSquare()
{

    if (lastMove != null && lastMove.isEnPassantVulnerable)
    {

        int x = lastMove.GetBoardX();
        int y = lastMove.GetBoardY();

        if (GetPlayer() == "white") //Black's en passant square
        {

            return GetSquareFromCoordinates(x, y + 1);
        }
        else if (GetPlayer() == "black")
        {

            return GetSquareFromCoordinates(x, y - 1);
        }
    }

    // Return - if no en passant move was made
    return "-";
}

// Convert coordinates to chess notation (e.g., 4,4 -> "e5")
private string GetSquareFromCoordinates(int x, int y)
{
    char file = (char)('a' + x);  // Convert the file (0 = 'a', 7 = 'h')
    int rank = y + 1;  // Convert the rank (0 = 1, 7 = 8)
    return file.ToString() + rank.ToString();
}

// Castling rights: check if kingside or queenside castling is legal using IsCastlingLegal()
private string GetCastlingRightsUsingIsCastlingLegal()
{
    StringBuilder castling = new StringBuilder();

    // Get the kings for white and black
    Piece whiteKing = GetKing("white");
    Piece blackKing = GetKing("black");

    // Check White King's castling rights
    if (whiteKing != null)
    {
        if (whiteKing.CanCastleKingsideFEN()) castling.Append("K");  // White kingside
        if (whiteKing.CanCastleQueensideFEN()) castling.Append("Q");  // White queenside
    }

    // Check Black King's castling rights
    if (blackKing != null)
    {
        if (blackKing.CanCastleKingsideFEN()) castling.Append("k");  // Black kingside
        if (blackKing.CanCastleQueensideFEN()) castling.Append("q");  // Black queenside
    }

    // Return the castling rights string or "-" if none exist
    return castling.Length > 0 ? castling.ToString() : "-";
}

public void AddFENToHistory(string fen)
    {
        if (fenHistory.ContainsKey(fen))
        {
            fenHistory[fen]++;
        }
        else
        {
            fenHistory[fen] = 1;
        }
    }

public void EndTurn(Piece movedPiece, bool pieceCaptured) {
    // Check if a pawn was moved or a piece was captured
    if (movedPiece.GetName() != null && movedPiece.GetName().Contains("pawn") || pieceCaptured) {
        halfMoveCounter = 0; // Reset half-move counter for pawn moves or captures
    } else {
        halfMoveCounter++; // Increment for non-pawn, non-capture moves
    }

    // If Black's turn just ended, increment the full move counter
    if (currentPlayer == "black") {
        moveCounter++;
    }
    movedPiece.GeneratePsuedoMoves();
    // Toggle the player's turn after a valid move
    TogglePlayerTurn();
}

private void TogglePlayerTurn()
{
    if (currentPlayer == "white")
    {
        currentPlayer = "black";
    }
    else
    {
        currentPlayer = "white";
    }
}


public void LoadPositionFromFEN(string fen)
{
    // Initialize arrays for white and black pieces (maximum 16 pieces each)
    playerWhite = new Piece[16];
    playerBlack = new Piece[16];

    int whiteIndex = 0;
    int blackIndex = 0;

    // Split the FEN string into its components
    string[] fenParts = fen.Split(' ');

    // Part 1: Piece placement (ranks 8 to 1)
    string[] ranks = fenParts[0].Split('/');
    for (int rank = 0; rank < 8; rank++) // Loop through each rank
    {
        int file = 0; // Start at file 'a'
        foreach (char symbol in ranks[rank])
        {
            if (char.IsDigit(symbol)) // Empty squares
            {
                file += (int)char.GetNumericValue(symbol); // Skip that many squares
            }
            else
            {
                // Create and place the piece on the board using CreatePieceFromFENSymbol
                Piece piece = CreatePieceFromFENSymbol(symbol, file, 7 - rank); // Y-coordinate is inverted
                if (piece != null)
                {
                    positions[file, 7 - rank] = piece; // Set piece at the correct file and rank
                    // Add piece to the correct player array
                    if (piece.GetName().Contains("white"))
                    {
                        playerWhite[whiteIndex] = piece;
                        whiteIndex++;
                    }
                    else if (piece.GetName().Contains("black"))
                    {
                        playerBlack[blackIndex] = piece;
                        blackIndex++;
                    }

                    file++; // Move to the next file
                }
            }
        }
    }

    // Part 2: Active player
    currentPlayer = (fenParts[1] == "w") ? "white" : "black";

    // Part 3: Castling availability
    UpdateCastlingRightsFromFEN(fenParts[2], GetKing("white"), GetWhiteKingsideRook(), GetWhiteQueensideRook(), GetKing("black"), GetBlackKingsideRook(), GetBlackQueensideRook());

    // Part 4: En passant target square
    string enPassantSquare = fenParts[3];
    if (enPassantSquare != "-")
    {
        SetEnPassantVulnerableFromFEN(enPassantSquare);
    }

    // Part 5: Half-move clock
    halfMoveCounter = int.Parse(fenParts[4]);

    // Part 6: Full-move number
    moveCounter = int.Parse(fenParts[5]);
}

private Piece CreatePieceFromFENSymbol(char symbol, int file, int rank)
{
    switch (symbol)
    {
        // White pieces (uppercase letters)
        case 'P': return Create("white_pawn", file, rank);
        case 'R': return Create("white_rook", file, rank);
        case 'N': return Create("white_knight", file, rank);
        case 'B': return Create("white_bishop", file, rank);
        case 'Q': return Create("white_queen", file, rank);
        case 'K': return Create("white_king", file, rank);

        // Black pieces (lowercase letters)
        case 'p': return Create("black_pawn", file, rank);
        case 'r': return Create("black_rook", file, rank);
        case 'n': return Create("black_knight", file, rank);
        case 'b': return Create("black_bishop", file, rank);
        case 'q': return Create("black_queen", file, rank);
        case 'k': return Create("black_king", file, rank);

        // If symbol is invalid, return null
        default: return null;
    }
}

private void SetEnPassantVulnerableFromFEN(string enPassantSquare)
{
    // Convert en passant square (e.g., "e3") to file and rank
    int file = enPassantSquare[0] - 'a'; // Convert file from 'a' to 'h' to 0-indexed number
    int rank = int.Parse(enPassantSquare[1].ToString()) - 1; // Convert rank to 0-indexed number

    // The pawn that moved is one rank above/below the en passant target square
    int pawnRank = (currentPlayer == "White") ? rank - 1 : rank + 1;

    // Check if there's a pawn at the calculated position
    Piece pawn = positions[file, pawnRank];
    if (pawn != null && pawn.GetName().Contains("pawn"))
    {
        // Set the pawn's enPassantVulnerable flag to true
        pawn.SetEnPassantVulnerable(true);
    }
}

public void UpdateCastlingRightsFromFEN(string castlingRights, Piece whiteKing, Piece whiteRookKingside, Piece whiteRookQueenside, Piece blackKing, Piece blackRookKingside, Piece blackRookQueenside)
{
    // Set all kings and rooks to having moved by default
    if (whiteKing != null) whiteKing.hasMoved = true;
    if (whiteRookKingside != null) whiteRookKingside.hasMoved = true;
    if (whiteRookQueenside != null) whiteRookQueenside.hasMoved = true;
    if (blackKing != null) blackKing.hasMoved = true;
    if (blackRookKingside != null) blackRookKingside.hasMoved = true;
    if (blackRookQueenside != null) blackRookQueenside.hasMoved = true;

    // Check the castling rights from the FEN string
    foreach (char right in castlingRights)
    {
        switch (right)
        {
            case 'K': // White kingside castling is available
                if (whiteKing != null) whiteKing.hasMoved = false;
                if (whiteRookKingside != null) whiteRookKingside.hasMoved = false;
                break;
            case 'Q': // White queenside castling is available
                if (whiteKing != null) whiteKing.hasMoved = false;
                if (whiteRookQueenside != null) whiteRookQueenside.hasMoved = false;
                break;
            case 'k': // Black kingside castling is available
                if (blackKing != null) blackKing.hasMoved = false;
                if (blackRookKingside != null) blackRookKingside.hasMoved = false;
                break;
            case 'q': // Black queenside castling is available
                if (blackKing != null) blackKing.hasMoved = false;
                if (blackRookQueenside != null) blackRookQueenside.hasMoved = false;
                break;
            default:
                // Ignore '-' or unrecognized symbols
                break;
        }
    }
}

public Piece GetWhiteKingsideRook()
{
    Piece piece = positions[7, 0]; // h1
    if (piece != null && piece.GetName() == "white_rook")
    {
        return piece; // This is the white kingside rook
    }
    return null; // No rook present
}

public Piece GetWhiteQueensideRook()
{
    Piece piece = positions[0, 0]; // a1
    if (piece != null && piece.GetName() == "white_rook")
    {
        return piece; // This is the white queenside rook
    }
    return null; // No rook present
}

public Piece GetBlackKingsideRook()
{
    Piece piece = positions[7, 7]; // h8
    if (piece != null && piece.GetName() == "black_rook")
    {
        return piece; // This is the black kingside rook
    }
    return null; // No rook present
}

public Piece GetBlackQueensideRook()
{
    Piece piece = positions[0, 7]; // a8
    if (piece != null && piece.GetName() == "black_rook")
    {
        return piece; // This is the black queenside rook
    }
    return null; // No rook present
}
public void CheckDrawOrCheckMate(){
        // Check for threefold repetition first
        if (IsThreefoldRepetition())
        {
        gameOver = true;
        SetWinner("draw");  // Declare the game as a draw
        return;
        }
        if (currentPlayer == "white"){
            if (!WhiteHasMoves() && IsKingInCheck(currentPlayer)){ //Checkmate for black
                gameOver = true;
                SetWinner("black");
            }
            if (!WhiteHasMoves() && !IsKingInCheck(currentPlayer)){ //Stalemate
                gameOver = true;
                SetWinner("draw");
            }
            // Check for draw by 50-move rule first
            if (halfMoveCounter >= 50) {
            gameOver = true;
            SetWinner("draw"); // Declare the game as a draw
            return;
    }
        }
        else{
            if (!BlackHasMoves() && IsKingInCheck(currentPlayer)){ //Checkmate for white
                gameOver = true;
                SetWinner("white");
            }
            if (!BlackHasMoves() && !IsKingInCheck(currentPlayer)){ //Stalemate
                gameOver = true;
                SetWinner("draw");
            }
            if (halfMoveCounter >= 50) {
            gameOver = true;
            SetWinner("draw"); // Declare the game as a draw
            return;
    }
        }
        
    }
    public void SetWinner(string winner){
        this.winner = winner;
    }

    public List<Move> GenerateAllMoves(string color)
{
    List<Move> allMoves = new List<Move>(); // List to store all legal moves
    
    // Get the correct player's pieces based on the color
    Piece[] playerPieces = (color == "white") ? GetPlayerWhite() : GetPlayerBlack();
    
    // Iterate through all the pieces of the player
    foreach (Piece piece in playerPieces)
    {
        if (piece != null) // Ensure the piece is not null
        {
            piece.GeneratePsuedoMoves();
            piece.CreateLegalMoves();
            // Generate legal moves for the current piece
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
}
