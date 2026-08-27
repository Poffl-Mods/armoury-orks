using UnityEngine;

namespace ArmouryOrks.Runtime
{
    public sealed class OrkWeaponMaterialBinder : MonoBehaviour
    {
        private void Awake() => BindGameShader();
        private void OnEnable() => BindGameShader();

        private void BindGameShader()
        {
            Shader shader = Shader.Find("Owlcat/Lit");
            if (shader == null) return;

            foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
            {
                Material[] materials = renderer.materials;
                bool changed = false;
                foreach (Material material in materials)
                {
                    if (material == null || material.shader == shader) continue;
                    material.shader = shader;
                    changed = true;
                }
                if (changed) renderer.materials = materials;
            }
        }
    }
}
