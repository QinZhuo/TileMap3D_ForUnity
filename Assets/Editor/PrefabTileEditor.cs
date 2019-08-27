using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(PrefabTile))]
public class PrefabTileEditor : Editor
{
    [SerializeField]
    private PrefabTile tile;
    [SerializeField]
    private string iconPath;
    private void OnEnable()
    {
        tile = (PrefabTile)target;
    }
   
    public override void OnInspectorGUI()
    {
        GUILayout.Space(10);
        EditorGUILayout.HelpBox("选择对应的预制体(Prefab)来配置Tile", MessageType.Info);
        GUILayout.Space(10);
        GUILayout.Label("Tile预制体：",EditorStyles.boldLabel);
        if (!tile.IsValid)
        {
            GUI.color = Color.grey;
        }
        if (GUILayout.Button(GUIContent.none,EditorStyles.helpBox, GUILayout.Width(100), GUILayout.Height(100)))
        {
            EditorGUIUtility.ShowObjectPicker<GameObject>(tile.prefab, false, "", 1);
            
        }


        Texture2D texture = tile.IsValid ? tile.GetIcon() : new Texture2D(16, 16);
        if (texture != null)
        {
            GUI.DrawTexture(GUILayoutUtility.GetLastRect(), texture);
        }
        
        
        if (!tile.IsValid)
        {
            GUI.Label(GUILayoutUtility.GetLastRect(), "无预制体", EditorStyles.centeredGreyMiniLabel);
        }
        
        if (Event.current.commandName == "ObjectSelectorClosed")
        {
            tile.prefab = EditorGUIUtility.GetObjectPickerObject() as GameObject;
        //    Debug.LogError("changePrefab" + tile.prefab);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(tile);
        }
    }
}


