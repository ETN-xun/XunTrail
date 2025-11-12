using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ToneGenerator : MonoBehaviour
{
[Header("埙参数")]
[Range(0.1f, 0.3f)] public float airNoiseIntensity = 0.18f;
[Range(0.2f, 1f)] public float formantCoupling = 0.8f;
[Range(0.2f, 0.4f)] public float breathPressure = 0.35f;
[Range(1f, 3f)] public float formantWidth = 2f;
[Range(0.1f, 0.6f)] public float preResonanceRatio = 0.4f;

[Header("表现")]
[Range(3f, 8f)] public float vibratoRate = 5.5f;
[Range(0f, 0.01f)] public float vibratoDepth = 0.002f;

    [Header("包络控制")]
    public float volume = 0.28f;
    [Range(0.03f, 0.1f)] public float attackTime = 0.05f;
    [Range(0.1f, 0.4f)] public float releaseTime = 0.25f;
    
    // 添加频率变化检测阈值
    [Range(0.001f, 0.1f)] public float frequencyChangeThreshold = 0.05f;

    public bool isStereo = false;
    public bool isTenHoleMode = false;
    
    public List<Sprite> eightHole = new List<Sprite>();
    public List<Sprite> tenHole = new List<Sprite>();

    private AudioSource audioSource;
    private double currentPhaseD = 0.0;
    private float _currentFrequency = 440f;
    private float _targetFrequency;
    private float _gain = 0f;
    private float _targetGain;
    private float _gainVelocity;
    private float _frequencyVelocity;
    private float _previousSmoothNoise = 0f;
    private float _noiseValueDamp = 0f;
    private float _gainFadeTime;
    private float _time;

    private const float frequencyFadeTime = 0.005f;
    private const int sampleRate = 44100;
    private uint _noiseSeed = 123456789;

    private AudioLowPassFilter _lowPassFilter;
    private AudioHighPassFilter _highPassFilter;
    private AudioEchoFilter _echoFilter;
    
    private AudioReverbFilter _reverbFilter;
    
    public Text text;
    public Text OttaText;
    public int ottava = 0;
    

    [Header("挑战模式")]
    private ChallengeManager challengeManager;
    private float lastNoteTime = 0f;

    private float noteCooldown = 0.5f; // 音符检测冷却时间
    private const float noteDetectionCooldown = 0.5f; // 音符检测冷却时间

    

public int key = 0;
    public Text keyText;
    public Animator xunAnimator;
    private float _lastDspTime;
    private float _previousSample;
    private float _previousDelta;

    private float _frozenFrequency;
    private bool _isFrequencyFrozen = false;
    private bool _isSpacePressed; // 线程安全的缓存变量
    private Dictionary<KeyCode, bool> _keyStates = new Dictionary<KeyCode, bool>();
    private float _bp1_b0, _bp1_b1, _bp1_b2, _bp1_a1, _bp1_a2;
    private float _bp1_x1, _bp1_x2, _bp1_y1, _bp1_y2;
    private float _bp2_b0, _bp2_b1, _bp2_b2, _bp2_a1, _bp2_a2;
    private float _bp2_x1, _bp2_x2, _bp2_y1, _bp2_y2;
    private float _prevWhite;
    
    // Xbox手柄控制相关变量
    private bool _isGamepadConnected = false;
    private bool _isGamepadButtonPressed = false;
    private float _gamepadLeftStickX = 0f;
    private float _gamepadLeftStickY = 0f;
    private bool _buttonAPressed = false;
    private bool _buttonBPressed = false;
    private bool _buttonXPressed = false;
    private bool _buttonYPressed = false;
    
    // 手柄按键对应的频率常量
    private const float FREQUENCY_LOW_6 = 220.00f; // A按键 - 低音6
    private const float FREQUENCY_MID_2 = 293.66f; // B按键 - 中音2
    private const float FREQUENCY_MID_5 = 392.00f; // X按键 - 中音5
    private const float FREQUENCY_HIGH_1 = 523.26f; // Y按键 - 高音1

    // 在ToneGenerator类中添加：
    public static ToneGenerator Instance { get; private set; }
    
    // 动态键位设置
    private KeyCode[] currentEightHoleKeys;
    private KeyCode[] currentTenHoleKeys;
    
    void Awake() {
        Instance = this; // 单例模式
        _lowPassFilter = GetOrAddComponent<AudioLowPassFilter>();
        _highPassFilter = GetOrAddComponent<AudioHighPassFilter>();
        _echoFilter = GetOrAddComponent<AudioEchoFilter>();
        _reverbFilter = GetOrAddComponent<AudioReverbFilter>();
    }

void Start()
    {
        // 加载动态键位设置
        LoadDynamicKeySettings();
        
        InitializeKeyStates();
        InitializeAudioComponents();
        
        // 初始检测是否有手柄连接
        CheckGamepadConnection();
        
        // 获取challengeManager引用
        challengeManager = FindObjectOfType<ChallengeManager>();
        if (challengeManager != null)
        {
            Debug.Log("找到ChallengeManager，挑战模式功能已启用");
        }
    }
    
    public void LoadDynamicKeySettings()
    {
        // 从KeySettingsManager加载当前键位设置
        currentEightHoleKeys = KeySettingsManager.Instance.GetEightHoleKeys();
        currentTenHoleKeys = KeySettingsManager.Instance.GetTenHoleKeys();
        
        // 重新初始化键位状态以包含新的键位
        InitializeKeyStates();
        
        Debug.Log("ToneGenerator已加载动态键位设置");
        Debug.Log($"八孔键位: {string.Join(", ", currentEightHoleKeys)}");
        Debug.Log($"十孔键位: {string.Join(", ", currentTenHoleKeys)}");
    }
    
    // 将硬编码的键位索引转换为动态键位
    private KeyCode GetDynamicKey(int keyIndex, bool isTenHole)
    {
        KeyCode[] keys = isTenHole ? currentTenHoleKeys : currentEightHoleKeys;
        if (keyIndex >= 0 && keyIndex < keys.Length)
        {
            return keys[keyIndex];
        }
        
        // 如果索引超出范围，返回默认键位
        Debug.LogWarning($"键位索引 {keyIndex} 超出范围，使用默认键位");
        return KeyCode.None;
    }
    
    // 检查动态键位组合
    private bool CheckDynamicKeys(bool isTenHole, params int[] keyIndices)
    {
        KeyCode[] keysToCheck = new KeyCode[keyIndices.Length];
        for (int i = 0; i < keyIndices.Length; i++)
        {
            keysToCheck[i] = GetDynamicKey(keyIndices[i], isTenHole);
        }
        
        return CheckKeys(keysToCheck);
    }

    private void InitializeAudioComponents()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        var existingSources = gameObject.GetComponents<AudioSource>();
        foreach (var source in existingSources)
        {
            if (source != audioSource) Destroy(source);
        }

        if (audioSource.clip == null)
        {
            audioSource.clip = AudioClip.Create("XunSound", sampleRate * 10, isStereo ? 2 : 1, sampleRate, false);
        }

        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.Play();

        
        ConfigureAudioComponents();
    }

    
    // 检测是否有任何音符键被按下
    private bool CheckAnyNoteKey()
    {
        // 根据当前模式检查对应的键位
        KeyCode[] keysToCheck = isTenHoleMode ? currentTenHoleKeys : currentEightHoleKeys;
        
        foreach (KeyCode key in keysToCheck)
        {
            if (Input.GetKey(key))
            {
                return true;
            }
        }
        
        return false;
    }

    // 根据检测到的频率计算音符
    private string GetNoteFromFrequency(float frequency)
    {
        if (frequency <= 0f) return "";
        
        string[] noteNames = {"C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"};
        
        // A4 = 440Hz 作为参考
        float a4Frequency = 440f;
        float noteNumber = 12 * Mathf.Log(frequency / a4Frequency, 2) + 69;
        
        int noteIndex = Mathf.RoundToInt(noteNumber) % 12;
        if (noteIndex < 0) noteIndex += 12;
        
        int octave = Mathf.FloorToInt(noteNumber / 12f) - 1;
        
        return noteNames[noteIndex] + octave;
    }

    private KeyCode GetBreathKey()
    {
        // 八孔模式：吹气键是第10个键位（索引9）
        // 十孔模式：吹气键是第12个键位（索引11）
        if (isTenHoleMode)
        {
            return currentTenHoleKeys != null && currentTenHoleKeys.Length > 11 ? currentTenHoleKeys[11] : KeyCode.Space;
        }
        else
        {
            return currentEightHoleKeys != null && currentEightHoleKeys.Length > 9 ? currentEightHoleKeys[9] : KeyCode.Space;
        }
    }

private void ConfigureAudioComponents()
    {
        if (_lowPassFilter == null || _highPassFilter == null) return;

        _highPassFilter.cutoffFrequency = 80f;
        _highPassFilter.highpassResonanceQ = 0.15f;

        _lowPassFilter.cutoffFrequency = 2500f;
        _lowPassFilter.lowpassResonanceQ = 0.2f;

        _echoFilter.delay = 25f;
        _echoFilter.decayRatio = 0.12f;
        _echoFilter.wetMix = 0.03f;
        _echoFilter.enabled = false;

        _reverbFilter.reverbPreset = AudioReverbPreset.User;
        _reverbFilter.room = 0.25f;
        _reverbFilter.hfReference = 650f;
        _reverbFilter.decayHFRatio = 0.22f;
        _reverbFilter.density = 0.38f;
        _reverbFilter.diffusion = 0.60f;

        _frozenFrequency = _currentFrequency;
        _gainFadeTime = attackTime;
    }

    private T GetOrAddComponent<T>() where T : Component
    {
        T component = GetComponent<T>();
        if (component == null)
        {
            component = gameObject.AddComponent<T>();
            if (component is AudioReverbFilter reverb)
            {
                reverb.room = 0.18f;
                reverb.decayHFRatio = 0.15f;
            }
        }
        return component;
    }

void Update()
    {
        _time = Time.time;
        
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            isTenHoleMode = !isTenHoleMode;
            if (isTenHoleMode)
            {
                Debug.Log("切换到十孔模式");
            }
            else
            {
                Debug.Log("切换到八孔模式");
            }
        }
        
        
        
        // 麦克风功能已删除
// 检测手柄连接状态
        CheckGamepadConnection();
        
        // 更新输入状态
        UpdateKeyStates();
        UpdateGamepadStates();
        HandleOctaveInput();
        
        // 检测音频异常状态并重置
        if (IsAudioStateAbnormal())
        {
            ResetAudioState();
        }

        float baseFrequency = GetBaseFrequency();
        float adjustedFrequency = GetFrequency();
        int animatorState = GetAnimatorState(baseFrequency);

        if (xunAnimator != null)
            xunAnimator.SetInteger("tone", animatorState);

        if (isTenHoleMode)
        {
            GetComponent<Image>().sprite = tenHole[animatorState];
        }
        else
        {
            GetComponent<Image>().sprite = eightHole[animatorState];
        }

        UpdateUI(adjustedFrequency);

        // 计算目标频率（key调号调整已在GetFrequencyFromSolfege中应用，这里只应用ottava八度调整）
        float newTargetFrequency = adjustedFrequency * Mathf.Pow(2f, ottava);
        
        // 检测频率变化是否显著
        bool significantFrequencyChange = Mathf.Abs(newTargetFrequency - _targetFrequency) / _targetFrequency > 0.05f;

        // 检测是否应该播放声音（必须按下空格键或手柄按键）
        bool anyNoteKeyPressed = CheckAnyNoteKey();
        bool shouldPlaySound = _isSpacePressed || _isGamepadButtonPressed||(isTenHoleMode&&CheckAnyKeys());
        //Debug.Log(CheckAnyKeys());
        
        if (shouldPlaySound)
        {
            _isFrequencyFrozen = false;
            _targetFrequency = newTargetFrequency;
            _targetGain = volume;

            // 频率变化较大时使用更平滑的淡入
            if (significantFrequencyChange && _gain > 0.1f) {
                _gainFadeTime = attackTime * 2.0f;
            }
            else if (_time - (float)AudioSettings.dspTime < attackTime + 0.05f)
                _gainFadeTime = 0.05f;
            else
                _gainFadeTime = attackTime * Mathf.InverseLerp(0.1f, 0.3f, breathPressure);
        }
        else
        {
            _isFrequencyFrozen = true;
            _targetGain = 0f;
            _gainFadeTime = releaseTime * Mathf.InverseLerp(400f, 800f, _currentFrequency);
            _targetFrequency = _frozenFrequency;
        }

        if (_isFrequencyFrozen && _gain < 0.01f)
            _currentFrequency = _targetFrequency;

        if (!_isFrequencyFrozen)
            _frozenFrequency = _currentFrequency;

        SetFilterCutoff(_currentFrequency);
        
        // 检测挑战模式中的音符匹配
        CheckNoteForChallenge();
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (!_lowPassFilter || !_highPassFilter || !_echoFilter || !_reverbFilter)
        {
            Debug.LogError("音频组件未正确初始化！");
            // 清空数据防止噪音
            for (int i = 0; i < data.Length; i++)
                data[i] = 0f;
            return;
        }

        float dspTime = (float)AudioSettings.dspTime;
        float deltaTime = Mathf.Max(0.001f, dspTime - _lastDspTime);
        _lastDspTime = dspTime;
        _previousDelta = deltaTime;

        // 线程安全的输入检测（使用缓存变量）
        float envelopeTarget = _isSpacePressed ? _targetGain : 0f;
        _gain = Mathf.SmoothDamp(
            _gain,
            envelopeTarget,
            ref _gainVelocity,
            _gainFadeTime,
            float.MaxValue,
            deltaTime);

        // 添加数值安全检查
        if (!float.IsFinite(_gain) || _gain < 0f)
        {
            _gain = 0f;
            _gainVelocity = 0f;
        }

        float freqSmoothTime = _isSpacePressed ? frequencyFadeTime : frequencyFadeTime * 0.3f;
        
        // 添加频率变化检测和相位调整
        float previousFrequency = _currentFrequency;
        
        // 使用更平滑的频率过渡
        _currentFrequency = Mathf.SmoothDamp(
            _currentFrequency,
            _targetFrequency,
            ref _frequencyVelocity,
            freqSmoothTime,
            float.MaxValue,
            deltaTime);
            
        // 添加频率安全检查
        if (!float.IsFinite(_currentFrequency) || _currentFrequency < 20f || _currentFrequency > 20000f)
        {
            _currentFrequency = Mathf.Clamp(_targetFrequency, 20f, 20000f);
            _frequencyVelocity = 0f;
        }
            
        // 改进相位调整逻辑，防止频率变化时的相位跳变
        if (Mathf.Abs(previousFrequency - _currentFrequency) > 0.01f && previousFrequency > 0.01f) {
            // 使用更安全的相位连续性计算
            double phaseRatio = (double)_currentFrequency / (double)previousFrequency;
            if (double.IsFinite(phaseRatio) && phaseRatio > 0.1 && phaseRatio < 10.0) {
                currentPhaseD = (currentPhaseD * phaseRatio) % (2.0 * Mathf.PI);
                // 确保相位值始终为正且有限
                if (currentPhaseD < 0 || !double.IsFinite(currentPhaseD)) 
                    currentPhaseD = 0.0;
            }
        }

        float vib = Mathf.Sin(2f * Mathf.PI * vibratoRate * dspTime) * vibratoDepth;
        double stepD = (2.0 * Mathf.PI) * (_currentFrequency * (1.0 + vib)) / sampleRate;
        
        // 计算频率变化量，用于后续处理
        float freqChangeAmount = Mathf.Abs(_frequencyVelocity) / Mathf.Max(100f, _currentFrequency);
        
        // 添加抗爆音处理
        bool isInitialAttack = _gain < 0.05f && _gainVelocity > 0.1f;
        bool isFrequencyChanging = freqChangeAmount > 0.01f;
        
        // 根据状态调整音频处理参数
        float antiClickGain = 1.0f;
        if (isInitialAttack) {
            // 初始吹奏时使用更平滑的增益曲线
            antiClickGain = Mathf.SmoothStep(0, 1, _gain / 0.05f);
        } else if (isFrequencyChanging) {
            // 频率变化时降低增益以减少噪声
            antiClickGain = Mathf.Lerp(0.85f, 1.0f, Mathf.Exp(-freqChangeAmount * 15f));
        }
        float previousSample = _previousSample;

        // 添加频率变化平滑因子
        freqChangeAmount = Mathf.Abs(previousFrequency - _currentFrequency) / previousFrequency;
        float smoothingFactor = Mathf.Lerp(0.85f, 0.95f, Mathf.Clamp01(freqChangeAmount * 10f));

        for (int i = 0; i < data.Length; i += channels)
        {
            // 确保相位值安全
            if (!double.IsFinite(currentPhaseD))
                currentPhaseD = 0.0;
                
            float phase = (float)(currentPhaseD % (2f * Mathf.PI));
            float baseWave = GenerateXunWave(phase);

            // 线程安全的噪声叠加（使用缓存变量）
            float noise = _isSpacePressed ? GenerateAirNoise(deltaTime) : 0f;
            float totalSignal = baseWave + noise;

            // 添加数值安全检查
            if (!float.IsFinite(totalSignal))
                totalSignal = 0f;

            totalSignal = WaveShaping(totalSignal, deltaTime);

            // 添加平滑过渡，防止爆音
            float newSample = _gain * totalSignal * antiClickGain;
            
            // 数值安全检查
            if (!float.IsFinite(newSample))
                newSample = 0f;
            
            // 根据频率变化程度动态调整平滑系数
            float transitionFactor = 0.0f;
            
            if (isInitialAttack) {
                // 初始吹奏时使用更强的平滑
                transitionFactor = 0.95f;
            } else if (isFrequencyChanging) {
                // 频率变化时使用动态平滑系数
                transitionFactor = Mathf.Lerp(0.85f, 0.98f, Mathf.Clamp01(freqChangeAmount * 20f));
            } else {
                transitionFactor = 0.8f;
            }
            
            // 应用平滑处理
            newSample = previousSample * transitionFactor + newSample * (1.0f - transitionFactor);

            // 应用额外的平滑处理，消除频率变化时的噪声
            if (freqChangeAmount > 0.01f || isInitialAttack) {
                // 在频率变化较大或初始吹奏时应用更强的平滑
                float antiClickFactor = Mathf.Exp(-(freqChangeAmount * 10f + (isInitialAttack ? 0.5f : 0f)));
                newSample = previousSample * (1.0f - antiClickFactor) + newSample * antiClickFactor;
            }

            // 最终数值安全检查和限幅
            if (!float.IsFinite(newSample))
                newSample = 0f;
            newSample = Mathf.Clamp(newSample, -1f, 1f);

            for (int c = 0; c < channels; c++)
                data[i + c] = newSample;

            previousSample = newSample;
            currentPhaseD += stepD;
        }

        // 更安全的相位重置逻辑
        if (currentPhaseD > 2 * Mathf.PI * 100 || !double.IsFinite(currentPhaseD))
            currentPhaseD = currentPhaseD % (2 * Mathf.PI);

        _previousSample = previousSample;
    }

    private void InitializeKeyStates()
    {
        // 清空现有的键位状态
        _keyStates.Clear();
        
        // 添加八孔模式的动态键位
        if (currentEightHoleKeys != null)
        {
            foreach (var k in currentEightHoleKeys)
            {
                if (!_keyStates.ContainsKey(k))
                    _keyStates.Add(k, false);
            }
        }
        
        // 添加十孔模式的动态键位
        if (currentTenHoleKeys != null)
        {
            foreach (var k in currentTenHoleKeys)
            {
                if (!_keyStates.ContainsKey(k))
                    _keyStates.Add(k, false);
            }
        }
        
        // 添加一些通用键位（方向键等）
        var commonKeys = new KeyCode[]{
            KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.Comma, KeyCode.Period
        };
        
        foreach (var k in commonKeys)
        {
            if (!_keyStates.ContainsKey(k))
                _keyStates.Add(k, false);
        }
        
        Debug.Log($"键位状态已初始化，共{_keyStates.Count}个键位");
    }

    private void UpdateKeyStates()
    {
        KeyCode breathKey = GetBreathKey();
        
        List<KeyCode> keysSnapshot = new List<KeyCode>(_keyStates.Keys);
        foreach (var k in keysSnapshot)
        {
            if (k == breathKey) continue; // 跳过吹气键，单独处理
            bool previousState = _keyStates[k];
                _keyStates[k] = Input.GetKey(k);
        }

        // 主线程更新吹气键状态到私有变量
        bool anyNoteKeyPressed = CheckAnyNoteKey();
        if (isTenHoleMode)
        {
            // 十孔模式：_isSpacePressed表示是否有任何音符按键被按下
            _isSpacePressed = CheckAnyKeys();
            // 十孔模式下吹气键状态单独记录
            _keyStates[breathKey] = Input.GetKey(breathKey);
        }
        else
        {
            // 八孔模式：_isSpacePressed表示是否按下吹气键（动态吹气键或手柄）
            _isSpacePressed = Input.GetKey(breathKey) || _isGamepadButtonPressed;
            _keyStates[breathKey] = _isSpacePressed;
        }
    }

    private void HandleOctaveInput()
    {
        // 如果有手柄连接并且正在使用，则不处理键盘的八度输入
        if (_isGamepadConnected && _isGamepadButtonPressed)
            return;
            
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            key--;
            if (key < -4)
            {
                ottava--;
                key = 7;
            }
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            key++;
            if (key > 7)
            {
                ottava++;
                key = -4;
            }
        }

        if (Input.GetKeyDown(KeyCode.Comma) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            ottava = Mathf.Max(ottava - 1, -3);
        }
        else if (Input.GetKeyDown(KeyCode.Period) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            ottava = Mathf.Min(ottava + 1, 2);
        }
    }

private float GetBaseFrequency()
    {
        int solfegeNote = -1; // -1表示没有按键
        
        // 如果有手柄按键被按下，优先使用手柄控制
        if (_isGamepadConnected && _isGamepadButtonPressed)
        {
            // 根据按下的按键获取简谱音名
            if (_buttonAPressed)
                solfegeNote = 6; // 低音6
            else if (_buttonBPressed)
                solfegeNote = 2; // 中音2
            else if (_buttonXPressed)
                solfegeNote = 25; // 中音5
            else if (_buttonYPressed)
                solfegeNote = 31; // 高音1
        }
        else
        {
            if (isTenHoleMode)//十孔模式
            {
                // 十孔模式按键检测 - 按优先级从高到低检测
                // 使用动态键位设置
                
                // 低音5 - 使用动态键位
                if (CheckDynamicKeys(true, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9))
                    solfegeNote = 5;
                // 低音5# - 使用动态键位
                else if (CheckDynamicKeys(true, 10, 1, 2, 3, 4, 5, 6, 7, 8, 9))
                    solfegeNote = 15;
                // 低音6 - 使用动态键位
                else if (CheckDynamicKeys(true, 1, 2, 3, 4, 5, 6, 7, 8, 9))
                    solfegeNote = 6;
                // 低音6# - 使用动态键位
                else if (CheckDynamicKeys(true, 0, 1, 2, 3, 4, 5, 7, 8, 9))
                    solfegeNote = 16;
                // 低音7 - 使用动态键位
                else if (CheckDynamicKeys(true, 0, 1, 2, 3, 4, 5, 6, 8, 9))
                    solfegeNote = 7;
                // 中音1 - 使用动态键位
                else if (CheckDynamicKeys(true, 1, 2, 3, 4, 5, 6, 8, 9))
                    solfegeNote = 1;
                // 中音1# - 使用动态键位
                else if (CheckDynamicKeys(true, 0, 1, 2, 3, 4, 5, 8, 9))
                    solfegeNote = 11;
                // 中音2 - 使用动态键位
                else if (CheckDynamicKeys(true, 1, 2, 3, 4, 5, 8, 9))
                    solfegeNote = 2;
                // 中音2# - 使用动态键位
                else if (CheckDynamicKeys(true, 0, 1, 2, 3, 4, 8, 9))
                    solfegeNote = 12;
                // 中音3 - 使用动态键位
                else if (CheckDynamicKeys(true, 1, 2, 3, 4, 8, 9))
                    solfegeNote = 3;
                // 中音4 - 使用动态键位
                else if (CheckDynamicKeys(true, 1, 2, 3, 5, 6, 8, 9))
                    solfegeNote = 4;
                // 中音4# - 使用动态键位
                else if (CheckDynamicKeys(true, 1, 2, 3, 5, 8, 9))
                    solfegeNote = 14;
                // 中音5 - 使用动态键位
                else if (CheckDynamicKeys(true, 1, 2, 3, 8, 9))
                    solfegeNote = 25;
                // 中音5# - 使用动态键位
                else if (CheckDynamicKeys(true, 2, 3, 5, 8, 9))
                    solfegeNote = 35;
                // 中音6 - 使用动态键位
                else if (CheckDynamicKeys(true, 2, 3, 8, 9))
                    solfegeNote = 26;
                // 中音6# - 使用动态键位
                else if (CheckDynamicKeys(true, 3, 5, 6, 8, 9))
                    solfegeNote = 36;
                // 中音7 - 使用动态键位
                else if (CheckDynamicKeys(true, 3, 8, 9))
                    solfegeNote = 27;
                // 高音1 - 使用动态键位
                else if (CheckDynamicKeys(true, 8, 9))
                    solfegeNote = 31;
                // 高音1# - 使用动态键位
                else if (CheckDynamicKeys(true, 2, 9))
                    solfegeNote = 41;
                // 高音2 - 使用动态键位
                else if (CheckDynamicKeys(true, 9))
                    solfegeNote = 32;
                // 高音2# - 使用动态键位
                else if (CheckDynamicKeys(true, 2))
                    solfegeNote = 42;
                // 高音3 - 使用动态键位
                else if (CheckDynamicKeys(true, 11))
                    solfegeNote = 33;
                else
                {
                    solfegeNote = -1;
                    _isSpacePressed = false;
                }
            }
            else
            {
                // 八孔模式使用动态键位设置
                if (CheckDynamicKeys(false, 0, 1, 2, 3, 4, 5, 6, 7))
                    solfegeNote = 5; // 低音5
                else if (CheckDynamicKeys(false, 0, 1, 2, 3, 4, 5, 6, 8))
                    solfegeNote = 15; // 低音5#
                else if (CheckDynamicKeys(false, 0, 1, 2, 3, 4, 5, 6))
                    solfegeNote = 6; // 低音6
                else if (CheckDynamicKeys(false, 0, 1, 2, 3, 4, 6, 7))
                    solfegeNote = 16; // 低音6#
                else if (CheckDynamicKeys(false, 0, 1, 2, 3, 4, 5, 7))
                    solfegeNote = 7; // 低音7
                else if (CheckDynamicKeys(false, 0, 1, 2, 3, 4, 5))
                    solfegeNote = 1; // 中音1
                else if (CheckDynamicKeys(false, 0, 1, 2, 3, 4, 7))
                    solfegeNote = 11; // 中音1#
                else if (CheckDynamicKeys(false, 0, 1, 2, 3, 4))
                    solfegeNote = 2; // 中音2
                else if (CheckDynamicKeys(false, 1, 2, 3, 4, 7))
                    solfegeNote = 12; // 中音2#
                else if (CheckDynamicKeys(false, 1, 2, 3, 4))
                    solfegeNote = 3; // 中音3
                else if (CheckDynamicKeys(false, 2, 3, 4, 5, 6))
                    solfegeNote = 4; // 中音4
                else if (CheckDynamicKeys(false, 2, 3, 4, 5))
                    solfegeNote = 14; // 中音4#
                else if (CheckDynamicKeys(false, 2, 3, 4))
                    solfegeNote = 25; // 中音5
                else if (CheckDynamicKeys(false, 3, 4, 5))
                    solfegeNote = 35; // 中音5#
                else if (CheckDynamicKeys(false, 3, 4))
                    solfegeNote = 26; // 中音6
                else if (CheckDynamicKeys(false, 4, 5, 6))
                    solfegeNote = 36; // 中音6#
                else if (CheckDynamicKeys(false, 4))
                    solfegeNote = 27; // 中音7
                // 如果上述按键都不按，则为高音1
                else
                    solfegeNote = 31; // 高音1 - 上述按键都不按
            }
        }
        
        if (solfegeNote == -1)
            return 0f; // 没有按键时返回0，表示没有演奏
        
        // 根据调号和简谱音名计算实际频率
        return GetFrequencyFromSolfege(solfegeNote);
    }
    
    // 根据简谱音名和调号计算实际频率
    private float GetFrequencyFromSolfege(int solfegeNote)
    {
        // 获取调号主音频率
        float tonicFrequency = GetTonicFrequency(key);
        
        // 计算相对于调号主音的半音数
        int semitoneOffset = 0;
        
        // 根据新的音符编号系统计算半音偏移
        switch (solfegeNote)
        {
            // 低音区
            case 5:  // 低音5 - AWEFJIO；
                semitoneOffset = 7 - 12; // 5的半音数 - 12(低一八度)
                break;
            case 15: // 低音5# - AWEFJIOP
                semitoneOffset = 8 - 12; // 5#的半音数 - 12(低一八度)
                break;
            case 6:  // 低音6 - AWEFJIO
                semitoneOffset = 9 - 12; // 6的半音数 - 12(低一八度)
                break;
            case 16: // 低音6# - AWEFJO；
                semitoneOffset = 10 - 12; // 6#的半音数 - 12(低一八度)
                break;
            case 7:  // 低音7 - AWEFJI；
                semitoneOffset = 11 - 12; // 7的半音数 - 12(低一八度)
                break;
                
            // 中音区
            case 1:  // 中音1 - AWEFJI
                semitoneOffset = 0; // 1的半音数
                break;
            case 11: // 中音1# - AWEFJ；
                semitoneOffset = 1; // 1#的半音数
                break;
            case 2:  // 中音2 - AWEFJ
                semitoneOffset = 2; // 2的半音数
                break;
            case 12: // 中音2# - WEFJ；
                semitoneOffset = 3; // 2#的半音数
                break;
            case 3:  // 中音3 - WEFJ
                semitoneOffset = 4; // 3的半音数
                break;
            case 4:  // 中音4 - EFJIO
                semitoneOffset = 5; // 4的半音数
                break;
            case 14: // 中音4# - EFJI
                semitoneOffset = 6; // 4#的半音数
                break;
            case 25: // 中音5 - EFJ
                semitoneOffset = 7; // 5的半音数
                break;
            case 35: // 中音5# - FJI
                semitoneOffset = 8; // 5#的半音数
                break;
            case 26: // 中音6 - FJ
                semitoneOffset = 9; // 6的半音数
                break;
            case 36: // 中音6# - JIO
                semitoneOffset = 10; // 6#的半音数
                break;
            case 27: // 中音7 - J
                semitoneOffset = 11; // 7的半音数
                break;
                
            // 高音区
            case 31: // 高音1 - C+M
                semitoneOffset = 0 + 12; // 1的半音数 + 12(高一八度)
                break;
            case 41: // 高音1# - 3+M
                semitoneOffset = 1 + 12; // 1#的半音数 + 12(高一八度)
                break;
            case 32: // 高音2 - M
                semitoneOffset = 2 + 12; // 2的半音数 + 12(高一八度)
                break;
            case 42: // 高音2# - 3
                semitoneOffset = 3 + 12; // 2#的半音数 + 12(高一八度)
                break;
            case 33: // 高音3 - 空格键
                semitoneOffset = 4 + 12; // 3的半音数 + 12(高一八度)
                break;
                
            default:
                return 0;
                /*
                semitoneOffset = 0; // 默认中音1
                _isSpacePressed = false;
                break;
                */
        }
        
        // 计算最终频率
        return tonicFrequency * Mathf.Pow(2f, semitoneOffset / 12f);
    }

public float GetFrequency()
    {
        float baseFrequency = GetBaseFrequency();
        
        // 如果有手柄按键被按下且摇杆被推动，调整频率
        if (_isGamepadConnected && _isGamepadButtonPressed)
        {
            if (Mathf.Abs(_gamepadLeftStickX) > 0.5f || Mathf.Abs(_gamepadLeftStickY) > 0.5f)
            {
                // 左右摇杆：降低/升高半个音
                if (_gamepadLeftStickX < -0.5f)
                    baseFrequency *= 0.9438743f; // 降低半音 (1/2^(1/12))
                else if (_gamepadLeftStickX > 0.5f)
                    baseFrequency *= 1.059463f;  // 升高半音 (2^(1/12))
                    
                // 上下摇杆：升高/降低一个音
                if (_gamepadLeftStickY > 0.5f)
                    baseFrequency *= 1.122462f;  // 升高全音 (2^(2/12))
                else if (_gamepadLeftStickY < -0.5f)
                    baseFrequency *= 0.8908987f; // 降低全音 (1/2^(2/12))
            }
            
            return baseFrequency;
        }
        if (isTenHoleMode)//十孔模式
            {
                // 使用动态键位设置，按优先级从高到低检测
                // 低音5 - 使用动态键位
                if (CheckDynamicKeys(true, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9))
                    return GetFrequencyFromSolfege(5);
                // 低音5# - 使用动态键位
                else if (CheckDynamicKeys(true, 10, 1, 2, 3, 4, 5, 6, 7, 8, 9))
                    return GetFrequencyFromSolfege(15);
                // 低音6 - 使用动态键位
                else if (CheckDynamicKeys(true, 1, 2, 3, 4, 5, 6, 7, 8, 9))
                    return GetFrequencyFromSolfege(6);
                // 低音6# - 使用动态键位
                else if (CheckDynamicKeys(true, 0, 1, 2, 3, 4, 5, 7, 8, 9))
                    return GetFrequencyFromSolfege(16);
                // 低音7 - 使用动态键位
                else if (CheckDynamicKeys(true, 0, 1, 2, 3, 4, 5, 6, 8, 9))
                    return GetFrequencyFromSolfege(7);
                // 中音1 - 使用动态键位
                else if (CheckDynamicKeys(true, 1, 2, 3, 4, 5, 6, 8, 9))
                    return GetFrequencyFromSolfege(1);
                // 中音1# - 使用动态键位
                else if (CheckDynamicKeys(true, 0, 1, 2, 3, 4, 5, 8, 9))
                    return GetFrequencyFromSolfege(11);
                // 中音2 - 使用动态键位
                else if (CheckDynamicKeys(true, 1, 2, 3, 4, 5, 8, 9))
                    return GetFrequencyFromSolfege(2);
                // 中音2# - 使用动态键位
                else if (CheckDynamicKeys(true, 0, 1, 2, 3, 4, 8, 9))
                    return GetFrequencyFromSolfege(12);
                // 中音3 - 使用动态键位
                else if (CheckDynamicKeys(true, 1, 2, 3, 4, 8, 9))
                    return GetFrequencyFromSolfege(3);
                // 中音4 - 使用动态键位
                else if (CheckDynamicKeys(true, 1, 2, 3, 5, 6, 8, 9))
                    return GetFrequencyFromSolfege(4);
                // 中音4# - 使用动态键位
                else if (CheckDynamicKeys(true, 1, 2, 3, 5, 8, 9))
                    return GetFrequencyFromSolfege(14);
                // 中音5 - 使用动态键位
                else if (CheckDynamicKeys(true, 1, 2, 3, 8, 9))
                    return GetFrequencyFromSolfege(25);
                // 中音5# - 使用动态键位
                else if (CheckDynamicKeys(true, 2, 3, 5, 8, 9))
                    return GetFrequencyFromSolfege(35);
                // 中音6 - 使用动态键位
                else if (CheckDynamicKeys(true, 2, 3, 8, 9))
                    return GetFrequencyFromSolfege(26);
                // 中音6# - 使用动态键位
                else if (CheckDynamicKeys(true, 3, 5, 6, 8, 9))
                    return GetFrequencyFromSolfege(36);
                // 中音7 - 使用动态键位
                else if (CheckDynamicKeys(true, 3, 8, 9))
                    return GetFrequencyFromSolfege(27);
                // 高音1 - 使用动态键位
                else if (CheckDynamicKeys(true, 8, 9))
                    return GetFrequencyFromSolfege(31);
                // 高音1# - 使用动态键位
                else if (CheckDynamicKeys(true, 2, 9))
                    return GetFrequencyFromSolfege(41);
                // 高音2 - 使用动态键位
                else if (CheckDynamicKeys(true, 9))
                    return GetFrequencyFromSolfege(32);
                // 高音2# - 使用动态键位
                else if (CheckDynamicKeys(true, 2))
                    return GetFrequencyFromSolfege(42);
                // 高音3 - 使用动态键位
                else if (CheckDynamicKeys(true, 11))
                    return GetFrequencyFromSolfege(33);
                else
                {
                    return GetFrequencyFromSolfege(-1); // 默认高音1
                }
            }
            else
            {
                // 八孔模式使用动态键位设置
                if (CheckDynamicKeys(false, 0, 1, 2, 3, 4, 5, 6, 7))
                    return GetFrequencyFromSolfege(5); // 低音5
                else if (CheckDynamicKeys(false, 0, 1, 2, 3, 4, 5, 6, 8))
                    return GetFrequencyFromSolfege(15); // 低音5#
                else if (CheckDynamicKeys(false, 0, 1, 2, 3, 4, 5, 6))
                    return GetFrequencyFromSolfege(6); // 低音6
                else if (CheckDynamicKeys(false, 0, 1, 2, 3, 4, 6, 7))
                    return GetFrequencyFromSolfege(16); // 低音6#
                else if (CheckDynamicKeys(false, 0, 1, 2, 3, 4, 5, 7))
                    return GetFrequencyFromSolfege(7); // 低音7
                else if (CheckDynamicKeys(false, 0, 1, 2, 3, 4, 5))
                    return GetFrequencyFromSolfege(1); // 中音1
                else if (CheckDynamicKeys(false, 0, 1, 2, 3, 4, 7))
                    return GetFrequencyFromSolfege(11); // 中音1#
                else if (CheckDynamicKeys(false, 0, 1, 2, 3, 4))
                    return GetFrequencyFromSolfege(2); // 中音2
                else if (CheckDynamicKeys(false, 1, 2, 3, 4, 7))
                    return GetFrequencyFromSolfege(12); // 中音2#
                else if (CheckDynamicKeys(false, 1, 2, 3, 4))
                    return GetFrequencyFromSolfege(3); // 中音3
                else if (CheckDynamicKeys(false, 2, 3, 4, 5, 6))
                    return GetFrequencyFromSolfege(4); // 中音4
                else if (CheckDynamicKeys(false, 2, 3, 4, 5))
                    return GetFrequencyFromSolfege(14); // 中音4#
                else if (CheckDynamicKeys(false, 2, 3, 4))
                    return GetFrequencyFromSolfege(25); // 中音5
                else if (CheckDynamicKeys(false, 3, 4, 5))
                    return GetFrequencyFromSolfege(35); // 中音5#
                else if (CheckDynamicKeys(false, 3, 4))
                    return GetFrequencyFromSolfege(26); // 中音6
                else if (CheckDynamicKeys(false, 4, 5, 6))
                    return GetFrequencyFromSolfege(36); // 中音6#
                else if (CheckDynamicKeys(false, 4))
                    return GetFrequencyFromSolfege(27); // 中音7
                // 高音1 - 上述按键都不按
                else
                    return GetFrequencyFromSolfege(31);
            }
    }

    private bool CheckKeys(params KeyCode[] keys)
    {
        foreach (KeyCode k in keys)
        {
            if (!_keyStates.TryGetValue(k, out bool state) || !state)
                return false;
        }
        return true;
    }

private bool CheckAnyKeys()
    {
        if (isTenHoleMode)
        {
            // 检查是否有任何十孔相关的按键被按下（使用动态键位）
            // 十孔键位映射：Q=0, 1=1, 2=2, 3=3, R=4, I=5, 9=6, 0=7, [=8, C=9, M=10, Space=11
            for (int i = 0; i < currentTenHoleKeys.Length; i++)
            {
                if (_keyStates.GetValueOrDefault(currentTenHoleKeys[i]))
                {
                    return true;
                }
            }
            return false;
        }
        else
        {
            // 检查是否有任何八孔相关的按键被按下（使用动态键位）
            for (int i = 0; i < currentEightHoleKeys.Length; i++)
            {
                if (_keyStates.GetValueOrDefault(currentEightHoleKeys[i]))
                {
                    return true;
                }
            }
            return false;
        }
    }


    private int GetAnimatorState(float baseFrequency)
    {
        if (isTenHoleMode)
        {
                            // 使用动态键位设置，按优先级从高到低检测
                // 低音5 - 使用动态键位
                if (CheckDynamicKeys(true, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9))
                    return 0;
                // 低音5# - 使用动态键位
                else if (CheckDynamicKeys(true, 10, 1, 2, 3, 4, 5, 6, 7, 8, 9))
                    return 1;
                // 低音6 - 使用动态键位
                else if (CheckDynamicKeys(true, 1, 2, 3, 4, 5, 6, 7, 8, 9))
                    return 2;
                // 低音6# - 使用动态键位
                else if (CheckDynamicKeys(true, 0, 1, 2, 3, 4, 5, 7, 8, 9))
                    return 3;
                // 低音7 - 使用动态键位
                else if (CheckDynamicKeys(true, 0, 1, 2, 3, 4, 5, 6, 8, 9))
                    return 4;
                // 中音1 - 使用动态键位
                else if (CheckDynamicKeys(true, 1, 2, 3, 4, 5, 6, 8, 9))
                    return 5;
                // 中音1# - 使用动态键位
                else if (CheckDynamicKeys(true, 0, 1, 2, 3, 4, 5, 8, 9))
                    return 6;
                // 中音2 - 使用动态键位
                else if (CheckDynamicKeys(true, 1, 2, 3, 4, 5, 8, 9))
                    return 7;
                // 中音2# - 使用动态键位
                else if (CheckDynamicKeys(true, 0, 1, 2, 3, 4, 8, 9))
                    return 8;
                // 中音3 - 使用动态键位
                else if (CheckDynamicKeys(true, 1, 2, 3, 4, 8, 9))
                    return 9;
                // 中音4 - 使用动态键位
                else if (CheckDynamicKeys(true, 1, 2, 3, 5, 6, 8, 9))
                    return 10;
                // 中音4# - 使用动态键位
                else if (CheckDynamicKeys(true, 1, 2, 3, 5, 8, 9))
                    return 11;
                // 中音5 - 使用动态键位
                else if (CheckDynamicKeys(true, 1, 2, 3, 8, 9))
                    return 12;
                // 中音5# - 使用动态键位
                else if (CheckDynamicKeys(true, 2, 3, 5, 8, 9))
                    return 13;
                // 中音6 - 使用动态键位
                else if (CheckDynamicKeys(true, 2, 3, 8, 9))
                    return 14;
                // 中音6# - 使用动态键位
                else if (CheckDynamicKeys(true, 3, 5, 6, 8, 9))
                    return 15;
                // 中音7 - 使用动态键位
                else if (CheckDynamicKeys(true, 3, 8, 9))
                    return 16;
                // 高音1 - 使用动态键位
                else if (CheckDynamicKeys(true, 8, 9))
                    return 17;
                // 高音1# - 使用动态键位
                else if (CheckDynamicKeys(true, 2, 9))
                    return 18;
                // 高音2 - 使用动态键位
                else if (CheckDynamicKeys(true, 9))
                    return 19;
                // 高音2# - 使用动态键位
                else if (CheckDynamicKeys(true, 2))
                    return 20;
                // 高音3 - 使用动态键位
                else if (CheckDynamicKeys(true, 11))
                    return 21;
                else
                {
                    return 21; // 默认高音1
                }
        }
        else
        {
            // 根据新的指法表返回对应的动画状态（使用动态键位）
            // 八孔键位映射：A=0, W=1, E=2, F=3, J=4, I=5, O=6, ;=7, P=8, Space=9
            
            // 低音5 - AWEFJIO；
            if (CheckDynamicKeys(false, 0, 1, 2, 3, 4, 5, 6, 7))
                return 0;
            // 低音5# - AWEFJIOP
            else if (CheckDynamicKeys(false, 0, 1, 2, 3, 4, 5, 6, 8))
                return 1;
            // 低音6 - AWEFJIO
            else if (CheckDynamicKeys(false, 0, 1, 2, 3, 4, 5, 6))
                return 2;
            // 低音6# - AWEFJO；
            else if (CheckDynamicKeys(false, 0, 1, 2, 3, 4, 6, 7))
                return 3;
            // 低音7 - AWEFJI；
            else if (CheckDynamicKeys(false, 0, 1, 2, 3, 4, 5, 7))
                return 4;
            // 中音1 - AWEFJI
            else if (CheckDynamicKeys(false, 0, 1, 2, 3, 4, 5))
                return 5;
            // 中音1# - AWEFJ；
            else if (CheckDynamicKeys(false, 0, 1, 2, 3, 4, 7))
                return 6;
            // 中音2 - AWEFJ
            else if (CheckDynamicKeys(false, 0, 1, 2, 3, 4))
                return 7;
            // 中音2# - WEFJ；
            else if (CheckDynamicKeys(false, 1, 2, 3, 4, 7))
                return 8;
            // 中音3 - WEFJ
            else if (CheckDynamicKeys(false, 1, 2, 3, 4))
                return 9;
            // 中音4 - EFJIO
            else if (CheckDynamicKeys(false, 2, 3, 4, 5, 6))
                return 10;
            // 中音4# - EFJI
            else if (CheckDynamicKeys(false, 2, 3, 4, 5))
                return 11;
            // 中音5 - EFJ
            else if (CheckDynamicKeys(false, 2, 3, 4))
                return 12;
            // 中音5# - FJI
            else if (CheckDynamicKeys(false, 3, 4, 5))
                return 13;
            // 中音6 - FJ
            else if (CheckDynamicKeys(false, 3, 4))
                return 14;
            // 中音6# - JIO
            else if (CheckDynamicKeys(false, 4, 5, 6))
                return 15;
            // 中音7 - J
            else if (_keyStates.GetValueOrDefault(GetDynamicKey(4, false)))
                return 16;
            // 高音1 - 上述按键都不按
            else
                return 17;
        }
    }

private string GetNoteName(float frequency)
    {
        if (frequency <= 0f) return "";
        
        // 根据当前调号获取主音的频率（第4八度）
        float tonicFrequency = GetTonicFrequency(key);
        
        // 计算相对于当前调号主音的半音数差
        float semitonesFromTonic = 12f * Mathf.Log(frequency / tonicFrequency, 2f);
        int semitones = Mathf.RoundToInt(semitonesFromTonic);
        
        // 计算音符在简谱中的位置（1-7）
        int noteIndex = semitones % 12;
        if (noteIndex < 0) noteIndex += 12;
        
        // 简谱音符映射（相对于调号主音）
        string[] solfegeNames = { "1", "1♯", "2", "2♯", "3", "4", "4♯", "5", "5♯", "6", "6♯", "7" };
        
        // 计算八度（相对于调号主音的第4八度）
        int octaveOffset = semitones / 12;
        
        // 确定音高前缀
        string prefix;
        if (octaveOffset < 0)
        {
            prefix = "低音";
        }
        else if (octaveOffset == 0)
        {
            prefix = "中音";
        }
        else
        {
            prefix = "高音";
        }
        
        return prefix + solfegeNames[noteIndex];
    }
    
    // 获取固定的音名（以C调为基准，不随调号变化）
    private string GetFixedNoteName(float frequency)
    {
        if (frequency <= 0f) return "";
        
        // 以C4 = 261.63Hz为基准
        float c4Frequency = 261.63f;
        
        // 计算相对于C4的半音数差
        float semitonesFromC4 = 12f * Mathf.Log(frequency / c4Frequency, 2f);
        int semitones = Mathf.RoundToInt(semitonesFromC4);
        
        // 正确计算八度和音符索引
        int octaveOffset;
        int noteIndex;
        
        if (semitones >= 0)
        {
            octaveOffset = semitones / 12;
            noteIndex = semitones % 12;
        }
        else
        {
            // 对于负数，需要特殊处理以确保正确的八度计算
            octaveOffset = (semitones - 11) / 12;  // 向下取整
            noteIndex = semitones - octaveOffset * 12;  // 确保noteIndex在0-11范围内
        }
        
        // 固定简谱音符映射（以C调为基准：C=1, D=2, E=3, F=4, G=5, A=6, B=7）
        string[] fixedSolfegeNames = { "1", "1♯", "2", "2♯", "3", "4", "4♯", "5", "5♯", "6", "6♯", "7" };
        
        // 确定音高前缀
        string prefix;
        if (octaveOffset < 0)
        {
            prefix = "低音";
        }
        else if (octaveOffset == 0)
        {
            prefix = "中音";
        }
        else
        {
            prefix = "高音";
        }
        
        return prefix + fixedSolfegeNames[noteIndex];
    }
    
    // 根据调号获取主音的频率（第4八度）
    private float GetTonicFrequency(int keyValue)
    {
        // 调号对应的主音半音数（相对于C）
        int tonicSemitone = keyValue switch
        {
            -4 => 8,  // A♭
            -3 => 9,  // A
            -2 => 10, // B♭
            -1 => 11, // B
            0 => 0,   // C
            1 => 1,   // D♭
            2 => 2,   // D
            3 => 3,   // E♭
            4 => 4,   // E
            5 => 5,   // F
            6 => 6,   // F♯
            7 => 7,   // G
            _ => 0    // 默认C
        };
        
        // C4 = 261.63Hz 作为基准，计算主音频率
        float c4Frequency = 261.63f;
        return c4Frequency * Mathf.Pow(2f, tonicSemitone / 12f);
    }

    private void UpdateUI(float frequency)
    {
        // 更新音符名称 - 使用基于按键组合的固定音名（不随调号变化）
        if (text != null)
            text.text = GetFixedNoteNameFromKeys();
        
        // 更新八度显示
        if (OttaText != null)
            OttaText.text = ottava >= 0
                ? $"升{(ottava * 8)}度" 
                : $"降{Mathf.Abs(ottava * 8)}度";
            
        // 更新调号显示
        UpdateKeyText();
        
        // 如果正在使用手柄，在UI中显示当前按下的按键
        if (_isGamepadConnected && _isGamepadButtonPressed && keyText != null)
        {
            string buttonName = "";
            if (_buttonAPressed) buttonName = "A - 低音6";
            else if (_buttonBPressed) buttonName = "B - 中音2";
            else if (_buttonXPressed) buttonName = "X - 中音5";
            else if (_buttonYPressed) buttonName = "Y - 高音1";
            
            // 在调号文本后添加手柄按键信息
            keyText.text += $" (手柄: {buttonName})";
        }
    }

    // 获取基于按键组合的固定音名（不受调号影响）
    private string GetFixedNoteNameFromKeys()
    {
        if (_isGamepadConnected && _isGamepadButtonPressed)
        {
            // 获取基础音名对应的简谱数字
            int baseSolfegeNote = -1;
            if (_buttonAPressed)
                baseSolfegeNote = 6; // 低音6
            else if (_buttonBPressed)
                baseSolfegeNote = 2; // 中音2
            else if (_buttonXPressed)
                baseSolfegeNote = 5; // 中音5 (实际显示)
            else if (_buttonYPressed)
                baseSolfegeNote = 1; // 高音1 (实际显示)
            
            if (baseSolfegeNote != -1)
            {
                // 计算摇杆调整的半音偏移
                int semitoneOffset = 0;
                
                if (Mathf.Abs(_gamepadLeftStickX) > 0.5f || Mathf.Abs(_gamepadLeftStickY) > 0.5f)
                {
                    // 左右摇杆：降低/升高半音
                    if (_gamepadLeftStickX < -0.5f)
                        semitoneOffset -= 1; // 降低半音
                    else if (_gamepadLeftStickX > 0.5f)
                        semitoneOffset += 1; // 升高半音
                        
                    // 上下摇杆：升高/降低全音
                    if (_gamepadLeftStickY > 0.5f)
                        semitoneOffset += 2; // 升高全音
                    else if (_gamepadLeftStickY < -0.5f)
                        semitoneOffset -= 2; // 降低全音
                }
                
                // 根据调整后的音名返回显示文本
                return GetSolfegeNameWithOffset(baseSolfegeNote, semitoneOffset);
            }
        }
        else
        {
            if (isTenHoleMode)
            {
                // 十孔模式按键组合对应的固定音名（以C调为基准）
                if (CheckDynamicKeys(true, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9))
                    return "低音5";
                else if (CheckDynamicKeys(true, 1, 2, 3, 4, 5, 6, 7, 8, 9))
                    return "低音5♯";
                else if (CheckDynamicKeys(true, 1, 2, 3, 4, 5, 6, 7, 8, 9))
                    return "低音6";
                else if (CheckDynamicKeys(true, 0, 1, 2, 3, 4, 5, 7, 8, 9))
                    return "低音6♯";
                else if (CheckDynamicKeys(true, 0, 1, 2, 3, 4, 5, 6, 8, 9))
                    return "低音7";
                else if (CheckDynamicKeys(true, 1, 2, 3, 4, 5, 6, 8, 9))
                    return "中音1";
                else if (CheckDynamicKeys(true, 0, 1, 2, 3, 4, 5, 8, 9))
                    return "中音1♯";
                else if (CheckDynamicKeys(true, 1, 2, 3, 4, 5, 8, 9))
                    return "中音2";
                else if (CheckDynamicKeys(true, 0, 1, 2, 3, 4, 8, 9))
                    return "中音2♯";
                else if (CheckDynamicKeys(true, 1, 2, 3, 4, 8, 9))
                    return "中音3";
                else if (CheckDynamicKeys(true, 1, 2, 3, 5, 6, 8, 9))
                    return "中音4";
                else if (CheckDynamicKeys(true, 1, 2, 3, 5, 8, 9))
                    return "中音4♯";
                else if (CheckDynamicKeys(true, 1, 2, 3, 8, 9))
                    return "中音5";
                else if (CheckDynamicKeys(true, 2, 3, 5, 8, 9))
                    return "中音5♯";
                else if (CheckDynamicKeys(true, 2, 3, 8, 9))
                    return "中音6";
                else if (CheckDynamicKeys(true, 3, 5, 6, 8, 9))
                    return "中音6♯";
                else if (CheckDynamicKeys(true, 3, 8, 9))
                    return "中音7";
                else if (CheckDynamicKeys(true, 8, 9))
                    return "高音1";
                else if (CheckDynamicKeys(true, 2, 9))
                    return "高音1♯";
                else if (CheckDynamicKeys(true, 9))
                    return "高音2";
                else if (CheckDynamicKeys(true, 2))
                    return "高音2♯";
                else if (CheckDynamicKeys(true, 11)) // 空格键，索引11
                    return "高音3";
            }
            else
            {
                // 八孔模式按键组合对应的固定音名（以C调为基准）
                if (CheckDynamicKeys(false, 0, 1, 2, 3, 4, 5, 6, 7))
                    return "低音5"; // AWEFJIO；
                else if (CheckDynamicKeys(false, 0, 1, 2, 3, 4, 5, 6, 8))
                    return "低音5♯"; // AWEFJIOP
                else if (CheckDynamicKeys(false, 0, 1, 2, 3, 4, 5, 6))
                    return "低音6"; // AWEFJIO
                else if (CheckDynamicKeys(false, 0, 1, 2, 3, 4, 6, 7))
                    return "低音6♯"; // AWEFJO；
                else if (CheckDynamicKeys(false, 0, 1, 2, 3, 4, 5, 7))
                    return "低音7"; // AWEFJI；
                else if (CheckDynamicKeys(false, 0, 1, 2, 3, 4, 5))
                    return "中音1"; // AWEFJI
                else if (CheckDynamicKeys(false, 0, 1, 2, 3, 4, 7))
                    return "中音1♯"; // AWEFJ；
                else if (CheckDynamicKeys(false, 0, 1, 2, 3, 4))
                    return "中音2"; // AWEFJ
                else if (CheckDynamicKeys(false, 1, 2, 3, 4, 7))
                    return "中音2♯"; // WEFJ；
                else if (CheckDynamicKeys(false, 1, 2, 3, 4))
                    return "中音3"; // WEFJ
                else if (CheckDynamicKeys(false, 2, 3, 4, 5, 6))
                    return "中音4"; // EFJIO
                else if (CheckDynamicKeys(false, 2, 3, 4, 5))
                    return "中音4♯"; // EFJI
                else if (CheckDynamicKeys(false, 2, 3, 4))
                    return "中音5"; // EFJ
                else if (CheckDynamicKeys(false, 3, 4, 5))
                    return "中音5♯"; // FJI
                else if (CheckDynamicKeys(false, 3, 4))
                    return "中音6"; // FJ
                else if (CheckDynamicKeys(false, 4, 5, 6))
                    return "中音6♯"; // JIO
                else if (CheckDynamicKeys(false, 4))
                    return "中音7"; // J
                else
                    return "高音1"; // 上述按键都不按
            }
        }
        
        return ""; // 没有按键时返回空字符串
    }
    
    // 根据基础简谱音名和半音偏移计算调整后的音名
    private string GetSolfegeNameWithOffset(int baseSolfegeNote, int semitoneOffset)
    {
        // 获取基础半音位置
        int baseSemitone = GetBaseSemitoneForGamepadButton(baseSolfegeNote);
        
        // 应用半音偏移
        int finalSemitone = baseSemitone + semitoneOffset;
        
        // 将半音位置转换回简谱音名
        return ConvertSemitoneToSolfegeName(finalSemitone);
    }
    
    // 获取手柄按键对应的基础半音位置
    private int GetBaseSemitoneForGamepadButton(int solfegeNote)
    {
        // 手柄按键映射：
        // A按键：低音6 = 9半音
        // B按键：中音2 = 14半音  
        // X按键：中音5 = 19半音
        // Y按键：高音1 = 24半音
        
        switch (solfegeNote)
        {
            case 6: return 9;   // 低音6 (A按键)
            case 2: return 14;  // 中音2 (B按键) 
            case 5: return 19;  // 中音5 (X按键)
            case 1: return 24;  // 高音1 (Y按键)
            default: return 0;
        }
    }
    
    // 将半音位置转换为简谱音名显示
    private string ConvertSemitoneToSolfegeName(int semitone)
    {
        // 确保半音在合理范围内
        if (semitone < 0) semitone = 0;
        if (semitone > 35) semitone = 35;
        
        // 半音到简谱音名的映射
        string[] solfegeNames = {
            "低音1", "低音1♯", "低音2", "低音2♯", "低音3", "低音4", "低音4♯", "低音5", "低音5♯", "低音6", "低音6♯", "低音7",
            "中音1", "中音1♯", "中音2", "中音2♯", "中音3", "中音4", "中音4♯", "中音5", "中音5♯", "中音6", "中音6♯", "中音7",
            "高音1", "高音1♯", "高音2", "高音2♯", "高音3", "高音4", "高音4♯", "高音5", "高音5♯", "高音6", "高音6♯", "高音7"
        };
        
        return solfegeNames[semitone];
    }

    private void UpdateKeyText()
    {
        if (keyText != null)
        {
            keyText.text = key switch
            {
                -4 => "1=A\u266D",
                -3 => "1=A",
                -2 => "1=B\u266D",
                -1 => "1=B",
                0 => "1=C",
                1 => "1=D\u266D",
                2 => "1=D",
                3 => "1=E\u266D",
                4 => "1=E",
                5 => "1=F",
                6 => "1=F\u266F",
                7 => "1=G",
                _ => "---"
            };
        }
    }

    private void SetFilterCutoff(float frequency)
    {
        float bandwidth = 120f;
        float lowPass = Mathf.Clamp(frequency + bandwidth, 300f, 1700f);
        float highPass = Mathf.Max(frequency * 0.45f, 70f);
        _lowPassFilter.cutoffFrequency = Mathf.Lerp(_lowPassFilter.cutoffFrequency, lowPass, _previousDelta * 16f);
        _highPassFilter.cutoffFrequency = Mathf.Lerp(_highPassFilter.cutoffFrequency, highPass, _previousDelta * 16f);
        _lowPassFilter.lowpassResonanceQ = Mathf.Lerp(1.2f, 1.8f, Mathf.Clamp01(frequency / 950f));
        _echoFilter.enabled = frequency < 420f && _isSpacePressed;
        _echoFilter.decayRatio = _echoFilter.enabled ? 0.1f : 0f;
        if (frequency > 800f)
        {
            float targetCutoff = 760f * preResonanceRatio;
            _lowPassFilter.cutoffFrequency = Mathf.Lerp(_lowPassFilter.cutoffFrequency, targetCutoff, _previousDelta * 0.4f);
        }
    }

    private void ComputeBandpass(float f, float q, ref float b0, ref float b1, ref float b2, ref float a1, ref float a2)
    {
        float w = 2f * Mathf.PI * f / sampleRate;
        float cosw = Mathf.Cos(w);
        float sinw = Mathf.Sin(w);
        float alpha = sinw / (2f * Mathf.Max(0.001f, q));
        float a0 = 1f + alpha;
        b0 = alpha / a0;
        b1 = 0f;
        b2 = -alpha / a0;
        a1 = (-2f * cosw) / a0;
        a2 = (1f - alpha) / a0;
    }

    private void UpdateFormantCoefficients(float frequency)
    {
        float f1 = Mathf.Clamp(frequency * Mathf.Lerp(0.9f, 1.05f, 0.5f), 80f, 3000f);
        float q1 = Mathf.Lerp(1.1f, 2.2f, Mathf.Clamp01(formantWidth * 0.3f));
        float f2 = Mathf.Clamp(frequency * (1.9f + Mathf.Clamp01(formantCoupling) * 0.3f), 200f, 5000f);
        float q2 = Mathf.Lerp(1.2f, 2.5f, Mathf.Clamp01(formantWidth * 0.2f));
        ComputeBandpass(f1, q1, ref _bp1_b0, ref _bp1_b1, ref _bp1_b2, ref _bp1_a1, ref _bp1_a2);
        ComputeBandpass(f2, q2, ref _bp2_b0, ref _bp2_b1, ref _bp2_b2, ref _bp2_a1, ref _bp2_a2);
    }

    private float ApplyFormants(float x)
    {
        float y1 = _bp1_b0 * x + _bp1_b1 * _bp1_x1 + _bp1_b2 * _bp1_x2 - _bp1_a1 * _bp1_y1 - _bp1_a2 * _bp1_y2;
        _bp1_x2 = _bp1_x1;
        _bp1_x1 = x;
        _bp1_y2 = _bp1_y1;
        _bp1_y1 = y1;
        float y2 = _bp2_b0 * x + _bp2_b1 * _bp2_x1 + _bp2_b2 * _bp2_x2 - _bp2_a1 * _bp2_y1 - _bp2_a2 * _bp2_y2;
        _bp2_x2 = _bp2_x1;
        _bp2_x1 = x;
        _bp2_y2 = _bp2_y1;
        _bp2_y1 = y2;
        float mix = Mathf.Lerp(0.35f, 0.55f, Mathf.Clamp01(breathPressure));
        return y1 * (0.6f * mix) + y2 * (0.4f * mix);
    }

    private float GenerateXunWave(float phase)
    {
        UpdateFormantCoefficients(_currentFrequency);
        float f = Mathf.Sin(phase);
        float h2 = Mathf.Sin(phase * 2f) * 0.20f;
        float h3 = Mathf.Sin(phase * 3f) * 0.30f;
        float h4 = Mathf.Sin(phase * 4f) * 0.10f;
        float roll = 1f - 0.75f * Mathf.Clamp01(_currentFrequency / 1200f);
        float baseSum = (f * 0.92f + h3 + h2 + h4) * roll;
        float shaped = ApplyFormants(baseSum);
        return baseSum * 0.75f + shaped * 0.25f;
    }

    private float GenerateAirNoise(float deltaTime)
    {
        _noiseSeed = (_noiseSeed * 16807u) % 2147483647u;
        if (_noiseSeed == 0) _noiseSeed = 1u;
        float rnd = (float)_noiseSeed / 2147483647f;
        if (!float.IsFinite(rnd)) rnd = 0.0f;
        float white = (rnd * 2f - 1f) * Mathf.Lerp(0.08f, 0.22f, Mathf.Clamp01(breathPressure));
        float low = Mathf.Lerp(_previousSmoothNoise, white, Mathf.Clamp01(deltaTime * 22f));
        float tilt = low + (white - low) * 0.25f;
        float freqAtten = Mathf.Lerp(0.92f, 0.65f, Mathf.Clamp01(_currentFrequency / 1200f));
        float dampingTime = _currentFrequency < 400f ? 0.038f : 0.022f;
        _previousSmoothNoise = Mathf.SmoothDamp(_previousSmoothNoise, tilt * freqAtten, ref _noiseValueDamp, dampingTime, float.MaxValue, deltaTime);
        return _previousSmoothNoise * Mathf.Lerp(airNoiseIntensity * 0.78f, airNoiseIntensity * 1.05f, Mathf.Clamp01(breathPressure));
    }

    private float WaveShaping(float sample, float deltaTime)
    {
        // 输入数值安全检查
        if (!float.IsFinite(sample))
            return 0f;
            
        float pressure = Mathf.Clamp01(_gain * 5f);
        float damping = Mathf.Exp(-pressure * 0.6f * (_currentFrequency / 1000f));
        
        // 确保damping值安全
        if (!float.IsFinite(damping) || damping <= 0f)
            damping = 1f;
            
        sample *= damping;

        // 改进动态限幅，防止爆音
        float compressionThreshold = 0.70f;
        float compressionRatio = 4.5f;
        
        if (Mathf.Abs(sample) > compressionThreshold) {
            float excess = Mathf.Abs(sample) - compressionThreshold;
            float compressed = compressionThreshold + (excess / compressionRatio);
            sample = Mathf.Sign(sample) * compressed;
        }

        // 使用更平滑的软限幅
        float softClip = sample / (1f + Mathf.Abs(sample) * 0.8f);
        
        // 频率变化时应用更强的软限幅
        float freqChangeAmount = Mathf.Abs(_frequencyVelocity) / Mathf.Max(0.1f, _currentFrequency);
        
        // 确保freqChangeAmount值安全
        if (!float.IsFinite(freqChangeAmount))
            freqChangeAmount = 0f;
            
        float softClipMix = Mathf.Lerp(0.08f, 0.4f, Mathf.Clamp01(freqChangeAmount * 8f));
        
        // 初始吹奏时也应用更强的软限幅
        if (_gain < 0.05f && _gainVelocity > 0.1f) {
            softClipMix = Mathf.Max(softClipMix, 0.35f);
        }
        
        sample = (softClip * softClipMix) + (sample * (1f - softClipMix));

        // 数值安全检查
        if (!float.IsFinite(sample))
            sample = 0f;

        // 线程安全的输入检测（使用缓存变量）
        if (!_isSpacePressed)
        {
            float decayExponent = _currentFrequency < 400f ? 0.2f : 0.6f;
            float decayFactor = Mathf.Pow(0.5f, decayExponent * (float)(AudioSettings.dspTime * 2));
            
            // 确保衰减因子安全
            if (!float.IsFinite(decayFactor))
                decayFactor = 0f;
                
            return sample * decayFactor;
        }
        
        // 最终限幅保护
        return Mathf.Clamp(sample, -1f, 1f);
    }
    
    // 检测手柄连接状态
    private void CheckGamepadConnection()
    {
        var gamepads = Gamepad.all;
        _isGamepadConnected = gamepads.Count > 0;
    }
    
    // 更新手柄输入状态
    private void UpdateGamepadStates()
    {
        if (!_isGamepadConnected) return;
        
        var gamepad = Gamepad.current;
        if (gamepad == null) return;
        
        // 检测ABXY按键
        bool prevButtonState = _isGamepadButtonPressed;
        _buttonAPressed = gamepad.aButton.isPressed;
        _buttonBPressed = gamepad.bButton.isPressed;
        _buttonXPressed = gamepad.xButton.isPressed;
        _buttonYPressed = gamepad.yButton.isPressed;
        
        // 更新是否有任意手柄按键被按下
        _isGamepadButtonPressed = _buttonAPressed || _buttonBPressed || _buttonXPressed || _buttonYPressed;
        
        // 读取左摇杆输入（仅用于在按下ABXY时调整频率）
        Vector2 leftStick = gamepad.leftStick.ReadValue();
        _gamepadLeftStickX = leftStick.x;
        _gamepadLeftStickY = leftStick.y;
        
        // 读取方向键输入
        bool dpadUpPressed = gamepad.dpad.up.wasPressedThisFrame;
        bool dpadDownPressed = gamepad.dpad.down.wasPressedThisFrame;
        bool dpadLeftPressed = gamepad.dpad.left.wasPressedThisFrame;
        bool dpadRightPressed = gamepad.dpad.right.wasPressedThisFrame;
        
        // 使用方向键上下控制八度
        if (dpadUpPressed)
        {
            ottava = Mathf.Min(ottava + 1, 2); // 升高八度
        }
        else if (dpadDownPressed)
        {
            ottava = Mathf.Max(ottava - 1, -3); // 降低八度
        }
        
        // 使用方向键左右控制调号（半音变化）
        if (dpadRightPressed)
        {
            key = Mathf.Min(key + 1, 7); // 升高半音
        }
        else if (dpadLeftPressed)
        {
            key = Mathf.Max(key - 1, -4); // 降低半音
        }
        
        // 限制ottava范围
        ottava = Mathf.Clamp(ottava, -3, 2);
    }

        // 获取当前频率的公共方法
    public float GetCurrentFrequency()
    {
        return _currentFrequency;
    }
    
    // 获取目标频率的公共方法
    public float GetTargetFrequency()
    {
        return _targetFrequency;
    }
    
// 添加当前频率的公共属性
    public float CurrentFrequency => _currentFrequency;


    // 检测当前演奏的音符是否与挑战模式中的目标音符匹配
void CheckNoteForChallenge()
    {
        // 只在挑战模式下检测音符
        if (ChallengeManager.Instance == null || !ChallengeManager.Instance.IsInChallenge())
            return;
            
        // 检查是否有足够的音量（正在演奏）
        if (_gain < 0.1f)
            return;
            
        // 检查冷却时间
        if (Time.time - lastNoteTime < noteCooldown)
            return;
            
        // 获取当前音符名称
        string currentNote = GetCurrentNoteName();
        if (string.IsNullOrEmpty(currentNote))
            return;
            
        Debug.Log($"正在检测音符: 当前={currentNote}, 音量={_gain:F2}");
        
        // 记录所有演奏的音符，不管是否正确
        lastNoteTime = Time.time;
        ChallengeManager.Instance.OnNoteDetected(currentNote);
        Debug.Log($"记录演奏音符: {currentNote}");
    }
    
    // 获取当前演奏的音符名称
    public string GetCurrentNoteName()
    {
        bool isBlowing=false;
        // 首先检查是否在"吹气"（按下空格键或手柄按键）
        if (isTenHoleMode)
        {
            isBlowing = CheckAnyKeys();
        }
        else
        {
            isBlowing = _isSpacePressed || _isGamepadButtonPressed ||(isTenHoleMode&&CheckAnyKeys());
        }

        if (!isBlowing)
        {
            return ""; // 没有吹气就没有演奏
        }
        
        // 获取实际演奏的频率（包含调号和八度调整）
        float actualFrequency = GetFrequency();
        
        // 应用八度调整（key调号调整已在GetFrequency中应用）
        float finalFrequency = actualFrequency * Mathf.Pow(2f, ottava);
        
        Debug.Log($"GetCurrentNoteName: 基础频率={actualFrequency:F2}Hz, 最终频率={finalFrequency:F2}Hz, 八度={ottava}, 调号={key}");
        
        // 转换为标准音符名称
        return GetNoteFromFrequency(finalFrequency);
    }
    

    
    // 检查是否为正确的音符
    private bool IsCorrectNote(string currentNote, string expectedNote)
    {
        // 提取音符名称的主要部分（去掉八度数字）
        string currentNoteBase = ExtractNoteBase(currentNote);
        string expectedNoteBase = ExtractNoteBase(expectedNote);
        
        // 比较基础音符名称
        return string.Equals(currentNoteBase, expectedNoteBase, System.StringComparison.OrdinalIgnoreCase);
    }
    
    // 提取音符的基础名称（去掉八度信息）
    private string ExtractNoteBase(string noteName)
    {
        if (string.IsNullOrEmpty(noteName))
            return "";
            
        string result = "";
        foreach (char c in noteName)
        {
            if (char.IsLetter(c) || c == '#')
            {
                result += c;
            }
            else
            {
                break; // 遇到数字就停止
            }
        }
        
        return result;
    }
    
    // 重置音频状态，防止异常状态导致的爆音或静音
    private void ResetAudioState()
    {
        _gain = 0f;
        _gainVelocity = 0f;
        _currentFrequency = 440f;
        _frequencyVelocity = 0f;
        _previousSample = 0f;
        currentPhaseD = 0.0;
        _noiseSeed = 123456789;
        _previousSmoothNoise = 0f;
        _noiseValueDamp = 0f;
        
        Debug.Log("音频状态已重置");
    }
    
    // 检测音频异常状态
    private bool IsAudioStateAbnormal()
    {
        return !float.IsFinite(_gain) || 
               !float.IsFinite(_currentFrequency) || 
               !double.IsFinite(currentPhaseD) ||
               Mathf.Abs(_previousSample) > 2f ||
               _gain < 0f ||
               _currentFrequency < 10f || _currentFrequency > 25000f;
    }
}
