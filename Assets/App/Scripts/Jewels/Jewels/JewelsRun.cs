using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JewelsRun : MonoBehaviour
{
    public void Update()
    {

    }

    public Board.Move GetBestMove(Board actualBoard)
    {

        return actualBoard.CalculateBestMoveForBoard();
    }
}
