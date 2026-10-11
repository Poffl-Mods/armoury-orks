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
            ChoppaProgressionController.EnsureController();
            modification.OnSetEnabled += enabled =>
            {
                PowerKlawFistAnimations.Enabled = enabled;
                if (enabled) ChoppaProgressionController.EnsureController();
            };
            Debug.Log("[ArmouryOrks] Runtime initialized.");
        }
    }
}
