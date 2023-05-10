using UnityEngine;

internal static class Common
{
    internal static readonly int _MAXLIGHTCOUNT = 8;

    internal static void CheckMaxLightCount(int c, Object o)
    {
        if (c > _MAXLIGHTCOUNT)
            Debug.LogWarning($"You have more than {_MAXLIGHTCOUNT} Buto Lights in scene. Buto will cull all but the nearest {_MAXLIGHTCOUNT} lights, but this will incur additional overhead.", o);
    }

    internal static readonly int _MAXVOLUMECOUNT = 8;
    internal static void CheckMaxFogVolumeCount(int c, Object o)
    {
        if (c > _MAXVOLUMECOUNT)
            Debug.LogWarning($"You have more than {_MAXVOLUMECOUNT} Buto Fog Volumes in scene. Buto will cull all but the nearest {_MAXVOLUMECOUNT} fog volumes, but this will incur additional overhead.", o);
    }
}