using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(PrefabTileMap))]
public class PrefabTileMapEditor : Editor
{
    private PrefabTileMap map;
    private bool editorMode=false;
    private Rect sceneEditWindowRect;
    int tWidth;
    int tHeight;
    Vector3 tTileSize;
    SceneView sceneView;
    Vector3 lookOffset = Vector3.zero;
    bool lastCamType;
    Quaternion lastCamRoation;
    GUIContent[] brushListGUI;
    private void OnEnable()
    {
        map = (PrefabTileMap)target;
        
        map.InitMap();
    }
    private void OnDisable()
    {
        if(editorMode) ExitEditMode();
    }
    private void OnSceneGUI()
    {
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        if (editorMode)
        {
            sceneView.LookAt(LookPosition());
        }
        DrawGrid();
        MouseCtr();
        if (sceneView)
        {
            sceneView.Repaint();
        }
    }
    
    private Vector3 LookPosition()
    {
        return map.transform.position + new Vector3(map.Width * map.tileSize.x, 0, map.Height * map.tileSize.z) / 2 + lookOffset;
    }
    private Vector3 GetPosition()
    {
      
        var CamOffset = (sceneView.camera.transform.position - map.transform.position);
        var z = map.transform.position.z + map.Height / 2;
        var position = new Vector3(Event.current.mousePosition.x, Event.current.mousePosition.y, CamOffset.y);
        var pos= sceneView.camera.ScreenToWorldPoint(position);
        return new Vector3(pos.x,0, z-( pos.z- z)+2*CamOffset.z - map.Height);//new Vector3( (int)(pos.x), 0,(int)(CamOffset.z*2 -pos.z))+map.tileSize/2;
    }
    private void MouseCtr()
    {
        if (!editorMode) return;
        Event e = Event.current;

        Handles.color = Color.red;
        Handles.DrawWireCube(GetPosition(), map.tileSize);

        var pos =map.Fix(GetPosition());
        
        if (pos != -Vector3.one)
        {
            Handles.color = Color.black;
            Handles.DrawWireCube(pos,map.tileSize);
        }
       
        
        if (e.type == EventType.MouseDown)
        {
            if (pos != -Vector3.one&&e.button == 0)
            {
                map.Draw(pos, map.brushIndex);
            
            }
            e.Use();
        }
        if (e.type == EventType.MouseUp)
        {
            if ( e.button == 0)
            {
                map.ChangeOver();
            }
            e.Use();
        }
        if (e.type == EventType.MouseDrag)
        {
            if (e.button == 0&&pos != -Vector3.one)
            {
                map.Draw(GetPosition(), map.brushIndex);
            }
            if (e.button==1)
            {
              
                lookOffset += new Vector3(-Event.current.delta.x, 0, Event.current.delta.y) * Time.deltaTime;
            }
            e.Use();
        }
    }
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        string text = (!editorMode ? "打 开" : "关 闭") + " 编 辑 模 式 ";
        editorMode = GUILayout.Toggle(editorMode, text, EditorStyles.miniButton, GUILayout.Height(30));
        if (EditorGUI.EndChangeCheck())
        {
            if (editorMode)
            {

                EnterEditMode();
            }
            else
            {
                ExitEditMode();
            }
        }
        EditCtr();
        if (editorMode)
        {
            BurshSetting();
        }
        
    }
    void EditCtr()
    {
        
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("宽 ["+map.Width.ToString()+"]",GUILayout.MaxWidth(60));
        tWidth =EditorGUILayout.IntField(tWidth);
        EditorGUILayout.LabelField("高 [" + map.Width.ToString() + "]", GUILayout.MaxWidth(60));
        tHeight = EditorGUILayout.IntField(tHeight);
        if (GUILayout.Button("更 改 地 图 大 小", EditorStyles.miniButton,GUILayout.MinWidth(100)))
        {
            if(EditorUtility.DisplayDialog("确定更改么", "更改地图大小会丢失多余数据", "更改", "取消"))
            {
                map.ResetMap(tWidth, tHeight);
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        tTileSize = EditorGUILayout.Vector3Field("网 格 大 小 "+map.tileSize, tTileSize);
        if (GUILayout.Button("更 改 网 格 大 小", EditorStyles.miniButton, GUILayout.MinWidth(100)))
        {
            if (EditorUtility.DisplayDialog("确定更改么", "更改网格大小需要对应大小的笔刷", "更改", "取消"))
            {
                map.tileSize = tTileSize;
               
            }
        }
        GUILayout.EndHorizontal();
        if (!editorMode) return;


        if (GUILayout.Button("清 空 地 形", EditorStyles.miniButton))
        {
            map.ClearAll();
        }
        GUILayout.BeginHorizontal();
        GUI.enabled = map.CanUndo;
        if (GUILayout.Button("撤 销", EditorStyles.miniButtonLeft))
        {
            map.Undo();
        }
        GUI.enabled = map.CanRedo;
        if (GUILayout.Button("重 做", EditorStyles.miniButtonRight))
        {
            map.Redo();
        }
        GUILayout.EndHorizontal();
        GUI.enabled = true;
    }
    void FreshBrushListGUI()
    {
        brushListGUI = new GUIContent[map.brushList.Count];
        for (int i = 0; i < map.brushList.Count; i++)
        {
            brushListGUI[i] = new GUIContent(GetIcon(map.brushList[i]), map.brushList[i].name);
        }
    }
    private Texture2D GetIcon(PrefabTile tile)
    {
        if (tile.icon == null)
        {
            tile.icon = AssetPreview.GetAssetPreview(tile.prefab);
        }
        return tile.icon;
    }
    private void BurshSetting()
    {

        if (GUILayout.Button("添加笔刷", EditorStyles.miniButtonLeft, GUILayout.Height(20)))
        {
            EditorGUIUtility.ShowObjectPicker<PrefabTile>(null, false, "", 0);
        }
        GUI.color = Color.white;
        if (brushListGUI == null)
        {
            FreshBrushListGUI();
        }
        map.brushIndex = GUILayout.Toolbar(map.brushIndex, brushListGUI, GUILayout.Height(64));
   
        if (Event.current.commandName == "ObjectSelectorClosed")
        {
            var brush = EditorGUIUtility.GetObjectPickerObject() as PrefabTile;
            if(brush!=null&&!map.brushList.Contains(brush)){
                map.AddBrush(brush);
                FreshBrushListGUI();
            }
          
        }
        if (GUI.changed)
        {
            EditorUtility.SetDirty(this);
        }
        if (map.brushIndex >= 0)
        {
            if (GUILayout.Button("删除笔刷", EditorStyles.miniButton, GUILayout.Height(20)))
            {
                map.RemoveBrush(map.brushIndex);
                FreshBrushListGUI();
                map.brushIndex = -1;
            }
        }
       
    }

    private void EnterEditMode()
    {
        sceneView = SceneView.lastActiveSceneView;
        lastCamType = sceneView.orthographic;
        sceneView.orthographic = true;
        lastCamRoation = sceneView.rotation;
        sceneView.rotation = Quaternion.LookRotation(Vector3.down);
        lookOffset = Vector3.zero;
        sceneView.LookAt(LookPosition());
        sceneView.Repaint();
        Tools.current = Tool.None;
        Tools.viewTool = ViewTool.None;
    }
    private void ExitEditMode()
    {
        var sceneView = SceneView.lastActiveSceneView;
        sceneView.rotation = lastCamRoation;
        sceneView.orthographic = lastCamType;
        sceneView.Repaint();
        
        saveScene();
    }
   
    private void DrawGrid()
    {
        var position = map.transform.position;
        Handles.color = Color.blue;
        Handles.DrawLine(new Vector3(position.x,0, position.z), new Vector3(map.Width*map.tileSize.x + position.x,0, position.z));
       
        Handles.DrawLine(new Vector3(position.x, 0, position.z), new Vector3(position.x,0, map.Height * map.tileSize.z + position.z));
        Handles.DrawLine(new Vector3(map.Width * map.tileSize.x + position.x, 0, position.z), new Vector3(map.Width * map.tileSize.x + position.x, 0, map.Height * map.tileSize.z + position.z));
        Handles.DrawLine(new Vector3(position.x, 0, map.Height * map.tileSize.z + position.z), new Vector3(map.Width * map.tileSize.x + position.x, 0, map.Height * map.tileSize.z + position.z));

        if (!editorMode) return;
        Handles.color = Color.white;
        Vector3 start = map.transform.position;
        for (float i = 1; i < map.Width; i++)
        {
            Handles.DrawLine(new Vector3(i * map.tileSize.x + start.x,0, start.z), new Vector3(i * map.tileSize.x + start.x,0, map.Height * map.tileSize.z + start.z));
        }
        for (float i = 1; i < map.Height; i++)
        {
            Handles.DrawLine(new Vector3( start.x, 0, i * map.tileSize.z + start.z), new Vector3(map.Width * map.tileSize.x + start.x, 0, i * map.tileSize.z + start.z));
        }


    }
    void saveScene()
    {
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
    }
}
