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

    private static Dictionary<MoveDirection, System.Func<Move, bool, (int, int)>> actualDictionary;
    private static Dictionary<MoveDirection, System.Func<Move, bool, (int, int)>> ActualDictionary
    {
        get
        {
            if (actualDictionary == null)
            {
                actualDictionary = new Dictionary<MoveDirection, System.Func<Move, bool, (int, int)>>()
                {
                    {MoveDirection.Down, (input, reverse) => { return (input.x, input.y + (reverse?1: -1));}},
                    {MoveDirection.Up, (input,reverse) => { return (input.x, input.y + (reverse? -1: 1));}},
                    {MoveDirection.Left, (input,reverse) => { return (input.x + (reverse?1: -1), input.y);}},
                    {MoveDirection.Right, (input,reverse) => { return (input.x + (reverse? -1: 1), input.y);}},
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
        return GetBestMove();
    }

    // [Il2CppSetOption(Option.NullChecks, false)]
    // [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    // [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
                var resultFromRecursive = RecursiveCalls(ref referenced, new Move(j, i, MoveDirection.Down));
                if (max < resultFromRecursive.Item1)
                {
                    result = resultFromRecursive.Item2;
                    max = resultFromRecursive.Item1;
                }

                referenced = 0;
                resultFromRecursive = RecursiveCalls(ref referenced, new Move(j, i, MoveDirection.Left));
                if (max < resultFromRecursive.Item1)
                {
                    result = resultFromRecursive.Item2;
                    max = resultFromRecursive.Item1;
                }

                referenced = 0;
                resultFromRecursive = RecursiveCalls(ref referenced, new Move(j, i, MoveDirection.Right));
                if (max < resultFromRecursive.Item1)
                {
                    result = resultFromRecursive.Item2;
                    max = resultFromRecursive.Item1;
                }

                referenced = 0;
                resultFromRecursive = RecursiveCalls(ref referenced, new Move(j, i, MoveDirection.Up));
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
        if (CheckIfInRawOrVertical(fromMove, ref actualPoints)) // will add to the ref value, will add new move if true
        {
            Vector2Int Nextposition = default;
            int max = actualPoints;
            int nextValue = actualPoints;
            Move result = default;

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

            // undo move
            RemoveMove(fromMove);
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
        if (GetPosition(checkMove, false, out Nextposition))
        {
            // Check
            int resultToAdd = 0;
            var previousPosition = new Vector2Int(checkMove.x, checkMove.y);
            var previousJewel = GetJewel(checkMove.x, checkMove.y);
            var nextSwappedJewel = GetJewel(Nextposition.x, Nextposition.y);
            resultToAdd += AddPoints(Nextposition, previousJewel);
            resultToAdd += AddPoints(previousPosition, nextSwappedJewel);
            oldPoint += resultToAdd;
            AddMove(checkMove);
            return true;
        }
        return false;
    }

    private void AddMove(Move moveToAdd)
    {
        Vector2Int Nextposition = default;
        if (GetPosition(moveToAdd, false, out Nextposition))
        {
            var previousPosition = new Vector2Int(moveToAdd.x, moveToAdd.y);
            var previousJewel = GetJewel(moveToAdd.x, moveToAdd.y);
            var nextSwappedJewel = GetJewel(Nextposition.x, Nextposition.y);
            // swap
            var any = nextSwappedJewel;
            actualBoard[Nextposition.y][Nextposition.x] = previousJewel;
            actualBoard[previousPosition.y][previousPosition.x] = any;
        }
    }

    private void RemoveMove(Move moveToUndo)
    {
        Vector2Int Nextposition = default;
        if (GetPosition(moveToUndo, true, out Nextposition))
        {
            var previousPosition = new Vector2Int(moveToUndo.x, moveToUndo.y);
            var previousJewel = GetJewel(moveToUndo.x, moveToUndo.y);
            var nextSwappedJewel = GetJewel(Nextposition.x, Nextposition.y);
            // swap
            var any = nextSwappedJewel;
            actualBoard[Nextposition.y][Nextposition.x] = previousJewel;
            actualBoard[previousPosition.y][previousPosition.x] = any;
        }
    }

    private int AddPoints(Vector2Int positionFromCheck, JewelKind toCheck)
    {
        int resultTotal = 0;

        int checkColumn = positionFromCheck.y;
        bool end = false;
        while (!end)
        {
            end = true;
            if (checkColumn < 0) continue;

            if (GetJewel(positionFromCheck.x, checkColumn - 1) == toCheck)
            {
                end = false;
                checkColumn--;
                resultTotal++;
            }
        }

        end = false;
        checkColumn = positionFromCheck.y;
        while (!end)
        {
            end = true;
            if (checkColumn >= height) continue;

            if (GetJewel(positionFromCheck.x, checkColumn + 1) == toCheck)
            {
                end = false;
                checkColumn++;
                resultTotal++;
            }
        }

        int checkRaws = positionFromCheck.x;
        end = false;
        while (!end)
        {
            end = true;
            if (checkColumn >= width) continue;

            if (GetJewel(positionFromCheck.x, checkRaws + 1) == toCheck)
            {
                end = false;
                checkRaws++;
                resultTotal++;
            }
        }

        checkRaws = positionFromCheck.x;
        end = false;
        while (!end)
        {
            end = true;
            if (checkRaws < 0) continue;

            if (GetJewel(positionFromCheck.x, checkRaws - 1) == toCheck)
            {
                end = false;
                checkRaws--;
                resultTotal++;
            }
        }
        return resultTotal;
    }

    private bool GetPosition(Move checkMove, bool reverse, out Vector2Int position)
    {
        var tuple = ActualDictionary[checkMove.direction](checkMove, reverse);
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