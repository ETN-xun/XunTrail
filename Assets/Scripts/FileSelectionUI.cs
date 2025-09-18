using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FileSelectionUI : MonoBehaviour
{
    private System.Action<string> onFileSelected;
    private Canvas canvas;
    private GameObject panel;
    private ScrollRect scrollRect;
    private Transform contentParent;
    private List<string> availableFiles = new List<string>();

    private System.Action<string> onFileSelectedCallback;
    
    public void Initialize(System.Action<string> onFileSelected = null)
    {
        onFileSelectedCallback = onFileSelected;
        CreateUI();
        ScanForFiles();
    }

    void CreateUI()
    {
        // 创建Canvas
        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        gameObject.AddComponent<GraphicRaycaster>();

        // 创建背景面板
        panel = new GameObject("Panel");
        panel.transform.SetParent(transform);
        
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);

        // 创建内容窗口
        GameObject window = new GameObject("Window");
        window.transform.SetParent(panel.transform);
        
        RectTransform windowRect = window.AddComponent<RectTransform>();
        windowRect.anchorMin = new Vector2(0.2f, 0.2f);
        windowRect.anchorMax = new Vector2(0.8f, 0.8f);
        windowRect.offsetMin = Vector2.zero;
        windowRect.offsetMax = Vector2.zero;

        Image windowImage = window.AddComponent<Image>();
        windowImage.color = Color.white;

        // 创建标题
        GameObject title = new GameObject("Title");
        title.transform.SetParent(window.transform);
        
        RectTransform titleRect = title.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.9f);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.offsetMin = new Vector2(20, 0);
        titleRect.offsetMax = new Vector2(-20, 0);

        Text titleText = title.AddComponent<Text>();
        titleText.text = "选择乐谱文件";
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 24;
        titleText.color = Color.black;
        titleText.alignment = TextAnchor.MiddleCenter;

        // 创建滚动视图
        GameObject scrollView = new GameObject("ScrollView");
        scrollView.transform.SetParent(window.transform);
        
        RectTransform scrollViewRect = scrollView.AddComponent<RectTransform>();
        scrollViewRect.anchorMin = new Vector2(0, 0.1f);
        scrollViewRect.anchorMax = new Vector2(1, 0.85f);
        scrollViewRect.offsetMin = new Vector2(20, 0);
        scrollViewRect.offsetMax = new Vector2(-20, 0);

        scrollRect = scrollView.AddComponent<ScrollRect>();
        scrollView.AddComponent<Image>().color = new Color(0.9f, 0.9f, 0.9f);

        // 创建内容容器
        GameObject content = new GameObject("Content");
        content.transform.SetParent(scrollView.transform);
        
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;

        contentParent = content.transform;
        scrollRect.content = contentRect;
        scrollRect.vertical = true;
        scrollRect.horizontal = false;

        VerticalLayoutGroup layoutGroup = content.AddComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = 5;
        layoutGroup.padding = new RectOffset(10, 10, 10, 10);

        ContentSizeFitter sizeFitter = content.AddComponent<ContentSizeFitter>();
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // 创建关闭按钮
        GameObject closeButton = new GameObject("CloseButton");
        closeButton.transform.SetParent(window.transform);
        
        RectTransform closeButtonRect = closeButton.AddComponent<RectTransform>();
        closeButtonRect.anchorMin = new Vector2(0.4f, 0.02f);
        closeButtonRect.anchorMax = new Vector2(0.6f, 0.08f);
        closeButtonRect.offsetMin = Vector2.zero;
        closeButtonRect.offsetMax = Vector2.zero;

        Button closeBtn = closeButton.AddComponent<Button>();
        closeBtn.onClick.AddListener(CloseUI);

        Image closeBtnImage = closeButton.AddComponent<Image>();
        closeBtnImage.color = new Color(0.8f, 0.8f, 0.8f);

        GameObject closeBtnText = new GameObject("Text");
        closeBtnText.transform.SetParent(closeButton.transform);
        
        RectTransform closeBtnTextRect = closeBtnText.AddComponent<RectTransform>();
        closeBtnTextRect.anchorMin = Vector2.zero;
        closeBtnTextRect.anchorMax = Vector2.one;
        closeBtnTextRect.offsetMin = Vector2.zero;
        closeBtnTextRect.offsetMax = Vector2.zero;

        Text closeBtnTextComponent = closeBtnText.AddComponent<Text>();
        closeBtnTextComponent.text = "取消";
        closeBtnTextComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        closeBtnTextComponent.fontSize = 18;
        closeBtnTextComponent.color = Color.black;
        closeBtnTextComponent.alignment = TextAnchor.MiddleCenter;
    }

    void ScanForFiles()
    {
        availableFiles.Clear();

        // 扫描StreamingAssets文件夹
        string streamingAssetsPath = Application.streamingAssetsPath;
        if (Directory.Exists(streamingAssetsPath))
        {
            string[] files = Directory.GetFiles(streamingAssetsPath, "*.txt");
            foreach (string file in files)
            {
                availableFiles.Add(file);
            }
        }

        // 扫描常见位置
        string[] searchPaths = {
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Application.dataPath + "/../乐谱",
            Application.dataPath + "/乐谱"
        };

        foreach (string searchPath in searchPaths)
        {
            try
            {
                if (Directory.Exists(searchPath))
                {
                    string[] files = Directory.GetFiles(searchPath, "*.txt", SearchOption.TopDirectoryOnly);
                    foreach (string file in files)
                    {
                        if (!availableFiles.Contains(file))
                        {
                            availableFiles.Add(file);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"FileSelectionUI: 扫描路径 {searchPath} 时出错: {e.Message}");
            }
        }

        CreateFileButtons();
    }

    void CreateFileButtons()
    {
        if (availableFiles.Count == 0)
        {
            CreateNoFilesMessage();
            return;
        }

        foreach (string filePath in availableFiles)
        {
            CreateFileButton(filePath);
        }
    }

    void CreateFileButton(string filePath)
    {
        GameObject button = new GameObject("FileButton");
        button.transform.SetParent(contentParent);

        RectTransform buttonRect = button.AddComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(0, 40);

        Button btn = button.AddComponent<Button>();
        btn.onClick.AddListener(() => SelectFile(filePath));

        Image btnImage = button.AddComponent<Image>();
        btnImage.color = new Color(0.9f, 0.9f, 0.9f);

        GameObject text = new GameObject("Text");
        text.transform.SetParent(button.transform);

        RectTransform textRect = text.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 0);
        textRect.offsetMax = new Vector2(-10, 0);

        Text textComponent = text.AddComponent<Text>();
        textComponent.text = Path.GetFileName(filePath);
        textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        textComponent.fontSize = 16;
        textComponent.color = Color.black;
        textComponent.alignment = TextAnchor.MiddleLeft;

        LayoutElement layoutElement = button.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 40;
    }

    void CreateNoFilesMessage()
    {
        GameObject message = new GameObject("NoFilesMessage");
        message.transform.SetParent(contentParent);

        RectTransform messageRect = message.AddComponent<RectTransform>();
        messageRect.sizeDelta = new Vector2(0, 60);

        Text messageText = message.AddComponent<Text>();
        messageText.text = "未找到txt文件\n请将乐谱文件放入StreamingAssets文件夹";
        messageText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        messageText.fontSize = 16;
        messageText.color = Color.red;
        messageText.alignment = TextAnchor.MiddleCenter;

        LayoutElement layoutElement = message.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 60;
    }

    void SelectFile(string filePath)
    {
        Debug.Log($"FileSelectionUI: 用户选择了文件: {filePath}");
        onFileSelectedCallback?.Invoke(filePath);
        CloseUI();
    }

    void CloseUI()
    {
        Debug.Log("FileSelectionUI: 关闭文件选择界面");
        onFileSelectedCallback?.Invoke(null);
        Destroy(gameObject);
    }
}