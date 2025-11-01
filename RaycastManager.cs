using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace GetGameObjectStructure
{
    /// <summary>
    /// 射线检测模式
    /// </summary>
    public enum RaycastMode
    {
        /// <summary>
        /// UI模式：优先检测UI元素，然后检测3D物体
        /// </summary>
        UIMode,
        
        /// <summary>
        /// 场景物体模式：只检测3D物体，屏蔽UI元素
        /// </summary>
        SceneObjectMode
    }

    /// <summary>
    /// 负责UI和3D射线检测
    /// </summary>
    public class RaycastManager
    {
        private Camera? mainCamera;
        private RaycastMode currentMode = RaycastMode.SceneObjectMode; // 默认场景物体模式
        
        // 索引变量
        private int currentUIIndex = 0;
        private int current3DIndex = 0;
        private int currentUIResultCount = 0;
        private int current3DHitCount = 0;

        public RaycastManager()
        {
            // 查找主摄像机
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = Object.FindObjectOfType<Camera>();
            }
        }

        /// <summary>
        /// 切换射线检测模式
        /// </summary>
        public void ToggleMode()
        {
            currentMode = currentMode == RaycastMode.UIMode 
                ? RaycastMode.SceneObjectMode 
                : RaycastMode.UIMode;
            
            // 切换模式时重置索引
            currentUIIndex = 0;
            current3DIndex = 0;
            
            Debug.Log($"[RaycastManager] 模式切换为: {currentMode}");
        }

        /// <summary>
        /// 获取当前检测模式
        /// </summary>
        public RaycastMode GetCurrentMode()
        {
            return currentMode;
        }

        /// <summary>
        /// 增加当前选中物体的索引（切换到下一个物体）
        /// </summary>
        public void IncrementIndex()
        {
            if (currentMode == RaycastMode.UIMode)
            {
                currentUIIndex++;
            }
            else
            {
                current3DIndex++;
            }
        }

        /// <summary>
        /// 减少当前选中物体的索引（切换到上一个物体）
        /// </summary>
        public void DecrementIndex()
        {
            if (currentMode == RaycastMode.UIMode)
            {
                currentUIIndex = Mathf.Max(0, currentUIIndex - 1);
            }
            else
            {
                current3DIndex = Mathf.Max(0, current3DIndex - 1);
            }
        }

        /// <summary>
        /// 获取当前选中的物体索引
        /// </summary>
        public int GetCurrentIndex()
        {
            if (currentMode == RaycastMode.UIMode)
            {
                return currentUIIndex;
            }
            else
            {
                return current3DIndex;
            }
        }

        /// <summary>
        /// 获取当前模式下检测到的物体总数
        /// </summary>
        public int GetHitCount()
        {
            if (currentMode == RaycastMode.UIMode)
            {
                return currentUIResultCount;
            }
            else
            {
                return current3DHitCount;
            }
        }

        /// <summary>
        /// 执行射线检测，根据模式决定检测UI还是只检测3D物体
        /// </summary>
        public GameObject? PerformRaycast()
        {
            if (currentMode == RaycastMode.UIMode)
            {
                // UI模式：优先检测UI元素，然后检测3D物体
                GameObject? hitObject = RaycastUI();
                
                // 如果UI检测没有命中，检测3D物体
                if (hitObject == null)
                {
                    hitObject = Raycast3D();
                    // UI模式下检测3D时，重置UI计数
                    currentUIResultCount = 0;
                }
                else
                {
                    // UI检测成功时，重置3D计数
                    current3DHitCount = 0;
                }

                return hitObject;
            }
            else
            {
                // 场景物体模式：只检测3D物体，屏蔽UI元素
                return Raycast3D();
            }
        }

        /// <summary>
        /// 检测UI元素
        /// </summary>
        private GameObject? RaycastUI()
        {
            if (EventSystem.current == null)
            {
                return null;
            }

            // 使用EventSystem检测UI
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count > 0)
            {
                // 保存检测结果数量
                currentUIResultCount = results.Count;
                // 确保索引在有效范围内
                currentUIIndex = Mathf.Clamp(currentUIIndex, 0, results.Count - 1);
                // 返回当前索引对应的UI元素
                return results[currentUIIndex].gameObject;
            }

            currentUIResultCount = 0;
            return null;
        }

        /// <summary>
        /// 检测3D物体
        /// </summary>
        private GameObject? Raycast3D()
        {
            // 重新查找摄像机（防止初始化时摄像机不存在或运行时发生变化）
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    mainCamera = Object.FindObjectOfType<Camera>();
                }
                
                if (mainCamera == null)
                {
                    Debug.LogWarning("[RaycastManager] 无法找到摄像机！");
                    return null;
                }
            }

            // 从摄像机位置向鼠标位置发射射线
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            
            // 使用 RaycastAll 检测所有命中的物体（不限制层）
            RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);
            
            if (hits.Length > 0)
            {
                // 保存检测结果数量
                current3DHitCount = hits.Length;
                // 确保索引在有效范围内
                current3DIndex = Mathf.Clamp(current3DIndex, 0, hits.Length - 1);
                // 返回当前索引对应的物体
                RaycastHit hit = hits[current3DIndex];
                Debug.Log($"[RaycastManager] 3D Hit [{current3DIndex + 1}/{hits.Length}]: {hit.collider.gameObject.name} (Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)})");
                return hit.collider.gameObject;
            }
            
            current3DHitCount = 0;
            // 如果没有找到，尝试使用 Physics2D（如果是2D游戏）
            // 将屏幕坐标转换为世界坐标
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCamera.nearClipPlane));
            // 从摄像机位置向鼠标世界位置发射2D射线
            RaycastHit2D hit2D = Physics2D.Raycast(worldPos, Vector2.zero, 0f);
            if (hit2D.collider != null)
            {
                Debug.Log($"[RaycastManager] 2D Hit: {hit2D.collider.gameObject.name}");
                return hit2D.collider.gameObject;
            }
            
            // 也可以尝试从摄像机位置向鼠标位置发射射线
            Ray ray2D = mainCamera.ScreenPointToRay(Input.mousePosition);
            hit2D = Physics2D.GetRayIntersection(ray2D, Mathf.Infinity);
            if (hit2D.collider != null)
            {
                Debug.Log($"[RaycastManager] 2D Hit (Ray Intersection): {hit2D.collider.gameObject.name}");
                return hit2D.collider.gameObject;
            }

            // 调试信息：输出详细信息帮助排查问题
            Debug.LogWarning($"[RaycastManager] 未检测到3D物体。鼠标位置: {Input.mousePosition}, 摄像机: {mainCamera?.name ?? "null"}");
            return null;
        }

        /// <summary>
        /// 根据指标向上获取父级对象
        /// </summary>
        public GameObject? GetParentAtLevel(GameObject obj, int levelOffset)
        {
            if (levelOffset <= 0 || obj == null)
            {
                return obj;
            }

            Transform current = obj.transform;
            for (int i = 0; i < levelOffset; i++)
            {
                if (current.parent == null)
                {
                    // 已经是最顶层了，停止迭代
                    Debug.Log($"Reached root level at offset {i + 1}");
                    break;
                }
                current = current.parent;
            }

            return current.gameObject;
        }

        /// <summary>
        /// 清理引用
        /// </summary>
        public void Cleanup()
        {
            mainCamera = null;
        }
    }
}

