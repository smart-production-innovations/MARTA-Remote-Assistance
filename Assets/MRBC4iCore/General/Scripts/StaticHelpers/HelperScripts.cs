using UnityEngine;

/// <summary>
/// Extends the game object functionality
/// </summary>
public static class HelperScripts
{
    /// <summary>
    /// Delete all game objects which are located in the hierarchy tree under the given game object
    /// </summary>
    /// <param name="transform">parent game object</param>
    public static void DeleteAllChildren(this Transform transform)
    {
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
}
