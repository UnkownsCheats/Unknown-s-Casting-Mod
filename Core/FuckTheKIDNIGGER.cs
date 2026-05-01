using HarmonyLib;

namespace Stats.Core
{
    public class FuckTheKIDNIGGER
    {
        [HarmonyPatch(typeof(PrivateUIRoom), "StartOverlay")]
        public class StartOverlayNIGGER
        {
            private static bool Postfix()
            {
                return false;
            }
        }
        
        public static void OnEnable()
        {
            if (ispatched) return;
            if (HarmonyInstance == null)
            {
                HarmonyInstance = new Harmony("com.UnknownCasting.Devs");
            }
            HarmonyInstance.PatchAll();
            ispatched = true;
        }
        
        static Harmony HarmonyInstance;
        static bool ispatched = false;
    }
}