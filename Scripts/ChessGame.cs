using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class ChessGame : MonoBehaviour{
    public GameObject chesspiece;
    public GameObject movePlate;
    private GameObject[,] positions = new GameObject[8, 8];
    private GameObject[] playerWhite = new GameObject [16]; 
    private GameObject[] playerBlack = new GameObject [16];
    public bool pieceDraggable = true;
    public bool promotionInProgress = false;
    public string promotionChoice = null;
    GameObject menuBackground;
    GameObject menuOptions;
    GameObject colorSelection;
    // Start is called before the first frame update
    public Button whiteQueenPromotionButton; public Button blackQueenPromotionButton;
    public Button whiteRookPromotionButton; public Button blackRookPromotionButton;
    public Button whiteBishopPromotionButton; public Button blackBishopPromotionButton;
    public Button whiteKnightPromotionButton; public Button blackKnightPromotionButton;
    public GameObject buttonMenu;
    public Position gamePosition;
    public string player1;
    public string player2;
    public string aiPlayer;
    public AI gameAI;
    public bool boardFlips = false;
    public AudioSource audioSource;
    public AudioClip winSound;
    public AudioClip loseSound;
    public AudioClip whiteMoveSound;
    public AudioClip blackMoveSound;
    public AudioClip takeSound;

    public string GetColorInFront(bool alternate) {
        if (alternate){
        if (gamePosition.GetPlayer() == player1) {
            return player1;
        }
        else if (gamePosition.GetPlayer() == player2) {
            return player2;
        }
        }
            return player1;

    }

    public bool isPlayerTurn(){
        if (gamePosition.GetPlayer() == player1 || gamePosition.GetPlayer() == player2) {
            return true;
        }
        else return false;
    }

    void Start()
    {
        InitializeMainMenu();
        menuBackground = GameObject.FindGameObjectWithTag("MenuBackground");
        buttonMenu = GameObject.FindGameObjectWithTag("InGameButtons");
        menuOptions = GameObject.FindGameObjectWithTag("MenuOptions");
        colorSelection = GameObject.FindGameObjectWithTag("ColorSelection");
        DisableButtonMenu();
        DisableColorSelection();
    }

    public void InitializeMainMenu()
{
    // Enable the GameObject with the tag "MenuBackground"
    menuBackground = GameObject.FindGameObjectWithTag("MenuBackground");
    if (menuBackground != null)
    {
        menuBackground.SetActive(true);
    }
}
public void DisableMainMenu()
{
    // Disable the GameObject with the tag "MenuBackground"
    GameObject menuBackground = GameObject.FindGameObjectWithTag("MenuBackground");
    if (menuBackground != null)
    {
        menuBackground.SetActive(false);
    }
}

public void InitializeColorSelection()
{
    UnityEngine.Debug.Log("ran");
    // Enable the GameObject with the tag "ColorSelection"
    if (colorSelection != null)
    {
        colorSelection.SetActive(true);
    }
}
public void DisableColorSelection()
{
    // Disable the GameObject with the tag "ColorSelection"
    if (colorSelection != null)
    {
        colorSelection.SetActive(false);
    }
}

public void InitializeMenuOptions()
{
    // Enable the GameObject with the tag "ColorSelection"
    if (menuOptions != null)
    {
        menuOptions.SetActive(true);
    }
}
public void DisableMenuOptions()
{
    // Disable the GameObject with the tag "ColorSelection"
    GameObject menuOptions = GameObject.FindGameObjectWithTag("MenuOptions");
    if (menuOptions != null)
    {
        menuOptions.SetActive(false);
    }
}

public void OnPlayerVsPlayerSelected()
{
    boardFlips = true;
    player1 = "white";
    player2 = "black";
    // Disable the main menu elements
    DisableMainMenu();

    // Start Player vs Player mode
    InitializeGame();
}
public void OnPracticeSelected()
{
    boardFlips = false;
    player1 = "white";
    player2 = "black";
    // Disable the main menu elements
    DisableMainMenu();

    // Start Player vs Player mode
    InitializeGame();
}
public void OnPlayerVsCPUSelected()
{
    DisableMenuOptions();
    InitializeColorSelection();
}

public void OnWhiteSelected()
{
    
    boardFlips = false;
    player1 = "white";
    aiPlayer = "black";
    gameAI = new AI();
    // Disable the main menu elements
    DisableMainMenu();

    // Start Player vs CPU mode
    InitializeGame();
}

public void OnBlackSelected()
{
    
    boardFlips = false;
    player1 = "black";
    aiPlayer = "white";
    gameAI = new AI();
    // Disable the main menu elements
    DisableMainMenu();

    // Start Player vs CPU mode
    InitializeGame();
    DelayedAIMove();

}

public void OnBackSelected()
{
    UnityEngine.Debug.Log("ran");
    DisableColorSelection();
    InitializeMenuOptions();
    
}

    public void InitializeGame() {
        gamePosition = new Position("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
        playerWhite = new GameObject[16];
        for (int i = 0; i < gamePosition.playerWhite.Length; i++)
        {
            if (gamePosition.playerWhite[i] != null)
            {
                playerWhite[i] = Create(gamePosition.playerWhite[i]);
            }
        }

        playerBlack = new GameObject[16];
        for (int i = 0; i < gamePosition.playerBlack.Length; i++)
        {
            if (gamePosition.playerBlack[i] != null)
            {
                playerBlack[i] = Create(gamePosition.playerBlack[i]);
            }
        }


        //Set piece positions
        for (int i=0; i < playerWhite.Length; i++) {
            SetPosition(playerWhite[i]);
            SetPosition(playerBlack[i]);
        }
        string startingFEN = gamePosition.GenerateFENForRepetitionTracking();
        gamePosition.AddFENToHistory(startingFEN);
        gameAI = new AI();
        gameAI.Activate();
    }
public void SetPosition(GameObject piece){ //Puts board coordinates for each piece into the position array
        PieceScript pScript = piece.GetComponent<PieceScript>();
        positions[pScript.myPiece.GetBoardX(), pScript.myPiece.GetBoardY()] = piece;
    }
public void SetGamePositionFromObject(PieceScript piece){ //Puts board coordinates for each piece into the position array
        Piece myPiece = piece.myPiece;
        gamePosition.SetPosition(myPiece, myPiece.GetBoardX(), myPiece.GetBoardY());
    }
public void SetPositionEmpty(int x, int y) { //Clear position when piece is taken
        positions[x, y] = null;
    }
    public GameObject GetPosition(int x, int y) {
        return positions[x, y];
    }


private void ClearBoard()
{
    // Destroy all GameObjects on the board (you can adjust this to your game logic)
    foreach (GameObject piece in playerWhite)
    {
        if (piece != null)
            Destroy(piece);
    }
    foreach (GameObject piece in playerBlack)
    {
        if (piece != null)
            Destroy(piece);
    }
}

    public GameObject Create(Piece piece) {
        GameObject gameObj = Instantiate(chesspiece, new Vector3(0,0,0), Quaternion.identity);
        PieceScript pScript = gameObj.GetComponent<PieceScript>();
        pScript.myPiece = piece;
        pScript.Activate();
        return gameObj;
    }
    
    public void MovePlateSpawn(Move moveToSpawn){
      GameObject mp = Instantiate(movePlate, new Vector3(0, 0, 0), Quaternion.identity);
      MovePlateScript mpScript = mp.GetComponent<MovePlateScript>();
      if (moveToSpawn != null){
      Move move = moveToSpawn;
      mpScript.myMove = move;
      if (move.isAttack) { //Change color for attacked pieces
          mpScript.isAttack = true;
          mp.GetComponent<SpriteRenderer>().color = new Color(1.0f, 0.0f, 0.0f, 0.3f);
          mp.GetComponent<SpriteRenderer>().transform.localScale += mpScript.scaleChange;
      }
      mpScript.SetCoordinates(move.GetBoardX(), move.GetBoardY());
      mpScript.SetCoordinates(GetColorInFront(boardFlips));
      }
    }

    public void DestroyMovePlates(){ //Destroy moveplates function for when piece is released
      GameObject[] movePlates = GameObject.FindGameObjectsWithTag("MovePlate");
      for (int i=0; i<movePlates.Length; i++){
        Destroy(movePlates[i]);
      }
    }
    
    public void SpawnLegalMoves(Piece pieceToMove){
        Move[] moves = pieceToMove.GetLegalMoves();
        foreach (Move moveToMake in moves){
            if (moveToMake != null){
            Move move = moveToMake;
            MovePlateSpawn(move);
            }
        }
    }
    public void SetWinner(string winner){
        if (buttonMenu != null)
        {
            buttonMenu.SetActive(true);
        }
        if (winner == aiPlayer){
            audioSource.PlayOneShot(loseSound);
        }
        else {
            audioSource.PlayOneShot(winSound);
        }
        switch(winner) {
            case "white":
            GameObject tmpText1 = GameObject.FindGameObjectWithTag("WinnerTextComponent");
            if (tmpText1 != null)
            {
                tmpText1.GetComponent<TMPro.TextMeshProUGUI>().text = "White Wins!";
            }
            break;
            case "black":
            GameObject tmpText2 = GameObject.FindGameObjectWithTag("WinnerTextComponent");
            if (tmpText2 != null)
            {
                tmpText2.GetComponent<TMPro.TextMeshProUGUI>().text = "Black Wins!";
            }
            break;
            case "draw":
            GameObject tmpText3 = GameObject.FindGameObjectWithTag("WinnerTextComponent");
            if (tmpText3 != null)
            {
                tmpText3.GetComponent<TMPro.TextMeshProUGUI>().text = "Draw!";
            }
            break;
        }
    }

    
public void HandlePromotion(Move moveMade)
{
    // Check if the move involves a pawn reaching the back rank for promotion
    if ((moveMade.myPiece.GetPlayerColor() == "white" && moveMade.GetBoardY() == 7) || (moveMade.myPiece.GetPlayerColor() == "black" && moveMade.myPiece.GetBoardY() == 0))
    {
        promotionInProgress = true;  // Pause the game

        if (moveMade.myPiece.GetPlayerColor() == "white")
        {
            EnableWhitePromotion();  // Enable white promotion UI
        }
        else if (moveMade.myPiece.GetPlayerColor() == "black")
        {
            EnableBlackPromotion();  // Enable black promotion UI
        }

    }
}

public void CompletePromotion(Piece piece)
{
    gamePosition.ApplyPromotion(piece, promotionChoice);
    ApplyPromotion(piece, promotionChoice);  // Apply the selected promotion

    // Hide the promotion UI
    DisablePromotionButtons();
    // Mark promotion as completed, allowing the game to continue
    promotionInProgress = false;
    promotionChoice = null;

}

public void CompleteAIPromotion(Piece piece, string promotionPiece)
{
    UnityEngine.Debug.Log("Piece: " + piece.name + " Square: " + piece.GetBoardX() + " " + piece.GetBoardY());
    gamePosition.ApplyPromotion(piece, promotionPiece);
    ApplyPromotion(piece, promotionPiece);  // Apply the selected promotion
}


private void ApplyPromotion(Piece piece, string pieceType)
{
    UnityEngine.Debug.Log("Piece: " + piece.name + " Square: " + piece.GetBoardX() + " " + piece.GetBoardY());
    PieceScript pScript = GetPosition(piece.GetBoardX(), piece.GetBoardY()).GetComponent<PieceScript>();
    // Replace the pawn with the selected piece
    switch (pieceType)
    {
        case "white_queen":
            
            pScript.gameObject.GetComponent<SpriteRenderer>().sprite = pScript.white_queen;
            break;
        case "white_rook":
            pScript.gameObject.GetComponent<SpriteRenderer>().sprite = pScript.white_rook;
            break;
        case "white_bishop":
            pScript.gameObject.GetComponent<SpriteRenderer>().sprite = pScript.white_bishop;
            break;
        case "white_knight":
            pScript.gameObject.GetComponent<SpriteRenderer>().sprite = pScript.white_knight;
            break;
        case "black_queen":
            pScript.gameObject.GetComponent<SpriteRenderer>().sprite = pScript.black_queen;
            break;
        case "black_rook":
            pScript.gameObject.GetComponent<SpriteRenderer>().sprite = pScript.black_rook;
            break;
        case "black_bishop":
            pScript.gameObject.GetComponent<SpriteRenderer>().sprite = pScript.black_bishop;
            break;
        case "black_knight":
            pScript.gameObject.GetComponent<SpriteRenderer>().sprite = pScript.black_knight;
            break;
    }
}

public void EnableWhitePromotion() 
{
    // Set positions for white promotion buttons
    whiteQueenPromotionButton.transform.localPosition = new Vector3(300, 70, 0);
    whiteRookPromotionButton.transform.localPosition = new Vector3(430, 70, 0);
    whiteBishopPromotionButton.transform.localPosition = new Vector3(300, -50, 0);
    whiteKnightPromotionButton.transform.localPosition = new Vector3(430, -50, 0);

    // Set scale for all white promotion buttons
    Vector3 scale = new Vector3(1.5f, 1.5f, 1);
    whiteQueenPromotionButton.transform.localScale = scale;
    whiteRookPromotionButton.transform.localScale = scale;
    whiteBishopPromotionButton.transform.localScale = scale;
    whiteKnightPromotionButton.transform.localScale = scale;

    // Enable promotion buttons
    whiteQueenPromotionButton.gameObject.SetActive(true);
    whiteRookPromotionButton.gameObject.SetActive(true);
    whiteBishopPromotionButton.gameObject.SetActive(true);
    whiteKnightPromotionButton.gameObject.SetActive(true);
}

public void EnableBlackPromotion() 
{
    // Set positions for black promotion buttons
    blackQueenPromotionButton.transform.localPosition = new Vector3(355, 70, 0);
    blackRookPromotionButton.transform.localPosition = new Vector3(485, 70, 0);
    blackBishopPromotionButton.transform.localPosition = new Vector3(355, -50, 0);
    blackKnightPromotionButton.transform.localPosition = new Vector3(485, -50, 0);

    // Set scale for all black promotion buttons
    Vector3 scale = new Vector3(1.5f, 1.5f, 1);
    blackQueenPromotionButton.transform.localScale = scale;
    blackRookPromotionButton.transform.localScale = scale;
    blackBishopPromotionButton.transform.localScale = scale;
    blackKnightPromotionButton.transform.localScale = scale;

    // Enable promotion buttons
    blackQueenPromotionButton.gameObject.SetActive(true);
    blackRookPromotionButton.gameObject.SetActive(true);
    blackBishopPromotionButton.gameObject.SetActive(true);
    blackKnightPromotionButton.gameObject.SetActive(true);
}

public void DisablePromotionButtons(){
  whiteQueenPromotionButton.gameObject.SetActive(false);
  whiteRookPromotionButton.gameObject.SetActive(false);
  whiteBishopPromotionButton.gameObject.SetActive(false);
  whiteKnightPromotionButton.gameObject.SetActive(false);
  blackQueenPromotionButton.gameObject.SetActive(false);
  blackRookPromotionButton.gameObject.SetActive(false);
  blackBishopPromotionButton.gameObject.SetActive(false);
  blackKnightPromotionButton.gameObject.SetActive(false);
}

public void CheckDrawOrCheckMate(){
    gamePosition.CheckDrawOrCheckMate();
    if (gamePosition.gameOver == true) {
        if (buttonMenu != null)
        {
            buttonMenu.SetActive(true);
        }
        switch(gamePosition.winner) {
            case "white":
            GameObject tmpText1 = GameObject.FindGameObjectWithTag("WinnerTextComponent");
            if (tmpText1 != null)
            {
                tmpText1.GetComponent<TMPro.TextMeshProUGUI>().text = "White Wins!";
            }
            break;
            case "black":
            GameObject tmpText2 = GameObject.FindGameObjectWithTag("WinnerTextComponent");
            if (tmpText2 != null)
            {
                tmpText2.GetComponent<TMPro.TextMeshProUGUI>().text = "Black Wins!";
            }
            break;
            case "draw":
            GameObject tmpText3 = GameObject.FindGameObjectWithTag("WinnerTextComponent");
            if (tmpText3 != null)
            {
                tmpText3.GetComponent<TMPro.TextMeshProUGUI>().text = "Draw!";
            }
            break;
        }
    }

}

    public void MainMenu () {
        ClearBoard();
        gamePosition.gameOver = false;
        if (menuBackground != null)
        {
            menuBackground.SetActive(true);
        }
        DisableButtonMenu();
    }

    public void RestartGame() {
        ClearBoard();
        gamePosition.gameOver = false;
        InitializeGame();
        GameObject buttonMenu = GameObject.FindGameObjectWithTag("InGameButtons");
        if (buttonMenu != null)
        {
            buttonMenu.SetActive(false);
        }
    }

    public void DisableButtonMenu() {
        if (buttonMenu != null)
        {
            buttonMenu.SetActive(false);
        }
    }

    public void WhiteQueenPromotion() {
        promotionChoice = "white_queen";
        promotionInProgress = false;
        gamePosition.CheckDrawOrCheckMate();
        if (gamePosition.GetPlayer() == aiPlayer){
            DelayedAIMove();
        }
    }
    public void WhiteRookPromotion() {
        promotionChoice = "white_rook";
        promotionInProgress = false;
        gamePosition.CheckDrawOrCheckMate();
        if (gamePosition.GetPlayer() == aiPlayer){
            DelayedAIMove();
        }
    }
    public void WhiteBishopPromotion() {
        promotionChoice = "white_bishop";
        promotionInProgress = false;
        gamePosition.CheckDrawOrCheckMate();
        if (gamePosition.GetPlayer() == aiPlayer){
            DelayedAIMove();
        }
    }
    public void WhiteKnightPromotion() {
        promotionChoice = "white_knight";
        promotionInProgress = false;
        gamePosition.CheckDrawOrCheckMate();
        if (gamePosition.GetPlayer() == aiPlayer){
            DelayedAIMove();
        }
    }
    public void BlackQueenPromotion() {
        promotionChoice = "black_queen";
        promotionInProgress = false;
        gamePosition.CheckDrawOrCheckMate();
        if (gamePosition.GetPlayer() == aiPlayer){
            DelayedAIMove();
        }
    }
    public void BlackRookPromotion() {
        promotionChoice = "black_rook";
        promotionInProgress = false;
        gamePosition.CheckDrawOrCheckMate();
        if (gamePosition.GetPlayer() == aiPlayer){
            DelayedAIMove();
        }
    }
    public void BlackBishopPromotion() {
        promotionChoice = "black_bishop";
        promotionInProgress = false;
        gamePosition.CheckDrawOrCheckMate();
        if (gamePosition.GetPlayer() == aiPlayer){
            DelayedAIMove();
        }
    }
    public void BlackKnightPromotion() {
        promotionChoice = "black_knight";
        promotionInProgress = false;
        gamePosition.CheckDrawOrCheckMate();
        if (gamePosition.GetPlayer() == aiPlayer){
            DelayedAIMove();
        }
    }
    public void SetAllCoordinates(string colorInFront) {
        if (colorInFront == "white") {
        foreach (GameObject piece in positions) {
            if (piece != null){
            piece.GetComponent<PieceScript>().SetCoordinates(colorInFront);
        }
        }
        }
        else {
        foreach (GameObject piece in positions) {
            if (piece!= null){
            piece.GetComponent<PieceScript>().SetCoordinates("black");
            }
        }
        }
    }
    public void PrintEvaluation(){
        int eval = gameAI.Evaluate(gamePosition);
        UnityEngine.Debug.Log("AI Evaluation: " + eval);
    }

public void ConsoleLogGame()
{
    StringBuilder sb = new StringBuilder();

    // Iterate through rows (top to bottom, as in chess notation)
    for (int i = 7; i >= 0; i--)
    {
        for (int j = 0; j <= 7; j++)
        {
            GameObject obj = positions[j, i];
            if (obj == null)
            {
                sb.Append("Empty "); // Empty square
            }
            else
            {
                PieceScript pieceScript = obj.GetComponent<PieceScript>();
                if (pieceScript != null && pieceScript.myPiece != null)
                {
                    sb.Append(pieceScript.myPiece.name + " "); // Append piece name
                }
                else
                {
                    sb.Append("Unknown "); // Fallback for missing scripts or pieces
                }
            }
        }
        sb.Append("\n"); // Newline for the next row
    }

    // Log the entire board state
    UnityEngine.Debug.Log(sb.ToString());
}

public void UndoAndLog(){
    gamePosition.UndoMove(gamePosition.lastMove);
    gamePosition.ConsoleLogPosition();
}

// Method to initiate the AI move after a delay
    public void DelayedAIMove()
    {
        // Start coroutine to delay the AI move
        StartCoroutine(WaitAndMakeMove(1.0f)); // Delay of 1 second
    }

    private IEnumerator WaitAndMakeMove(float delay)
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delay);

        // Now make the AI move
        gameAI.MakeAIMove();
    }
}
