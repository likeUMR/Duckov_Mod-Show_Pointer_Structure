using UnityEngine;
using System.IO;
using System;
using UnityEngine.SceneManagement;

namespace HideTheEquipment
{
    /// <summary>
    /// 主Mod行为类，负责协调各个功能模块
    /// </summary>
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        private SceneHierarchyExporter? sceneExporter;
        private GameObjectChildrenActivator? childrenActivator;
        
        private bool isEnabled = false;
        
        // 是否已经触发过默认状态推进
        private bool hasTriggeredDefaultState = false;
        
        // 保存的状态索引（在OnEnable时读取，在首次进入场景时应用）
        private int savedStateIndex = 0;

        // === OnEnable：启用时创建所有组件 ===
        void OnEnable()
        {
            isEnabled = true;
            
            // 初始化功能模块
            sceneExporter = new SceneHierarchyExporter();
            childrenActivator = new GameObjectChildrenActivator();
            
            // 读取保存的状态索引（但不立即应用，等进入场景后再应用）
            savedStateIndex = childrenActivator.LoadSavedState();
            
            Debug.Log($"[ModBehaviour] OnEnable: 已读取保存的状态索引: {savedStateIndex}");
        }

        // === OnDisable：禁用时清理所有组件 ===
        void OnDisable()
        {
            isEnabled = false;
            
            // 清理各个功能模块
            childrenActivator?.Cleanup();
            
            sceneExporter = null;
            childrenActivator = null;
            
            Debug.Log("[ModBehaviour] OnDisable: All components cleaned up.");
        }

        // === 游戏循环 ===
        void Update()
        {
            // 如果未启用，不执行任何逻辑
            if (!isEnabled)
            {
                return;
            }

            // 检测是否第一次进入 Base_SceneV2 场景
            if (!hasTriggeredDefaultState)
            {
                if (SceneManager.GetActiveScene().name == "Base_SceneV2")
                {
                    // 如果保存的状态不是0，需要Toggle到目标状态
                    // 因为当前状态是0（关闭），需要Toggle savedStateIndex次才能到达目标状态
                    if (savedStateIndex > 0)
                    {
                        for (int i = 0; i < savedStateIndex; i++)
                        {
                            childrenActivator?.ToggleEnabled();
                        }
                    }
                    
                    hasTriggeredDefaultState = true;
                    Debug.Log($"[ModBehaviour] 首次进入 Base_SceneV2 场景，已恢复到状态索引: {savedStateIndex}");
                }
            }

            // 检查是否按下数字键9，切换子物体激活状态管理
            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                childrenActivator?.ToggleEnabled();
            }

            // 管理指定GameObject的激活状态（如果已启用）
            if (childrenActivator != null && childrenActivator.IsEnabled())
            {
                childrenActivator.Update();
            }

            // 检查是否按下数字键8，导出场景层级结构
            // if (Input.GetKeyDown(KeyCode.Alpha8))
            // {
            //     ExportSceneHierarchy();
            // }
        }

        /// <summary>
        /// 导出场景层级结构到JSON文件
        /// </summary>
        private void ExportSceneHierarchy()
        {
            if (sceneExporter == null)
            {
                Debug.LogError("[ModBehaviour] SceneExporter is null!");
                return;
            }

            // 生成文件名（使用时间戳）
            string fileName = $"SceneHierarchy_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            // 获取桌面路径
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string filePath = Path.Combine(desktopPath, fileName);

            sceneExporter.ExportSceneHierarchy(filePath);
            Debug.Log($"[ModBehaviour] 场景层级结构已导出到: {filePath}");
        }
    }
}