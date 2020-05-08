using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Board;

namespace Tests
{
    public class JewelBoardTests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void JewelBoardTestsSimplePasses()
        {
            // Use the Assert class to test conditions
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator JewelBoardTestsWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
            var actualBoard = new JewelKind[][]
            {
                new JewelKind[]{JewelKind.Blue, JewelKind.Green, JewelKind.Blue},
                new JewelKind[]{JewelKind.Green, JewelKind.Blue, JewelKind.Green},
                new JewelKind[]{JewelKind.Indigo, JewelKind.Red, JewelKind.Yellow},
            };

            var newBoard = new Board(actualBoard);
            var bestMove = new Move(0, 1, MoveDirection.Down);
            var AlternativebestMove = new Move(1, 1, MoveDirection.Up);

            var Move = newBoard.CalculateBestMoveForBoard();
            Debug.LogWarning(" Result : " + newBoard.CalculateBestMoveForBoard());
            bool isEqual = Move.IsTheSame(bestMove) || Move.IsTheSame(AlternativebestMove);
            Assert.AreEqual(isEqual, false, " Move is bad");
        }
    }
}
