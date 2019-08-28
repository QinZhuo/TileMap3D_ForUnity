using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Xml;
using System.Xml.Serialization;
[System.Serializable]
public class PrefabTileMapData
{
    public Vector3 mapTileSize ;
    public int mapWidth = 10;
    public int mapLength = 10;
    public List<string> tileBrushList;
    public int[] tileIndexMap;
    public int[] heightMap;
    public List<List<string>> tileObjMap;
    public int[] rotationMap;
    public int[] prefabIndexMap;
    public List<string> prefabBrushList;
}

[CustomEditor(typeof(PrefabTileMap))]
public class PrefabTileMapEditor : Editor
{
    public static string autoSavePath="Assets/Temp";
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
    GUIContent[] tileBrushListGUI;
    GUIContent[] toolsListGUI;
    GUIContent[] heightBrushListGUI;
    GUIContent[] prefabBrushListGUI;
    private void OnEnable()
    {
        map = (PrefabTileMap)target;
        toolsListGUI = new GUIContent[3];
        toolsListGUI[0] = new GUIContent("Tile地板");
        toolsListGUI[1] = new GUIContent("地板调整");
        toolsListGUI[2] = new GUIContent("物体");
        heightBrushListGUI = new GUIContent[3];
        heightBrushListGUI[0] = new GUIContent("提高");
        heightBrushListGUI[1] = new GUIContent("降低");
        heightBrushListGUI[2] = new GUIContent("旋转");
        LoadObjMap();
        map.InitMap();
    }
    private void OnDisable()
    {
        if(editorMode) ExitEditMode();
    }
    private void OnSceneGUI()
    {
        
        //屏蔽镜头控制
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
        return map.transform.position + new Vector3(map.width * map.tileSize.x, 0, map.length * map.tileSize.z) / 2 + lookOffset;
    }
    private Vector3 GetPosition()
    {
      
        var CamOffset = (sceneView.camera.transform.position - map.transform.position);
        var z = map.transform.position.z + map.length / 2;
        var position = new Vector3(Event.current.mousePosition.x, Event.current.mousePosition.y, CamOffset.y);
        var pos= sceneView.camera.ScreenToWorldPoint(position);
        return new Vector3(pos.x,0, z-( pos.z- z)+2*CamOffset.z - map.length);//new Vector3( (int)(pos.x), 0,(int)(CamOffset.z*2 -pos.z))+map.tileSize/2;
    }
   
    private void SelectSave()
    {
        var data = GetData();
        var xml = FileManager.Serialize(data);
        var path= FileManager.SaveSelectPath(xml, "保存地形数据","map","xml",map.savePath==""?"Assets":map.savePath);
        if (System.IO.File.Exists(path))
        {
            map.savePath = path;
        }

    }
    private void Save()
    {
        if (map.savePath == "")
        {
            AutoSave();
        }
        var data = GetData();
        var xml = FileManager.Serialize(data);
        FileManager.Save(map.savePath, xml);
    }
    private void Load()
    {
        var xml = FileManager.Load( map.savePath);
        if (xml == "") return;
        ParseXml(xml);
        FreshBrushListGUI();
    }
    [MenuItem("GameObject/PrefabTileMap/Create TileMap", priority = 0)]
    public static void CreateMap()
    {
        var obj = new GameObject();
        obj.name = "TileMap";
        obj.AddComponent<PrefabTileMap>();
    }
    private PrefabTileMapData GetData()
    {
        var data = new PrefabTileMapData()
        {
            mapTileSize = map.tileSize,
            mapWidth = map.width,
            mapLength = map.length,
            tileBrushList = new List<string>(),
            tileIndexMap = map.map,
            heightMap = map.mapHeight,
            tileObjMap = new List<List<string>>(),
            rotationMap = map.mapRotation,
            prefabIndexMap = map.prefabMap,
            prefabBrushList = new List<string>(),
        };
        for (int i = 0; i < map.brushList.Count; i++)
        {
            if (map.brushList[i] != null)
            {
                data.tileBrushList.Add(AssetDatabase.GetAssetPath(map.brushList[i].GetInstanceID()));
            }
            else
            {
                data.tileBrushList.Add("");
            }
            
        }
        for (int i = 0; i < map.tileObjMap.Length; i++)
        {
            data.tileObjMap.Add( new List<string>());
            for (int j = 0; j <map.tileObjMap[i].Count; j++)
            {
                if (map.tileObjMap[i][j] != null)
                {
                    data.tileObjMap[i].Add(map.tileObjMap[i][j].name);
                }
                else
                {
                    data.tileObjMap[i].Add("");
                }
            }
        }
        for (int i = 0; i < map.prefabBrushList.Count; i++)
        {
            data.prefabBrushList.Add(AssetDatabase.GetAssetPath(map.prefabBrushList[i]));
        }
        return data;
    }
    private void AutoSave()
    {
        var xml = FileManager.Serialize(GetData());
        var path = autoSavePath + "/" + map.GetInstanceID() + "_MapAutoSave.xml";
        map.savePath = FileManager.Save(map.savePath == "" ? path : map.savePath, xml);
    }

    private void LoadObjMap()
    {
        if (!System.IO.File.Exists(map.savePath))
        {
            return;
        }
        if (map.tileObjMap != null)
        {
            return;
        }
    
        var data= FileManager.Deserialize<PrefabTileMapData>(FileManager.Load(map.savePath));
        LoadObjMap(data);

    }
    public void LoadObjMap(PrefabTileMapData data)
    {
        map.tileObjMap = new List<GameObject>[data.tileObjMap.Count];
        for (int i = 0; i < map.tileObjMap.Length; i++)
        {
            map.tileObjMap[i] = new List<GameObject>();
            for (int j = 0; j < data.tileObjMap[i].Count; j++)
            {
                var objId = data.tileObjMap[i][j];
                if (objId != null)
                {
                    if (map.transform.Find(objId) == null)
                    {
                        Debug.LogError("空物体");
                    }
                    else
                    {
                        map.tileObjMap[i].Add(map.transform.Find(objId).gameObject);
                    }
                }
            }
        }
    }
    private void SelectLoad()
    {
      
        var xml = FileManager.LoadSelectPath("读取地形数据", "xml", map.savePath == "" ? "Assets" : map.savePath);
        if (xml == "") return;
        ParseXml(xml);
        FreshBrushListGUI();
    }
    private void ParseXml(string xml)
    {
        var data = FileManager.Deserialize<PrefabTileMapData>(xml);
        map.ResetMap(data.mapWidth, data.mapLength);
        map.brushList.Clear();
        foreach (var b in data.tileBrushList)
        {
            map.brushList.Add(AssetDatabase.LoadAssetAtPath<PrefabTile>(b));
        }
        foreach (var b in data.prefabBrushList)
        {
            map.prefabBrushList.Add(AssetDatabase.LoadAssetAtPath<GameObject>(b));
        }


        for (int i = 0; i < data.tileIndexMap.Length; i++)
        {
            map.DrawTile(i, data.tileIndexMap[i], false, true);
            map.Rotate(i, data.rotationMap[i], false, true);
            map.ChangeHeight(i, data.heightMap[i], false, true);
            map.DrawPrefab(i, data.prefabIndexMap[i], false, true);
        }
    }
    public bool KeyDown(KeyCode key)
    {
        return (Event.current.isKey && Event.current.type == EventType.KeyDown && Event.current.keyCode == key)
     ;
    }
    private void MouseCtr()
    {
      
        if (!editorMode) return;
        Event e = Event.current;
        if (KeyDown(KeyCode.Z))
        {
            map.Undo();
        }
        if (KeyDown(KeyCode.Y))
        {
            map.Redo();
        }

        if (Event.current.isKey && Event.current.type == EventType.KeyDown && Event.current.keyCode==KeyCode.Z)
        {
            map.Undo();
        }
        //Handles.color = Color.red;
        //Handles.DrawWireCube(GetPosition(), map.tileSize);

        var pos =-Vector3.one;
        if (map.is2D)
        {
            pos = map.Fix(GetPosition());
        }
        else
        {
            var hit = new RaycastHit();
            if (Physics.Raycast(
            HandleUtility.GUIPointToWorldRay(
                Event.current.mousePosition), out hit))
            {
                pos = map.Fix(hit.point);
            }

        }


        if (pos != -Vector3.one)
        {
            Handles.color = Color.black;
            Handles.DrawWireCube(map.GetPosition(map.Index(pos),map.mapHeight[map.Index(pos)]),map.tileSize);
        }
       
        
        if (e.type == EventType.MouseDown)
        {
           
            if (pos != -Vector3.one&&e.button == 0)
            {
                MapChange(pos);
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
                MapChange(pos);
            }
            if (e.button==1)
            {
              
                lookOffset += new Vector3(-Event.current.delta.x, 0, Event.current.delta.y) * Time.deltaTime;
            }
            e.Use();
        }
    }
    public void MapChange(Vector3 pos)
    {
    
        switch (map.toolIndex)
        {
            case 0:
                if (Event.current.shift)
                {
                    map.DrawTile(pos, PrefabTileMap.spaceIndex);
                }
                else
                {
                    map.DrawTile(pos, map.tileBrushIndex);
                }
                break;
            case 1:
              
                switch (map.heightBrushIndex)
                {
                    case 0: 
                        if (Event.current.shift)
                        {
                            map.ChangeHeight(pos, -1);
                        }
                        else
                        {
                            map.ChangeHeight(pos, 1);
                        }
                        break;
                    case 1: map.ChangeHeight(pos, -1); break;
                    case 2:map.Rotate(map.Index(pos));break;
                    default:
                        
                        break;
                }
                
                break;
            case 2:
                if (Event.current.shift)
                {
                    map.DrawPrefab(map.Index(pos), PrefabTileMap.spaceIndex);
                }
                else
                {
                    map.DrawPrefab(map.Index(pos), map.prefabBrushIndex);
                }
                break;
            default:
                break;
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
            map.toolIndex = GUILayout.Toolbar(map.toolIndex, toolsListGUI, GUILayout.Height(30));

            switch (map.toolIndex)
            {
                case 0: BurshSetting();To2D(); break;
                case 1: HeightEditor();To3D(); break;
                case 2:PrefabBrushSetting();To3D();break;
                default:
                    break;
            }
            
        }
        
    }
    void EditCtr()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("保存文件", EditorStyles.miniButtonLeft))
        {
            SelectSave();
        }
        if (GUILayout.Button("加载文件", EditorStyles.miniButtonMid))
        {
            SelectLoad();
        }
        if (GUILayout.Button("强制刷新", EditorStyles.miniButtonRight))
        {
            ForceFresh();
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("宽 ["+map.width.ToString()+"]",GUILayout.MaxWidth(60));
        tWidth =EditorGUILayout.IntField(tWidth);
        EditorGUILayout.LabelField("长 [" + map.width.ToString() + "]", GUILayout.MaxWidth(60));
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
                map.ResetMap(map.width, map.length);

            }
        }
        GUILayout.EndHorizontal();
        if (!editorMode) return;


        if (GUILayout.Button("清 空 地 形", EditorStyles.miniButton))
        {
            map.ClearTile();
            map.ClearPrefab();
        }
        GUILayout.BeginHorizontal();
        GUI.enabled = map.CanUndo;
    
        if (GUILayout.Button("撤 销 [Z]", EditorStyles.miniButtonLeft))
        {
            map.Undo();
        }
        GUI.enabled = map.CanRedo;
        if (GUILayout.Button("重 做 [Y]", EditorStyles.miniButtonRight))
        {
            map.Redo();
        }
        GUILayout.EndHorizontal();
        GUI.enabled = true;
    }
    void FreshBrushListGUI()
    {
        tileBrushListGUI = new GUIContent[map.brushList.Count];
        for (int i = 0; i < map.brushList.Count; i++)
        {
            if (map.brushList[i] == null)
            {
                tileBrushListGUI[i] = new GUIContent("丢失Tile笔刷index:"+i);
            }
            else
            {
                var icon = map.brushList[i].GetIcon();

                if (icon != null)
                {
                    tileBrushListGUI[i] = new GUIContent(icon, map.brushList[i].name);
                }
                else
                {
                    tileBrushListGUI[i] = new GUIContent(map.brushList[i].name);
                }
            }
        }
    }
    void FreshPrefabBrushListGUI()
    {
        prefabBrushListGUI = new GUIContent[map.prefabBrushList.Count];
        for (int i = 0; i < map.prefabBrushList.Count; i++)
        {
            if (map.prefabBrushList[i] == null)
            {
                prefabBrushListGUI[i] = new GUIContent("丢失Prefab笔刷index:" + i);
            }
            else
            {
                var icon = AssetPreview.GetAssetPreview(map.prefabBrushList[i]);
                if (icon != null)
                {
                    prefabBrushListGUI[i] = new GUIContent(icon, map.prefabBrushList[i].name);
                }
                else
                {
                    prefabBrushListGUI[i] = new GUIContent(map.prefabBrushList[i].name);
                }
            }
            
           
        }
    }
    private void HeightEditor()
    {
        map.heightBrushIndex = GUILayout.Toolbar(map.heightBrushIndex, heightBrushListGUI, GUILayout.Height(64));
       

    }
    private void PrefabBrushSetting()
    {
        if (GUILayout.Button("添加预制体", EditorStyles.miniButtonLeft, GUILayout.Height(20)))
        {
            EditorGUIUtility.ShowObjectPicker<GameObject>(null, false, "", 3);
        }
        if (prefabBrushListGUI == null)
        {
            FreshPrefabBrushListGUI();
        }
        map.prefabBrushIndex = GUILayout.Toolbar(map.prefabBrushIndex,
            prefabBrushListGUI, GUILayout.Height(64));
        if (Event.current.commandName == "ObjectSelectorClosed")
        {
            if (EditorGUIUtility.GetObjectPickerControlID() == 3)
            {
                var brush = EditorGUIUtility.GetObjectPickerObject() as GameObject;
                if (brush != null && !map.prefabBrushList.Contains(brush))
                {
                    map.AddPrefabBrush(brush);
                    FreshPrefabBrushListGUI();
                }
            }
        }
        if (GUI.changed)
        {
            EditorUtility.SetDirty(this);
        }
        if (map.prefabBrushIndex >= 0)
        {
            if (GUILayout.Button("删除预制体", EditorStyles.miniButton, GUILayout.Height(20)))
            {
                if (EditorUtility.DisplayDialog("删除预制体", "删除预制体同时" +
                    "会更改地图构成" +
                    "且无法撤销之前操作", "删除", "取消"))
                {
                    map.RemovePrefabBrush(map.tileBrushIndex);
                    FreshPrefabBrushListGUI();
                    map.prefabBrushIndex = -1;
                }
            }
        }
    }

    private void BurshSetting()
    {
        
        if (GUILayout.Button("添加笔刷", EditorStyles.miniButtonLeft, GUILayout.Height(20)))
        {
            EditorGUIUtility.ShowObjectPicker<PrefabTile>(null, false, "", 0);
        }
        GUI.color = Color.white;
        if (tileBrushListGUI == null)
        {
            FreshBrushListGUI();
        }

        map.tileBrushIndex = GUILayout.Toolbar(map.tileBrushIndex, tileBrushListGUI, GUILayout.Height(64));
   
        if (Event.current.commandName == "ObjectSelectorClosed")
        {
            if (EditorGUIUtility.GetObjectPickerControlID() == 0)
            {
                var brush = EditorGUIUtility.GetObjectPickerObject() as PrefabTile;
                if (brush != null && !map.brushList.Contains(brush))
                {
                    map.AddBrush(brush);
                    FreshBrushListGUI();
                }
            }
        }
        if (GUI.changed)
        {
            EditorUtility.SetDirty(this);
        }
        if (map.tileBrushIndex >= 0)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("替换笔刷", EditorStyles.miniButtonLeft, GUILayout.Height(20)))
            {
                if (EditorUtility.DisplayDialog("替换笔刷", "替换笔刷" +
                    "无法撤销", "替换", "取消"))
                {
                    EditorGUIUtility.ShowObjectPicker<PrefabTile>(null, false, "", 1);

                }
            }
            if (GUILayout.Button("删除笔刷", EditorStyles.miniButtonRight, GUILayout.Height(20)))
            {
                if (EditorUtility.DisplayDialog("删除笔刷", "删除笔刷同时" +
                    "会更改地图构成" +
                    "且无法撤销之前操作", "删除", "取消"))
                {
                    map.RemoveBrush(map.tileBrushIndex);
                    FreshBrushListGUI();
                    map.tileBrushIndex = -1;
                }
            }
           
            GUILayout.EndHorizontal();
            if (Event.current.commandName == "ObjectSelectorClosed")
            {
                if(EditorGUIUtility.GetObjectPickerControlID() == 1)
                {
                    var brush = EditorGUIUtility.GetObjectPickerObject() as PrefabTile;
                    if (brush != null && !map.brushList.Contains(brush))
                    {
                        map.ChangeBrush(brush);
                        FreshBrushListGUI();
                    }
                }
               

            }
        }
       
    }

    public void ForceFresh()
    {
        Save();
        Load();
    }
    private void EnterEditMode()
    {
        sceneView = SceneView.lastActiveSceneView;
        To2D();
        CheckCollider(true);
    }
    private void To2D()
    {
        if (!map.is2D)
        {
            lastCamType = sceneView.orthographic;
            sceneView.orthographic = true;
            lastCamRoation = sceneView.rotation;
            sceneView.rotation = Quaternion.LookRotation(Vector3.down);
            sceneView.LookAt(LookPosition());
            sceneView.Repaint();
            Tools.current = Tool.None;
            Tools.viewTool = ViewTool.None;
            lookOffset = Vector3.zero;
            map.is2D = true;
        }
       
    }
    private void To3D()
    {
        if (map.is2D)
        {
            sceneView.rotation = lastCamRoation;
            sceneView.orthographic = lastCamType;
            sceneView.Repaint();
            map.is2D = false;
        }
    }
    private void ExitEditMode()
    {
        var sceneView = SceneView.lastActiveSceneView;
        To3D();
        CheckCollider(false);
        AutoSave();
        saveScene();
    }
    private void CheckCollider(bool enable)
    {
        var trs = map.transform.Find("checkCollider");

        BoxCollider cld = null;
        if (trs != null)
        {
            cld= trs.GetComponentInChildren<BoxCollider>(true);
            trs.localPosition = new Vector3(map.tileSize.x * map.width / 2, 0, map.tileSize.z * map.length / 2);
        }
        else
        {
            trs = new GameObject("checkCollider").transform;
            trs.hideFlags = HideFlags.HideInHierarchy;
            trs.SetParent( map.transform);
            trs.localPosition = new Vector3(map.tileSize.x * map.width / 2, 0, map.tileSize.z * map.length / 2);
        }
        if (cld != null)
        {
            cld.size = new Vector3(map.tileSize.x * map.width, 0, map.tileSize.z * map.length);
            if (cld.enabled != enable)
            {
                cld.enabled = enable;
            }
        }
        else
        {
            cld = trs.gameObject.AddComponent<BoxCollider>();
            cld.size = new Vector3(map.tileSize.x * map.width, 0, map.tileSize.z * map.length);

        }
        foreach (var objList in map.tileObjMap)
        {
            foreach (var obj in objList)
            {
                if (obj == null) continue;
                var col = obj.GetComponentInChildren<BoxCollider>(true);
                if (col != null)
                {
                    if (col.enabled == !enable)
                    {
                        col.enabled = enable;
                    }
                }
                else
                {
                    col = obj.AddComponent<BoxCollider>();
                    col.size = map.tileSize;
                    col.enabled = enable;
                }
            }
        }
    }
    private void DrawGrid()
    {
        var position = map.transform.position;
        Handles.color = Color.blue;
        Handles.DrawLine(new Vector3(position.x,0, position.z), new Vector3(map.width*map.tileSize.x + position.x,0, position.z));
       
        Handles.DrawLine(new Vector3(position.x, 0, position.z), new Vector3(position.x,0, map.length * map.tileSize.z + position.z));
        Handles.DrawLine(new Vector3(map.width * map.tileSize.x + position.x, 0, position.z), new Vector3(map.width * map.tileSize.x + position.x, 0, map.length * map.tileSize.z + position.z));
        Handles.DrawLine(new Vector3(position.x, 0, map.length * map.tileSize.z + position.z), new Vector3(map.width * map.tileSize.x + position.x, 0, map.length * map.tileSize.z + position.z));

        if (!editorMode) return;
        Handles.color = Color.white;
        Vector3 start = map.transform.position;
        for (float i = 1; i < map.width; i++)
        {
            Handles.DrawLine(new Vector3(i * map.tileSize.x + start.x,0, start.z), new Vector3(i * map.tileSize.x + start.x,0, map.length * map.tileSize.z + start.z));
        }
        for (float i = 1; i < map.length; i++)
        {
            Handles.DrawLine(new Vector3( start.x, 0, i * map.tileSize.z + start.z), new Vector3(map.width * map.tileSize.x + start.x, 0, i * map.tileSize.z + start.z));
        }


    }
    void saveScene()
    {
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
    }
}
