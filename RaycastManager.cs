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
        private RaycastMode currentMode = RaycastMode.UIMode; // 默认UI模式

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
                // 返回第一个命中的UI元素
                GameObject hitObj = results[0].gameObject;
                return hitObj;
            }

            return null;
        }

        /// <summary>
        /// 检测3D物体
        /// </summary>
        private GameObject? Raycast3D()
        {
            if (mainCamera == null)
            {
                return null;
            }

            // 从摄像机位置向鼠标位置发射射线
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // 检测所有层级的碰撞
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                Debug.Log($"3D Hit: {hit.collider.gameObject.name}");
                return hit.collider.gameObject;
            }

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

