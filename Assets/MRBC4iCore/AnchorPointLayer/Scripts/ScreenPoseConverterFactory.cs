using System;


/// <summary>
/// This component-factory can be used to select and create a specific pose converter.
/// </summary>
public class ScreenPoseConverterFactory : ComponentFactory<ScreenPoseConverter>
{
    public ScreenPoseConverterFactory()
    {
#if ARFoundation
        AddType<ARFoundationScreenPoseConverter>("AR Foundation Default");
#elif ARFoundation2
        AddType<ARFoundation2ScreenPoseConverter>("AR Foundation 2 Default");
#elif ARFoundation3
        AddType<ARFoundation3ScreenPoseConverter>("AR Foundation 3 Default");
#elif ARCore
        AddType<ARCoreScreenPoseConverter>("AR Core Default");
#elif Vuforia
        AddType<VuforiaScreenPoseConverter>("Vuforia Default");
#endif
        AddType<BasicScreenPoseConverter>("Basic");
    }

}

