using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GetGameObjectStructure
{
    /// <summary>
    /// 负责获取物体的层级信息
    /// </summary>
    public class HierarchyInspector
    {
        private ComponentInspector componentInspector;

        public HierarchyInspector()
        {
            componentInspector = new ComponentInspector();
        }

        /// <summary>
        /// 获取物体的层级信息
        /// </summary>
        public string GetHierarchyInfo(GameObject obj)
        {
            StringBuilder sb = new StringBuilder();
            
            // 标题
            sb.AppendLine("=== Object Inspector ===");
            sb.AppendLine();

            // 当前物体名称
            sb.AppendLine($"Name: {obj.name}");
            sb.AppendLine();

            // 层级路径（从根到当前物体）
            sb.AppendLine("Hierarchy Path:");
            List<string> path = new List<string>();
            Transform current = obj.transform;
            
            // 向上遍历到根节点
            while (current != null)
            {
                path.Insert(0, current.name);
                current = current.parent;
            }
            
            // 构建路径字符串
            sb.AppendLine(string.Join(" -> ", path));
            sb.AppendLine();

            // 组件列表
            Transform transform = obj.transform;
            sb.AppendLine("Components:");
            Component[] components = obj.GetComponents<Component>();
            
            // 收集特定组件
            UnityEngine.UI.Button? buttonComponent = null;
            UnityEngine.UI.Image? imageComponent = null;
            TMPro.TextMeshProUGUI? textMeshProComponent = null;
            UnityEngine.UI.ScrollRect? scrollRectComponent = null;
            UnityEngine.UI.Mask? maskComponent = null;
            RectTransform? rectTransformComponent = null;

            foreach (Component comp in components)
            {
                if (comp != null)
                {
                    sb.AppendLine($"  - {comp.GetType().Name}");
                    // 检查特定组件类型
                    if (comp is UnityEngine.UI.Button btn)
                    {
                        buttonComponent = btn;
                    }
                    if (comp is UnityEngine.UI.Image img)
                    {
                        imageComponent = img;
                    }
                    if (comp is TMPro.TextMeshProUGUI tmp)
                    {
                        textMeshProComponent = tmp;
                    }
                    if (comp is UnityEngine.UI.ScrollRect scrollRect)
                    {
                        scrollRectComponent = scrollRect;
                    }
                    if (comp is UnityEngine.UI.Mask mask)
                    {
                        maskComponent = mask;
                    }
                    if (comp is RectTransform rectTransform)
                    {
                        rectTransformComponent = rectTransform;
                    }
                }
            }
            sb.AppendLine();

            // 显示特定组件的详细信息
            if (buttonComponent != null)
            {
                componentInspector.DisplayButtonEvents(sb, buttonComponent);
            }

            if (imageComponent != null)
            {
                componentInspector.DisplayImageInfo(sb, imageComponent);
            }

            if (textMeshProComponent != null)
            {
                componentInspector.DisplayTextMeshProUGUIInfo(sb, textMeshProComponent);
            }

            if (scrollRectComponent != null)
            {
                componentInspector.DisplayScrollRectInfo(sb, scrollRectComponent);
            }

            if (maskComponent != null)
            {
                componentInspector.DisplayMaskInfo(sb, maskComponent);
            }

            if (rectTransformComponent != null)
            {
                componentInspector.DisplayRectTransformInfo(sb, rectTransformComponent);
            }

            // 当前物体的子物体
            sb.AppendLine($"Children Count: {transform.childCount}");
            if (transform.childCount > 0)
            {
                sb.AppendLine("Children:");
                for (int i = 0; i < transform.childCount; i++)
                {
                    sb.AppendLine($"  [{i}] {transform.GetChild(i).name}");
                }
                sb.AppendLine();
            }

            // 父物体的所有子物体
            if (transform.parent != null)
            {
                Transform parentTransform = transform.parent;
                
                sb.AppendLine($"Parent: {parentTransform.name}");
                
                // 父物体的所有子物体
                sb.AppendLine($"Parent's Children Count: {parentTransform.childCount}");
                if (parentTransform.childCount > 0)
                {
                    sb.AppendLine("Parent's Children:");
                    for (int i = 0; i < parentTransform.childCount; i++)
                    {
                        Transform sibling = parentTransform.GetChild(i);
                        string marker = (sibling == transform) ? "← (Current)" : "";
                        sb.AppendLine($"  [{i}] {sibling.name} {marker}");
                    }
                }
            }
            else
            {
                sb.AppendLine("Parent: None (Root Object)");
            }

            return sb.ToString();
        }
    }
}

