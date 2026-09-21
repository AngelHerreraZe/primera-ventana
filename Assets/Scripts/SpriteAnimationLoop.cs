using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Componente Profesional de Animación en Bucle utilizando Ocho (8) Sprites para Unity.
/// Implementa los conceptos de animación por cuadros (Frame-by-Frame Animation):
/// - Secuencia de 8 Sprites en bucle continuo (ej. Llama de fuego oscilante, Ciclo de caminata, Gema 3D o Pulso de energía).
/// - Modos de Reproducción: Bucle Infinito (Loop), Ping-Pong (Ida y Vuelta), Una vez (Play Once), Inverso (Reverse).
/// - Control de Tasa de Cuadros (FPS): Velocidad ajustable de 1 a 60 FPS.
/// - Depuración y Scrubber: Navegación cuadro a cuadro (Step Forward/Backward) y salto directo a cualquier fotograma (0 a 7).
/// - Integración con UI Image y SpriteRenderer.
/// </summary>
public class SpriteAnimationLoop : MonoBehaviour
{
    public enum AnimationLoopMode
    {
        Loop,       // 0 -> 1 -> 2 -> ... -> 7 -> 0
        PingPong,   // 0 -> 1 -> ... -> 7 -> 6 -> ... -> 0
        Once,       // 0 -> 1 -> ... -> 7 -> Stop
        Reverse     // 7 -> 6 -> ... -> 0 -> 7
    }

    [Header("Configuración de Cuadros (Sprites)")]
    [Tooltip("Colección de sprites para la animación en bucle (8 sprites sugeridos por defecto).")]
    public Sprite[] animationFrames = new Sprite[8];

    [Tooltip("Nombre descriptivo de la secuencia actual.")]
    public string sequenceName = "Animación 8 Sprites";

    [Header("Control de Reproducción")]
    [Tooltip("Indica si la animación se reproduce automáticamente al iniciar.")]
    public bool playOnAwake = true;

    [Tooltip("Modo de repetición del bucle.")]
    public AnimationLoopMode loopMode = AnimationLoopMode.Loop;

    [Tooltip("Velocidad de reproducción en fotogramas por segundo (FPS).")]
    [Range(1f, 60f)]
    public float framesPerSecond = 8f;

    [Tooltip("Usa tiempo no escalado para que la animación continúe incluso si Time.timeScale es 0.")]
    public bool useUnscaledTime = false;

    [Header("Referencias Visuales")]
    public Image targetImage;
    public SpriteRenderer targetSpriteRenderer;

    [Header("Eventos y Notificaciones")]
    public Action<int, Sprite> OnFrameChanged;
    public Action<int> OnLoopCompleted;
    public Action<bool> OnPlayStateChanged;
    public Action<string> OnStatusMessage;

    // Estado Interno
    private int _currentFrameIndex = 0;
    private float _timer = 0f;
    private bool _isPlaying = true;
    private int _pingPongDirection = 1; // 1 = adelante, -1 = atrás
    private int _completedLoops = 0;

    public int CurrentFrameIndex => _currentFrameIndex;
    public int TotalFrames => animationFrames != null ? animationFrames.Length : 0;
    public bool IsPlaying => _isPlaying;
    public float FramesPerSecond => framesPerSecond;
    public AnimationLoopMode LoopMode => loopMode;
    public int CompletedLoops => _completedLoops;
    public Sprite CurrentSprite => (animationFrames != null && _currentFrameIndex >= 0 && _currentFrameIndex < animationFrames.Length) ? animationFrames[_currentFrameIndex] : null;

    void Awake()
    {
        if (targetImage == null) targetImage = GetComponent<Image>();
        if (targetSpriteRenderer == null) targetSpriteRenderer = GetComponent<SpriteRenderer>();

        _isPlaying = playOnAwake;
    }

    void Start()
    {
        ApplyCurrentFrame();
    }

    void Update()
    {
        if (!_isPlaying || TotalFrames == 0) return;

        float delta = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        if (delta <= 0f) return;

        _timer += delta;
        float frameDuration = 1f / Mathf.Max(framesPerSecond, 0.1f);

        if (_timer >= frameDuration)
        {
            _timer -= frameDuration;
            AdvanceNextFrame();
        }
    }

    private void AdvanceNextFrame()
    {
        if (TotalFrames == 0) return;

        switch (loopMode)
        {
            case AnimationLoopMode.Loop:
                _currentFrameIndex = (_currentFrameIndex + 1) % TotalFrames;
                if (_currentFrameIndex == 0)
                {
                    _completedLoops++;
                    OnLoopCompleted?.Invoke(_completedLoops);
                }
                break;

            case AnimationLoopMode.Reverse:
                _currentFrameIndex = (_currentFrameIndex - 1 + TotalFrames) % TotalFrames;
                if (_currentFrameIndex == TotalFrames - 1)
                {
                    _completedLoops++;
                    OnLoopCompleted?.Invoke(_completedLoops);
                }
                break;

            case AnimationLoopMode.PingPong:
                _currentFrameIndex += _pingPongDirection;
                if (_currentFrameIndex >= TotalFrames)
                {
                    _currentFrameIndex = TotalFrames - 2;
                    if (_currentFrameIndex < 0) _currentFrameIndex = 0;
                    _pingPongDirection = -1;
                    _completedLoops++;
                    OnLoopCompleted?.Invoke(_completedLoops);
                }
                else if (_currentFrameIndex < 0)
                {
                    _currentFrameIndex = 1;
                    if (_currentFrameIndex >= TotalFrames) _currentFrameIndex = 0;
                    _pingPongDirection = 1;
                    _completedLoops++;
                    OnLoopCompleted?.Invoke(_completedLoops);
                }
                break;

            case AnimationLoopMode.Once:
                if (_currentFrameIndex < TotalFrames - 1)
                {
                    _currentFrameIndex++;
                }
                else
                {
                    _isPlaying = false;
                    _completedLoops++;
                    OnPlayStateChanged?.Invoke(false);
                    OnLoopCompleted?.Invoke(_completedLoops);
                    OnStatusMessage?.Invoke($"🏁 Animación '{sequenceName}' completada (Play Once).");
                }
                break;
        }

        ApplyCurrentFrame();
    }

    public void ApplyCurrentFrame()
    {
        if (TotalFrames == 0 || _currentFrameIndex < 0 || _currentFrameIndex >= TotalFrames) return;

        Sprite activeSprite = animationFrames[_currentFrameIndex];

        if (targetImage != null && activeSprite != null)
        {
            targetImage.sprite = activeSprite;
        }

        if (targetSpriteRenderer != null && activeSprite != null)
        {
            targetSpriteRenderer.sprite = activeSprite;
        }

        OnFrameChanged?.Invoke(_currentFrameIndex, activeSprite);
    }

    // ==========================================
    // Métodos Públicos de Control de Reproducción
    // ==========================================

    /// <summary>
    /// Inicia o reanuda la animación de sprites en bucle.
    /// </summary>
    public void Play()
    {
        _isPlaying = true;
        OnPlayStateChanged?.Invoke(true);
        OnStatusMessage?.Invoke($"▶️ Animación '{sequenceName}' en reproducción ({framesPerSecond:F0} FPS).");
    }

    /// <summary>
    /// Pausa la animación en el cuadro actual.
    /// </summary>
    public void Pause()
    {
        _isPlaying = false;
        OnPlayStateChanged?.Invoke(false);
        OnStatusMessage?.Invoke($"⏸️ Animación '{sequenceName}' pausada en cuadro #{_currentFrameIndex + 1}/{TotalFrames}.");
    }

    /// <summary>
    /// Alterna entre reproducir y pausar la animación.
    /// </summary>
    public bool TogglePlayPause()
    {
        if (_isPlaying)
        {
            Pause();
            return false;
        }
        else
        {
            Play();
            return true;
        }
    }

    /// <summary>
    /// Detiene la animación y rebobina al primer cuadro (0).
    /// </summary>
    public void Stop()
    {
        _isPlaying = false;
        _currentFrameIndex = 0;
        _timer = 0f;
        _pingPongDirection = 1;
        ApplyCurrentFrame();
        OnPlayStateChanged?.Invoke(false);
        OnStatusMessage?.Invoke($"⏹️ Animación '{sequenceName}' detenida y reiniciada al cuadro #1.");
    }

    /// <summary>
    /// Salta directamente a un índice de cuadro específico (0 a TotalFrames-1).
    /// </summary>
    public void SetFrame(int frameIndex)
    {
        if (TotalFrames == 0) return;
        _currentFrameIndex = Mathf.Clamp(frameIndex, 0, TotalFrames - 1);
        _timer = 0f;
        ApplyCurrentFrame();
        OnStatusMessage?.Invoke($"🔍 Cuadro establecido manualmente en: #{_currentFrameIndex + 1}/{TotalFrames}");
    }

    /// <summary>
    /// Avanza un cuadro hacia adelante (Step Forward).
    /// </summary>
    public void StepForward()
    {
        if (TotalFrames == 0) return;
        _currentFrameIndex = (_currentFrameIndex + 1) % TotalFrames;
        _timer = 0f;
        ApplyCurrentFrame();
        OnStatusMessage?.Invoke($"⏩ Cuadro siguiente: #{_currentFrameIndex + 1}/{TotalFrames}");
    }

    /// <summary>
    /// Retrocede un cuadro hacia atrás (Step Backward).
    /// </summary>
    public void StepBackward()
    {
        if (TotalFrames == 0) return;
        _currentFrameIndex = (_currentFrameIndex - 1 + TotalFrames) % TotalFrames;
        _timer = 0f;
        ApplyCurrentFrame();
        OnStatusMessage?.Invoke($"⏪ Cuadro anterior: #{_currentFrameIndex + 1}/{TotalFrames}");
    }

    /// <summary>
    /// Modifica la velocidad de fotogramas por segundo (FPS).
    /// </summary>
    public void SetFramesPerSecond(float fps)
    {
        framesPerSecond = Mathf.Clamp(fps, 1f, 60f);
        OnStatusMessage?.Invoke($"⚡ Velocidad de animación: {framesPerSecond:F0} FPS");
    }

    /// <summary>
    /// Cambia el modo de bucle (Loop, PingPong, Once, Reverse).
    /// </summary>
    public void SetLoopMode(AnimationLoopMode mode)
    {
        loopMode = mode;
        OnStatusMessage?.Invoke($"🔄 Modo de bucle cambiado a: {mode}");
    }

    /// <summary>
    /// Asigna un nuevo conjunto de 8 sprites (o cualquier longitud) para la animación.
    /// </summary>
    public void SetSpriteFrames(Sprite[] newFrames, string newSequenceName = "Secuencia de Sprites")
    {
        if (newFrames == null || newFrames.Length == 0)
        {
            OnStatusMessage?.Invoke("⚠️ Error: El array de sprites proporcionado está vacío.");
            return;
        }

        animationFrames = newFrames;
        sequenceName = newSequenceName;
        _currentFrameIndex = 0;
        _timer = 0f;
        _pingPongDirection = 1;
        ApplyCurrentFrame();
        OnStatusMessage?.Invoke($"🎨 Nueva secuencia cargada: '{sequenceName}' con {newFrames.Length} sprites.");
    }
}
