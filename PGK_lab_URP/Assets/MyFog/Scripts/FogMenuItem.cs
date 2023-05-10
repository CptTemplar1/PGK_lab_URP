using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class FogMenuItem : EditorWindow
{
    private static string objectName = "Fog Volume";
    private static List<System.Type> types = new List<System.Type>()
        {
            typeof(Volume)
        };

    [MenuItem("GameObject/Volume/Fog Volume")]
    public static void CreateFogVolume()
    {
        EditorCommon.CreateChildAndSelect(objectName, types);
    }
}