using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
/// <summary>
/// 地图更改信息
/// </summary>
public class MapChange
{
    public ChangeType type = ChangeType.index;
    public int pos=-1;
    public int index1=0;
    public int index2=0;
}
public enum ChangeType
{
    index,
    height,
    rotate,
    prefab,
}
public class PrefabTileMap : MonoBehaviour
{
    public string savePath = "";
    /// <summary>
    /// 瓦片长宽高
    /// </summary>
    public Vector3 tileSize=new Vector3(2,0.5f,2);
    /// <summary>
    /// 宽度
    /// </summary>
    public int width=10;
    /// <summary>
    /// 长度
    /// </summary>
    public int length=10;
    /// <summary>
    /// 笔刷索引数
    /// </summary>
    public int tileBrushIndex = -1;
    /// <summary>
    /// 地图信息
    /// </summary>
    public int[] map;
    /// <summary>
    /// 地图高度信息
    /// </summary>
    public int[] mapHeight;
    /// <summary>
    /// 游戏对象记录
    /// </summary>
    public List<GameObject>[] tileObjMap;
    public GameObject[] prefabObjMap;
    public int[] mapRotation;
    /// <summary>
    /// 空位置索引数
    /// </summary>
    public readonly static int spaceIndex = 200;
    /// <summary>
    /// 历史更改信息
    /// </summary>
    public List<List<MapChange>> historyChange=new List<List<MapChange>>();
   /// <summary>
   /// 更改信息缓存
   /// </summary>
    public List<MapChange> historyTemp = new List<MapChange>();
    /// <summary>
    /// 历史信息指针
    /// </summary>
    public int historyIndex=-1;
    /// <summary>
    /// 笔刷列表
    /// </summary>
    public List<PrefabTile> brushList=new List<PrefabTile>();
    public int prefabBrushIndex=-1;
    public List<GameObject> prefabBrushList = new List<GameObject>();
    public int[] prefabMap;
    public int toolIndex = 0;
    public int heightBrushIndex=0;
    public bool is2D=false;
    
    public int this[int x,int y]
    {
        get
        {
            return map[Index(x, y)];
        }
    }
    List<int> changedList = new List<int>();
    public bool AddBrush(PrefabTile newBrush)
    {
        if (brushList.Count < 50)
        {
            if (brushList.Contains(newBrush))
            {
                return false;
            }
            brushList.Add(newBrush);
            tileBrushIndex = brushList.Count-1;
            return true;
        }
        else
        {
            Debug.LogError("笔刷数目达到上限");
            return false;
        }
    }
    public bool AddPrefabBrush(GameObject prefab)
    {
        if (prefabBrushList.Count < 50)
        {
            if (!prefabBrushList.Contains(prefab))
            {
                prefabBrushList.Add(prefab);
                prefabBrushIndex = prefabBrushList.Count - 1;
                return true;
            }
            else
            {
                return false;
            }
           
        }
        else
        {
            Debug.LogError("预制体数目达到上限");
            return false;
        }
    }
    public void ChangeBrush(PrefabTile newBrush)
    {
     //   Debug.LogError(tileBrushIndex + " " + brushList.Count);
        brushList.RemoveAt(tileBrushIndex);
        brushList.Insert(tileBrushIndex,newBrush);
        for (int i = 0; i < tileObjMap.Length; i++)
        {
            for (int j = 0; j < tileObjMap[i].Count; j++)
            {
                if (map[i] == tileBrushIndex)
                {
                    DestroyImmediate(tileObjMap[i][j]);
                    tileObjMap[i][j] = CreateTile(i, tileBrushIndex, j);
                }
               
            }
        }
    }
    public void ClearHistory()
    {
        historyIndex = -1;
        historyChange.Clear();
        historyTemp.Clear();
    }
    public void RemovePrefabBrush(int index)
    {
        DrawAllPrefab(index, spaceIndex);
        for (int i = 0; i < prefabMap.Length; i++)
        {
            if (prefabMap[i] > index && prefabMap[i] != spaceIndex)
            {
                prefabMap[i] = prefabMap[i] - 1;
            }
        }
        if (prefabBrushList.Count > index)
        {
            prefabBrushList.RemoveAt(index);
        }
        ClearHistory();
    }
    public void RemoveBrush(int index)
    {
        DrawAllTile(index, spaceIndex);
        for (int j = 0; j < map.Length; j++)
        {
            if (map[j] >index && map[j] != spaceIndex)
            {
                map[j] = map[j] - 1;
            }
        }
        if (brushList.Count > index)
        {
            brushList.RemoveAt(index);
        }
        ClearHistory();
    }
    public void DrawAllTile(int index1,int index2)
    {
        for (int i = 0; i < map.Length; i++)
        {
            if (map[i] == index1)
            {
                DrawTile(i, index2, false);
            }
        }
    }
    public void DrawAllPrefab(int index1,int index2)
    {
        for (int i = 0; i < prefabMap.Length; i++)
        {
            if (prefabMap[i] == index1)
            {
                DrawPrefab(i, index2, false);
            }
        }
    }
    

    public void InitMap()
    {
        if (map == null)
        {
          //  Debug.Log("map为空自动初始化");
            map = new int[width * length];
            for (int i = 0; i < map.Length; i++)
            {
                map[i] = spaceIndex;
            }
        }
        if (tileObjMap == null)
        {
         //   Debug.Log("tileMap为空自动初始化");
            tileObjMap = new List<GameObject>[width * length];
            for (int i = 0; i < tileObjMap.Length; i++)
            {
                tileObjMap[i] = new List<GameObject>();
            }
        }
        if (prefabObjMap == null)
        {
            prefabObjMap = new GameObject[width * length];

        }
        if (mapHeight == null)
        {
        //    Debug.Log("mapHeight为空自动初始化");
            mapHeight = new int[width * length];
        }
        if (mapRotation == null)
        {
            mapRotation = new int[width * length];
        }
        if (prefabMap == null)
        {
            prefabMap = new int[width * length];
            for (int i = 0; i < prefabMap.Length; i++)
            {
                prefabMap[i] = spaceIndex;
            }
        }
        
    }
    public void ResetMap(int w,int l)
    {
        ClearTile();
        ClearPrefab();
        width = w;
        length = l;
        map = null;
        mapHeight = null;
        tileObjMap = null;
        mapRotation = null;
        prefabMap = null;
        prefabObjMap = null;
        InitMap();
        ClearHistory();
    }
    public int[] GetPos(Vector3 pos)
    {
        var index= Index(pos);
        return new int[] { (int)(index % width ), (int)(index / width ) };
    }
    public int Index(Vector3 pos)
    {
        pos -= transform.position;
        return Index((int)(pos.x/tileSize.x), (int)(pos.z/tileSize.z));
    }
    public int Index(int x,int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < length)
        {
            return x + y * width;
        }
        else
        {
            return -1;
        }
        
    }
    public Vector3 GetPosition(int index,int height=0)
    {
        if (index < 0)
        {
            return -Vector3.one;
        }
        else
        {
            return transform.position + new Vector3(index % width*tileSize.x, 0, index / width*tileSize.z) + new Vector3(tileSize.x/2,-tileSize.y/2,tileSize.z/2)+ Vector3.up * tileSize.y * height;
        }
    }
    public Vector3 Fix(Vector3 pos)
    {
        return GetPosition(Index(pos));
    }
 

   
    
    public bool CanUndo
    {
        get
        {
            return (historyIndex >= 0 && historyIndex < historyChange.Count);
        }
    }
    public bool CanRedo
    {
        get
        {
            return (historyIndex +1>= 0 && historyIndex+1 < historyChange.Count);
        }
    }
    public void Undo()
    {
        if (CanUndo)
        {
            var change = historyChange[historyIndex];
            foreach (var c in change)
            {
                switch (c.type)
                {
                    case ChangeType.index:
                        DrawTile(c.pos, c.index1, false,true);
                        break;
                    case ChangeType.height:
                        ChangeHeight(c.pos, -c.index1, false,true);
                        break;
                    case ChangeType.rotate:
                        //Debug.Log(c.pos + " " + -c.index1);
                        Rotate(c.pos, -c.index1, false, true);
                        break;
                    case ChangeType.prefab:
                     //   Debug.LogError(prefabMap[c.pos] + "->" + c.index1);
                        DrawPrefab(c.pos, c.index1, false, true);
                        break;
                    default:
                        break;
                }
               
            }
           
            historyIndex--;
        }
        
    } 
    public void Redo()
    {
        if (CanRedo)
        {
            var change = historyChange[historyIndex+1];
            foreach (var c in change)
            {
                switch (c.type)
                {
                    case ChangeType.index:
                        DrawTile(c.pos, c.index2, false,true);
                        break;
                    case ChangeType.height:
                        ChangeHeight(c.pos, c.index1, false,true);
                        break;
                    case ChangeType.rotate:
                        Rotate(c.pos, c.index1, false, true);
                        break;
                    case ChangeType.prefab:
                        DrawPrefab(c.pos, c.index2, false, true);
                        break;
                    default:
                        break;
                }
                
            }
            
            historyIndex++;
        }
    }
    public void ChangeOver()
    {
        if(historyIndex>= historyChange.Count)
        {
            historyIndex = historyChange.Count - 1;
        }
        if(historyIndex != historyChange.Count - 1)
        {
     //       Debug.LogError(historyIndex + "/" + historyChange.Count);
            historyChange.RemoveRange(historyIndex+1, historyChange.Count - historyIndex-1);
           
        }

        historyChange.Add(historyTemp);
        historyTemp = new List<MapChange>();
        historyIndex = historyChange.Count - 1;
        changedList.Clear();
    }
    public void ChangeHeight(Vector3 pos, int change)
    {
        ChangeHeight(Index(pos), change);
    }
    public void ChangeHeight(int pos,int changeScale,bool addHistory = true,bool ignoreChangedList=false)
    {
        //Debug.Log(pos + ":" + changeScale);
        if (pos > 0 && pos < mapHeight.Length && (!changedList.Contains(pos)|| ignoreChangedList))
        {
            if (addHistory)
            {
                var change = new MapChange()
                {
                    type = ChangeType.height,
                    pos = pos,
                    index1 = changeScale,
                };
                historyTemp.Add(change);
            }
            if (changeScale > 0)
            {
                if (map[pos] != spaceIndex)
                {
                    
                    for (int i = 0; i < changeScale; i++)
                    {
                        var tile = CreateTile(pos, map[pos], mapHeight[pos] + 1);
                        while (tileObjMap[pos].Count <= mapHeight[pos] + 1)
                        {
                            tileObjMap[pos].Add(null);
                        }
                        tileObjMap[pos][(int)mapHeight[pos] + 1] = tile;
                        mapHeight[pos]++;
                     
                    }
                    if (!ignoreChangedList)
                    {
                        changedList.Add(pos);
                    }
                }
                
            }
            else if (changeScale < 0)
            {
                if (mapHeight[pos] > 0)
                {
                    for (int i = 0; i > changeScale; i--)
                    {
                        if (mapHeight[pos] < tileObjMap[pos].Count)
                        {
                            var tile = tileObjMap[pos][mapHeight[pos]];
                            tileObjMap[pos].Remove(tile);
                            DestroyImmediate(tile);
                         //   Debug.LogError("高度删除成功 ");
                        }
                        mapHeight[pos]--;
                    }
                    if (!ignoreChangedList)
                    {
                        changedList.Add(pos);
                    }
                }
               
            }
            FreshPrefabHeight(pos);


        }
    }
    public void FreshPrefabHeight(int pos)
    {
        if (prefabMap[pos] != spaceIndex)
        {
            if (prefabObjMap[pos] != null)
            {
                prefabObjMap[pos].transform.position = GetPosition(pos, mapHeight[pos] + 1);
            }
        }
    }
    public GameObject CreateTile(int pos,int index,int height=0)
    {
        if (index > brushList.Count)
        {
            Debug.LogError("不存在的Tile笔刷 " + index);
        }
        GameObject tile = null;
        if(brushList[index]==null|| brushList[index].prefab == null)
        {

        }
        else
        {
            tile = Instantiate(brushList[index].prefab, GetPosition(pos, height),
            Quaternion.Euler(0, mapRotation[pos] * 90, 0), transform);
            tile.name = "tile_" + pos + "_" + height;
            tile.hideFlags = HideFlags.HideInHierarchy;
        }
        return tile;
    }
    public GameObject CreatePrefab(int pos,int index)
    {
        if (index > prefabBrushList.Count)
        {
            Debug.LogError("不存在的prefab笔刷" + index);
        }
        var prefab = Instantiate(prefabBrushList[index], GetPosition(pos, mapHeight[pos] + 1),
            Quaternion.identity,transform);
        prefab.name = "prefab_" + pos;
        //prefab.hideFlags = HideFlags.HideInHierarchy;
        return prefab;
    }
    public MapChange Rotate(int pos,int roteScale=1,bool addHistory=true,bool ignoreChangedList=false)
    {
        MapChange change = null;
        if (pos > 0 && pos < map.Length && map[pos] != spaceIndex
           &&(!changedList.Contains(pos)||ignoreChangedList))
        {
            if (addHistory)
            {
                change = new MapChange()
                {
                    type = ChangeType.rotate,
                    pos = pos,
                    index1 = roteScale
                };
                historyTemp.Add(change);
            }
           // Debug.LogError(pos + " " + roteScale);
            mapRotation[pos] += roteScale;
            if (tileObjMap[pos] != null)
            {
                Quaternion rotation = Quaternion.Euler(0, mapRotation[pos]*90, 0);
                for (int i = 0; i < tileObjMap[pos].Count; i++)
                {
                    if (tileObjMap[pos][i] != null)
                    {
                        tileObjMap[pos][i].transform.rotation = rotation;
                    }
                }
            }
            if (!ignoreChangedList)
            {
                changedList.Add(pos);
            }
            
        }
        return change;
    }
    public MapChange DrawPrefab(int pos,int brushIndex,bool addHistory=true,bool ignoreChangedList = false)
    {
        MapChange change = null;
        if (pos >= 0 && pos < prefabMap.Length && prefabMap[pos] != brushIndex
            && (!changedList.Contains(pos) || ignoreChangedList))
        {
            if (addHistory)
            {
                change = new MapChange() { type=ChangeType.prefab,
                    pos = pos,
                    index1 = prefabMap[pos],
                    index2 = brushIndex };
                historyTemp.Add(change);
            }
            prefabMap[pos] = brushIndex;
            if (prefabObjMap[pos] != null)
            {
                DestroyImmediate(prefabObjMap[pos]);
            }
            if (brushIndex < prefabBrushList.Count)
            {
                if (brushIndex >= 0)
                {
                    var prefab = CreatePrefab(pos, brushIndex);
                    prefabObjMap[pos] = prefab;
                }
                if (!ignoreChangedList)
                {
                    changedList.Add(pos);
                }
            }
            else if(brushIndex==spaceIndex)
            {

            }
            else
            {
                Debug.LogError("不存在的Prefab笔刷 " + brushIndex);
            }
         
        }
        return change;
    }
    public MapChange DrawTile(Vector3 pos, int brushIndex)
    {
        return DrawTile(Index(pos), brushIndex);
    }
    public MapChange DrawTile(int pos,int brushIndex,bool addHistory=true,bool ignoreChangedList=false)
    {
    
        MapChange change = null;
        if (pos>=0&&pos < map.Length && map[pos] != brushIndex
            &&(!changedList.Contains(pos)|| ignoreChangedList))
        {
            if (addHistory)
            {
                change = new MapChange() { pos = pos, index1 = map[pos], index2 = brushIndex };
                historyTemp.Add(change);
               
            }
            map[pos] = brushIndex;
            if (tileObjMap[pos] != null)
            {
                for (int i = 0; i < tileObjMap[pos].Count; i++)
                {
                    DestroyImmediate(tileObjMap[pos][i]);
                }
            }
            if (brushIndex < brushList.Count)
            {
                if (brushIndex >= 0)
                {
                    if (tileObjMap[pos].Count == 0)
                    {
                        var tile = CreateTile(pos, brushIndex);
                        tileObjMap[pos].Add(tile);
                    }
                    else
                    {
                        for (int i = 0; i < tileObjMap[pos].Count; i++)
                        {
                            
                            var tile = CreateTile(pos, brushIndex, i);
                            tileObjMap[pos][i] = tile;
                        }
                    }
                    if (!ignoreChangedList)
                    {
                        changedList.Add(pos);
                    }
                    
                }
            }
            else if(brushIndex==spaceIndex)
            {
                changedList.Remove(pos);
                ChangeHeight(pos, -mapHeight[pos], addHistory,ignoreChangedList);
                Rotate(pos, -mapRotation[pos], addHistory, ignoreChangedList);
            }
            else
            {
                Debug.LogError("不存在的Tile笔刷 " + brushIndex);
            }
          
         
        }
        return change;
    }

    public void ClearTile()
    {
        for (int i = 0; i < map.Length; i++)
        {
            DrawTile(i, spaceIndex);
        }
        ChangeOver();
        //Debug.Log(historyChange.Count + " : " + CanUndo);
    }
    public void ClearPrefab()
    {
        for (int i = 0; i < prefabMap.Length; i++)
        {
            DrawPrefab(i, spaceIndex);
        }
        ChangeOver();
    }
   
}
