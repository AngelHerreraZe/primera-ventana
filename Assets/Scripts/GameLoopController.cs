using System;
using UnityEngine;

/// <summary>
/// Controlador Profesional del Bucle de Videojuego (Game Loop Controller) para Unity.
/// Administra el ciclo de vida de la ejecución interactiva:
/// - Fases del Bucle: Entrada (Input), Lógica (Update), Animación (Animation), Física (FixedUpdate), Render (LateUpdate).
/// - Estados del Bucle: En Ejecución (Running), Pausado (Paused), Paso a Paso (Stepping), Detenido (Stopped).
/// - Control de Tiempo: Escala de tiempo (Time.timeScale), limitador de cuadros por segundo (Application.targetFrameRate).
/// - Telemetría en Tiempo Real: FPS actual, tiempo delta (ms), conteo total de cuadros, tiempo acumulado de juego vs. tiempo real.
/// </summary>
public class GameLoopController : MonoBehaviour
{
    public enum GameLoopState
    {
        Running,
        Paused,
        Stepping,
        Stopped
    }

    [Header("Configuración del Bucle")]
    [Tooltip("Estado actual del bucle de videojuego.")]
    [SerializeField] private GameLoopState currentState = GameLoopState.Running;

    [Tooltip("Escala de velocidad del tiempo en el juego (1.0 = normal).")]
    [Range(0.05f, 5.0f)]
    [SerializeField] private float timeScaleMultiplier = 1.0f;

    [Tooltip("Límite de cuadros por segundo (-1 = sin límite).")]
    [SerializeField] private int targetFramerate = 60;

    [Header("Telemetría en Vivo (Métricas)")]
    [SerializeField] private float currentFps = 60f;
    [SerializeField] private float deltaTimeMs = 16.6f;
    [SerializeField] private ulong totalFramesProcessed = 0;
    [SerializeField] private float elapsedGameTime = 0f;
    [SerializeField] private float elapsedRealTime = 0f;

    [Header("Eventos y Notificaciones")]
    public Action<GameLoopState> OnStateChanged;
    public Action<float, ulong> OnGameLoopTick;         // deltaTime, frameCount
    public Action<float> OnGameLoopFixedTick;          // fixedDeltaTime
    public Action<GameLoopTelemetry> OnTelemetryUpdate;
    public Action<string> OnStatusLog;

    // Control de cálculo de FPS suavizado
    private float _fpsAccumulator = 0f;
    private int _fpsFrameCount = 0;
    private float _fpsTimeLeft = 0.5f;
    private bool _stepTriggered = false;
    private float _savedTimeScale = 1.0f;

    public GameLoopState CurrentState => currentState;
    public float CurrentTimeScale => timeScaleMultiplier;
    public int TargetFramerate => targetFramerate;
    public float CurrentFps => currentFps;
    public float DeltaTimeMs => deltaTimeMs;
    public ulong TotalFrames => totalFramesProcessed;
    public float ElapsedGameTime => elapsedGameTime;
    public float ElapsedRealTime => elapsedRealTime;
    public bool IsRunning => currentState == GameLoopState.Running;
    public bool IsPaused => currentState == GameLoopState.Paused;

    [System.Serializable]
    public struct GameLoopTelemetry
    {
        public GameLoopState State;
        public float Fps;
        public float DeltaTimeMs;
        public ulong TotalFrames;
        public float GameTime;
        public float RealTime;
        public float TimeScale;
        public int TargetFps;
    }

    void Awake()
    {
        ApplyTargetFramerate(targetFramerate);
        SetTimeScale(timeScaleMultiplier);
    }

    void Update()
    {
        float realDelta = Time.unscaledDeltaTime;
        float gameDelta = Time.deltaTime;

        elapsedRealTime += realDelta;

        // Actualizar telemetría de FPS
        UpdateFpsMetrics(realDelta);

        // Gestión de estado del bucle
        if (currentState == GameLoopState.Stopped)
        {
            return;
        }

        if (currentState == GameLoopState.Paused && !_stepTriggered)
        {
            return;
        }

        // Si se ejecutó un paso individual, pausar nuevamente
        if (_stepTriggered)
        {
            _stepTriggered = false;
            currentState = GameLoopState.Paused;
            Time.timeScale = 0f;
            OnStateChanged?.Invoke(currentState);
            OnStatusLog?.Invoke($"⏯️ Paso a paso ejecutado. Cuadro #{totalFramesProcessed} procesado.");
        }

        totalFramesProcessed++;
        elapsedGameTime += gameDelta;

        // Disparar ciclo del bucle
        OnGameLoopTick?.Invoke(gameDelta, totalFramesProcessed);
        BroadcastTelemetry();
    }

    void FixedUpdate()
    {
        if (currentState == GameLoopState.Running)
        {
            OnGameLoopFixedTick?.Invoke(Time.fixedDeltaTime);
        }
    }

    private void UpdateFpsMetrics(float realDelta)
    {
        if (realDelta > 0.0001f)
        {
            deltaTimeMs = realDelta * 1000f;
        }

        _fpsAccumulator += (realDelta > 0.0001f) ? (1f / realDelta) : 0f;
        _fpsFrameCount++;
        _fpsTimeLeft -= realDelta;

        if (_fpsTimeLeft <= 0.0f)
        {
            currentFps = (_fpsFrameCount > 0) ? (_fpsAccumulator / _fpsFrameCount) : 60f;
            _fpsAccumulator = 0f;
            _fpsFrameCount = 0;
            _fpsTimeLeft = 0.5f;
            BroadcastTelemetry();
        }
    }

    private void BroadcastTelemetry()
    {
        var tele = new GameLoopTelemetry
        {
            State = currentState,
            Fps = currentFps,
            DeltaTimeMs = deltaTimeMs,
            TotalFrames = totalFramesProcessed,
            GameTime = elapsedGameTime,
            RealTime = elapsedRealTime,
            TimeScale = timeScaleMultiplier,
            TargetFps = targetFramerate
        };

        OnTelemetryUpdate?.Invoke(tele);
    }

    // ==========================================
    // Métodos Públicos de Control del Bucle
    // ==========================================

    /// <summary>
    /// Reanuda o inicia el bucle de videojuego a velocidad normal.
    /// </summary>
    public void Play()
    {
        currentState = GameLoopState.Running;
        Time.timeScale = timeScaleMultiplier;
        OnStateChanged?.Invoke(currentState);
        OnStatusLog?.Invoke("▶️ Bucle de videojuego en EJECUCIÓN (Running).");
        BroadcastTelemetry();
    }

    /// <summary>
    /// Pausa la ejecución del bucle de juego congelando Time.timeScale.
    /// </summary>
    public void Pause()
    {
        if (currentState == GameLoopState.Paused) return;

        _savedTimeScale = timeScaleMultiplier;
        currentState = GameLoopState.Paused;
        Time.timeScale = 0f;
        OnStateChanged?.Invoke(currentState);
        OnStatusLog?.Invoke("⏸️ Bucle de videojuego PAUSADO.");
        BroadcastTelemetry();
    }

    /// <summary>
    /// Alterna entre Reproducir y Pausar.
    /// </summary>
    public bool TogglePlayPause()
    {
        if (currentState == GameLoopState.Running)
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
    /// Ejecuta exactamente un cuadro (Single Frame Step) y luego se pausa.
    /// </summary>
    public void StepFrame()
    {
        currentState = GameLoopState.Stepping;
        _stepTriggered = true;
        Time.timeScale = timeScaleMultiplier;
        OnStateChanged?.Invoke(currentState);
        OnStatusLog?.Invoke("⏩ Avanzando 1 cuadro (Single Step)...");
    }

    /// <summary>
    /// Detiene y reinicia el bucle de juego y sus cronómetros.
    /// </summary>
    public void StopAndReset()
    {
        currentState = GameLoopState.Stopped;
        Time.timeScale = 0f;
        totalFramesProcessed = 0;
        elapsedGameTime = 0f;
        elapsedRealTime = 0f;
        OnStateChanged?.Invoke(currentState);
        OnStatusLog?.Invoke("⏹️ Bucle de videojuego DETENIDO y cronómetros reiniciados a 0.");
        BroadcastTelemetry();
    }

    /// <summary>
    /// Modifica el multiplicador de velocidad del tiempo (Time.timeScale).
    /// </summary>
    public void SetTimeScale(float multiplier)
    {
        timeScaleMultiplier = Mathf.Clamp(multiplier, 0.05f, 5.0f);
        if (currentState == GameLoopState.Running)
        {
            Time.timeScale = timeScaleMultiplier;
        }
        OnStatusLog?.Invoke($"⚡ Escala de tiempo del bucle establecida en: {timeScaleMultiplier:F2}x");
        BroadcastTelemetry();
    }

    /// <summary>
    /// Establece el límite objetivo de fotogramas por segundo (targetFrameRate).
    /// </summary>
    public void ApplyTargetFramerate(int fps)
    {
        targetFramerate = fps;
        Application.targetFrameRate = fps;
        string fpsLabel = fps > 0 ? $"{fps} FPS" : "Sin Límite (Máximo)";
        OnStatusLog?.Invoke($"🎯 Límite de fotogramas del bucle: {fpsLabel}");
        BroadcastTelemetry();
    }

    void OnDestroy()
    {
        // Restaurar timeScale normal al destruir el componente
        Time.timeScale = 1.0f;
    }
}
