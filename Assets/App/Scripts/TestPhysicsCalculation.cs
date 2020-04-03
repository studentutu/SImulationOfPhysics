using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Scripts.Async;

public class TestPhysicsCalculation : MonoBehaviour
{
    private const int INTERATIONS = 10000;
    private const float EPSILON = 0.0005f;

    [SerializeField] private float width = 20f;
    [SerializeField] private float guessHeight = 2f;
    [SerializeField] private Vector2 velocity = default;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private Vector2 point = default;
    [SerializeField] private Transform objectPoint = default;
    [SerializeField] private Transform leftBorder = default;
    [SerializeField] private Transform RightBorder = default;
    [SerializeField] private Transform Base = default;
    [SerializeField] private Transform GuessHeightObject = default;

    [Space]
    [SerializeField] private Rigidbody2D prefab = null;
    [SerializeField] private Transform controlMeasure = default;
    [SerializeField] private TMPro.TMP_Text text = null;

    [SerializeField] private bool test1 = false;
    [SerializeField] private bool testAngle = false;
    [SerializeField] private bool testReal = false;

    [SerializeField] private LineRenderer lineRendererReal = default;
    [SerializeField] private LineRenderer lineRendererCalc = default;


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
        if (RightBorder != null && leftBorder != null && Base != null && GuessHeightObject != null)
        {

            Base.position = AsRight * width / 2;
            Base.localScale = new Vector3(width, Base.localScale.y, Base.localScale.z);
            GuessHeightObject.localScale = new Vector3(width, GuessHeightObject.localScale.y, GuessHeightObject.localScale.z);
            GuessHeightObject.position = Base.position + AsUp * guessHeight;

            RightBorder.position = leftBorder.position + AsRight * width;
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
            ThreadTools.StartCoroutine(delayStart(() => TestRealPos()));
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

    private void OnDrawGizmos()
    {
        if (objectPoint != null)
        {
            var angle = getAngle(velocity);
            var rotaterNormal = angle * AsUp;
            Gizmos.color = Color.green;
            // Vector3 normalied =
            Gizmos.DrawLine(objectPoint.position, objectPoint.position + rotaterNormal * 10f);
        }
    }

    public void TestAngle()
    {
        Rigidbody2D gameobjectNew = Instantiate(prefab, objectPoint.position, Quaternion.identity);
        gameobjectNew.AddForce(velocity.normalized * velocity.magnitude, ForceMode2D.Impulse);
        gameobjectNew.gameObject.SafeDestroy(3f);
    }

    public void TestRealPos()
    {
        Rigidbody2D gameobjectNew = Instantiate(prefab, objectPoint.position, Quaternion.identity);
        gameobjectNew.AddForce(velocity.normalized * velocity.magnitude, ForceMode2D.Impulse);
        float iterations = INTERATIONS;
        bool isFinished = false;
        float timeDelta = Time.fixedDeltaTime;
        bool previous = Physics2D.autoSimulation;
        Physics2D.autoSimulation = false;
        Vector3 previouspos = gameobjectNew.transform.position;
        Vector3 nextPosition = previouspos;
        bool reset = false;
        float closeT0 = 0.5f;
        float closeWithHeight = guessHeight;
        float smallestStep = 0.0001f;

        lineRendererReal.positionCount = 0;
        List<Vector3> allPoints = new List<Vector3>();
        for (int i = 0; i < iterations && !isFinished; i++)
        {
            // Reset
            if (reset)
            {
                reset = false;
                closeT0 = closeT0 * 0.5f;
                timeDelta *= 0.5f;
                timeDelta = Mathf.Max(timeDelta, smallestStep);
            }
            closeWithHeight = guessHeight + closeT0;

            // Simulate
            Physics2D.Simulate(timeDelta);
            nextPosition = gameobjectNew.transform.position;
            allPoints.Add(nextPosition);

            Debug.LogWarning(gameobjectNew.transform.position.y + " vel " + gameobjectNew.velocity.ToString("F4"));
            // Check
            if (Mathf.Abs(nextPosition.y - guessHeight) <= EPSILON)
            {
                controlMeasure.position = new Vector3(nextPosition.x, 0, 0);
                text.text = controlMeasure.position.x.ToString("F4");
                isFinished = true;
            }

            // Optimize search. Make Sure to not overkill the range
            if (!isFinished && (closeWithHeight < nextPosition.y && closeWithHeight > previouspos.y)
                || (closeWithHeight > nextPosition.y && closeWithHeight < previouspos.y))
            {
                reset = true;
            }

            // Saving state
            previouspos = gameobjectNew.transform.position;
        }
        lineRendererReal.positionCount = allPoints.Count;
        lineRendererReal.SetPositions(allPoints.ToArray());

        Physics2D.autoSimulation = previous;
        Debug.LogWarning(" if finished " + isFinished);
        gameobjectNew.gameObject.SafeDestroy();
    }

    /// <summary>
    /// Get angle from Vector3.Up to Point
    /// </summary>
    /// <param name="p2"> Velocity </param>
    /// <returns></returns>
    private Quaternion getAngle(Vector2 p2)
    {
        // var angle = Mathf.Atan2(p2.y - p1.y, p2.x - p1.x) * 180 / Mathf.PI;
        var angle = Math3d.AngleBetweenVector2(AsUp, p2);
        return Quaternion.AngleAxis(angle, AsForward);
    }


    public bool TryCalculateXPositionAtHeight(float h, Vector2 p, Vector2 v, float G, float w, ref float xPosition)
    {
        float timeDelta = Time.fixedDeltaTime;
        float iterations = INTERATIONS;
        Vector2 newPos = p;
        Vector2 gravityForce = Vector2.up * G;
        bool LeftBorder = false;
        bool bottom = false;

        Vector2 leftNormal = Vector2.right;
        Vector2 rightNotmal = -Vector2.right;
        Vector2 bottomNormal = Vector2.up;

        Vector2 inNormal;
        Vector2 previouspos = newPos;
        bool isFinished = false;
        Vector2 allForces = v;
        Vector2 finalForce = allForces;

        bool reset = false;
        float closeT0 = 0.5f;
        float closeWithHeight = h;
        float smallestStep = 0.001f;

        lineRendererCalc.positionCount = 0;
        List<Vector3> allPoints = new List<Vector3>();
        for (int i = 0; i < iterations && !isFinished; i++)
        {
            // Reset
            if (reset)
            {
                reset = false;
                closeT0 = closeT0 * 0.5f;
                timeDelta *= 0.5f;
                timeDelta = Mathf.Max(timeDelta, smallestStep);
            }
            closeWithHeight = h + closeT0;

            // Simulate
            newPos += (allForces + 0.5f * gravityForce * timeDelta) * timeDelta;
            allPoints.Add(newPos);

            // check if we collide with borders
            LeftBorder = newPos.x <= 0;
            bottom = newPos.y <= 0;
            if (LeftBorder || bottom || newPos.x >= w)
            {
                if (bottom)
                {
                    inNormal = bottomNormal;
                }
                else
                {
                    inNormal = LeftBorder ? leftNormal : rightNotmal;
                }
                allForces = Vector2.Reflect(allForces, inNormal);
            }

            // safe if we will go back
            Debug.LogWarning(newPos.ToString("F4"));
            // Check
            if (Mathf.Abs(newPos.y - h) <= EPSILON)
            {
                xPosition = newPos.x;
                isFinished = true;
            }

            // Optimize search. Make Sure to not overkill the range
            if (!isFinished && ((closeWithHeight < newPos.y && closeWithHeight > previouspos.y) ||
                                (closeWithHeight > newPos.y && closeWithHeight < previouspos.y)))
            {
                reset = true;
            }

            // Saving state
            previouspos = newPos;
        }
        lineRendererCalc.positionCount = allPoints.Count;
        lineRendererCalc.SetPositions(allPoints.ToArray());
        return isFinished;
    }
}
