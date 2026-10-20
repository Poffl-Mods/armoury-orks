using Kingmaker.Modding;
using UnityEngine;

namespace ArmouryOrks.Runtime
{
    public static class ArmouryOrksRuntime
    {
        [OwlcatModificationEnterPoint]
        public static void Initialize(OwlcatModification modification)
        {
            PowerKlawFistAnimations.Initialize();
            ChainChoppaCombat.Initialize();
            ChoppaProgressionController.EnsureController();
            OrkArmouryCacheController.EnsureController();
            modification.OnSetEnabled += enabled =>
            {
                PowerKlawFistAnimations.Enabled = enabled;
                ChainChoppaCombat.Enabled = enabled;
                OrkArmouryCacheController.Enabled = enabled;
                if (enabled)
                {
                    ChoppaProgressionController.EnsureController();
                    OrkArmouryCacheController.EnsureController();
                }
            };
            Debug.Log("[ArmouryOrks] Runtime initialized.");
        }
    }
}
