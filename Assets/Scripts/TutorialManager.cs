using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class TutorialStep
{
    public string text;
    public bool hasNextButton;
    public KeyCode[] requiredKeys; // 需要按下的按键组合
    public bool requiresBreathKey; // 是否需要按下吹气键
    
    public TutorialStep(string text, bool hasNextButton, KeyCode[] requiredKeys = null, bool requiresBreathKey = false)
    {
        this.text = text;
        this.hasNextButton = hasNextButton;
        this.requiredKeys = requiredKeys;
        this.requiresBreathKey = requiresBreathKey;
    }
}

public class TutorialManager : MonoBehaviour
{
    [Header("UI References")]
    public Text tutorialText;
    public Button nextButton;
    public Button exitButton;
    public GameObject tutorialPanel;
    
    [Header("Tutorial Settings")]
    public float textDisplaySpeed = 0.05f;
    
    private List<TutorialStep> tutorialSteps;
    private int currentStepIndex = 0;
    private bool isDisplayingText = false;
    private bool isWaitingForInput = false;
    private bool isActive = false;
    private bool typewriterEffect = true;
    private KeySettingsManager keySettingsManager;
    private KeyCode[] currentEightHoleKeys;
    private KeyCode[] currentTenHoleKeys;
    
    // 教程步骤定义
private void InitializeTutorialSteps()
    {
        tutorialSteps = new List<TutorialStep>();
        
        // 获取当前八孔按键设置
        currentEightHoleKeys = keySettingsManager.GetEightHoleKeys();
        
        // 获取当前十孔按键设置
        currentTenHoleKeys = keySettingsManager.GetTenHoleKeys();
        
        // 添加所有教程步骤
        tutorialSteps.Add(new TutorialStep("欢迎来到埙音游《埙途》，在教程中你将学习怎样吹埙", true));
        
        tutorialSteps.Add(new TutorialStep("首先学习如何吹气，按下【八孔按键9】吹气", true));
        
        tutorialSteps.Add(new TutorialStep("好的，你已经学会吹气了。吹气就可以发出声音", true));
        
        tutorialSteps.Add(new TutorialStep("接下来教给你如何按孔，按下【八孔按键4】键试试吧", true));
        
        tutorialSteps.Add(new TutorialStep("好的，保持按住【八孔按键4】键，同时按下【八孔按键9】吹气试试", true));
        
        tutorialSteps.Add(new TutorialStep("发现了吗？吹出来的音与刚才不一样了！按孔可以改变吹出声音的频率，不同的按键组合对应不同的频率。刚才已经为你演示高音1和中音7了，接下来学习其他频率的演奏方式吧", true));
        
        tutorialSteps.Add(new TutorialStep("温馨提示：某些按键组合可能按不出来，这并不是游戏产生的bug，而是因为你的键盘产生了按键冲突。请买一个全键无冲的键盘，或者调整为不冲突的键位", true));
        
        tutorialSteps.Add(new TutorialStep("按住【八孔按键3】和【八孔按键4】，再按下【八孔按键9】，吹出中音6", true));
        
        tutorialSteps.Add(new TutorialStep("按住【八孔按键2】、【八孔按键3】和【八孔按键4】，再按下【八孔按键9】，吹出中音5", true));
        
        tutorialSteps.Add(new TutorialStep("按住【八孔按键2】、【八孔按键3】、【八孔按键4】、【八孔按键5】和【八孔按键6】，再按下【八孔按键9】，吹出中音4", true));
        
        tutorialSteps.Add(new TutorialStep("按住【八孔按键1】、【八孔按键2】、【八孔按键3】和【八孔按键4】，再按下【八孔按键9】，吹出中音3", true));
        
        tutorialSteps.Add(new TutorialStep("按住【八孔按键0】、【八孔按键1】、【八孔按键2】、【八孔按键3】和【八孔按键4】，再按下【八孔按键9】，吹出中音2", true));
        
        tutorialSteps.Add(new TutorialStep("按住【八孔按键0】、【八孔按键1】、【八孔按键2】、【八孔按键3】、【八孔按键4】和【八孔按键5】，再按下【八孔按键9】，吹出中音1", true));
        
        tutorialSteps.Add(new TutorialStep("按住【八孔按键0】、【八孔按键1】、【八孔按键2】、【八孔按键3】、【八孔按键4】、【八孔按键5】和【八孔按键7】，再按下【八孔按键9】，吹出低音7", true));
        
        tutorialSteps.Add(new TutorialStep("按住【八孔按键0】、【八孔按键1】、【八孔按键2】、【八孔按键3】、【八孔按键4】、【八孔按键5】和【八孔按键6】，再按下【八孔按键9】，吹出低音6", true));
        
        tutorialSteps.Add(new TutorialStep("按住【八孔按键0】、【八孔按键1】、【八孔按键2】、【八孔按键3】、【八孔按键4】、【八孔按键5】、【八孔按键6】和【八孔按键7】，再按下【八孔按键9】，吹出低音5", true));
        
        tutorialSteps.Add(new TutorialStep("好了，你已经学会最基本的全音指法了！", true));
        
        tutorialSteps.Add(new TutorialStep("按下↑、↓键可以升八度、降八度", true));
        
        tutorialSteps.Add(new TutorialStep("按下←、→键可以降半调、升半调", true));
        
        tutorialSteps.Add(new TutorialStep("好了，你已经学会最基本的操作了。可以直接退出教程，或者点“下一步”继续学习半音指法、十孔指法、手柄操作等进阶操作", true));
        
        tutorialSteps.Add(new TutorialStep("还想学些进阶操作？好，首先我们来学习八孔埙的所有半音指法", true));
        
        tutorialSteps.Add(new TutorialStep("按住【八孔按键4】、【八孔按键5】和【八孔按键6】，再按下【八孔按键9】，吹出中音6#", true));
        
        tutorialSteps.Add(new TutorialStep("按住【八孔按键3】、【八孔按键4】和【八孔按键5】，再按下【八孔按键9】，吹出中音5#", true));
        
        tutorialSteps.Add(new TutorialStep("按住【八孔按键2】、【八孔按键3】、【八孔按键4】和【八孔按键5】，再按下【八孔按键9】，吹出中音4#", true));
        tutorialSteps.Add(new TutorialStep("按住【八孔按键1】、【八孔按键2】、【八孔按键3】、【八孔按键4】和【八孔按键7】，再按下【八孔按键9】，吹出中音2#", true));
        tutorialSteps.Add(new TutorialStep("按住【八孔按键0】、【八孔按键1】、【八孔按键2】、【八孔按键3】、【八孔按键4】和【八孔按键7】，再按下【八孔按键9】，吹出中音1#", true));
        tutorialSteps.Add(new TutorialStep("按住【八孔按键0】、【八孔按键1】、【八孔按键2】、【八孔按键3】、【八孔按键4】、【八孔按键6】和【八孔按键7】，再按下【八孔按键9】，吹出低音6#", true));
        tutorialSteps.Add(new TutorialStep("按住【八孔按键0】、【八孔按键1】、【八孔按键2】、【八孔按键3】、【八孔按键4】、【八孔按键5】、【八孔按键6】和【八孔按键8】，再按下【八孔按键9】，吹出低音5#", true));
        tutorialSteps.Add(new TutorialStep("接下来让我们学习十孔模式的指法，按下【Shift】切换为十孔模式", true));
        tutorialSteps.Add(new TutorialStep("与八孔模式不同的是，十孔模式没有“吹气”键，只要按下键就出声。不按键或按键不符合任何一种按键组合则不发声", true));
        tutorialSteps.Add(new TutorialStep("按下【十孔按键11】，吹出高音3", true));
        tutorialSteps.Add(new TutorialStep("按下【十孔按键2】，吹出高音2#", true));
        tutorialSteps.Add(new TutorialStep("按下【十孔按键9】，吹出高音2", true));
        tutorialSteps.Add(new TutorialStep("按下【十孔按键2】、【十孔按键9】，吹出高音1#", true));
        tutorialSteps.Add(new TutorialStep("按下【十孔按键8】、【十孔按键9】，吹出高音1", true));
        tutorialSteps.Add(new TutorialStep("按下【十孔按键3】、【十孔按键8】、【十孔按键9】，吹出中音7", true));
        tutorialSteps.Add(new TutorialStep("按下【十孔按键3】、【十孔按键5】、【十孔按键6】、【十孔按键8】、【十孔按键9】，吹出中音6#", true));
        tutorialSteps.Add(new TutorialStep("按下【十孔按键2】、【十孔按键3】、【十孔按键8】、【十孔按键9】，吹出中音6", true));
        tutorialSteps.Add(new TutorialStep("按下【十孔按键2】、【十孔按键3】、【十孔按键5】、【十孔按键8】、【十孔按键9】，吹出中音5#", true));
        tutorialSteps.Add(new TutorialStep("按下【十孔按键1】、【十孔按键2】、【十孔按键3】、【十孔按键8】、【十孔按键9】，吹出中音5", true));
        tutorialSteps.Add(new TutorialStep("按下【十孔按键1】、【十孔按键2】、【十孔按键3】、【十孔按键5】、【十孔按键8】、【十孔按键9】，吹出中音4#", true));
        tutorialSteps.Add(new TutorialStep("按下【十孔按键1】、【十孔按键2】、【十孔按键3】、【十孔按键5】、【十孔按键6】、【十孔按键8】、【十孔按键9】，吹出中音4", true));
        tutorialSteps.Add(new TutorialStep("按下【十孔按键1】、【十孔按键2】、【十孔按键3】、【十孔按键4】、【十孔按键8】、【十孔按键9】，吹出中音3", true));
        tutorialSteps.Add(new TutorialStep("按下【十孔按键0】、【十孔按键1】、【十孔按键2】、【十孔按键3】、【十孔按键4】、【十孔按键8】、【十孔按键9】，吹出中音2#", true));
        tutorialSteps.Add(new TutorialStep("按下【十孔按键1】、【十孔按键2】、【十孔按键3】、【十孔按键4】、【十孔按键5】、【十孔按键8】、【十孔按键9】，吹出中音2", true));
        tutorialSteps.Add(new TutorialStep("按下【十孔按键0】、【十孔按键1】、【十孔按键2】、【十孔按键3】、【十孔按键4】、【十孔按键5】、【十孔按键8】、【十孔按键9】，吹出中音1#", true));
        tutorialSteps.Add(new TutorialStep("按下【十孔按键1】、【十孔按键2】、【十孔按键3】、【十孔按键4】、【十孔按键5】、【十孔按键6】、【十孔按键8】、【十孔按键9】，吹出中音1", true));
        tutorialSteps.Add(new TutorialStep("按下【十孔按键0】、【十孔按键1】、【十孔按键2】、【十孔按键3】、【十孔按键4】、【十孔按键5】、【十孔按键6】、【十孔按键8】、【十孔按键9】，吹出低音7", true));
        tutorialSteps.Add(new TutorialStep("按下【十孔按键0】、【十孔按键1】、【十孔按键2】、【十孔按键3】、【十孔按键4】、【十孔按键5】、【十孔按键6】、【十孔按键7】、【十孔按键9】，吹出低音6#", true));
        tutorialSteps.Add(new TutorialStep("按下【十孔按键0】、【十孔按键1】、【十孔按键2】、【十孔按键3】、【十孔按键4】、【十孔按键5】、【十孔按键7】、【十孔按键8】、【十孔按键9】，吹出低音6", true));
        tutorialSteps.Add(new TutorialStep("按下【十孔按键1】、【十孔按键2】、【十孔按键3】、【十孔按键4】、【十孔按键5】、【十孔按键6】、【十孔按键7】、【十孔按键8】、【十孔按键9】、【十孔按键10】，吹出低音5#", true));
        tutorialSteps.Add(new TutorialStep("按下【十孔按键0】、【十孔按键1】、【十孔按键2】、【十孔按键3】、【十孔按键4】、【十孔按键5】、【十孔按键6】、【十孔按键7】、【十孔按键8】、【十孔按键9】，吹出低音5", true));
        tutorialSteps.Add(new TutorialStep("好了，十孔埙的全半音指法你也都学会了！", true));
        tutorialSteps.Add(new TutorialStep("接下来就是手柄操作了，以下内容以Xbox手柄为准", true));
        tutorialSteps.Add(new TutorialStep("按下【A】键，吹出低音6", true));
        tutorialSteps.Add(new TutorialStep("按下【B】键，吹出中音2", true));
        tutorialSteps.Add(new TutorialStep("按下【X】键，吹出中音5", true));
        tutorialSteps.Add(new TutorialStep("按下【Y】键，吹出高音1", true));
        tutorialSteps.Add(new TutorialStep("好的。这四个音是基准音。实际演奏时要使用左边的摇杆进行调整", true));
        tutorialSteps.Add(new TutorialStep("摇杆向上，升一个音；摇杆向下，降一个音", true));
        tutorialSteps.Add(new TutorialStep("摇杆向右，升半个音；摇杆向左，降半个音", true));
        tutorialSteps.Add(new TutorialStep("举个例子吧，按下【A】键，本来是低音6", true));
        tutorialSteps.Add(new TutorialStep("按住【A】键，然后将摇杆向上推，就变成了低音7", true));
        tutorialSteps.Add(new TutorialStep("按住【A】键，然后将摇杆向下推，就变成了低音5", true));
        tutorialSteps.Add(new TutorialStep("按住【A】键，然后将摇杆向右推，就变成了低音6#", true));
        tutorialSteps.Add(new TutorialStep("按住【A】键，然后将摇杆向左推，就变成了低音5#", true));
        tutorialSteps.Add(new TutorialStep("另外三个键同理。以基准音+调音的方式，能够演奏低音5到高音2的所有全半音", true));
        tutorialSteps.Add(new TutorialStep("好了，你已经学完《埙途》的全部内容了，现在可以去自由模式随意练习，或者在挑战模式挑战乐谱了！", true));
    }
    
    void Start()
    {
        keySettingsManager = KeySettingsManager.Instance;
        if (keySettingsManager == null)
        {
            Debug.LogError("KeySettingsManager not found!");
            return;
        }
        
        InitializeTutorialSteps();
        SetupUI();
        
        // 不再自动启动教程，只有在明确调用StartTutorial()时才启动
        Debug.Log("TutorialManager: 已初始化，等待明确调用StartTutorial()");
    }
    
private void SetupUI()
    {
        // 查找TutorialPanel
        tutorialPanel = GameObject.Find("TutorialPanel");
        if (tutorialPanel == null)
        {
            // 如果在根级别找不到，尝试在Canvas下查找
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                tutorialPanel = canvas.transform.Find("TutorialPanel")?.gameObject;
            }
        }

        if (tutorialPanel == null)
        {
            Debug.LogError("TutorialPanel not found!");
            return;
        }

        // 查找子组件
        tutorialText = tutorialPanel.transform.Find("TutorialText")?.GetComponent<Text>();
        nextButton = tutorialPanel.transform.Find("NextButton")?.GetComponent<Button>();
        exitButton = tutorialPanel.transform.Find("ExitButton")?.GetComponent<Button>();

        if (tutorialText == null || nextButton == null || exitButton == null)
        {
            Debug.LogError("Tutorial UI components not found!");
            return;
        }

        Debug.Log("TutorialPanel found: " + tutorialPanel.name);
        Debug.Log("TutorialText found: " + tutorialText.name);
        Debug.Log("NextButton found: " + nextButton.name);
        Debug.Log("ExitButton found: " + exitButton.name);

        // 添加按钮监听器
        nextButton.onClick.AddListener(OnNextButtonClicked);
        exitButton.onClick.AddListener(OnExitButtonClicked);

        // 初始设置为不活跃
        tutorialPanel.SetActive(false);
    }
    
public void StartTutorial()
    {
        if (tutorialSteps == null || tutorialSteps.Count == 0)
        {
            Debug.LogError("教程步骤未初始化");
            return;
        }
        
        currentStepIndex = 0;
        isActive = true;
        
        Debug.Log("StartTutorial: 开始教程");
        
        // 显示教程界面
        if (tutorialPanel != null)
        {
            Debug.Log($"StartTutorial: 激活TutorialPanel，当前状态: {tutorialPanel.activeSelf}");
            tutorialPanel.SetActive(true);
            Debug.Log($"StartTutorial: TutorialPanel激活后状态: {tutorialPanel.activeSelf}");
        }
        else
        {
            Debug.LogError("StartTutorial: TutorialPanel为null!");
        }
        
        // 显示第一个步骤
        ShowCurrentStep();
    }
    
private void ShowCurrentStep()
    {
        if (currentStepIndex >= tutorialSteps.Count)
        {
            // 教程结束
            CompleteTutorial();
            return;
        }

        TutorialStep currentStep = tutorialSteps[currentStepIndex];
        
        // 更新文本
        string displayText = ReplaceKeyPlaceholders(currentStep.text);
        
        if (tutorialText != null)
        {
            if (typewriterEffect)
            {
                StartCoroutine(TypewriterEffect(displayText));
            }
            else
            {
                tutorialText.text = displayText;
            }
        }
        
        // 检查是否是最后一步
        bool isLastStep = currentStepIndex == tutorialSteps.Count - 1;
        
        // 更新按钮显示 - 所有步骤都显示"下一步"按钮
        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(true); // 始终显示下一步按钮
            
            // 如果是最后一步，更改按钮文本
            if (isLastStep)
            {
                var buttonText = nextButton.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    buttonText.text = "返回标题界面";
                }
            }
            else
            {
                var buttonText = nextButton.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    buttonText.text = "下一步";
                }
            }
        }
        
        if (exitButton != null)
        {
            exitButton.gameObject.SetActive(true); // 始终显示退出按钮
        }
        
        Debug.Log($"显示教程步骤 {currentStepIndex + 1}: {displayText}");
    }
    
    private string ReplaceKeyPlaceholders(string text)
    {
        // 替换八孔按键占位符为实际按键名称
        if (currentEightHoleKeys != null)
        {
            for (int i = 0; i < currentEightHoleKeys.Length; i++)
            {
                string placeholder = $"【八孔按键{i}】";
                string keyName = currentEightHoleKeys[i].ToString();
                text = text.Replace(placeholder, $"【{keyName}】");
            }
        }
        
        // 替换十孔按键占位符为实际按键名称
        if (currentTenHoleKeys != null)
        {
            for (int i = 0; i < currentTenHoleKeys.Length; i++)
            {
                string placeholder = $"【十孔按键{i}】";
                string keyName = currentTenHoleKeys[i].ToString();
                text = text.Replace(placeholder, $"【{keyName}】");
            }
        }
        
        return text;
    }
    
    private IEnumerator DisplayTextGradually(string text)
    {
        isDisplayingText = true;
        tutorialText.text = "";
        
        for (int i = 0; i <= text.Length; i++)
        {
            tutorialText.text = text.Substring(0, i);
            yield return new WaitForSeconds(textDisplaySpeed);
        }
        
        isDisplayingText = false;
    }
    
    private void Update()
    {
        // 移除所有按键检测逻辑，现在只通过按钮进行交互
    }
    
public void OnNextButtonClicked()
    {
        if (!isActive || currentStepIndex >= tutorialSteps.Count||Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.Joystick1Button0))
        {
            return;
        }
        
        // 如果文字还在显示中，立即显示完整文字，不跳转到下一步
        if (isDisplayingText)
        {
            SkipTextAnimation();
            return;
        }
        
        // 检查是否是最后一步
        bool isLastStep = currentStepIndex == tutorialSteps.Count - 1;
        
        if (isLastStep)
        {
            // 最后一步，返回标题界面
            OnExitButtonClicked();
        }
        else
        {
            // 进入下一步
            NextStep();
        }
    }
    
    private void NextStep()
    {
        currentStepIndex++;
        ShowCurrentStep();
    }
    

    
public void OnExitButtonClicked()
    {
        Debug.Log("退出教程");
        
        // 停止挑战模式
        if (ChallengeManager.Instance != null)
        {
            ChallengeManager.Instance.ForceStopChallenge();
        }
        
        // 重置游戏模式
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.SetFreeMode();
        }
        
        // 返回标题界面
        UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScene");
    }
    
    public void SkipTextAnimation()
    {
        if (isDisplayingText)
        {
            StopAllCoroutines();
            TutorialStep currentStep = tutorialSteps[currentStepIndex];
            string displayText = ReplaceKeyPlaceholders(currentStep.text);
            tutorialText.text = displayText;
            isDisplayingText = false;
        }
    }


private IEnumerator TypewriterEffect(string text)
    {
        isDisplayingText = true;
        tutorialText.text = "";
        
        foreach (char c in text)
        {
            tutorialText.text += c;
            yield return new WaitForSeconds(textDisplaySpeed);
        }
        
        isDisplayingText = false;
    }
    
    private void CompleteTutorial()
    {
        Debug.Log("教程完成");
        isActive = false;
        
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
        
        // 返回标题界面
        OnExitButtonClicked();
    }
}