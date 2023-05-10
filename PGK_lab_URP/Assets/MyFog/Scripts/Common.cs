using UnityEngine;
using UnityEngine.UI;

internal static class Common
{
    internal static readonly int _MAXVOLUMECOUNT = 8;
    internal static void CheckMaxFogVolumeCount(int c, Object o)
    {
        if (c > _MAXVOLUMECOUNT)
            Debug.LogWarning($"Masz wiêcej ni¿ {_MAXVOLUMECOUNT} FogVolume na scenie. Wszystkie FogVolume oprócz najbli¿szych {_MAXVOLUMECOUNT} zostan¹ usnuniête.", o);
    }
}
