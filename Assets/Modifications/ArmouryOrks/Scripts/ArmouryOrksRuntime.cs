using Kingmaker.Modding;
using UnityEngine;

namespace ArmouryOrks.Runtime
{
    public static class ArmouryOrksRuntime
    {
        [OwlcatModificationEnterPoint]
        public static void Initialize(OwlcatModification modification)
        {
            Debug.Log("[ArmouryOrks] Runtime initialized.");
        }
    }
}
