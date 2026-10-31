using HarmonyLib;
using Keen.Game2.Client.UI.Library;
using Keen.VRage.Core;
using Keen.VRage.Library.Utils;
using Keen.VRage.UI.EngineComponents;

namespace RSSG.ClientPlugin;

// Small adapter around the game's shared UI service, following the SE2 client-plugin template.
internal static class GameAccess
{
    public static SharedUIComponent GetSharedUI()
    {
        var engine = Singleton<VRageCore>.Instance?.Engine;
        if (engine == null)
            return null;

        var gameAppType = AccessTools.TypeByName("Keen.Game2.GameAppComponent");
        if (gameAppType == null)
            return null;

        var getMethod = AccessTools.FirstMethod(engine.GetType(),
            method => method.Name == "Get" && method.IsGenericMethodDefinition && method.GetGenericArguments().Length == 1);
        var gameApp = getMethod?.MakeGenericMethod(gameAppType).Invoke(engine, new object[] { default(StringId) });
        if (gameApp == null)
            return null;

        return AccessTools.Method(gameAppType, "GetSharedUI")?.Invoke(gameApp, null) as SharedUIComponent;
    }
}
