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
    public int Width=10;
    /// <summary>
    /// 长度
    /// </summary>
    public int Length=10;
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
    public List<GameObject>[] objMap;
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
    public int toolIndex = 0;
    public int heightBrushIndex=0;
    public bool is2D=false;
    public int[] mapRotation;

    List<int> changedList = new List<int>();
    public bool AddBrush(PrefabTile newBrush)
    {
        if (brushList.Count < 50)
        {
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
    public void ChangeBrush(PrefabTile newBrush)
    {
        Debug.LogError(tileBrushIndex + " " + brushList.Count);
        brushList.RemoveAt(tileBrushIndex);
        brushList.Insert(tileBrushIndex,newBrush);
        for (int i = 0; i < objMap.Length; i++)
        {
            for (int j = 0; j < objMap[i].Count; j++)
            {
                if (map[i] == tileBrushIndex)
                {
                    DestroyImmediate(objMap[i][j]);
                    objMap[i][j] = CreateTile(i, tileBrushIndex, j);
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
    public void RemoveBrush(int index)
    {
        DrawAll(index, spaceIndex);
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
    public void DrawAll(int index1,int index2)
    {
        for (int i = 0; i < map.Length; i++)
        {
            if (map[i] == index1)
            {
                Draw(i, index2, false);
            }
        }
    }
    

    public void InitMap()
    {
        if (map == null)
        {
          //  Debug.Log("map为空自动初始化");
            map = new int[Width * Length];
            for (int i = 0; i < map.Length; i++)
            {
                map[i] = spaceIndex;
            }
        }
        if (objMap == null)
        {
         //   Debug.Log("tileMap为空自动初始化");
            objMap = new List<GameObject>[Width * Length];
            for (int i = 0; i < objMap.Length; i++)
            {
                objMap[i] = new List<GameObject>();
            }
        }
        if (mapHeight == null)
        {
        //    Debug.Log("mapHeight为空自动初始化");
            mapHeight = new int[Width * Length];
        }
    }
    public void ResetMap(int w,int l)
    {
        ClearTile();
        Width = w;
        Length = l;
        map = null;
        mapHeight = null;
        objMap = null;
        InitMap();
        ClearHistory();
    }
    public int Index(Vector3 pos)
    {
        pos -= transform.position;
        return Index((int)(pos.x/tileSize.x), (int)(pos.z/tileSize.z));
    }
    public int Index(int x,int y)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Length)
        {
            return x + y * Width;
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
            return transform.position + new Vector3(index % Width*tileSize.x, 0, index / Width*tileSize.z) + new Vector3(tileSize.x/2,-tileSize.y/2,tileSize.z/2)+ Vector3.up * tileSize.y * height;
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
                        Draw(c.pos, c.index1, false,true);
                        break;
                    case ChangeType.height:
                        ChangeHeight(c.pos, -c.index1, false,true);
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
                        Draw(c.pos, c.index2, false,true);
                        break;
                    case ChangeType.height:
                        ChangeHeight(c.pos, c.index1, false,true);
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
                        while (objMap[pos].Count <= mapHeight[pos] + 1)
                        {
                            objMap[pos].Add(null);
                        }
                        objMap[pos][(int)mapHeight[pos] + 1] = tile;
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
                        if (mapHeight[pos] < objMap[pos].Count)
                        {
                            var tile = objMap[pos][mapHeight[pos]];
                            objMap[pos].Remove(tile);
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
           
        }
    }
    public GameObject CreateTile(int pos,int index,int height=0)
    {
        if (index > brushList.Count)
        {
            Debug.LogError("不存在的笔刷 " + index);
        }
        
        var tile = Instantiate(brushList[index].prefab, GetPosition(pos,height), Quaternion.identity, transform);
        tile.name = "tile_" + pos + "_" + height;
        tile.hideFlags = HideFlags.HideInHierarchy;
        return tile;
    }
    public MapChange Draw(Vector3 pos, int brushIndex)
    {
        return Draw(Index(pos), brushIndex);
    }
    public MapChange Draw(int pos,int brushIndex,bool addHistory=true,bool ignoreChangedList=false)
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
            if (objMap[pos] != null)
            {
                for (int i = 0; i < objMap[pos].Count; i++)
                {
                    DestroyImmediate(objMap[pos][i]);
                }
            }
            if (brushIndex < brushList.Count)
            {
                if (brushIndex >= 0)
                {
                    if (objMap[pos].Count == 0)
                    {
                        var tile = CreateTile(pos, brushIndex);
                        objMap[pos].Add(tile);
                    }
                    else
                    {
                        for (int i = 0; i < objMap[pos].Count; i++)
                        {
                            
                            var tile = CreateTile(pos, brushIndex, i);
                            objMap[pos][i] = tile;
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
            }
            else
            {
                Debug.LogError("不存在的笔刷 " + brushIndex);
            }
          
         
        }
        return change;
    }

    public void ClearTile()
    {
        for (int i = 0; i < map.Length; i++)
        {
            Draw(i, spaceIndex);
        }
        ChangeOver();
        //Debug.Log(historyChange.Count + " : " + CanUndo);
    }
    private void OnDestroy()
    {
        Debug.LogError("地图被删除");
    }
}
