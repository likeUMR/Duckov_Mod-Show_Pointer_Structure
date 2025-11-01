using System;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GetGameObjectStructure
{
    /// <summary>
    /// 负责显示各种组件的详细信息
    /// </summary>
    public class ComponentInspector
    {
        /// <summary>
        /// 显示Button组件的事件信息
        /// </summary>
        public void DisplayButtonEvents(StringBuilder sb, Button button)
        {
            sb.AppendLine("=== Button Component Info ===");
            
            // 获取onClick事件
            var onClickEvent = button.onClick;
            if (onClickEvent == null)
            {
                sb.AppendLine("onClick: No listeners");
                sb.AppendLine();
                return;
            }

            // 使用反射获取UnityEvent的内部调用信息
            try
            {
                // 获取UnityEventBase的类型（UnityEvent继承自UnityEventBase）
                Type eventType = onClickEvent.GetType().BaseType;
                if (eventType == null)
                {
                    eventType = onClickEvent.GetType();
                }

                // 尝试获取PersistentCalls（这是Unity存储序列化调用的地方）
                FieldInfo persistentCallsField = eventType.GetField("m_PersistentCalls", 
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                
                if (persistentCallsField != null)
                {
                    object persistentCalls = persistentCallsField.GetValue(onClickEvent);
                    if (persistentCalls != null)
                    {
                        // 获取调用列表
                        PropertyInfo callsProperty = persistentCalls.GetType().GetProperty("m_Calls",
                            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                        
                        if (callsProperty != null)
                        {
                            object calls = callsProperty.GetValue(persistentCalls);
                            if (calls is System.Collections.ICollection callsCollection && callsCollection.Count > 0)
                            {
                                sb.AppendLine($"onClick Listeners Count: {callsCollection.Count}");
                                sb.AppendLine();

                                int index = 0;
                                foreach (object call in callsCollection)
                                {
                                    sb.AppendLine($"Listener [{index}]:");
                                    
                                    // 获取目标对象
                                    PropertyInfo targetProperty = call.GetType().GetProperty("target",
                                        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                                    PropertyInfo methodNameProperty = call.GetType().GetProperty("methodName",
                                        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                                    PropertyInfo modeProperty = call.GetType().GetProperty("mode",
                                        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                                    PropertyInfo argumentsProperty = call.GetType().GetProperty("arguments",
                                        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                                    if (targetProperty != null)
                                    {
                                        UnityEngine.Object? target = targetProperty.GetValue(call) as UnityEngine.Object;
                                        if (target != null)
                                        {
                                            sb.AppendLine($"  Target: {target.name} ({target.GetType().Name})");
                                        }
                                        else
                                        {
                                            sb.AppendLine($"  Target: null");
                                        }
                                    }

                                    if (methodNameProperty != null)
                                    {
                                        string? methodName = methodNameProperty.GetValue(call) as string;
                                        sb.AppendLine($"  Method: {methodName ?? "Unknown"}");
                                    }

                                    if (modeProperty != null)
                                    {
                                        object? mode = modeProperty.GetValue(call);
                                        sb.AppendLine($"  Mode: {mode ?? "Unknown"}");
                                    }

                                    if (argumentsProperty != null)
                                    {
                                        object? arguments = argumentsProperty.GetValue(call);
                                        if (arguments != null)
                                        {
                                            sb.AppendLine($"  Arguments: {arguments}");
                                        }
                                        else
                                        {
                                            sb.AppendLine($"  Arguments: None");
                                        }
                                    }

                                    sb.AppendLine();
                                    index++;
                                }
                            }
                            else
                            {
                                sb.AppendLine("onClick: No persistent calls found");
                                sb.AppendLine();
                            }
                        }
                        else
                        {
                            sb.AppendLine("onClick: Cannot access calls property");
                            sb.AppendLine();
                        }
                    }
                    else
                    {
                        sb.AppendLine("onClick: No persistent calls");
                        sb.AppendLine();
                    }
                }
                else
                {
                    // 备用方法：检查事件是否有监听器
                    int listenerCount = onClickEvent.GetPersistentEventCount();
                    if (listenerCount > 0)
                    {
                        sb.AppendLine($"onClick Listeners Count: {listenerCount}");
                        sb.AppendLine();
                        for (int i = 0; i < listenerCount; i++)
                        {
                            sb.AppendLine($"Listener [{i}]:");
                            sb.AppendLine($"  Target: {onClickEvent.GetPersistentTarget(i)?.name ?? "null"}");
                            sb.AppendLine($"  Method: {onClickEvent.GetPersistentMethodName(i)}");
                            sb.AppendLine();
                        }
                    }
                    else
                    {
                        sb.AppendLine("onClick: No listeners (or using runtime listeners)");
                        sb.AppendLine();
                    }
                }
            }
            catch (System.Exception ex)
            {
                sb.AppendLine($"Error accessing Button onClick info: {ex.Message}");
                sb.AppendLine();
            }
        }

        /// <summary>
        /// 显示Image组件的详细信息
        /// </summary>
        public void DisplayImageInfo(StringBuilder sb, Image image)
        {
            sb.AppendLine("=== Image Component Info ===");
            
            // 基础属性
            sb.AppendLine($"Color: RGBA({image.color.r:F2}, {image.color.g:F2}, {image.color.b:F2}, {image.color.a:F2})");
            sb.AppendLine($"Raycast Target: {image.raycastTarget}");
            sb.AppendLine($"Raycast Padding: {image.raycastPadding}");
            sb.AppendLine();

            // Image类型
            sb.AppendLine($"Image Type: {image.type}");
            
            // 根据Image类型显示不同信息
            if (image.type == Image.Type.Simple || image.type == Image.Type.Tiled)
            {
                sb.AppendLine($"Fill Amount: {image.fillAmount}");
                sb.AppendLine($"Fill Method: {image.fillMethod}");
                sb.AppendLine($"Fill Origin: {image.fillOrigin}");
                sb.AppendLine($"Fill Clockwise: {image.fillClockwise}");
            }
            
            if (image.type == Image.Type.Sliced || image.type == Image.Type.Tiled)
            {
                sb.AppendLine($"Pixels Per Unit Multiplier: {image.pixelsPerUnitMultiplier}");
            }
            
            sb.AppendLine();

            // Sprite信息
            if (image.sprite != null)
            {
                sb.AppendLine($"Sprite: {image.sprite.name}");
                sb.AppendLine($"Sprite Rect: {image.sprite.rect}");
                sb.AppendLine($"Sprite Pixels Per Unit: {image.sprite.pixelsPerUnit}");
            }
            else
            {
                sb.AppendLine("Sprite: None");
            }
            
            sb.AppendLine();

            // Material信息
            if (image.material != null && image.material != image.defaultMaterial)
            {
                sb.AppendLine($"Material: {image.material.name}");
            }
            else
            {
                sb.AppendLine("Material: Default");
            }
            
            sb.AppendLine();
        }

        /// <summary>
        /// 显示TextMeshProUGUI组件的详细信息
        /// </summary>
        public void DisplayTextMeshProUGUIInfo(StringBuilder sb, TextMeshProUGUI textMeshPro)
        {
            sb.AppendLine("=== TextMeshProUGUI Component Info ===");
            
            // 文本内容
            sb.AppendLine($"Text: {(string.IsNullOrEmpty(textMeshPro.text) ? "(Empty)" : textMeshPro.text)}");
            sb.AppendLine($"Text Length: {textMeshPro.text?.Length ?? 0}");
            sb.AppendLine();

            // 基础属性
            sb.AppendLine($"Font Size: {textMeshPro.fontSize}");
            sb.AppendLine($"Font Style: {textMeshPro.fontStyle}");
            sb.AppendLine($"Color: RGBA({textMeshPro.color.r:F2}, {textMeshPro.color.g:F2}, {textMeshPro.color.b:F2}, {textMeshPro.color.a:F2})");
            sb.AppendLine($"Raycast Target: {textMeshPro.raycastTarget}");
            sb.AppendLine();

            // 对齐方式
            sb.AppendLine($"Alignment: {textMeshPro.alignment}");
            sb.AppendLine();

            // 布局和换行
            sb.AppendLine($"Enable Word Wrapping: {textMeshPro.enableWordWrapping}");
            sb.AppendLine($"Overflow Mode: {textMeshPro.overflowMode}");
            sb.AppendLine();

            // 字体相关
            if (textMeshPro.font != null)
            {
                sb.AppendLine($"Font: {textMeshPro.font.name}");
                sb.AppendLine($"Font Asset: {textMeshPro.font.name}");
            }
            else
            {
                sb.AppendLine("Font: None");
            }
            sb.AppendLine($"Font Size Min: {textMeshPro.fontSizeMin}");
            sb.AppendLine($"Font Size Max: {textMeshPro.fontSizeMax}");
            sb.AppendLine();

            // 间距和行距
            sb.AppendLine($"Character Spacing: {textMeshPro.characterSpacing}");
            sb.AppendLine($"Line Spacing: {textMeshPro.lineSpacing}");
            sb.AppendLine($"Paragraph Spacing: {textMeshPro.paragraphSpacing}");
            sb.AppendLine();

            // 边距
            sb.AppendLine($"Margin: Left={textMeshPro.margin.x:F1}, Top={textMeshPro.margin.y:F1}, Right={textMeshPro.margin.z:F1}, Bottom={textMeshPro.margin.w:F1}");
            sb.AppendLine();

            // 其他属性
            sb.AppendLine($"Rich Text: {textMeshPro.richText}");
            sb.AppendLine($"Raycast Padding: {textMeshPro.raycastPadding}");
            sb.AppendLine($"Maskable: {textMeshPro.maskable}");
            sb.AppendLine();
        }

        /// <summary>
        /// 显示ScrollRect组件的详细信息
        /// </summary>
        public void DisplayScrollRectInfo(StringBuilder sb, ScrollRect scrollRect)
        {
            sb.AppendLine("=== ScrollRect Component Info ===");
            
            // 基础属性
            sb.AppendLine($"Horizontal: {scrollRect.horizontal}");
            sb.AppendLine($"Vertical: {scrollRect.vertical}");
            sb.AppendLine($"Movement Type: {scrollRect.movementType}");
            sb.AppendLine($"Elasticity: {scrollRect.elasticity}");
            sb.AppendLine($"Inertia: {scrollRect.inertia}");
            sb.AppendLine($"Deceleration Rate: {scrollRect.decelerationRate}");
            sb.AppendLine($"Scroll Sensitivity: {scrollRect.scrollSensitivity}");
            sb.AppendLine();

            // 视图和内容
            if (scrollRect.viewport != null)
            {
                sb.AppendLine($"Viewport: {scrollRect.viewport.name}");
            }
            else
            {
                sb.AppendLine("Viewport: None");
            }

            if (scrollRect.content != null)
            {
                sb.AppendLine($"Content: {scrollRect.content.name}");
            }
            else
            {
                sb.AppendLine("Content: None");
            }
            sb.AppendLine();

            // 滚动条
            if (scrollRect.horizontalScrollbar != null)
            {
                sb.AppendLine($"Horizontal Scrollbar: {scrollRect.horizontalScrollbar.name}");
            }
            else
            {
                sb.AppendLine("Horizontal Scrollbar: None");
            }

            if (scrollRect.verticalScrollbar != null)
            {
                sb.AppendLine($"Vertical Scrollbar: {scrollRect.verticalScrollbar.name}");
            }
            else
            {
                sb.AppendLine("Vertical Scrollbar: None");
            }
            sb.AppendLine();

            // 滚动位置
            sb.AppendLine($"Horizontal Normalized Position: {scrollRect.horizontalNormalizedPosition:F3}");
            sb.AppendLine($"Vertical Normalized Position: {scrollRect.verticalNormalizedPosition:F3}");
            sb.AppendLine($"Velocity: {scrollRect.velocity}");
            sb.AppendLine();
        }

        /// <summary>
        /// 显示Mask组件的详细信息
        /// </summary>
        public void DisplayMaskInfo(StringBuilder sb, Mask mask)
        {
            sb.AppendLine("=== Mask Component Info ===");
            
            // 基础属性
            sb.AppendLine($"Show Mask Graphic: {mask.showMaskGraphic}");
            sb.AppendLine();

            // RectTransform信息（Mask通常基于RectTransform）
            RectTransform? rectTransform = mask.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                sb.AppendLine($"RectTransform Size: {rectTransform.sizeDelta}");
                sb.AppendLine($"RectTransform Anchored Position: {rectTransform.anchoredPosition}");
            }
            sb.AppendLine();
        }

        /// <summary>
        /// 显示RectTransform组件的详细信息
        /// </summary>
        public void DisplayRectTransformInfo(StringBuilder sb, RectTransform rectTransform)
        {
            sb.AppendLine("=== RectTransform Component Info ===");
            
            // 锚点信息
            sb.AppendLine($"Anchor Min: ({rectTransform.anchorMin.x:F3}, {rectTransform.anchorMin.y:F3})");
            sb.AppendLine($"Anchor Max: ({rectTransform.anchorMax.x:F3}, {rectTransform.anchorMax.y:F3})");
            sb.AppendLine($"Pivot: ({rectTransform.pivot.x:F3}, {rectTransform.pivot.y:F3})");
            sb.AppendLine();

            // 位置和尺寸
            sb.AppendLine($"Anchored Position: ({rectTransform.anchoredPosition.x:F1}, {rectTransform.anchoredPosition.y:F1})");
            sb.AppendLine($"Size Delta: ({rectTransform.sizeDelta.x:F1}, {rectTransform.sizeDelta.y:F1})");
            sb.AppendLine($"Rect: {rectTransform.rect}");
            sb.AppendLine();

            // 旋转和缩放
            sb.AppendLine($"Local Rotation: {rectTransform.localRotation.eulerAngles}");
            sb.AppendLine($"Local Scale: ({rectTransform.localScale.x:F2}, {rectTransform.localScale.y:F2}, {rectTransform.localScale.z:F2})");
            sb.AppendLine();

            // 偏移量
            sb.AppendLine($"Offset Min: ({rectTransform.offsetMin.x:F1}, {rectTransform.offsetMin.y:F1})");
            sb.AppendLine($"Offset Max: ({rectTransform.offsetMax.x:F1}, {rectTransform.offsetMax.y:F1})");
            sb.AppendLine();

            // 父级信息
            if (rectTransform.parent != null)
            {
                sb.AppendLine($"Parent: {rectTransform.parent.name}");
            }
            else
            {
                sb.AppendLine("Parent: None (Root)");
            }
            sb.AppendLine();

            // 子物体数量
            sb.AppendLine($"Child Count: {rectTransform.childCount}");
            sb.AppendLine();
        }
    }
}

