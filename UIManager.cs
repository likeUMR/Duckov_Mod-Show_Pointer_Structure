using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GetGameObjectStructure
{
    /// <summary>
    /// 负责UI Canvas和信息文本的创建和管理
    /// </summary>
    public class UIManager
    {
        private Canvas? uiCanvas;
        private GraphicRaycaster? graphicRaycaster;
        private TextMeshProUGUI? infoText;
        private GameObject? infoTextObj;
        private GameObject? canvasObj;
        private Transform parentTransform;

        public UIManager(Transform parent)
        {
            parentTransform = parent;
        }

        /// <summary>
        /// 创建UI Canvas
        /// </summary>
        public void CreateUICanvas()
        {
            // 创建Canvas GameObject
            canvasObj = new GameObject("ModUICanvas");
            canvasObj.transform.SetParent(parentTransform);

            // 添加Canvas组件
            uiCanvas = canvasObj.AddComponent<Canvas>();
            uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            uiCanvas.sortingOrder = 1000;

            // 添加GraphicRaycaster组件（用于UI交互）
            graphicRaycaster = canvasObj.AddComponent<GraphicRaycaster>();

            Debug.Log("UI Canvas created successfully.");
        }

        /// <summary>
        /// 创建信息显示文本
        /// </summary>
        public void CreateInfoText()
        {
            if (uiCanvas == null)
            {
                Debug.LogError("UI Canvas is null. Cannot create info text.");
                return;
            }

            // 创建Text GameObject
            infoTextObj = new GameObject("InspectorInfoText");
            infoTextObj.transform.SetParent(uiCanvas.transform, false);
            infoTextObj.SetActive(true);

            // 添加CanvasRenderer组件（UI渲染必需）
            CanvasRenderer canvasRenderer = infoTextObj.AddComponent<CanvasRenderer>();

            // 添加TextMeshProUGUI组件
            infoText = infoTextObj.AddComponent<TextMeshProUGUI>();
            infoText.fontSize = 20;
            infoText.color = Color.white;
            infoText.alignment = TextAlignmentOptions.TopLeft;
            infoText.fontStyle = FontStyles.Normal;
            infoText.enableWordWrapping = true;

            // 设置RectTransform，使用百分比定位在屏幕右边
            RectTransform rectTransform = infoTextObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.85f, 0.1f);
            rectTransform.anchorMax = new Vector2(1.0f, 0.9f);
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;

            // 设置文本不阻挡射线
            infoText.raycastTarget = false;
        }

        /// <summary>
        /// 更新信息文本内容
        /// </summary>
        public void UpdateInfoText(string content)
        {
            if (infoText != null)
            {
                infoText.text = content;
            }
        }

        /// <summary>
        /// 清理所有组件
        /// </summary>
        public void Cleanup()
        {
            // 清理信息文本对象
            if (infoTextObj != null)
            {
                infoText = null;
                infoTextObj = null;
            }

            // 清理Canvas对象（会删除所有子对象）
            if (canvasObj != null)
            {
                Object.Destroy(canvasObj);
                canvasObj = null;
            }

            // 清理组件引用
            uiCanvas = null;
            graphicRaycaster = null;
            infoText = null;
            infoTextObj = null;
        }

        /// <summary>
        /// 检查UI是否已创建
        /// </summary>
        public bool IsUIAvailable()
        {
            return uiCanvas != null;
        }
    }
}

