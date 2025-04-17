using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        UpdateKeyStates();
        HandleOctaveInput();

        float baseFrequency = GetFrequency();
        int animatorState = GetAnimatorState(baseFrequency);

        if (xunAnimator != null)
            xunAnimator.SetInteger("tone", animatorState);

        UpdateUI(baseFrequency);

        // 空格键触发声音检测移到主线程
        if (Input.GetKey(KeyCode.Space))
        {
            _isFrequencyFrozen = false;
            _targetFrequency = baseFrequency * Mathf.Pow(2f, ottava + key / 12f);
            _targetGain = volume;

            if (_time - (float)AudioSettings.dspTime < attackTime + 0.05f)
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
        _currentFrequency = Mathf.SmoothDamp(
            _currentFrequency,
            _targetFrequency,
            ref _frequencyVelocity,
            freqSmoothTime,
            float.MaxValue,
            deltaTime);

        double stepD = (2.0 * Mathf.PI) * _currentFrequency / sampleRate;
        float previousSample = _previousSample;

        for (int i = 0; i < data.Length; i += channels)
        {
            float phase = (float)(currentPhaseD % (2f * Mathf.PI));
            float baseWave = GenerateXunWave(phase);

            // 线程安全的噪声叠加（使用缓存变量）
            float noise = _isSpacePressed ? GenerateAirNoise(deltaTime) : 0f;
            float totalSignal = baseWave + noise;

            totalSignal = WaveShaping(totalSignal, deltaTime);

            float newSample = _gain * totalSignal;
            newSample = previousSample * 0.25f + newSample * 0.75f;

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
        _isSpacePressed = Input.GetKey(KeyCode.Space);
        _keyStates[KeyCode.Space] = _isSpacePressed;
    }

    private void HandleOctaveInput()
    {
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

        if (Input.GetKeyDown(KeyCode.Comma))
        {
            ottava = Mathf.Max(ottava - 1, -3);
        }
        else if (Input.GetKeyDown(KeyCode.Period))
        {
            ottava = Mathf.Min(ottava + 1, 2);
        }
    }

    private float GetFrequency()
    {
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

    private string GetNoteName(float baseFrequency)
    {
        return baseFrequency switch
        {
            196f => "低音5",
            207.65f => "低音5♯",
            220f => "低音6",
            233.08f => "低音6♯",
            246.94f => "低音7",
            261.63f => "中音1",
            277.18f => "中音1♯",
            293.66f => "中音2",
            311.13f => "中音2♯",
            329.63f => "中音3",
            349.23f => "中音4",
            369.99f => "中音4♯",
            392f => "中音5",
            415.30f => "中音5♯",
            440f => "中音6",
            _ => "---"
        };
    }

    private void UpdateUI(float baseFrequency)
    {
        text.text = GetNoteName(baseFrequency);
        OttaText.text = ottava >= 0
            ? $"升{(ottava * 8)}度" 
            : $"降{Mathf.Abs(ottava * 8)}度";
        UpdateKeyText();
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

        float softClip = sample / (1f + Mathf.Abs(sample) * 0.4f);
        sample = (softClip * 0.95f) + (sample * 0.05f);

        // 线程安全的输入检测（使用缓存变量）
        if (!_isSpacePressed)
        {
            float decayExponent = _currentFrequency < 400f ? 0.2f : 0.6f;
            return sample * Mathf.Pow(0.5f, decayExponent * (float)(AudioSettings.dspTime * 2));
        }
        return sample;
    }

    // 添加当前频率的公共属性
    public float CurrentFrequency => _currentFrequency;
}