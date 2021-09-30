using UnityEngine;
using System.Collections;

/// <summary>
/// helper class for debugging infinity loops on mobile devices
/// </summary>
public static class DebugHelper
{
    /// <summary>
    /// is the debugging of infinite loops active?
    /// </summary>
    private static bool debugLoopMessagesActive = false;

    /// <summary>
    /// Add an infinity loop debugging entry
    /// </summary>
    /// <param name="message">debug message</param>
    public static void InfinityLoopLog(string message)
    {
        if (debugLoopMessagesActive)
            Debug.Log("[LoopTest]" + message);
    }
}
