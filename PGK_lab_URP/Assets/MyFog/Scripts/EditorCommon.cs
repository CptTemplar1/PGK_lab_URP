using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

internal static class EditorCommon
{
    internal static GameObject CreateChildAndSelect(string name, List<System.Type> types)
    {
        GameObject go = new GameObject(name, types.ToArray());
        go.transform.parent = Selection.activeTransform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        Selection.activeGameObject = go;
        return go;
    }
}
