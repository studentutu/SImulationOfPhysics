using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Board
{
    public enum JewelKind
    {
        Empty,
        Red,
        Orange,
        Yellow,
        Green,
        Blue,
        Indigo,
        Violet
    };

    public enum MoveDirection
    {
        Up,
        Down,
        Left,
        Right
    };

    public struct Move
    {
        public int x;
        public int y;
        public MoveDirection direction;
        public Move(int x, int y, MoveDirection direction)
        {
            this.x = x;
            this.y = y;
            this.direction = direction;
        }

        public bool IsTheSame(Move moveToBeChecked)
        {
            return x == moveToBeChecked.x && y == moveToBeChecked.y && direction == moveToBeChecked.direction;
        }
    };
    private int width;
    private int height;
    private JewelKind[][] actualBoard;

    public Board(JewelKind[][] newBoard)
    {
        actualBoard = newBoard;
        height = newBoard.Length;
        width = newBoard[0].Length;
    }

    int GetWidth()
    {
        return width;
    }
    int GetHeight()
    {
        return height;
    }

    JewelKind GetJewel(int x, int y)
    {
        return actualBoard[y][x];
    }
    void SetJewel(int x, int y, JewelKind kind)
    {
        actualBoard[y][x] = kind;
    }

    //Implement this function
    // recursion
    public Move CalculateBestMoveForBoard()
    {

        return default;
    }


};