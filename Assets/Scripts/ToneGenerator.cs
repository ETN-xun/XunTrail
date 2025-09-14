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

    [Header("包络控制")]
    public float volume = 0.28f;
    [Range(0.03f, 0.1f)] public float attackTime = 0.05f;
    [Range(0.1f, 0.4f)] public float releaseTime = 0.25f;
    
    // 添加频率变化检测阈值
    [Range(0.001f, 0.1f)] public float frequencyChangeThreshold = 0.05f;

    public bool isStereo = false;

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
    
    void Awake() {
        Instance = this; // 单例模式
        _lowPassFilter = GetOrAddComponent<AudioLowPassFilter>();
        _highPassFilter = GetOrAddComponent<AudioHighPassFilter>();
        _echoFilter = GetOrAddComponent<AudioEchoFilter>();
        _reverbFilter = GetOrAddComponent<AudioReverbFilter>();
    }

    void Start()
    {
        InitializeKeyStates();
        InitializeAudioComponents();
        
        // 初始检测是否有手柄连接
        CheckGamepadConnection();
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

    private void ConfigureAudioComponents()
    {
        if (_lowPassFilter == null || _highPassFilter == null) return;

        _highPassFilter.cutoffFrequency = 80f;
        _highPassFilter.highpassResonanceQ = 0.15f;

        _lowPassFilter.cutoffFrequency = 2500f;
        _lowPassFilter.lowpassResonanceQ = 0.2f;

        _echoFilter.delay = 25f;
        _echoFilter.decayRatio = 0.18f;
        _echoFilter.wetMix = 0.03f;
        _echoFilter.enabled = false;

        _reverbFilter.reverbPreset = AudioReverbPreset.User;
        _reverbFilter.room = 0.18f;
        _reverbFilter.hfReference = 600f;
        _reverbFilter.decayHFRatio = 0.15f;
        _reverbFilter.density = 0.28f;
        _reverbFilter.diffusion = 0.45f;

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
        
        // 检测手柄连接状态
        CheckGamepadConnection();
        
        // 更新输入状态
        UpdateKeyStates();
        UpdateGamepadStates();
        HandleOctaveInput();

        float baseFrequency = GetBaseFrequency();
        float adjustedFrequency = GetFrequency();
        int animatorState = GetAnimatorState(baseFrequency);

        if (xunAnimator != null)
            xunAnimator.SetInteger("tone", animatorState);

        UpdateUI(adjustedFrequency);

        // 计算目标频率
        float newTargetFrequency = adjustedFrequency * Mathf.Pow(2f, ottava + key / 12f);
        
        // 检测频率变化是否显著
        bool significantFrequencyChange = Mathf.Abs(newTargetFrequency - _targetFrequency) / _targetFrequency > 0.05f;

        // 检测是否应该播放声音（空格键或手柄按键）
        bool shouldPlaySound = Input.GetKey(KeyCode.Space) || _isGamepadButtonPressed;
        
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
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (!_lowPassFilter || !_highPassFilter || !_echoFilter || !_reverbFilter)
        {
            Debug.LogError("音频组件未正确初始化！");
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
            
        // 改进相位调整逻辑，防止频率变化时的相位跳变
        if (Mathf.Abs(previousFrequency - _currentFrequency) > 0.01f) {
            // 使用更精确的相位连续性计算
            double phaseRatio = _currentFrequency / previousFrequency;
            currentPhaseD = (currentPhaseD * phaseRatio) % (2.0 * Mathf.PI);
            // 确保相位值始终为正
            if (currentPhaseD < 0) currentPhaseD += 2.0 * Mathf.PI;
        }

        double stepD = (2.0 * Mathf.PI) * _currentFrequency / sampleRate;
        
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
            float phase = (float)(currentPhaseD % (2f * Mathf.PI));
            float baseWave = GenerateXunWave(phase);

            // 线程安全的噪声叠加（使用缓存变量）
            float noise = _isSpacePressed ? GenerateAirNoise(deltaTime) : 0f;
            float totalSignal = baseWave + noise;

            totalSignal = WaveShaping(totalSignal, deltaTime);

            // 添加平滑过渡，防止爆音
            float newSample = _gain * totalSignal * antiClickGain;
            
            // 根据频率变化程度动态调整平滑系数
            float transitionFactor = 0.0f;
            
            if (isInitialAttack) {
                // 初始吹奏时使用更强的平滑
                transitionFactor = 0.95f;
            } else if (isFrequencyChanging) {
                // 频率变化时使用动态平滑系数
                transitionFactor = Mathf.Lerp(0.85f, 0.98f, Mathf.Clamp01(freqChangeAmount * 20f));
            } else {
                // 正常状态下的平滑系数
                transitionFactor = 0.7f;
            }
            
            // 应用平滑处理
            newSample = previousSample * transitionFactor + newSample * (1.0f - transitionFactor);

            // 应用额外的平滑处理，消除频率变化时的噪声
            if (freqChangeAmount > 0.01f || isInitialAttack) {
                // 在频率变化较大或初始吹奏时应用更强的平滑
                float antiClickFactor = Mathf.Exp(-(freqChangeAmount * 10f + (isInitialAttack ? 0.5f : 0f)));
                newSample = previousSample * (1.0f - antiClickFactor) + newSample * antiClickFactor;
            }

            for (int c = 0; c < channels; c++)
                data[i + c] = newSample;

            previousSample = newSample;
            currentPhaseD += stepD;
        }

        if (currentPhaseD > 2 * Mathf.PI * 1000)
            currentPhaseD -= 2 * Mathf.PI * 1000;

        _previousSample = previousSample;
    }

    private void InitializeKeyStates()
    {
        var keys = new KeyCode[]{
            KeyCode.A, KeyCode.W, KeyCode.E, KeyCode.F,
            KeyCode.J, KeyCode.I, KeyCode.O, KeyCode.Semicolon,
            KeyCode.P, KeyCode.Comma, KeyCode.Period, KeyCode.Space,
            KeyCode.LeftArrow, KeyCode.RightArrow
        };

        foreach (var k in keys)
        {
            if (!_keyStates.ContainsKey(k))
                _keyStates.Add(k, false);
        }
    }

    private void UpdateKeyStates()
    {
        List<KeyCode> keysSnapshot = new List<KeyCode>(_keyStates.Keys);
        foreach (var k in keysSnapshot)
        {
            if (k == KeyCode.Space) continue;
            _keyStates[k] = Input.GetKey(k);
        }

        // 主线程更新空格键状态到私有变量
        _isSpacePressed = Input.GetKey(KeyCode.Space) || _isGamepadButtonPressed;
        _keyStates[KeyCode.Space] = _isSpacePressed;
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
        // 如果有手柄按键被按下，优先使用手柄控制
        if (_isGamepadConnected && _isGamepadButtonPressed)
        {
            // 根据按下的按键获取基础频率
            if (_buttonAPressed)
                return FREQUENCY_LOW_6;
            else if (_buttonBPressed)
                return FREQUENCY_MID_2;
            else if (_buttonXPressed)
                return FREQUENCY_MID_5;
            else if (_buttonYPressed)
                return FREQUENCY_HIGH_1;
        }
        
        // 如果没有手柄输入，使用键盘控制
        if (CheckKeys(KeyCode.A, KeyCode.W, KeyCode.E, KeyCode.F, 
            KeyCode.J, KeyCode.I, KeyCode.O, KeyCode.Semicolon))
            return 196f;
        else if (CheckKeys(KeyCode.W, KeyCode.E, KeyCode.F, 
            KeyCode.J, KeyCode.I, KeyCode.O, KeyCode.Semicolon))
            return 207.65f;
        else if (CheckKeys(KeyCode.E, KeyCode.F, 
            KeyCode.J, KeyCode.I, KeyCode.O, KeyCode.Semicolon))
            return 220f;
        else if (CheckKeys(KeyCode.F, 
            KeyCode.J, KeyCode.I, KeyCode.O, KeyCode.Semicolon))
            return 233.08f;
        else if (CheckKeys(KeyCode.J, KeyCode.I, KeyCode.O, KeyCode.Semicolon))
            return 246.94f;
        else if (CheckKeys(KeyCode.I, KeyCode.O, KeyCode.Semicolon))
            return 261.63f;
        else if (CheckKeys(KeyCode.O, KeyCode.Semicolon))
            return 277.18f;
        else if (_keyStates.GetValueOrDefault(KeyCode.Semicolon))
            return 293.66f;
        else if (CheckKeys(KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.J))
            return 311.13f;
        else if (CheckKeys(KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.J))
            return 329.63f;
        else if (CheckKeys(KeyCode.D, KeyCode.F, KeyCode.J))
            return 349.23f;
        else if (CheckKeys(KeyCode.F, KeyCode.J))
            return 369.99f;
        else if (_keyStates.GetValueOrDefault(KeyCode.J))
            return 392f;
        else if (CheckKeys(KeyCode.A, KeyCode.F, KeyCode.J))
            return 415.30f;
        else if (CheckKeys(KeyCode.F, KeyCode.J))
            return 440f;
        else if (_keyStates.GetValueOrDefault(KeyCode.J))
            return 466.16f;
        else if (_keyStates.GetValueOrDefault(KeyCode.W))
            return 493.88f;
        else
            return 523.26f;
    }

    private float GetFrequency()
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
        
        // 如果没有手柄输入，使用键盘控制
        if (CheckKeys(KeyCode.A, KeyCode.W, KeyCode.E, KeyCode.F, 
            KeyCode.J, KeyCode.I, KeyCode.O, KeyCode.Semicolon))
            return 196f;
        else if (CheckKeys(KeyCode.A, KeyCode.W, KeyCode.E, KeyCode.F, 
            KeyCode.J, KeyCode.I, KeyCode.O))
            return _keyStates.GetValueOrDefault(KeyCode.P) ? 207.65f : 220f;
        else if (CheckKeys(KeyCode.A, KeyCode.W, KeyCode.E, KeyCode.F, 
            KeyCode.J, KeyCode.O))
            return 233.08f;
        else if (CheckKeys(KeyCode.A, KeyCode.W, KeyCode.E, KeyCode.F, 
            KeyCode.J, KeyCode.I))
            return 246.94f;
        else if (CheckKeys(KeyCode.A, KeyCode.W, KeyCode.E, KeyCode.F, 
            KeyCode.J))
            return 261.63f;
        else if (CheckKeys(KeyCode.W, KeyCode.E, KeyCode.F, KeyCode.J, 
            KeyCode.Semicolon))
            return 277.18f;
        else if (CheckKeys(KeyCode.W, KeyCode.E, KeyCode.F, KeyCode.J))
            return 293.66f;
        else if (CheckKeys(KeyCode.O, KeyCode.E, KeyCode.F, KeyCode.J))
            return 311.13f;
        else if (CheckKeys(KeyCode.E, KeyCode.F, KeyCode.J))
            return 329.63f;
        else if (CheckKeys(KeyCode.A, KeyCode.F, KeyCode.J))
            return 349.23f;
        else if (CheckKeys(KeyCode.F, KeyCode.J))
            return 369.99f;
        else if (_keyStates.GetValueOrDefault(KeyCode.J))
            return 392f;
        else if (_keyStates.GetValueOrDefault(KeyCode.W))
            return 415.30f;
        else
            return 440f;
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

    private int GetAnimatorState(float baseFrequency)
    {
        if (CheckKeys(KeyCode.A, KeyCode.W, KeyCode.E, KeyCode.F, 
            KeyCode.J, KeyCode.I, KeyCode.O, KeyCode.Semicolon))
            return 0;
        else if (CheckKeys(KeyCode.A, KeyCode.W, KeyCode.E, KeyCode.F, 
            KeyCode.J, KeyCode.I, KeyCode.O))
            return _keyStates.GetValueOrDefault(KeyCode.P) ? 1 : 2;
        else if (CheckKeys(KeyCode.A, KeyCode.W, KeyCode.E, KeyCode.F, 
            KeyCode.J, KeyCode.O))
            return 3;
        else if (CheckKeys(KeyCode.A, KeyCode.W, KeyCode.E, KeyCode.F, 
            KeyCode.J, KeyCode.I))
            return 4;
        else if (CheckKeys(KeyCode.A, KeyCode.W, KeyCode.E, KeyCode.F, 
            KeyCode.J))
            return 5;
        else if (CheckKeys(KeyCode.W, KeyCode.E, KeyCode.F, KeyCode.J, 
            KeyCode.Semicolon))
            return 6;
        else if (CheckKeys(KeyCode.W, KeyCode.E, KeyCode.F, KeyCode.J))
            return 7;
        else if (CheckKeys(KeyCode.O, KeyCode.E, KeyCode.F, KeyCode.J))
            return 8;
        else if (CheckKeys(KeyCode.E, KeyCode.F, KeyCode.J))
            return 9;
        else if (CheckKeys(KeyCode.A, KeyCode.F, KeyCode.J))
            return 10;
        else if (CheckKeys(KeyCode.F, KeyCode.J))
            return 11;
        else if (_keyStates.GetValueOrDefault(KeyCode.J))
            return 12;
        else if (_keyStates.GetValueOrDefault(KeyCode.W))
            return 13;
        else
            return 14;
    }

    private string GetNoteName(float frequency)
    {
        // 使用频率范围匹配，允许一定的误差
        float tolerance = 2f;
        
        // 直接映射频率到音名
        if (Mathf.Abs(frequency - 196f) < tolerance) return "低音5";
        if (Mathf.Abs(frequency - 207.65f) < tolerance) return "低音5♯";
        if (Mathf.Abs(frequency - 220f) < tolerance) return "低音6";
        if (Mathf.Abs(frequency - 233.08f) < tolerance) return "低音6♯";
        if (Mathf.Abs(frequency - 246.94f) < tolerance) return "低音7";
        if (Mathf.Abs(frequency - 261.63f) < tolerance) return "中音1";
        if (Mathf.Abs(frequency - 277.18f) < tolerance) return "中音1♯";
        if (Mathf.Abs(frequency - 293.66f) < tolerance) return "中音2";
        if (Mathf.Abs(frequency - 311.13f) < tolerance) return "中音2♯";
        if (Mathf.Abs(frequency - 329.63f) < tolerance) return "中音3";
        if (Mathf.Abs(frequency - 349.23f) < tolerance) return "中音4";
        if (Mathf.Abs(frequency - 369.99f) < tolerance) return "中音4♯";
        if (Mathf.Abs(frequency - 392f) < tolerance) return "中音5";
        if (Mathf.Abs(frequency - 415.30f) < tolerance) return "中音5♯";
        if (Mathf.Abs(frequency - 440f) < tolerance) return "中音6";
        if (Mathf.Abs(frequency - 466.16f) < tolerance) return "中音6♯";
        if (Mathf.Abs(frequency - 493.88f) < tolerance) return "中音7";
        if (Mathf.Abs(frequency - 523.26f) < tolerance) return "高音1";
        if (Mathf.Abs(frequency - 554.37f) < tolerance) return "高音1♯";
        if (Mathf.Abs(frequency - 587.32f) < tolerance) return "高音2";
        
        // 如果没有精确匹配，使用计算方法作为后备
        float c4 = 261.63f;
        float semitonesFromC4 = 12f * Mathf.Log(frequency / c4, 2f);
        int semitones = Mathf.RoundToInt(semitonesFromC4);
        
        string[] noteNames = { "1", "1♯", "2", "2♯", "3", "4", "4♯", "5", "5♯", "6", "6♯", "7" };
        int noteIndex = semitones % 12;
        if (noteIndex < 0) noteIndex += 12;
        
        int octave = 4 + semitones / 12;
        string prefix = octave <= 3 ? "低音" : (octave == 4 ? "中音" : "高音");
        
        return prefix + noteNames[noteIndex];
    }

    private void UpdateUI(float frequency)
    {
        // 更新音符名称
        text.text = GetNoteName(frequency);
        
        // 更新八度显示
        OttaText.text = ottava >= 0
            ? $"升{(ottava * 8)}度" 
            : $"降{Mathf.Abs(ottava * 8)}度";
            
        // 更新调号显示
        UpdateKeyText();
        
        // 如果正在使用手柄，在UI中显示当前按下的按键
        if (_isGamepadConnected && _isGamepadButtonPressed)
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

    private void UpdateKeyText()
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

    private void SetFilterCutoff(float frequency)
    {
        float bandwidth = 150f;
        float lowPass = Mathf.Clamp(frequency + bandwidth, frequency, 2000f);
        float highPass = Mathf.Max(frequency - bandwidth, 50f);

        _lowPassFilter.cutoffFrequency = Mathf.Lerp(
            _lowPassFilter.cutoffFrequency, 
            lowPass, 
            _previousDelta * 20f);

        _highPassFilter.cutoffFrequency = Mathf.Lerp(
            _highPassFilter.cutoffFrequency, 
            highPass,
            _previousDelta * 20f);

        _lowPassFilter.lowpassResonanceQ = Mathf.Lerp(
            1.2f, 
            1.8f, 
            Mathf.Clamp01(frequency / 800f));

        _echoFilter.enabled = frequency < 450f && _isSpacePressed;
        _echoFilter.decayRatio = _echoFilter.enabled ? 0.12f : 0f;

        if (frequency > 800f)
        {
            float targetCutoff = 800f * preResonanceRatio;
            _lowPassFilter.cutoffFrequency = Mathf.Lerp(
                _lowPassFilter.cutoffFrequency, 
                targetCutoff,
                _previousDelta * 0.5f);
        }
    }

    private float GenerateXunWave(float phase)
    {
        float fundamental = Mathf.Sin(phase) * 0.85f;

        float harmonicSum = fundamental;
        float harmonicDecay = 0.65f;
        for (int n = 2; n <= 6; n++)
        {
            float amplitude = fundamental 
                * Mathf.Pow(harmonicDecay, n) 
                * (1f - 0.75f * Mathf.Clamp01((_currentFrequency * n)/1000f));

            harmonicSum += Mathf.Sin(phase * n) * amplitude;
        }

        float formant1 = 0.15f * 
            Mathf.Sin(phase * (1.25f + formantWidth * 0.03f)) *
            (Mathf.Sin(phase * 0.15f) * formantWidth + 1f) *
            Mathf.Pow(Mathf.Sin(phase * 0.35f), 2f);

        float formant2 = 0.05f * 
            Mathf.Sin(phase * (2.1f - formantCoupling * 0.1f )) * 
            Mathf.Pow(Mathf.Sin(phase * 0.55f), 3f);

        float formantSum = formant1 + (formant2 * (formantCoupling * 0.8f ));
        formantSum = Mathf.Clamp(formantSum, -1f, 1f);

        return harmonicSum * 0.92f + formantSum * 0.08f;
    }

    private float GenerateAirNoise(float deltaTime)
    {
        float modFrequency = breathPressure * 0.06f;
        float breathMod = Mathf.Sin(2f * Mathf.PI * modFrequency * _time) * 0.18f;

        _noiseSeed = (_noiseSeed * 16807 + 2147483647) & 0xFFFFFFFF;
        float randomFactor = (float)(_noiseSeed & 0xFFFFFFFF) / (float)0xFFFFFFFF;
        float spikeNoise = randomFactor < 0.003f * breathPressure ?
            Mathf.Sin(2f * Mathf.PI * 1000f * _time) * 0.03f : 0f;

        float totalNoise = (breathMod * 0.7f) + (spikeNoise * 0.3f);
        float dampingTime = _currentFrequency < 400f ? 0.03f : 0.015f;
        _previousSmoothNoise = Mathf.SmoothDamp(
            _previousSmoothNoise,
            totalNoise,
            ref _noiseValueDamp,
            dampingTime,
            float.MaxValue,
            deltaTime);

        return _previousSmoothNoise * (airNoiseIntensity * 1.2f );
    }

    private float WaveShaping(float sample, float deltaTime)
    {
        float pressure = Mathf.Clamp01(_gain * 5f);
        float damping = Mathf.Exp(-pressure * 0.6f * (_currentFrequency / 1000f));
        sample *= damping;

        // 改进动态限幅，防止爆音
        float compressionThreshold = 0.65f;  // 进一步降低阈值
        float compressionRatio = 6.0f;      // 增加压缩比
        
        if (Mathf.Abs(sample) > compressionThreshold) {
            float excess = Mathf.Abs(sample) - compressionThreshold;
            float compressed = compressionThreshold + (excess / compressionRatio);
            sample = Mathf.Sign(sample) * compressed;
        }

        // 使用更平滑的软限幅
        float softClip = sample / (1f + Mathf.Abs(sample) * 0.8f);
        
        // 频率变化时应用更强的软限幅
        float freqChangeAmount = Mathf.Abs(_frequencyVelocity) / Mathf.Max(0.1f, _currentFrequency);
        float softClipMix = Mathf.Lerp(0.1f, 0.5f, Mathf.Clamp01(freqChangeAmount * 8f));
        
        // 初始吹奏时也应用更强的软限幅
        if (_gain < 0.05f && _gainVelocity > 0.1f) {
            softClipMix = Mathf.Max(softClipMix, 0.4f);
        }
        
        sample = (softClip * softClipMix) + (sample * (1f - softClipMix));

        // 线程安全的输入检测（使用缓存变量）
        if (!_isSpacePressed)
        {
            float decayExponent = _currentFrequency < 400f ? 0.2f : 0.6f;
            return sample * Mathf.Pow(0.5f, decayExponent * (float)(AudioSettings.dspTime * 2));
        }
        return sample;
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

    // 添加当前频率的公共属性
    public float CurrentFrequency => _currentFrequency;
}