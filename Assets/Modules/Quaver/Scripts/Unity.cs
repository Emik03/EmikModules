using System.Collections;
using UnityEngine;

public class Unity : MonoBehaviour
{
    public Unity Instance;

    /// <summary>
    /// Clones the object original and returns the clone.
    /// </summary>
    /// <param name="original">An existing object that you want to make a copy of.</param>
    /// <param name="parent">Parent that will be assigned to the new object.</param>
    /// <param name="instantiateInWorldSpace">When you assign a parent Object, pass true to position the new object directly in world space. Pass false to set the Object’s position relative to its new parent.</param>
    /// <returns>Object: The instantiated clone.</returns>
    internal static Object Instantiate(GameObject original, Transform parent, bool instantiateInWorldSpace)
    {
        return Object.Instantiate(original, parent, instantiateInWorldSpace);
    }

    /// <summary>
    /// Starts a Coroutine.
    /// </summary>
    /// <param name="routine">The coroutine to use.</param>
    /// <returns>Returns upon the first yield return, however you can yield the result, which waits until the coroutine has finished execution.</returns>
    internal new Coroutine StartCoroutine(IEnumerator routine)
    {
        return Instance.StartCoroutine(routine);
    }
}
