using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System;
using System.IO;
public class FileManager
{


#if UNITY_EDITOR



    public static string Serialize<T>(T t)
    {
        using (StringWriter sw = new StringWriter())
        {
            if (t == null||t.GetType()==null) { Debug.LogError(t + "t" + t.GetType()); }
            XmlSerializer xz = new XmlSerializer(t.GetType());
            xz.Serialize(sw, t);
            return sw.ToString();
        }
    }
    public static T Deserialize<T>(string s) where T :class
    {
        using (StringReader sr = new StringReader(s))
        {
            XmlSerializer xz = new XmlSerializer(typeof(T));
            return xz.Deserialize(sr) as T;
        }
    }


    public static string SaveSelectPath(string data,string title="保存",string name="temp",string extension="obj", string directory = "Assets")
    {
        var path = UnityEditor.EditorUtility.SaveFilePanel(
            title,
            directory,
            name,
            extension
            );
        if (path != string.Empty)
        {
            Save(path, data);
        }
        return path;
    }
    public static string LoadSelectPath(string title = "读取", string extension = "obj", string directory = "Assets")
    {
        var path = UnityEditor.EditorUtility.OpenFilePanel(
            title,
            directory,
            extension
            );
        if (System.IO.File.Exists(path))
        {
            return Load(path);
        }
        else
        {
            return "";
        }
        
    }
    public static string Save(string path,string data)
    {
        using (var file = System.IO.File.Create(path))
        {
            using (var sw = new System.IO.StreamWriter(file))
            {
                sw.Write(data);
            }
        }
        UnityEditor.AssetDatabase.Refresh();
        return path;
    }
    public static string Load(string path)
    {
        string data = "";
        if (!System.IO.File.Exists(path))
        {
            Debug.LogError("不存在文件：" + path);
            return data;
        }
        using (var file = System.IO.File.Open(path, System.IO.FileMode.Open))
        {
            using (var sw = new System.IO.StreamReader(file))
            {
                while (!sw.EndOfStream)
                {
                    
                    data += sw.ReadLine();
                }
            }
        }
        return data;    
    }
#endif
}
