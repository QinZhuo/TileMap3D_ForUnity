using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="PrefabTileMap/NewTile",fileName ="newTile")]
public class PrefabTile : ScriptableObject
{
    public GameObject prefab;
    public bool autoChange;
    private Texture2D icon;
    public GameObject[] changeTile;
    public bool IsValid
    {
        get
        {
            return prefab != null;
        }
    }
#if UNITY_EDITOR
    public Texture2D GetIcon()
    {
        if (icon == null)
        {
             icon=UnityEditor.AssetPreview.GetAssetPreview(prefab);
        }
        return icon;
    }
#endif
}
