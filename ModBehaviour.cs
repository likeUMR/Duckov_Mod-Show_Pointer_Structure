using UnityEngine;

namespace GetGameObjectStructure
{
    /// <summary>
    /// 主Mod行为类，负责协调各个功能模块
    /// </summary>
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private UIManager? uiManager;
        private RaycastManager? raycastManager;
        private HierarchyInspector? hierarchyInspector;
        
        private bool isEnabled = false;
        private int parentLevelOffset = 0; // 父级指标偏移量（鼠标滚轮控制）

        // === OnEnable：启用时创建所有组件 ===
        void OnEnable()
        {
            isEnabled = true;
            
            // 初始化各个功能模块
            uiManager = new UIManager(transform);
            uiManager.CreateUICanvas();
            uiManager.CreateInfoText();
            
            raycastManager = new RaycastManager();
            hierarchyInspector = new HierarchyInspector();
            
            Debug.Log("[ModBehaviour] OnEnable: All components created.");
        }

        // === OnDisable：禁用时清理所有组件 ===
        void OnDisable()
        {
            isEnabled = false;
            
            // 清理各个功能模块
            uiManager?.Cleanup();
            raycastManager?.Cleanup();
            
            uiManager = null;
            raycastManager = null;
            hierarchyInspector = null;
            
            Debug.Log("[ModBehaviour] OnDisable: All components cleaned up.");
        }

        // === 游戏循环：每帧检测射线 ===
        void Update()
        {
            // 如果未启用，不执行任何逻辑
            if (!isEnabled)
            {
                return;
            }

            // 检测鼠标滚轮输入来控制父级指标
            HandleScrollWheelInput();

            // 每帧检测鼠标位置下的物体
            PerformRaycast();

            // 检查是否按下数字键9，切换射线检测模式
            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                raycastManager?.ToggleMode();
            }
        }

        /// <summary>
        /// 处理鼠标滚轮输入，控制父级指标偏移
        /// </summary>
        private void HandleScrollWheelInput()
        {
            float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
            if (scrollDelta > 0f)
            {
                // 向上滚动，父级指标+1
                parentLevelOffset++;
                Debug.Log($"Parent Level Offset: {parentLevelOffset}");
            }
            else if (scrollDelta < 0f)
            {
                // 向下滚动，父级指标-1，最少为0
                parentLevelOffset = Mathf.Max(0, parentLevelOffset - 1);
                Debug.Log($"Parent Level Offset: {parentLevelOffset}");
            }
        }

        /// <summary>
        /// 执行射线检测并更新显示
        /// </summary>
        private void PerformRaycast()
        {
            // 如果未启用或UI未创建，不执行检测
            if (!isEnabled || uiManager == null || raycastManager == null || hierarchyInspector == null)
            {
                return;
            }

            if (!uiManager.IsUIAvailable())
            {
                return;
            }

            // 执行射线检测
            GameObject? hitObject = raycastManager.PerformRaycast();

            // 根据父级指标向上迭代获取父级对象
            if (hitObject != null && parentLevelOffset > 0)
            {
                hitObject = raycastManager.GetParentAtLevel(hitObject, parentLevelOffset);
            }

            // 更新显示
            if (hitObject != null)
            {
                string hierarchyInfo = hierarchyInspector.GetHierarchyInfo(hitObject);
                
                // 添加模式信息
                string modeInfo = raycastManager?.GetCurrentMode() == RaycastMode.UIMode 
                    ? "[UI Mode]" 
                    : "[Scene Object Mode]";
                
                // 如果有父级偏移，在信息中显示
                if (parentLevelOffset > 0)
                {
                    hierarchyInfo = $"{modeInfo} [Parent Level +{parentLevelOffset}]\n\n" + hierarchyInfo;
                }
                else
                {
                    hierarchyInfo = $"{modeInfo}\n\n" + hierarchyInfo;
                }
                
                uiManager.UpdateInfoText(hierarchyInfo);
            }
            else
            {
                // 显示当前模式和提示信息
                string modeInfo = raycastManager?.GetCurrentMode() == RaycastMode.UIMode 
                    ? "[UI Mode]" 
                    : "[Scene Object Mode]";
                uiManager.UpdateInfoText($"{modeInfo}\n\nNo object detected");
            }
        }
    }
}