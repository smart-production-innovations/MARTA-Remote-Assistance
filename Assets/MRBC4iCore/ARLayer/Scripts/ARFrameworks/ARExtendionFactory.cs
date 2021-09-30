using UnityEngine;
using System.Collections;

/// <summary>
/// Select the correct preparation class to get access to the AR features
/// </summary>
public class ARExtendionFactory : ComponentFactory<ARExtensionHelper>
{
    public ARExtendionFactory()
    {
#if ARFoundation2
        AddType<ARFoundation2ExtensionHelper>("AR Foundation 2 Default");
#elif ARFoundation3
        AddType<ARFoundation3ExtensionHelper>("AR Foundation 3 Default");
#endif
        AddType<ARExtensionHelper>("None");
    }
}
