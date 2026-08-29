using Kingmaker.Modding;
using UnityEngine;

namespace ArmouryOrks.Runtime
{
    public static class ArmouryOrksRuntime
    {
        [OwlcatModificationEnterPoint]
        public static void Initialize(OwlcatModification modification)
        {
            ChoppaProgressionController.EnsureController();
            modification.OnSetEnabled += enabled =>
            {
                if (enabled) ChoppaProgressionController.EnsureController();
            };
            Debug.Log("[ArmouryOrks] Runtime initialized.");
        }
    }
}
