using System;
using UnityEngine;

namespace ArmouryOrks.Runtime
{
    /// <summary>
    /// Rolls only the Big Choppa mesh around its own shaft while it is attached to a back slot.
    /// EquipmentOffsets continues to control the position and overall orientation of the prefab.
    /// </summary>
    public sealed class BigChoppaHolsterController : MonoBehaviour
    {
        private static readonly Quaternion HolsteredMeshRoll =
            Quaternion.AngleAxis(90f, Vector3.right);

        private Transform m_Model;
        private Quaternion m_DrawnRotation;

        private void Awake()
        {
            Transform visual = transform.Find("BigChoppa_Visual");
            m_Model = visual != null ? visual.Find("BigChoppa_FBX_Model") : null;
            if (m_Model != null) m_DrawnRotation = m_Model.localRotation;
        }

        private void OnEnable()
        {
            ApplyPose();
        }

        private void LateUpdate()
        {
            ApplyPose();
        }

        private void OnDisable()
        {
            if (m_Model != null) m_Model.localRotation = m_DrawnRotation;
        }

        private void ApplyPose()
        {
            if (m_Model == null) return;
            Transform parent = transform.parent;
            bool holstered = parent != null && parent.name.IndexOf(
                "back_weapon_slot", StringComparison.OrdinalIgnoreCase) >= 0;
            m_Model.localRotation = holstered
                ? m_DrawnRotation * HolsteredMeshRoll
                : m_DrawnRotation;
        }
    }
}
