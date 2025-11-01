using UnityEngine;
using UnityEngine.UI;

namespace GetGameObjectStructure
{
    /// <summary>
    /// 负责触发特定按钮的点击事件
    /// </summary>
    public class ButtonInvoker
    {
        private bool isEnabled = false;

        public void SetEnabled(bool enabled)
        {
            isEnabled = enabled;
        }

        /// <summary>
        /// 触发指定路径的Button点击事件
        /// </summary>
        public void TriggerPasteButton()
        {
            // 如果未启用，不执行
            if (!isEnabled)
            {
                return;
            }

            // 通过层级路径精确查找（推荐，最稳定）
            string targetPath = "CustomFace/CustomFaceCanvas/DATA_3/Paste";
            GameObject pasteObject = GameObject.Find(targetPath);

            if (pasteObject == null)
            {
                Debug.LogError($"[ButtonInvoker] 未找到路径为 '{targetPath}' 的对象。");
                return;
            }

            // 获取 Button 组件
            Button pasteButton = pasteObject.GetComponent<Button>();

            if (pasteButton == null)
            {
                Debug.LogError($"[ButtonInvoker] 在对象 '{pasteObject.name}' 上未找到 Button 组件。");
                return;
            }

            // 检查按钮是否可交互（避免调用灰色不可点的按钮）
            if (!pasteButton.interactable)
            {
                Debug.LogWarning($"[ButtonInvoker] 按钮 '{pasteObject.name}' 当前不可交互（interactable=false）。");
                return;
            }

            // 执行点击！这会触发所有绑定的监听器，包括 Interact.PastyDataAndApply
            pasteButton.onClick.Invoke();

            Debug.Log($"[ButtonInvoker] 已成功触发按钮 '{pasteObject.name}' 的点击事件！");
        }
    }
}

