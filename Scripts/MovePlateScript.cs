using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MovePlateScript : MonoBehaviour {
    //References 
    int matrixX;
    int matrixY;
    int matrixZ;
    public Move myMove = null;
    public Vector3 scaleChange = new Vector3(0.1f, 0.1f, 0.0f); //Scale plates up when they are behind pieces(They are hard to see)

    //true if target square contains enemy piece, false if regular move to empty square
    public bool isAttack = false;

    public void SetCoordinates(string colorInFront) {
      float x = matrixX;
      float y = matrixY;
      if (colorInFront == "black") {
      x = 7-matrixX;
      y = 7-matrixY;
      }
      matrixZ = 9;
      
      x *= 1.92f; //Find transform for center of square
      y *= 1.92f;
      x += -6.7f;
      y += -6.06f;

      this.transform.position = new Vector3(x, y, matrixZ); //Put z layer at original Y +0.5 so its above the board but below the pieces
    }
      
    public void SetCoordinates(int x, int y){
      matrixX = x;
      matrixY = y;
      matrixZ = y;
    }
    public int GetMatrixX(){
      return matrixX;
    }
    public int GetMatrixY(){
      return matrixY;
    }

}
