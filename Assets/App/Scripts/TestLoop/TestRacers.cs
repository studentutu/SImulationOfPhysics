using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace Unity.IL2CPP.CompilerServices
{
    using System;

    /// <summary>
    ///     The code generation options available for IL to C++ conversion.
    ///     Enable or disabled these with caution.
    /// </summary>
    public enum Option
    {
        /// <summary>
        ///     Enable or disable code generation for null checks.
        ///     Global null check support is enabled by default when il2cpp.exe
        ///     is launched from the Unity editor.
        ///     Disabling this will prevent NullReferenceException exceptions from
        ///     being thrown in generated code. In *most* cases, code that dereferences
        ///     a null pointer will crash then. Sometimes the point where the crash
        ///     happens is later than the location where the null reference check would
        ///     have been emitted though.
        /// </summary>
        NullChecks = 1,

        /// <summary>
        ///     Enable or disable code generation for array bounds checks.
        ///     Global array bounds check support is enabled by default when il2cpp.exe
        ///     is launched from the Unity editor.
        ///     Disabling this will prevent IndexOutOfRangeException exceptions from
        ///     being thrown in generated code. This will allow reading and writing to
        ///     memory outside of the bounds of an array without any runtime checks.
        ///     Disable this check with extreme caution.
        /// </summary>
        ArrayBoundsChecks = 2,

        /// <summary>
        ///     Enable or disable code generation for divide by zero checks.
        ///     Global divide by zero check support is disabled by default when il2cpp.exe
        ///     is launched from the Unity editor.
        ///     Enabling this will cause DivideByZeroException exceptions to be
        ///     thrown in generated code. Most code doesn't need to handle this
        ///     exception, so it is probably safe to leave it disabled.
        /// </summary>
        DivideByZeroChecks = 3
    }

    /// <summary>
    ///     Use this attribute on a class, method, or property to inform the IL2CPP code conversion utility to override the
    ///     global setting for one of a few different runtime checks.
    ///     Example:
    ///     [Il2CppSetOption(Option.NullChecks, false)]
    ///     public static string MethodWithNullChecksDisabled()
    ///     {
    ///     var tmp = new Object();
    ///     return tmp.ToString();
    ///     }
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class Il2CppSetOptionAttribute : Attribute
    {
        public Option Option { get; }
        public object Value { get; }

        public Il2CppSetOptionAttribute(Option option, object value)
        {
            this.Option = option;
            this.Value = value;
        }
    }
}
namespace Scripts.TestLoop
{

    public class SpatialHash<T>
    {
        private Dictionary<int, List<T>> dict;
        private Dictionary<T, int> objects;
        private float cellSize;

        public SpatialHash(float cellSize)
        {
            this.cellSize = cellSize;
            dict = new Dictionary<int, List<T>>();
            objects = new Dictionary<T, int>();
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Insert(Vector3 vector, T obj)
        {
            var key = Key(vector);
            if (dict.ContainsKey(key))
            {
                dict[key].Add(obj);
            }
            else
            {
                dict[key] = new List<T> { obj };
            }
            objects[obj] = key;
        }

        public void UpdatePosition(Vector3 vector, T obj)
        {
            if (objects.ContainsKey(obj))
            {
                dict[objects[obj]].Remove(obj);
            }
            Insert(vector, obj);
        }

        public List<T> QueryPosition(Vector3 vector)
        {
            var key = Key(vector);
            return dict.ContainsKey(key) ? dict[key] : new List<T>();
        }
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(Vector3 vector)
        {
            return dict.ContainsKey(Key(vector));
        }

        public void Clear()
        {
            foreach (var key in dict.Keys)
            {
                dict[key].Clear();
            }
            Reset();
        }
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            dict.Clear();
            objects.Clear();
        }

        private const int BIG_ENOUGH_INT = 16 * 1024;
        private const double BIG_ENOUGH_FLOOR = BIG_ENOUGH_INT + 0.0000;

        private static int FastFloor(float f)
        {
            return (int)(f + BIG_ENOUGH_FLOOR) - BIG_ENOUGH_INT;
        }
        // http://www.beosil.com/download/CollisionDetectionHashing_VMV03.pdf
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Key(Vector3 v)
        {
            return ((FastFloor(v.x / cellSize) * 73856093) ^
                    (FastFloor(v.y / cellSize) * 19349663) ^
                    (FastFloor(v.z / cellSize) * 83492791));
        }
    }
    public class TestRacers : MonoBehaviour
    {


        /// <summary>
        ///  Ideally struct
        /// </summary>
        private class Racer
        {
            // Index of Racer
            private bool isAlive = true; // can be property
            private bool isCollidable = false; // can be property
            public Vector3 Position;
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

        // Instead of saving whole racer - we can use nly his id.
        // ideally - no dictionary required 
        private readonly Dictionary<Racer, bool> racersAndState = new Dictionary<Racer, bool>(NUMBER_OF_RACERS);
        private readonly SpatialHash<Racer> spatialFromRaces = new SpatialHash<Racer>(5f); // size of Races x 2

        // Spatial hashing might be the fastest way
        // it can match O(n^2) in worst case - all racers in the same spatial cell - not desirable
        // best O(n)
        // Also further improvement is to use ECS systems to query for Alive and check their position with hashed Position
        // Problem with ecs - no Dictionary/list or reference typed values.
        private void UpdateRacers(float deltaTimeS, List<Racer> racers)
        {
            // assume dictionary it is empty
            // dictionary can also be placed here ,but it is better when we do not generate garbage 
            // assume Racer is a class and not a struct! (struct behaves differently in Dictionaries inside Unity env.)

            // Fill in the dictionary racersAndState
            int Count = racers.Count;
            bool isAlive = false;
            Racer racer1 = null;
            // each frame clear and fill
            spatialFromRaces.Clear();
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
                    //Racer update takes milliseconds
                    racers[i].Update(deltaTimeMs);

                    // fill in only the alive
                    spatialFromRaces.Insert(racer1.Position, racer1);
                }
            }

            // Solution with Bag Problem ( removing dimension/squashing space) :
            // To further improve it  - we can use a sort O(n*log(n)) on the racing line - make a line from the racer ( position +- with/height)
            //  (0 is start + ( position +- with/height) - 100 finishing line) ( make it 1 dimensional)
            // traverse sorted array of rectangles and check with the min distance if any of them collides with the  - O(n) 
            // this way we can achieve O(n*log(n)) . 
            // See https://www.geeksforgeeks.org/top-algorithms-and-data-structures-for-competitive-programming/#algo1

            // Solution with Octree :  
            // Requires contant update on Octree
            // Check each racer in with all of the racers from his Octant and it's neighbor's Octant  O (n * (8 + 6 + 1) ) === O(n)

            // Spatial hashing might be the fastest way

            // Collides
            // instead of whole Racer - we can use index of Racer
            Racer racer2 = null;
            int counter2 = 0;
            for (int racerIndex1 = 0; racerIndex1 < Count; racerIndex1++)
            {
                racer1 = racers[racerIndex1];

                var listOthers = spatialFromRaces.QueryPosition(racer1.Position);
                counter2 = listOthers.Count;

                // maybe skip  if we are already processed - need to test
                for (int racerIndex2 = 0; racerIndex2 < counter2; racerIndex2++)
                {
                    racer2 = racers[racerIndex2];
                    // main issue here - CollidesWith() - the less it is called the better
                    if (racer1.IsCollidable() && racer2.IsCollidable() && racer1.CollidesWith(racer2))
                    {
                        OnRacerExplodes(racer1);
                        // OnRacerExplodes(racer2); // need to test if it has the same behavior
                        racersAndState[racer1] = false;
                        racersAndState[racer2] = false;
                    }
                }

                // Solution with simpler loop
                // O(n^2) --> O( n *(n -1)/2)
                // already less than halfed of what it was.

                // additionally first check if the racer1 is dead - will also help a little but may change the behavior
                // other racers may not explode even when they appear to collide with it
                // if (racersAndState[racer1]) //need to test if it is the same behavior
                // also check if racer1.IsCollidable() - it can be not collidable when Destroyed
                // {
                //     for (int racerIndex2 = racerIndex1; racerIndex2 < Count; racerIndex2++)
                //     {
                //         racer2 = racers[racerIndex2];

                //         // main issue here - CollidesWith() - the less it is called the better
                //         if (racer1.IsCollidable() && racer2.IsCollidable() && racer1.CollidesWith(racer2))
                //         {
                //             OnRacerExplodes(racer1);
                //             // OnRacerExplodes(racer2); // need to test if it is the same behavior
                //             racersAndState[racer1] = false;
                //             racersAndState[racer2] = false;
                //         }
                //     }
                // }
            }

            // Gets the racers that are still alive
            // Get rid of all the exploded racers
            // GC can spike( 
            racers.Clear();

            // Builds the list of remaining racers
            // foreach is not ideal, but will be sufficient 
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
            // Garbage(  Ideally if this would not needed if it was implemented on a stack 
            racersAndState.Clear();

            // Another solution - 
            // With the help of Octree - Each Racer internally will keep track of the cars nearby
            // this way there will be no need in the unified dictionary of any sort and we would not 
            // need to worry about the garbage genereted. Just simple method - check if collides.
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