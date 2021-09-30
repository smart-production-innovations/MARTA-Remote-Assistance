using System;

public class ARPlaneFinderFactory : ComponentFactory<ARPlaneFinder>
{
    public ARPlaneFinderFactory()
    {
#if ARFoundation
        AddType<ARFoundationPlaneFinder>("AR Foundation Default");
#elif ARFoundation2
        AddType<ARFoundation2PlaneFinder>("AR Foundation 2 Default");
#elif ARFoundation3
        AddType<ARFoundation3PlaneFinder>("AR Foundation 3 Default");
#elif ARCore
        AddType<ARCorePlaneFinder>("AR Core Default");
#elif Vuforia
        AddType<VuforiaPlaneFinder>("Vuforia Default");
#endif
        AddType<NullARPlaneFinder>("None");
    }

}

