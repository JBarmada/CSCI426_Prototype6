#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom Inspector for AttackSystem that adds play-mode debug buttons.
/// All buttons are no-ops in Edit mode.
/// </summary>
[CustomEditor(typeof(AttackSystem))]
public class AttackSystemDebugEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("── Debug (Play Mode Only) ──", EditorStyles.boldLabel);

        bool isPlaying = Application.isPlaying;

        using (new EditorGUI.DisabledScope(!isPlaying))
        {
            EditorGUILayout.LabelField("Select Attack", EditorStyles.miniBoldLabel);

            if (GUILayout.Button("Select Bite"))
                AttackSystem.Instance?.SelectAttack(AttackType.Bite);

            if (GUILayout.Button("Select Stab"))
                AttackSystem.Instance?.SelectAttack(AttackType.Stab);

            if (GUILayout.Button("Select Zap"))
                AttackSystem.Instance?.SelectAttack(AttackType.Zap);

            if (GUILayout.Button("Select Poison"))
                AttackSystem.Instance?.SelectAttack(AttackType.Poison);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Execute", EditorStyles.miniBoldLabel);

            if (GUILayout.Button("Execute Attack"))
            {
                int dmg = AttackSystem.Instance?.ExecuteAttack() ?? 0;
                Debug.Log($"[AttackSystemDebug] ExecuteAttack → {dmg} damage");
            }

            if (GUILayout.Button("Roll Meat Drop"))
            {
                int meat = AttackSystem.Instance?.RollMeatDrop() ?? 0;
                Debug.Log($"[AttackSystemDebug] RollMeatDrop → {meat} pieces");
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Register Meat (+10)", EditorStyles.miniBoldLabel);

            foreach (AttackType type in System.Enum.GetValues(typeof(AttackType)))
            {
                if (GUILayout.Button($"Register 10 {type} Meat"))
                    AttackUpgradeSystem.Instance?.RegisterMeat(type, 10);
            }
        }
    }
}
#endif
