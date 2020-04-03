using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtensions
{
    public static void SafeDestroy(this GameObject gameObject, float timeUntilDestoy = 0)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            GameObject.DestroyImmediate(gameObject);
        }
        else
#endif
        {
            GameObject.Destroy(gameObject, timeUntilDestoy);
        }
    }
}
