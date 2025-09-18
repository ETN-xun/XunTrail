using UnityEngine;
using UnityEngine.UI;

public class PlayDetectionTest : MonoBehaviour
{
    [Header("测试UI")]
    public Text statusText;
    public Text noteText;
    public Text instructionText;
    
    private ToneGenerator toneGenerator;
    private ChallengeManager challengeManager;
    
    void Start()
    {
        toneGenerator = FindObjectOfType<ToneGenerator>();
        challengeManager = FindObjectOfType<ChallengeManager>();
        
        if (instructionText != null)
        {
            instructionText.text = "测试说明：\n" +
                                 "1. 只按空格键 -> 应该显示'中音6'\n" +
                                 "2. 只按音符键(如A) -> 应该显示'未演奏'\n" +
                                 "3. 按空格键+音符键(如A) -> 应该显示对应音符\n" +
                                 "4. 什么都不按 -> 应该显示'未演奏'";
        }
    }
    
    void Update()
    {
        if (toneGenerator == null) return;
        
        // 获取当前演奏状态
        string currentNote = toneGenerator.GetCurrentNoteName();
        
        // 检测输入状态
        bool spacePressed = Input.GetKey(KeyCode.Space);
        bool anyNoteKey = CheckAnyNoteKey();
        
        // 更新状态显示
        if (statusText != null)
        {
            string status = $"空格键: {(spacePressed ? "按下" : "未按")}\n";
            status += $"音符键: {(anyNoteKey ? "按下" : "未按")}\n";
            status += $"演奏状态: {(string.IsNullOrEmpty(currentNote) ? "未演奏" : "演奏中")}";
            statusText.text = status;
        }
        
        // 更新音符显示
        if (noteText != null)
        {
            noteText.text = string.IsNullOrEmpty(currentNote) ? "未演奏" : currentNote;
        }
        
        // 在控制台输出测试结果
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log($"[测试] 空格键: {spacePressed}, 音符键: {anyNoteKey}, 当前音符: '{currentNote}'");
        }
    }
    
    private bool CheckAnyNoteKey()
    {
        return Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.E) ||
               Input.GetKey(KeyCode.F) || Input.GetKey(KeyCode.J) || Input.GetKey(KeyCode.I) ||
               Input.GetKey(KeyCode.O) || Input.GetKey(KeyCode.P) || Input.GetKey(KeyCode.Q) ||
               Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.T) || Input.GetKey(KeyCode.Y) ||
               Input.GetKey(KeyCode.U) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D) ||
               Input.GetKey(KeyCode.G) || Input.GetKey(KeyCode.H) || Input.GetKey(KeyCode.K) ||
               Input.GetKey(KeyCode.L) || Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.X) ||
               Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.V) || Input.GetKey(KeyCode.B) ||
               Input.GetKey(KeyCode.N) || Input.GetKey(KeyCode.M);
    }
}