using GorillaNetworking;

namespace Stats.Core
{
    public class AntiAFK
    {
        public static void AntiAFKKick()
        {
            PhotonNetworkController.Instance.disableAFKKick = true;
        }
    }
}