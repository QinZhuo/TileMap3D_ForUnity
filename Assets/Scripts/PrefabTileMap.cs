using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class MapChange
{
    public int pos;
    public int lastBrushIndex;
    public int newBrushIndex;
}
public class PrefabTileMap : MonoBehaviour
{
    public Vector3 tileSize=new Vector3(2,1,2);
    public int Width=10;
    public int Height=10;
    public int brushIndex = -1;
    public byte[] map;

    public GameObject[] tileMap;
    public readonly static int spaceIndex = 200;

    public List<List<MapChange>> historyChange=new List<List<MapChange>>();
    public List<MapChange> historyTemp = new List<MapChange>();
    public int historyIndex=-1;

    public List<PrefabTile> brushList=new List<PrefabTile>();
    public bool AddBrush(PrefabTile newBrush)
    {
        if (brushList.Count < 50)
        {
            brushList.Add(newBrush);
            brushIndex = brushList.Count-1;
            return true;
        }
        else
        {
            Debug.LogError("笔刷数目达到上限");
            return false;
        }
      
    }
    public void RemoveBrush(int index)
    {
        if (brushList.Count > index)
        {
            brushList.RemoveAt(index);
        }
    }

    public void InitMap()
    {
        if (map == null)
        {
            map = new byte[Width * Height];
            for (int i = 0; i < map.Length; i++)
            {
                map[i] = (byte)spaceIndex;
            }
        }
        if (tileMap == null)
        {
            tileMap = new GameObject[Width * Height];
        }
    }
    public void ResetMap(int w,int h)
    {
        ClearTile();
        var lastW = Width;
        var lastMap = map;
        map = new byte[w * h];
        Width = w;
        Height = h;
        for (int i = 0; i < map.Length; i++)
        {
            map[i] = (byte)spaceIndex;
        }
        tileMap = new GameObject[w * h];
        for (int i = 0; i < lastMap.Length; i++)
        {
            var index = i % lastW + i /  lastW*Width;
            Draw(index, lastMap[i], false);
        }
       
    }
    public int Index(Vector3 pos)
    {
        pos -= transform.position;
        return Index((int)(pos.x/tileSize.x), (int)(pos.z/tileSize.z));
    }
    public int Index(int x,int y)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
        {
            return x + y * Width;
        }
        else
        {
            return -1;
        }
        
    }
    public Vector3 GetPosition(int index)
    {
        if (index < 0)
        {
            return -Vector3.one;
        }
        else
        {
            return transform.position + new Vector3(index % Width*tileSize.x, 0, index / Width*tileSize.z) + new Vector3(tileSize.x/2,-tileSize.y/2,tileSize.z/2);
        }
    }
    public Vector3 Fix(Vector3 pos)
    {
        return GetPosition(Index(pos));
    }
    public MapChange Draw(Vector3 pos,int brushIndex)
    {
        return Draw(Index(pos), brushIndex);
    }
    public void ClearAll()
    {
        for (int i = 0; i < map.Length; i++)
        {
            Draw(i, spaceIndex);
        }
    }
    public bool CanUndo
    {
        get
        {
            return (historyIndex > 0 && historyIndex < historyChange.Count);
        }
    }
    public bool CanRedo
    {
        get
        {
            return (historyIndex +1> 0 && historyIndex+1 < historyChange.Count);
        }
    }
    public void Undo()
    {
        if (CanUndo)
        {
            var change = historyChange[historyIndex];
            foreach (var c in change)
            {
                Draw(c.pos, c.lastBrushIndex, false);
            }
           
            historyIndex--;
        }
        
    }
    public void Redo()
    {
        if (historyIndex+1 > 0 && historyIndex+1 < historyChange.Count)
        {
            var change = historyChange[historyIndex+1];
            foreach (var c in change)
            {
                Draw(c.pos, c.newBrushIndex, false);
            }
            
            historyIndex++;
        }
    }
    public void ChangeOver()
    {
        historyChange.Add(historyTemp);
        historyTemp = new List<MapChange>();
        historyIndex = historyChange.Count - 1;
    }
    public MapChange Draw(int pos,int brushIndex,bool addHistory=true)
    {
        MapChange change = null;
        if (pos>=0&&pos < map.Length && map[pos] != brushIndex)
        {
            if (addHistory)
            {
                change = new MapChange() { pos = pos, lastBrushIndex = map[pos], newBrushIndex = brushIndex };
                historyTemp.Add(change);
               
            }
            map[pos] = (byte)brushIndex;
            if (tileMap[pos] != null)
            {
                DestroyImmediate(tileMap[pos]);
            }
            if (brushIndex < brushList.Count)
            {
                if (brushIndex >= 0)
                {
                    var tile = Instantiate(brushList[brushIndex].prefab, GetPosition(pos), Quaternion.identity, transform);
                    tile.hideFlags = HideFlags.HideInHierarchy;
                    tileMap[pos] = tile;
                }
            }
            else if(brushIndex==spaceIndex)
            {

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
        foreach (var tile in tileMap)
        {
            if (tile != null)
            {
                Debug.Log(tile);
                DestroyImmediate(tile);
            }
        }
    }
}
