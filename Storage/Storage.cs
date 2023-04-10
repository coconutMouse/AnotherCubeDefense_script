using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

[Serializable]
public struct StorageWave<T>
{
    public string key;
    public T data;
    public StorageWave(string _key, T _data)
    {
        key = _key;
        data = _data;
    }
}

public abstract class ScriptableObjectWithStringList : ScriptableObject 
{ 
    public abstract List<string> GetStringList();
}


public class Storage<T> : ScriptableObjectWithStringList
{
    [Serializable]
    public class SerializeDicString : SerializeDictionary<string, T> { }

    [SerializeField]
    private SerializeDicString datas;
    [SerializeField]
    private List<StorageWave<T>> waves;
    [SerializeField]
    private string path;
    private void OnEnable()
    {
        waves = GetDataTypeList();

        if (path == "")
            path = "Assets/Script/DefineKey/";
    }
    private void OnValidate()
    {
        SaveDatas();

        if (!path.Contains("Assets/Script/DefineKey/") || path.IndexOf("Assets/Script/DefineKey/") != 0)
        {
            path = "Assets/Script/DefineKey/";
        }
    }
    public override List<string> GetStringList()
    {
        return datas.GetKeys();
    }
    public bool ContainsKey(string _codeName)
    {
        return datas.ContainsKey(_codeName);
    }

    public T GetValue(string _codeName)
    {
        return datas[_codeName];
    }

    public List<T> GetValues()
    {
        return datas.GetValues();
    }

    public Dictionary<string, T> GetDataAll()
    {
        return datas;
    }
    public void SaveDatas()
    {
        if (datas == null)
            datas = new SerializeDicString();
        else
            datas.Clear();

        foreach (StorageWave<T> wave in waves)
        {
            if (!datas.ContainsKey(wave.key) && wave.key != "")
            {
                datas.Add(wave.key, wave.data);
            }
        }
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }
    private List<StorageWave<T>> GetDataTypeList()
    {
        if (datas == null)
        {
            return new List<StorageWave<T>>();
        }
        List<StorageWave<T>> copyData = new List<StorageWave<T>>();

        foreach (KeyValuePair<string, T> pair in datas)
        {
            StorageWave<T> storageWave = new StorageWave<T>(pair.Key, pair.Value);
            copyData.Add(storageWave);
        }
        return copyData;
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(Storage<>), true)]
public class StorageEditor : Editor
{
    private ReorderableList list;
    private string storageName;
    private static GUIContent warnicon_SameKey;
    private static GUIContent warnicon_BlankKey;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (list != null)
            list.DoLayoutList();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("path"), GUIContent.none);

        serializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("Generate key definition script"))
        {
            CreateDefine();
        }
    }
    public void CreateDefine()
    {
        List<string> keyList = new List<string>();
        foreach (SerializedProperty property in list.serializedProperty)
        {
            string key = property.FindPropertyRelative("key").stringValue;
            if (!keyList.Contains(key) && key != "")
                keyList.Add(key);
        }

        string path = serializedObject.FindProperty("path").stringValue + storageName + "Key.cs";

        StringBuilder enumBuilder = new StringBuilder();

        enumBuilder.AppendLine("namespace StorageKeySapce");
        enumBuilder.AppendLine("{ ");
        enumBuilder.AppendLine("\tpublic static class " + storageName + "Key");
        enumBuilder.AppendLine("\t{ ");
        foreach (string key in keyList)
        {
            enumBuilder.Append("\t\tpublic const string " + key + " = ");
            enumBuilder.AppendFormat("\"{0}\";", key).AppendLine();
        }
        enumBuilder.AppendLine("\t}");
        enumBuilder.AppendLine("}");

        var directoryName = Path.GetDirectoryName(path);
        if (Directory.Exists(directoryName) == false)
        {
            Directory.CreateDirectory(directoryName);
        }
        File.WriteAllText(path, enumBuilder.ToString(), Encoding.UTF8);
        AssetDatabase.ImportAsset(path);
    }

    private static GUIContent IconContent(string name, string tooltip)
    {
        var builtinIcon = EditorGUIUtility.IconContent(name);
        return new GUIContent(builtinIcon.image, tooltip);
    }

    private void OnEnable()
    {
        string storagePath = AssetDatabase.GetAssetPath(target.GetInstanceID());
        storageName = Path.GetFileName(storagePath);
        if(storageName.Length > 0) storageName = storageName.Substring(0, storageName.IndexOf('.'));
        storageName = storageName.Replace(" ", "");

        if (warnicon_SameKey == null)
            warnicon_SameKey = IconContent("console.warnicon@2x", "동일한 key가 있습니다.  \n이 값은 저장되지 않습니다.");
        if (warnicon_BlankKey == null)
            warnicon_BlankKey = IconContent("console.warnicon@2x", "key값을 입력해 주세요. \nkey값이 공백인 값은 저장되지 않습니다.");

        list = new ReorderableList(serializedObject, serializedObject.FindProperty("waves"), true, true, true, true)
        {

            elementHeightCallback = ElementHeightCallback,
            drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Storage");
            },
            drawElementCallback =
            (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = list.serializedProperty.GetArrayElementAtIndex(index);

                string elementKey = element.FindPropertyRelative("key").stringValue;
                rect.y += 2;
                int num = 0;
                if (elementKey == "")
                {
                    GUI.Label(new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight), warnicon_BlankKey);
                }
                else
                {
                    foreach (SerializedProperty prefab in list.serializedProperty)
                    {
                        if (num == index)
                            break;
                        string key = prefab.FindPropertyRelative("key").stringValue;

                        if (key == elementKey)
                        {
                            GUI.Label(new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight), warnicon_SameKey);
                            break;
                        }
                        num++;
                    }
                }

                EditorGUI.PropertyField(
                    new Rect(rect.x + 20, rect.y, 110, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("key"), GUIContent.none);

                EditorGUI.PropertyField(
                    new Rect(rect.x + 145, rect.y, rect.width - 150, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("data"), GUIContent.none, true);
            },
        };

    }
    private float ElementHeightCallback(int index)
    {
       SerializedProperty wavesList = serializedObject.FindProperty("waves");
        if (wavesList.arraySize == 0)
            return EditorGUIUtility.standardVerticalSpacing;

        var element = wavesList.GetArrayElementAtIndex(index);
        SerializedProperty namesProp = element.FindPropertyRelative("data");
        return EditorGUI.GetPropertyHeight(namesProp) + EditorGUIUtility.standardVerticalSpacing;
    }
}
#endif


[AttributeUsage(AttributeTargets.Field)]
public class StringFromScriptableObjectAttribute : PropertyAttribute {}


#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(StringFromScriptableObjectAttribute), true)]
public class SelectStringFromScriptableObjectAttributeDrawer : PropertyDrawer
{
    private ScriptableObjectWithStringList scriptableObjectWithStringList;
    private readonly float boxSpacing = 8;
    private int listIndex;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {    
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float propertyHeight = lineHeight;
        if (property.isExpanded)
        {
            propertyHeight += lineHeight + boxSpacing * 2;
            if (scriptableObjectWithStringList != null)
            {
                List<string> list = scriptableObjectWithStringList.GetStringList();
                if (list != null)
                    propertyHeight += (lineHeight * 3) + boxSpacing;
                else
                    propertyHeight += (lineHeight * 2) + boxSpacing;
            }

            return propertyHeight + 3;
        }
        return lineHeight;
    }
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float labelWidth = EditorGUIUtility.labelWidth + 2.5f;
        float propertyHeight = lineHeight;

        if (property.propertyType != SerializedPropertyType.String)
        {
            GUI.Label(new Rect(position.x, position.y, labelWidth, lineHeight), property.name);
            EditorGUI.HelpBox(new Rect(position.x + labelWidth, position.y, position.width - labelWidth, lineHeight), "Only works with string", MessageType.Error);
            return;
        }

        EditorGUI.BeginProperty(position, GUIContent.none, property);
        {
            GUI.Label(new Rect(position.x + labelWidth, position.y, position.width - labelWidth, lineHeight), "\"" + property.stringValue + "\"");

            Rect rect = position;
            rect.height = lineHeight;
            property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, label);

            if (property.isExpanded)
            {
                rect.x += boxSpacing;
                rect.width -= boxSpacing * 2;
                rect.y += lineHeight + boxSpacing;
                scriptableObjectWithStringList = EditorGUI.ObjectField(rect, "Object to get string", scriptableObjectWithStringList, typeof(ScriptableObjectWithStringList), false) as ScriptableObjectWithStringList;

                propertyHeight += lineHeight + boxSpacing * 2;

                if (scriptableObjectWithStringList != null)
                {
                    List<string> list = scriptableObjectWithStringList.GetStringList();

                    if(list != null)
                    {
                        rect.y += lineHeight + boxSpacing;
                        listIndex = EditorGUI.Popup(rect, "Select string to change", listIndex, list.ToArray());

                        rect.y += lineHeight;
                        if (GUI.Button(rect, "Change string to " + "\""+ list[listIndex] + "\""))
                        {
                            property.stringValue = list[listIndex];
                        }
                        rect.y += lineHeight;
                        if (GUI.Button(rect, "Change string to none"))
                        {
                            property.stringValue = "";
                        }
                        propertyHeight += (lineHeight * 3) + boxSpacing;
                    }
                    else
                    {
                        rect.y += lineHeight + boxSpacing; 
                        EditorGUI.HelpBox(
                            new Rect(rect.x, rect.y, rect.width, lineHeight * 2),
                            "Make sure the return value of the \"GetStringList()\" function is \"null\".", MessageType.Error);
                        propertyHeight += (lineHeight*2) + boxSpacing; 
                    }

                }

                GUI.Box(new Rect(position.x, position.y + lineHeight, position.width, propertyHeight - lineHeight), GUIContent.none, EditorStyles.helpBox);
            }
        }
        EditorGUI.EndProperty();
    }
}
#endif