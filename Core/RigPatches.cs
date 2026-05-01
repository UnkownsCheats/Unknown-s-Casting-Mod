using HarmonyLib;
using GorillaNetworking;

namespace iiMenu.Patches.Menu
{
    [HarmonyPatch]
    public static class RigPatches
    {
        [HarmonyPatch(typeof(VRRig), "OnDisable")]
        [HarmonyPrefix]
        static void OnDisable(ref VRRig __instance)
        {
            if (__instance == GorillaTagger.Instance?.offlineVRRig)
            {
                return;
            }
        }

        [HarmonyPatch(typeof(VRRig), "Awake")]
        [HarmonyPrefix]
        static void Awake(ref VRRig __instance)
        {
            if (__instance == GorillaTagger.Instance?.offlineVRRig)
            {
                return;
            }
        }

        [HarmonyPatch(typeof(VRRig), "PostTick")]
        [HarmonyPrefix]
        static void PostTick(ref VRRig __instance)
        {
            if (__instance == GorillaTagger.Instance?.offlineVRRig)
            {
                return;
            }
        }
    }
}
