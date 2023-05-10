using System.Collections.Generic;
using UnityEditor;

public class FogDensityMaskEditor : EditorWindow
{
    private static string objectName = "Buto Fog Density Mask";
    private static List<System.Type> types = new List<System.Type>()
        {
            typeof(FogDensityMask)
        };

    [MenuItem("GameObject/Effects/Buto Fog Density Mask")]
    public static void CreateFogDensityMask()
    {
        EditorCommon.CreateChildAndSelect(objectName, types);
    }
}
