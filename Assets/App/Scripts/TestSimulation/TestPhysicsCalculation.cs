using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Scripts.Async;
using Unity.IL2CPP.CompilerServices;
using System.Runtime.CompilerServices;
using UnityEngine.SceneManagement;

namespace Scripts.TestSimulation
{

    public class TestPhysicsCalculation : MonoBehaviour
    {
        private const int INTERATIONS = 10000;
        private const float EPSILON = 0.0005f;
        private const float SMALLEST_TIME_STEP = 0.00001f;
        private const float INITIAL_DIFFERENCE_WITH_HEIGHT = 1f;

        [SerializeField] private float width = 20f;
        [SerializeField] private float guessHeight = 2f;
        [SerializeField] private Vector2 velocity = default;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private Vector2 point = default;
        [SerializeField] private Transform objectPoint = default;
        [SerializeField] private Transform leftBorder = default;
        [SerializeField] private Transform rightBorder = default;
        [SerializeField] private Transform @base = default;
        [SerializeField] private Transform guessHeightObject = default;

        [Space, Header("Tests")]
        [SerializeField] private Rigidbody2D prefab = default;
        [SerializeField] private Transform controlMeasure = default;
        [SerializeField] private TMP_Text text = default;
        [SerializeField] private LineRenderer lineRendererReal = default;
        [SerializeField] private LineRenderer lineRendererCalc = default;

        [SerializeField] private bool test1 = false;
        [SerializeField] private bool testAngle = false;
        [SerializeField] private bool testReal = false;

        private Vector3 AsUp = Vector3.up;
        private Vector3 AsRight = Vector3.right;
        private Vector3 AsForward = Vector3.forward;

        private Vector2 Point
        {
            get
            {
                return new Vector2(objectPoint.position.x, objectPoint.position.y);
            }
        }

        private void Awake()
        {
            ThreadTools.Initialize();
            lineRendererReal.useWorldSpace = true;
            lineRendererCalc.useWorldSpace = true;
        }

        private void OnValidate()
        {
            if (objectPoint != null)
            {
                objectPoint.position = new Vector3(point.x, point.y, 0);
            }

            if (rightBorder != null && leftBorder != null && @base != null && guessHeightObject != null)
            {
                @base.position = AsRight * width / 2;
                @base.localScale = new Vector3(width, @base.localScale.y, @base.localScale.z);
                guessHeightObject.localScale = new Vector3(width, guessHeightObject.localScale.y, guessHeightObject.localScale.z);
                guessHeightObject.position = @base.position + AsUp * guessHeight;

                rightBorder.position = leftBorder.position + AsRight * width;
            }

            if (test1)
            {
                test1 = false;
                ThreadTools.StartCoroutine(delayStart(() => TryCalcalate()));
            }

            if (testAngle)
            {
                testAngle = false;
                ThreadTools.StartCoroutine(delayStart(() => TestAngle()));
            }

            if (testReal)
            {
                testReal = false;
                ThreadTools.StartCoroutine(delayStart(() => TestCalculateRealPos()));
            }
        }

        private void OnDrawGizmos()
        {
            if (objectPoint != null)
            {
                var angle = getAngle(velocity);
                var rotateNormal = angle * AsUp;
                Gizmos.color = Color.green;
                Gizmos.DrawLine(objectPoint.position, objectPoint.position + rotateNormal * 10f);
            }
        }

        private void TryCalcalate()
        {
            float xPos = -1;
            bool isSuccess = TryCalculateXPositionAtHeight(guessHeight, Point,
                                                velocity, gravity, width, ref xPos);
            if (isSuccess)
            {
                controlMeasure.position = new Vector3(xPos, 0, 0);
                text.text = controlMeasure.position.x.ToString("F4");
            }
            Debug.LogWarning($" bool {isSuccess} pos.x {xPos.ToString("F4")} ");
        }

        private IEnumerator delayStart(System.Action action)
        {
            yield return null;
            action?.Invoke();
        }

        private void TestAngle()
        {
            Rigidbody2D rb2D = Instantiate(prefab, objectPoint.position, Quaternion.identity);
            rb2D.AddForce(velocity.normalized * velocity.magnitude, ForceMode2D.Impulse);
            rb2D.gameObject.SafeDestroy(3f);
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TestCalculateRealPos()
        {
            Rigidbody2D gameobjectNew = Instantiate(prefab, objectPoint.position, Quaternion.identity);
            gameobjectNew.AddForce(velocity.normalized * velocity.magnitude, ForceMode2D.Impulse);

            float iterations = INTERATIONS;
            bool isFinished = false;
            float timeDelta = Time.fixedDeltaTime;
            float maxDifferenceWithHeight = INITIAL_DIFFERENCE_WITH_HEIGHT;
            float smallestStep = SMALLEST_TIME_STEP;

            Vector3 previouspos = gameobjectNew.transform.position;
            Vector3 nextPosition = previouspos;
            bool reset = false;
            float closeToHeight;

            bool previousAutoSimulation = Physics2D.autoSimulation;
            Physics2D.autoSimulation = false;

            lineRendererReal.positionCount = 0;
            List<Vector3> allPoints = new List<Vector3>();

            for (int i = 0; i < iterations && !isFinished; i++)
            {
                // Reset
                if (reset)
                {
                    reset = false;
                    maxDifferenceWithHeight = maxDifferenceWithHeight * 0.5f;
                    timeDelta *= 0.5f;
                    timeDelta = Mathf.Max(timeDelta, smallestStep);
                }
                closeToHeight = guessHeight + maxDifferenceWithHeight;

                // Simulate
                Physics2D.Simulate(timeDelta);
                nextPosition = gameobjectNew.transform.position;
                allPoints.Add(nextPosition);

                // Check
                if (Mathf.Abs(nextPosition.y - guessHeight) <= EPSILON)
                {
                    controlMeasure.position = new Vector3(nextPosition.x, 0, 0);
                    text.text = controlMeasure.position.x.ToString("F4");
                    isFinished = true;
                }

                // Optimize search. Make Sure to not overkill the range
                if (!isFinished && (closeToHeight < nextPosition.y && closeToHeight > previouspos.y)
                    || (closeToHeight > nextPosition.y && closeToHeight < previouspos.y))
                {
                    reset = true;
                }

                // Saving state
                previouspos = gameobjectNew.transform.position;
            }

            lineRendererReal.positionCount = allPoints.Count;
            lineRendererReal.SetPositions(allPoints.ToArray());

            Physics2D.autoSimulation = previousAutoSimulation;
            gameobjectNew.gameObject.SafeDestroy();
        }

        /// <summary>
        /// Get angle from Vector3.Up to Point
        /// </summary>
        /// <param name="p2"> Velocity </param>
        /// <returns></returns>
        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Quaternion getAngle(Vector2 p2)
        {
            // var angle = Mathf.Atan2(p2.y - p1.y, p2.x - p1.x) * 180 / Mathf.PI;
            var angle = Math3d.AngleBetweenVector2(AsUp, p2);
            return Quaternion.AngleAxis(angle, AsForward);
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CalculateViaPhysicsScene()
        {

            Scene _scene = SceneManager.CreateScene("Procedural Scene");
            // _scene. // add object in here and start the TestCalculateRealPos
            PhysicsScene physicsScene = _scene.GetPhysicsScene();
        }

        [Il2CppSetOption(Option.NullChecks, false)]
        [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
        [Il2CppSetOption(Option.DivideByZeroChecks, false)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Still is not correct calculation!
        public bool TryCalculateXPositionAtHeight(float h, Vector2 p, Vector2 v, float G, float w, ref float xPosition)
        {
            float timeDelta = Time.fixedDeltaTime;
            float iterations = INTERATIONS;
            bool isFinished = false;

            Vector2 gravityForce = Vector2.up * G;
            Vector2 leftNormal = Vector2.right;
            Vector2 rightNormal = -Vector2.right;
            Vector2 bottomNormal = Vector2.up;
            Vector2 inNormal;
            bool LeftBorder = false;
            bool bottom = false;

            Vector2 newPos = p;
            Vector2 previousPos = newPos;
            Vector2 forceAdditive = v;
            Vector2 finalVelocity;
            Vector2 newPos2;

            bool reset = false;
            float maxDifferenceWithHeight = INITIAL_DIFFERENCE_WITH_HEIGHT;
            float smallestStep = SMALLEST_TIME_STEP;
            float closeToHeight;
            Vector2 gravityVelocity = Vector2.zero;

            lineRendererCalc.positionCount = 0;
            List<Vector3> allPoints = new List<Vector3>();

            for (int i = 0; i < iterations && !isFinished; i++)
            {
                // Reset
                if (reset)
                {
                    reset = false;
                    maxDifferenceWithHeight = maxDifferenceWithHeight * 0.5f;
                    timeDelta *= 0.5f;
                    timeDelta = Mathf.Max(timeDelta, smallestStep);
                }
                closeToHeight = h + maxDifferenceWithHeight;

                // Simulate
                // constant additive force
                gravityVelocity = gravityForce * timeDelta;
                forceAdditive += gravityVelocity;
                // Position equation
                finalVelocity = forceAdditive * timeDelta;
                newPos2 = newPos + finalVelocity;

                // check if we collide with borders
                LeftBorder = newPos2.x <= 0;
                bottom = newPos2.y <= 0;
                if (LeftBorder || bottom || newPos2.x >= w)
                {
                    if (bottom)
                    {
                        inNormal = bottomNormal;
                    }
                    else
                    {
                        inNormal = LeftBorder ? leftNormal : rightNormal;
                    }

                    forceAdditive = Vector2.Reflect(forceAdditive, inNormal);
                    // Position equation
                }

                finalVelocity = (forceAdditive + gravityVelocity) * timeDelta;
                newPos += finalVelocity;
                allPoints.Add(newPos);

                // Check
                if (Mathf.Abs(newPos.y - h) <= EPSILON)
                {
                    xPosition = newPos.x;
                    isFinished = true;
                }

                // Optimize search. Make Sure to not overkill the range
                if (!isFinished && ((closeToHeight < newPos.y && closeToHeight > previousPos.y) ||
                                    (closeToHeight > newPos.y && closeToHeight < previousPos.y)))
                {
                    reset = true;
                }

                // Saving state
                previousPos = newPos;
            }

            lineRendererCalc.positionCount = allPoints.Count;
            lineRendererCalc.SetPositions(allPoints.ToArray());

            return isFinished;
        }
    }
}