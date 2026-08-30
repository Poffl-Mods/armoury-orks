using System;
using UnityEngine;

namespace ArmouryOrks.Runtime
{
    /// <summary>Keeps the Power Klaw on the hand, but hides it in ordinary weapon holsters.</summary>
    public sealed class PowerKlawVisibilityController : MonoBehaviour
    {
        private Transform m_Visual;
        private float m_NextCheck;

        private void Awake() => m_Visual = transform.Find("PowerKlaw_Visual");

        private void Update()
        {
            if (m_Visual == null || Time.unscaledTime < m_NextCheck) return;
            m_NextCheck = Time.unscaledTime + 0.1f;
            bool holstered = false;
            for (Transform current = transform.parent; current != null; current = current.parent)
            {
                string name = current.name ?? string.Empty;
                if (name.IndexOf("back_weapon_slot", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    name.IndexOf("belt", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    holstered = true;
                    break;
                }
            }
            if (m_Visual.gameObject.activeSelf == holstered)
                m_Visual.gameObject.SetActive(!holstered);
        }
    }
}
