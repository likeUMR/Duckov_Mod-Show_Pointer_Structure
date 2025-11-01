using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace HideTheEquipment
{
    /// <summary>
    /// 场景层级结构数据类
    /// </summary>
    [Serializable]
    public class SceneHierarchyData
    {
        public string name = string.Empty;
        public bool active = true;
        public List<SceneHierarchyData> children = new List<SceneHierarchyData>();
    }

    /// <summary>
    /// 负责导出场景层级结构到JSON文件
    /// </summary>
    public class SceneHierarchyExporter
    {
        /// <summary>
        /// 导出场景层级结构到JSON文件
        /// </summary>
        public void ExportSceneHierarchy(string filePath)
        {
            try
            {
                List<SceneHierarchyData> rootObjects = new List<SceneHierarchyData>();

                // 查找指定的根对象 Character(Clone)
                string targetRootName = "Character(Clone)";
                GameObject? targetRoot = null;
                
                // 遍历场景中所有的根对象
                GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                foreach (GameObject obj in allObjects)
                {
                    // 只处理根对象（没有父对象的）且名称匹配
                    if (obj.transform.parent == null && obj.name == targetRootName)
                    {
                        targetRoot = obj;
                        break;
                    }
                }

                // 如果找到目标根对象，构建层级数据
                if (targetRoot != null)
                {
                    SceneHierarchyData data = BuildHierarchyData(targetRoot);
                    rootObjects.Add(data);
                }
                else
                {
                    Debug.LogWarning($"[SceneHierarchyExporter] 未找到根对象 '{targetRootName}'，无法导出");
                    return;
                }

                // 构建JSON
                string json = BuildJSON(rootObjects);

                // 保存到文件
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(filePath, json, Encoding.UTF8);
                Debug.Log($"[SceneHierarchyExporter] 场景层级结构已保存到: {filePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SceneHierarchyExporter] 导出失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 构建层级结构数据
        /// </summary>
        private SceneHierarchyData BuildHierarchyData(GameObject obj)
        {
            SceneHierarchyData data = new SceneHierarchyData();
            data.name = obj.name;
            data.active = obj.activeSelf;

            // 递归处理子对象
            Transform transform = obj.transform;
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject child = transform.GetChild(i).gameObject;
                SceneHierarchyData childData = BuildHierarchyData(child);
                data.children.Add(childData);
            }

            return data;
        }

        /// <summary>
        /// 构建JSON字符串（手动构建，避免依赖外部JSON库）
        /// </summary>
        private string BuildJSON(List<SceneHierarchyData> rootObjects)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine("  \"scene\": {");
            sb.AppendLine($"    \"exportTime\": \"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\",");
            sb.AppendLine($"    \"rootObjectCount\": {rootObjects.Count},");
            sb.AppendLine("    \"rootObjects\": [");

            for (int i = 0; i < rootObjects.Count; i++)
            {
                sb.Append(SerializeObject(rootObjects[i], "      "));
                if (i < rootObjects.Count - 1)
                {
                    sb.AppendLine(",");
                }
                else
                {
                    sb.AppendLine();
                }
            }

            sb.AppendLine("    ]");
            sb.AppendLine("  }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        /// <summary>
        /// 序列化对象为JSON
        /// </summary>
        private string SerializeObject(SceneHierarchyData data, string indent)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(indent + "{");
            
            string nextIndent = indent + "  ";
            sb.AppendLine($"{nextIndent}\"name\": \"{EscapeJsonString(data.name)}\",");
            sb.AppendLine($"{nextIndent}\"active\": {data.active.ToString().ToLower()},");

            // Children
            sb.Append($"{nextIndent}\"children\": [");
            if (data.children.Count > 0)
            {
                sb.AppendLine();
                for (int i = 0; i < data.children.Count; i++)
                {
                    sb.Append(SerializeObject(data.children[i], nextIndent + "  "));
                    if (i < data.children.Count - 1)
                    {
                        sb.AppendLine(",");
                    }
                    else
                    {
                        sb.AppendLine();
                    }
                }
                sb.Append(nextIndent);
            }
            sb.AppendLine("]");

            sb.Append(indent + "}");
            return sb.ToString();
        }

        /// <summary>
        /// 转义JSON字符串中的特殊字符
        /// </summary>
        private string EscapeJsonString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return "";
            }

            return str.Replace("\\", "\\\\")
                     .Replace("\"", "\\\"")
                     .Replace("\n", "\\n")
                     .Replace("\r", "\\r")
                     .Replace("\t", "\\t");
        }
    }
}

