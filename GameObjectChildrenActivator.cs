using UnityEngine;
using System.Collections.Generic;

namespace HideTheEquipment
{
    /// <summary>
    /// 负责管理指定GameObject的激活状态
    /// </summary>
    public class GameObjectChildrenActivator
    {
        private bool isEnabled = false;
        
        // 基础路径（相对于Character(Clone)的完整路径）
        private string basePath = "Character(Clone)/ModelRoot/0_CharacterModel_Custom_Template(Clone)/CustomFaceInstance/Armature/Root/Pelvis/Spine.001/Spine.002/Spine.003/Spine.004";
        
        // 基础路径的GameObject缓存
        private GameObject? cachedBaseObject = null;
        private bool isBaseObjectValid = false;
        
        // 需要设置为active的路径列表（相对路径）
        private List<string> activeList = new List<string>();
        
        // 需要设置为not active的路径列表（相对路径）
        private List<string> inactiveList = new List<string>();
        
        // 已找到的GameObject缓存（路径 -> GameObject）
        private Dictionary<string, GameObject> cachedObjects = new Dictionary<string, GameObject>();
        
        // 原始状态记录（路径 -> active状态）
        private Dictionary<string, bool> originalStates = new Dictionary<string, bool>();
        
        // 是否已记录原始状态
        private bool hasRecordedStates = false;
        
        // 特殊处理的路径常量
        private const string HELMAT_SOCKET_PATH = "Head/HelmatSocket";
        private const string HEAD_COLLIDER_NAME = "HeadCollider(Clone)";

        public GameObjectChildrenActivator()
        {
            // 初始化列表
            // activeList: 需要设置为active的路径列表
            activeList.Add("Head/HairSocket");
            activeList.Add("Head/MouthSocket");
            
            // inactiveList: 需要设置为not active的路径列表
            inactiveList.Add("Head/HelmatSocket");
            inactiveList.Add("Head/FaceMaskSocket");
            inactiveList.Add("ArmorSocket");
            inactiveList.Add("BackpackSocket");
        }

        /// <summary>
        /// 切换启用状态
        /// </summary>
        public void ToggleEnabled()
        {
            if (!isEnabled)
            {
                // 开启：先记录所有目标物体的激活状态
                RecordOriginalStates();
                isEnabled = true;
                Debug.Log("[GameObjectChildrenActivator] 功能已开启，已记录原始状态");
            }
            else
            {
                // 关闭：先停止每帧修改，然后恢复原始状态
                isEnabled = false;
                RestoreOriginalStates();
                Debug.Log("[GameObjectChildrenActivator] 功能已关闭，已恢复原始状态");
            }
        }

        /// <summary>
        /// 获取当前启用状态
        /// </summary>
        public bool IsEnabled()
        {
            return isEnabled;
        }

        /// <summary>
        /// 每帧更新：查找目标物体并管理激活状态
        /// </summary>
        public void Update()
        {
            // 如果未启用，不执行任何逻辑
            if (!isEnabled)
            {
                return;
            }

            // 查找或验证基础路径的GameObject
            if (!isBaseObjectValid || cachedBaseObject == null)
            {
                FindBaseObject();
            }
            else
            {
                // 每帧校验基础对象是否仍然有效
                if (cachedBaseObject == null || !cachedBaseObject)
                {
                    isBaseObjectValid = false;
                    cachedBaseObject = null;
                    cachedObjects.Clear(); // 基础对象失效，清除所有缓存
                    FindBaseObject();
                }
            }

            // 如果基础对象有效且已记录原始状态，处理列表中的对象
            if (isBaseObjectValid && cachedBaseObject != null && hasRecordedStates)
            {
                ProcessActiveList();
                ProcessInactiveList();
            }
        }

        /// <summary>
        /// 查找基础路径的GameObject
        /// </summary>
        private void FindBaseObject()
        {
            GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
            
            string[] pathParts = basePath.Split('/');
            
            foreach (GameObject obj in allObjects)
            {
                if (IsMatchPath(obj, pathParts))
                {
                    cachedBaseObject = obj;
                    isBaseObjectValid = true;
                    Debug.Log($"[GameObjectChildrenActivator] 找到基础对象: {obj.name}");
                    return;
                }
            }

            // 如果没找到，重置状态
            cachedBaseObject = null;
            isBaseObjectValid = false;
        }

        /// <summary>
        /// 检查GameObject是否匹配指定路径
        /// </summary>
        private bool IsMatchPath(GameObject obj, string[] pathParts)
        {
            Transform current = obj.transform;

            // 从路径的最后一部分开始匹配（当前物体）
            if (current.name != pathParts[pathParts.Length - 1])
            {
                return false;
            }

            // 向上遍历父级，检查路径是否匹配
            for (int i = pathParts.Length - 2; i >= 0; i--)
            {
                if (current.parent == null)
                {
                    return false;
                }
                current = current.parent;
                if (current.name != pathParts[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 处理active列表
        /// </summary>
        private void ProcessActiveList()
        {
            foreach (string relativePath in activeList)
            {
                GameObject? targetObj = GetOrFindGameObject(relativePath);
                if (targetObj != null)
                {
                    if (!targetObj.activeSelf)
                    {
                        targetObj.SetActive(true);
                    }
                }
            }
        }

        /// <summary>
        /// 处理inactive列表
        /// </summary>
        private void ProcessInactiveList()
        {
            foreach (string relativePath in inactiveList)
            {
                // 特殊处理：Head/HelmatSocket
                if (relativePath == HELMAT_SOCKET_PATH)
                {
                    ProcessHelmatSocketSpecial();
                }
                else
                {
                    // 普通处理
                    GameObject? targetObj = GetOrFindGameObject(relativePath);
                    if (targetObj != null)
                    {
                        if (targetObj.activeSelf)
                        {
                            targetObj.SetActive(false);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 特殊处理 HelmatSocket：关闭其除了 HeadCollider(Clone) 之外的所有子节点
        /// </summary>
        private void ProcessHelmatSocketSpecial()
        {
            GameObject? helmatSocket = GetOrFindGameObject(HELMAT_SOCKET_PATH);
            if (helmatSocket == null)
            {
                return;
            }

            Transform helmatTransform = helmatSocket.transform;
            
            // 遍历所有一级子节点
            for (int i = 0; i < helmatTransform.childCount; i++)
            {
                Transform child = helmatTransform.GetChild(i);
                GameObject childObject = child.gameObject;
                
                // 如果不是 HeadCollider(Clone)，设置为非激活状态
                if (childObject.name != HEAD_COLLIDER_NAME)
                {
                    if (childObject.activeSelf)
                    {
                        childObject.SetActive(false);
                    }
                }
            }
        }

        /// <summary>
        /// 获取或查找GameObject（带缓存）
        /// </summary>
        private GameObject? GetOrFindGameObject(string relativePath)
        {
            // 检查缓存
            if (cachedObjects.ContainsKey(relativePath))
            {
                GameObject cached = cachedObjects[relativePath];
                // 校验缓存是否有效
                if (cached != null && cached)
                {
                    return cached;
                }
                else
                {
                    // 缓存失效，移除
                    cachedObjects.Remove(relativePath);
                }
            }

            // 缓存中没有或失效，查找
            if (cachedBaseObject == null || !cachedBaseObject)
            {
                return null;
            }

            // 从基础对象开始查找
            GameObject? found = FindGameObjectByRelativePath(cachedBaseObject.transform, relativePath);
            if (found != null)
            {
                cachedObjects[relativePath] = found;
                return found;
            }

            return null;
        }

        /// <summary>
        /// 根据相对路径查找GameObject（从基础Transform开始）
        /// </summary>
        private GameObject? FindGameObjectByRelativePath(Transform baseTransform, string relativePath)
        {
            string[] pathParts = relativePath.Split('/');
            Transform current = baseTransform;

            // 按路径向下查找
            for (int i = 0; i < pathParts.Length; i++)
            {
                string partName = pathParts[i];
                Transform? foundChild = null;

                // 在当前Transform的子物体中查找
                for (int j = 0; j < current.childCount; j++)
                {
                    Transform child = current.GetChild(j);
                    if (child.name == partName)
                    {
                        foundChild = child;
                        break;
                    }
                }

                if (foundChild == null)
                {
                    // 路径不匹配
                    return null;
                }

                current = foundChild;
            }

            return current.gameObject;
        }

        /// <summary>
        /// 记录所有目标物体的原始激活状态
        /// </summary>
        private void RecordOriginalStates()
        {
            // 先查找基础对象（如果还没找到）
            if (!isBaseObjectValid || cachedBaseObject == null)
            {
                FindBaseObject();
            }

            // 如果基础对象无效，无法记录
            if (!isBaseObjectValid || cachedBaseObject == null)
            {
                Debug.LogWarning("[GameObjectChildrenActivator] 无法找到基础对象，无法记录原始状态");
                return;
            }

            // 清除之前的记录
            originalStates.Clear();
            hasRecordedStates = false;

            // 记录activeList中的所有对象状态
            foreach (string relativePath in activeList)
            {
                GameObject? targetObj = GetOrFindGameObject(relativePath);
                if (targetObj != null)
                {
                    originalStates[relativePath] = targetObj.activeSelf;
                }
            }

            // 记录inactiveList中的所有对象状态（跳过 Head/HelmatSocket）
            foreach (string relativePath in inactiveList)
            {
                // 特殊处理：不记录 Head/HelmatSocket 的原始状态
                if (relativePath == HELMAT_SOCKET_PATH)
                {
                    continue;
                }
                
                GameObject? targetObj = GetOrFindGameObject(relativePath);
                if (targetObj != null)
                {
                    originalStates[relativePath] = targetObj.activeSelf;
                }
            }

            hasRecordedStates = true;
            Debug.Log($"[GameObjectChildrenActivator] 已记录 {originalStates.Count} 个对象的原始状态");
        }

        /// <summary>
        /// 恢复所有目标物体的原始激活状态
        /// </summary>
        private void RestoreOriginalStates()
        {
            if (!hasRecordedStates || originalStates.Count == 0)
            {
                Debug.LogWarning("[GameObjectChildrenActivator] 没有记录原始状态，无法恢复");
                return;
            }

            int restoredCount = 0;

            // 恢复所有记录的状态
            foreach (var kvp in originalStates)
            {
                string relativePath = kvp.Key;
                bool originalActive = kvp.Value;

                GameObject? targetObj = GetOrFindGameObject(relativePath);
                if (targetObj != null)
                {
                    if (targetObj.activeSelf != originalActive)
                    {
                        targetObj.SetActive(originalActive);
                        restoredCount++;
                    }
                }
            }

            // 特殊处理：如果 inactiveList 中包含 Head/HelmatSocket，开启其所有一级子节点
            if (inactiveList.Contains(HELMAT_SOCKET_PATH))
            {
                RestoreHelmatSocketChildren();
            }

            // 清除记录
            originalStates.Clear();
            hasRecordedStates = false;

            // 清理缓存
            cachedBaseObject = null;
            isBaseObjectValid = false;
            cachedObjects.Clear();

            Debug.Log($"[GameObjectChildrenActivator] 已恢复 {restoredCount} 个对象的原始状态");
        }

        /// <summary>
        /// 恢复 HelmatSocket 的所有一级子节点（开启它们）
        /// </summary>
        private void RestoreHelmatSocketChildren()
        {
            GameObject? helmatSocket = GetOrFindGameObject(HELMAT_SOCKET_PATH);
            if (helmatSocket == null)
            {
                return;
            }

            Transform helmatTransform = helmatSocket.transform;
            
            // 遍历所有一级子节点并开启它们
            for (int i = 0; i < helmatTransform.childCount; i++)
            {
                Transform child = helmatTransform.GetChild(i);
                GameObject childObject = child.gameObject;
                
                if (!childObject.activeSelf)
                {
                    childObject.SetActive(true);
                }
            }
            
            Debug.Log("[GameObjectChildrenActivator] 已恢复 HelmatSocket 的所有一级子节点");
        }

        /// <summary>
        /// 清理缓存
        /// </summary>
        public void Cleanup()
        {
            cachedBaseObject = null;
            isBaseObjectValid = false;
            cachedObjects.Clear();
            originalStates.Clear();
            hasRecordedStates = false;
        }
    }
}

