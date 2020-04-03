using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.TestLoop
{
    public class TestRacers : MonoBehaviour
    {
        private class Racer
        {
            private bool isAlive = true; // can be property
            private bool isCollidable = false; // can be property

            public void IsNotAlive()
            {
                isAlive = false;
            }

            public bool IsAlive()
            {
                return isAlive;
            }
            public void Update(float deltaTime)
            {

            }
            public bool IsCollidable()
            {
                return isCollidable;
            }
            public bool CollidesWith(Racer another)
            {
                return false;
            }
            public void Destroy()
            {

            }
        }

        private void OnRacerExplodes(Racer who)
        {
            // explode
            who.IsNotAlive();
        }

        private const int NUMBER_OF_RACERS = 1000;
        private Dictionary<Racer, bool> racersAndState = new Dictionary<Racer, bool>(NUMBER_OF_RACERS);

        private void UpdateRacers(float deltaTimeS, List<Racer> racers)
        {
            // assume dictionary it is empty
            // dictionary can also be placed here ,but it is better when we do not generate garbage 
            // assume Racer is a class and not a struct! (struct behaves differently in Dictionaries inside Unity env.)

            // Fill in the dictionary racersAndState
            int Count = racers.Count;
            Racer racer1 = null;
            bool isAlive = false;
            // Racer update takes milliseconds
            float deltaTimeMs = deltaTimeS * 1000.0f;
            for (int i = 0; i < Count; i++)
            {
                racer1 = racers[i];
                isAlive = racer1.IsAlive();
                racersAndState.Add(racer1, isAlive);
                // Updates the racers that are alive
                if (isAlive)
                {
                    // Racers update (is it needed here?) it can be placed see below 
                    // that way there is potentially less racers to update at a cost of an initial (start) one frame lag
                    //Racer update takes milliseconds
                    racers[i].Update(deltaTimeMs);
                }
            }


            // O(n^2) --> O( n *(n -1)/2)
            // already less than halfed of what it was.

            // Solution with Bag Problem ( removing dimension/squashing space) :
            // To further improve it  - we can use a sort O(n*log(n)) on the racing line - make a line from the racer ( position +- with/height)
            //  (0 is start + ( position +- with/height) - 100 finishing line) ( make it 1 dimensional)
            // traverse sorted array of rectangles and check with the min distance if any of them collides with the  - O(n) 
            // this way we can achieve O(n*log(n)) .

            // Solution with Octree :  
            // Requires contant update on Octree
            // Check each racer in with all of the racers from his Octant and it's neighbor's Octant  O (n * (8 + 6 + 1) ) === O(n)

            // Also there are other advanced techniques to speed up the calculation such as Triangulation and such.

            // Collides
            Racer racer2 = null;
            for (int racerIndex1 = 0; racerIndex1 < Count; racerIndex1++)
            {
                for (int racerIndex2 = racerIndex1; racerIndex2 < Count; racerIndex2++)
                {
                    racer1 = racers[racerIndex1];
                    racer2 = racers[racerIndex2];
                    // additionally first check if the racer1 is dead - will also help a little but may change the behaviour
                    // other racers may not explode even when they appear to collide with it
                    // racersNeedingRemoved[racer1] === true

                    // main issue here - CollidesWith() - the less it is called the better
                    if (racer1.IsCollidable() && racer2.IsCollidable() && racer1.CollidesWith(racer2))
                    {
                        OnRacerExplodes(racer1);
                        //   OnRacerExplodes(racer2); // need to test
                        racersAndState[racer1] = false;
                        racersAndState[racer2] = false;
                    }
                }
            }

            // Gets the racers that are still alive
            // Get rid of all the exploded racers
            racers.Clear();

            // Builds the list of remaining racers
            foreach (var item in racersAndState.Keys)
            {
                if (racersAndState[item])
                {
                    racers.Add(item);
                    // Racers update can be placed here - only alive one will be updated
                    // racers.Update(deltaTimeMs);
                }
            }

            // clear up the memory
            racersAndState.Clear();
        }

        /// <summary>
        /// Update only alive racers
        /// </summary>
        /// <param name="deltaTimeS"> delta Time in seconds</param>
        /// <param name="racers"> All racers</param>
        // [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void UpdateAliveRacers(float deltaTimeS, List<Racer> racers)
        {
            //Racer update takes milliseconds
            float deltaTimeMs = deltaTimeS * 1000.0f;
            // Updates the racers that are alive
            foreach (var item in racersAndState.Keys)
            {
                // is alive
                if (racersAndState[item])
                {
                    //Racer update takes milliseconds
                    item.Update(deltaTimeMs);
                }
            }
        }
    }
}