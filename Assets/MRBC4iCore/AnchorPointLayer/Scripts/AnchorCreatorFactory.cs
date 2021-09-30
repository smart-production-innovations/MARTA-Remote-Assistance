using System;

public class AnchorCreatorFactory : ComponentFactory<AnchorCreator>
{
    public AnchorCreatorFactory()
    {
#if ARCore
        AddType<ARCoreAnchorCreator>("AR Core Default");
#elif ARFoundation
        AddType<ARFoundationAnchorCreator>("AR Foundation Default");
#elif ARFoundation2
        AddType<ARFoundation2AnchorCreator>("AR Foundation 2 Default");
#elif ARFoundation3
        AddType<ARFoundation3AnchorCreator>("AR Foundation 3 Default");
#endif
        AddType<NullAnchorCreator>("None");
    }

}

