using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PCBuildSequence))]
public class PCBuildSequenceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PCBuildSequence seq = (PCBuildSequence)target;

        GUILayout.Space(10);
        if (GUILayout.Button("🔄 Reset Sequence"))
        {
            seq.ResetSequence();
        }

        if (GUILayout.Button("➡️ Next Step"))
        {
            seq.NextStep();
        }
    }
}


