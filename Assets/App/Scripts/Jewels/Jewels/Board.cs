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

    private static Dictionary<MoveDirection, System.Func<Move, (int, int)>> actualDictionary;
    private static Dictionary<MoveDirection, System.Func<Move, (int, int)>> ActualDictionary
    {
        get
        {
            if (actualDictionary == null)
            {
                actualDictionary = new Dictionary<MoveDirection, System.Func<Move, (int, int)>>()
                {
                    {MoveDirection.Down, (input) => { return (input.x, input.y -1);}},
                    {MoveDirection.Up, (input) => { return (input.x, input.y +1);}},
                    {MoveDirection.Left, (input) => { return (input.x -1, input.y);}},
                    {MoveDirection.Right, (input) => { return (input.x+1, input.y);}},
                };
            }
            return actualDictionary;
        }
    }


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
        var result = GetBestMove();
        return result;
    }

    private Move GetBestMove()
    {
        Move result = default;
        int max = 0;
        int referenced = 0;
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                referenced = 0;
                var resultFromRecursive = RecursiveCalls(ref referenced, new Move(i, j, MoveDirection.Down));
                if (max < resultFromRecursive.Item1)
                {
                    result = resultFromRecursive.Item2;
                    max = resultFromRecursive.Item1;
                }

                referenced = 0;
                resultFromRecursive = RecursiveCalls(ref referenced, new Move(i, j, MoveDirection.Left));
                if (max < resultFromRecursive.Item1)
                {
                    result = resultFromRecursive.Item2;
                    max = resultFromRecursive.Item1;
                }

                referenced = 0;
                resultFromRecursive = RecursiveCalls(ref referenced, new Move(i, j, MoveDirection.Right));
                if (max < resultFromRecursive.Item1)
                {
                    result = resultFromRecursive.Item2;
                    max = resultFromRecursive.Item1;
                }

                referenced = 0;
                resultFromRecursive = RecursiveCalls(ref referenced, new Move(i, j, MoveDirection.Up));
                if (max < resultFromRecursive.Item1)
                {
                    result = resultFromRecursive.Item2;
                    max = resultFromRecursive.Item1;
                }
            }
        }
        return result;
    }

    private (int, Move) RecursiveCalls(ref int actualPoints, Move fromMove)
    {
        if (CheckIfInRawOrVertical(fromMove, ref actualPoints)) // will add to the ref value
        {
            Vector2Int Nextposition = default;
            int max = actualPoints;
            int nextValue = actualPoints;
            Move result = default;
            if (GetPosition(fromMove, out Nextposition))
            {
                // Check
                nextValue = actualPoints;
                var check = RecursiveCalls(ref nextValue, new Move(Nextposition.x, Nextposition.y, MoveDirection.Down));
                if (max < check.Item1)
                {
                    result = check.Item2;
                    max = check.Item1;
                }
                // Check
                nextValue = actualPoints;
                check = RecursiveCalls(ref nextValue, new Move(Nextposition.x, Nextposition.y, MoveDirection.Left));
                if (max < check.Item1)
                {
                    result = check.Item2;
                    max = check.Item1;
                }
                // Check
                nextValue = actualPoints;
                check = RecursiveCalls(ref nextValue, new Move(Nextposition.x, Nextposition.y, MoveDirection.Right));
                if (max < check.Item1)
                {
                    result = check.Item2;
                    max = check.Item1;
                }
                // Check
                nextValue = actualPoints;
                check = RecursiveCalls(ref nextValue, new Move(Nextposition.x, Nextposition.y, MoveDirection.Up));
                if (max < check.Item1)
                {
                    result = check.Item2;
                    max = check.Item1;
                }
            }
            return (max, result);
        }
        else
        {
            return (0, default);
        }
    }

    private bool CheckIfInRawOrVertical(Move checkMove, ref int oldPoint)
    {
        Vector2Int Nextposition = default;
        if (GetPosition(checkMove, out Nextposition))
        {
            // Check
            int resultToAdd = 0;
            for (int i = Nextposition.y; i < height; i++)
            {

            }
        }
        return false;
    }

    private bool GetPosition(Move checkMove, out Vector2Int position)
    {
        var tuple = ActualDictionary[checkMove.direction](checkMove);
        position = new Vector2Int(tuple.Item1, tuple.Item2);
        if (position.x <= 0 || position.x >= width)
        {
            return false;
        }

        if (position.y <= 0 || position.y >= height)
        {
            return false;
        }

        return true;
    }

};