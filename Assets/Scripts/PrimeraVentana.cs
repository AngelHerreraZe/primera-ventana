using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

/// <summary>
/// Aplicación Interactiva Responsiva y Modular para Unity.
/// Cumple con todos los criterios de la actividad:
/// 1. Arrastre de Sprites funcional mediante eventos del EventSystem y botones de control.
/// 2. Control interactivo de fuentes tipográficas (familias, tamaños, estilos, colores y efectos).
/// 3. Fijación de scripts a GameObjects en tiempo de ejecución (AddComponent) y navegación funcional con historial, migas de pan y pestañas.
/// 4. Bucle de Videojuego (Game Loop) configurable con control de tiempo, estados y telemetría en tiempo real.
/// 5. Animación cuadro a cuadro en bucle continuo utilizando ocho (8) sprites (Fuego, Caminata, Gema 3D y Orbe de Energía).
/// </summary>
public class PrimeraVentana : MonoBehaviour
{
    // ==========================================
    // Paleta de Colores Moderna (Dark Glass Theme)
    // ==========================================
    static readonly Color BgDark          = new Color(0.07f, 0.09f, 0.13f, 0.96f);
    static readonly Color CardBg          = new Color(0.12f, 0.15f, 0.22f, 0.94f);
    static readonly Color CardHeaderBg    = new Color(0.16f, 0.20f, 0.29f, 1f);
    static readonly Color SidebarBg       = new Color(0.10f, 0.12f, 0.18f, 0.96f);
    static readonly Color SandboxBg       = new Color(0.06f, 0.08f, 0.12f, 1f);
    static readonly Color AccentPrimary   = new Color(0.24f, 0.51f, 0.98f, 1f); // Azul vibrante
    static readonly Color AccentSuccess   = new Color(0.13f, 0.77f, 0.53f, 1f); // Verde esmeralda
    static readonly Color AccentWarning   = new Color(0.96f, 0.62f, 0.15f, 1f); // Ámbar
    static readonly Color AccentDanger    = new Color(0.94f, 0.27f, 0.38f, 1f); // Rosa / Coral
    static readonly Color AccentPurple    = new Color(0.63f, 0.38f, 0.96f, 1f); // Violeta neón
    static readonly Color AccentCyan      = new Color(0.12f, 0.75f, 0.88f, 1f); // Cian brillante
    static readonly Color AccentFlame     = new Color(1.00f, 0.45f, 0.10f, 1f); // Fuego naranja
    static readonly Color TextWhite       = new Color(0.98f, 0.99f, 1.00f, 1f);
    static readonly Color TextMuted       = new Color(0.60f, 0.65f, 0.75f, 1f);
    static readonly Color TextDimmed      = new Color(0.40f, 0.44f, 0.54f, 1f);
    static readonly Color BorderColor     = new Color(0.22f, 0.28f, 0.38f, 0.7f);
    static readonly Color ButtonNormal    = new Color(0.18f, 0.23f, 0.33f, 1f);
    static readonly Color ButtonHover     = new Color(0.26f, 0.33f, 0.46f, 1f);

    // ==========================================
    // Sprites Procedurales Básicos
    // ==========================================
    Sprite _roundedSprite;
    Sprite _circleSprite;
    Sprite _diamondSprite;
    Sprite _starSprite;
    Sprite _shieldSprite;

    // ==========================================
    // Sets de Animación de Ocho (8) Sprites
    // ==========================================
    Sprite[] _fireSprites   = new Sprite[8]; // Fuego / Hoguera animada (8 cuadros)
    Sprite[] _walkSprites   = new Sprite[8]; // Ciclo de caminata (8 cuadros)
    Sprite[] _gemSprites    = new Sprite[8]; // Gema 3D giratoria (8 cuadros)
    Sprite[] _energySprites = new Sprite[8]; // Orbe de energía / Pulso mágico (8 cuadros)

    // ==========================================
    // Componentes del Sistema Principal
    // ==========================================
    Canvas _canvas;
    CanvasScaler _canvasScaler;
    NavigationController _navigationController;
    ScriptBinder _scriptBinder;
    GameLoopController _gameLoopController;

    // UI Global
    Text _statusText;
    Text _breadcrumbsText;
    Text _resolutionInfoText;
    Text _logConsoleContent;
    readonly List<string> _logEntries = new List<string>();

    // Módulo: Arrastre de Sprites
    RectTransform _spriteSandboxArea;
    DraggableSprite _primaryDraggableSprite;
    Text _spriteCoordsText;
    Text _spriteStateBadge;
    readonly List<DraggableSprite> _spawnedSprites = new List<DraggableSprite>();

    // Módulo: Control de Fuentes
    FontController _fontController;
    Text _samplePreviewText;
    Text _fontMetricsText;

    // Módulo: Fijar Scripts a Objetos
    GameObject _testTargetGameObject;
    Text _attachedScriptsText;

    // Módulo: Bucle de Videojuego y Animación 8 Sprites
    SpriteAnimationLoop _spriteAnimationLoop;
    DraggableSprite _animDraggableSprite;
    Image _animatedSpriteImg;
    Text _loopStateBadge;
    Text _loopTelemetryFps;
    Text _loopTelemetryDelta;
    Text _loopTelemetryFrames;
    Text _loopTelemetryTime;
    Text _loopTimeScaleText;
    Text _animActiveFrameBadge;
    Text _animFpsText;
    Text _animSequenceTitle;
    Text _animLoopCountText;
    readonly Image[] _storyboardThumbnails = new Image[8];
    readonly Image[] _storyboardBorders = new Image[8];
    readonly Text[] _storyboardLabels = new Text[8];

    // Presets de dispositivos
    struct DevicePreset
    {
        public string Name;
        public Vector2 Resolution;
        public string AspectRatio;
        public string Icon;
    }

    readonly DevicePreset[] _devicePresets = new DevicePreset[]
    {
        new DevicePreset { Name = "Escritorio / Laptop", Resolution = new Vector2(1920, 1080), AspectRatio = "16:9", Icon = "💻" },
        new DevicePreset { Name = "Móvil Vertical", Resolution = new Vector2(1080, 1920), AspectRatio = "9:16", Icon = "📱" },
        new DevicePreset { Name = "Móvil Horizontal", Resolution = new Vector2(1920, 1080), AspectRatio = "16:9", Icon = "📲" },
        new DevicePreset { Name = "Tablet / iPad", Resolution = new Vector2(2048, 1536), AspectRatio = "4:3", Icon = "📟" },
        new DevicePreset { Name = "Monitor UltraWide", Resolution = new Vector2(2560, 1080), AspectRatio = "21:9", Icon = "🖥️" },
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoBootstrap()
    {
        if (UnityEngine.Object.FindAnyObjectByType<PrimeraVentana>() != null) return;
        var go = new GameObject("AplicacionInteractivaUnity");
        go.AddComponent<PrimeraVentana>();
    }

    void Awake()
    {
        GenerateProceduralSprites();
        Generate8SpriteSets();
        EnsureEventSystem();
        BuildUI();
    }

    void Start()
    {
        AddLog("🚀 Sistema interactivo de Unity inicializado correctamente.");
        AddLog("✓ Módulo 'Arrastrar Sprite' configurado con soporte para eventos y botones.");
        AddLog("✓ Módulo 'Controlar Fuentes' vinculado con FontController.");
        AddLog("✓ Módulo 'Fijar Scripts y Navegación' activo con NavigationController y ScriptBinder.");
        AddLog("✓ Módulo 'Bucle de Videojuego y Animación con 8 Sprites' activo con GameLoopController y SpriteAnimationLoop.");

        // Inicializar navegación en la página de inicio
        if (_navigationController != null)
        {
            _navigationController.NavigateTo(0);
        }
    }

    void Update()
    {
        UpdateLiveMetrics();
    }

    // ==========================================
    // Construcción General de la Interfaz UI
    // ==========================================
    void BuildUI()
    {
        // 1. Canvas y Canvas Scaler adaptativo
        var canvasGo = new GameObject("InteractiveCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);
        _canvas = canvasGo.GetComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 10;

        _canvasScaler = canvasGo.GetComponent<CanvasScaler>();
        _canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        _canvasScaler.referenceResolution = new Vector2(1920, 1080);
        _canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        _canvasScaler.matchWidthOrHeight = 0.5f;

        // 2. Controladores principales
        _navigationController = canvasGo.AddComponent<NavigationController>();
        _scriptBinder = canvasGo.AddComponent<ScriptBinder>();
        _gameLoopController = canvasGo.AddComponent<GameLoopController>();

        _navigationController.OnStatusLog = msg => AddLog(msg);
        _scriptBinder.OnStatusLog = msg => AddLog(msg);
        _scriptBinder.OnComponentsChanged = RefreshAttachedScriptsDisplay;
        _gameLoopController.OnStatusLog = msg => AddLog(msg);

        // 3. Fondo general
        var background = Panel(canvasGo.transform, BgDark);
        background.name = "BackgroundLayer";
        Stretch(background.rectTransform, Vector2.zero, Vector2.one);

        // 4. Contenedor Maestro Vertical
        var masterContainer = Panel(canvasGo.transform, Color.clear);
        masterContainer.name = "MasterContainer";
        Stretch(masterContainer.rectTransform, new Vector2(0.015f, 0.02f), new Vector2(0.985f, 0.98f));

        var masterVGroup = masterContainer.gameObject.AddComponent<VerticalLayoutGroup>();
        masterVGroup.padding = new RectOffset(0, 0, 0, 0);
        masterVGroup.spacing = 10;
        masterVGroup.childControlWidth = true;
        masterVGroup.childControlHeight = true;
        masterVGroup.childForceExpandWidth = true;
        masterVGroup.childForceExpandHeight = false;

        // 5. Header / Barra Superior con Logo, Breadcrumbs y Pestañas (6 Pestañas)
        BuildHeader(masterContainer.transform);

        // 6. Área de Contenido Central (Páginas)
        var contentArea = Panel(masterContainer.transform, Color.clear);
        contentArea.name = "ContentArea";
        Flex(contentArea.gameObject, flexH: 1);

        // Construcción de las 6 Páginas Principales
        var page0 = BuildPageInicio(contentArea.transform);
        var page1 = BuildPageArrastrarSprite(contentArea.transform);
        var page2 = BuildPageControlFuentes(contentArea.transform);
        var page3 = BuildPageFijarScripts(contentArea.transform);
        var page4 = BuildPageNavegacionYDispositivos(contentArea.transform);
        var page5 = BuildPageBucleYAnimacion(contentArea.transform);

        // Registrar 6 páginas en NavigationController
        _navigationController.RegisterPage("inicio", "Inicio", "🏠", page0, _navTabButtons[0], _navTabIndicators[0]);
        _navigationController.RegisterPage("sprites", "Arrastrar Sprite", "🎯", page1, _navTabButtons[1], _navTabIndicators[1]);
        _navigationController.RegisterPage("fuentes", "Control de Fuentes", "🔤", page2, _navTabButtons[2], _navTabIndicators[2]);
        _navigationController.RegisterPage("scripts", "Fijar Scripts", "🧩", page3, _navTabButtons[3], _navTabIndicators[3]);
        _navigationController.RegisterPage("navegacion", "Navegación & Pantalla", "🧭", page4, _navTabButtons[4], _navTabIndicators[4]);
        _navigationController.RegisterPage("bucle", "Bucle & 8 Sprites", "🎬", page5, _navTabButtons[5], _navTabIndicators[5]);

        // 7. Footer / Barra Inferior de Navegación y Estado (6 Dots)
        BuildFooter(masterContainer.transform);
    }

    // ==========================================
    // 1. Header y Barra de Navegación Superior
    // ==========================================
    Button[] _navTabButtons = new Button[6];
    Image[] _navTabIndicators = new Image[6];

    void BuildHeader(Transform parent)
    {
        var headerPanel = Panel(parent, CardHeaderBg);
        headerPanel.name = "HeaderNavBar";
        Size(headerPanel.gameObject, prefH: 72);

        var hLayout = headerPanel.gameObject.AddComponent<HorizontalLayoutGroup>();
        hLayout.padding = new RectOffset(16, 16, 8, 8);
        hLayout.spacing = 10;
        hLayout.childControlWidth = false;
        hLayout.childControlHeight = true;
        hLayout.childForceExpandWidth = false;
        hLayout.childForceExpandHeight = true;
        hLayout.childAlignment = TextAnchor.MiddleLeft;

        // Logo
        var logoBox = Panel(headerPanel.transform, AccentFlame);
        logoBox.name = "AppLogo";
        Size(logoBox.gameObject, prefW: 46, prefH: 46);
        var logoText = CreateText(logoBox.transform, "🔥", 22, TextAnchor.MiddleCenter, TextWhite, FontStyle.Bold);
        Stretch(logoText.rectTransform, Vector2.zero, Vector2.one);

        // Título y Migas de Pan (Breadcrumbs)
        var titleCol = Column(headerPanel.transform, 2);
        Size(titleCol.gameObject, prefW: 240, prefH: 54);
        titleCol.childAlignment = TextAnchor.MiddleLeft;

        var titleText = CreateText(titleCol.transform, "UNITY INTERACTIVO", 15, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(titleText.gameObject, prefH: 22);

        _breadcrumbsText = CreateText(titleCol.transform, "Inicio", 11, TextAnchor.MiddleLeft, AccentCyan);
        Size(_breadcrumbsText.gameObject, prefH: 18);
        _navigationController.txtBreadcrumbs = _breadcrumbsText;

        // Botones de Historial Rápido (Back / Forward)
        var btnBack = CreateIconButton(headerPanel.transform, "◀", ButtonNormal, 40, 42, () => _navigationController.GoBack());
        btnBack.name = "BtnQuickBack";
        _navigationController.btnBack = btnBack;

        var btnForward = CreateIconButton(headerPanel.transform, "▶", ButtonNormal, 40, 42, () => _navigationController.GoForward());
        btnForward.name = "BtnQuickForward";
        _navigationController.btnForward = btnForward;

        CreateSpacer(headerPanel.transform, flexW: 1);

        // Pestañas Principales (6 Pestañas)
        string[] tabLabels = { "🏠 Inicio", "🎯 Sprites", "🔤 Fuentes", "🧩 Scripts", "🧭 Navegación", "🎬 Bucle & 8 Sprites" };

        for (int i = 0; i < tabLabels.Length; i++)
        {
            var tabBtnGo = Panel(headerPanel.transform, ButtonNormal);
            tabBtnGo.name = "TabBtn_" + i;
            Size(tabBtnGo.gameObject, prefW: 145, prefH: 46);

            var btn = tabBtnGo.gameObject.AddComponent<Button>();
            btn.targetGraphic = tabBtnGo;

            var tabVGroup = tabBtnGo.gameObject.AddComponent<VerticalLayoutGroup>();
            tabVGroup.padding = new RectOffset(6, 6, 4, 3);
            tabVGroup.spacing = 2;
            tabVGroup.childControlWidth = true;
            tabVGroup.childControlHeight = true;
            tabVGroup.childForceExpandWidth = true;
            tabVGroup.childForceExpandHeight = true;

            var label = CreateText(tabBtnGo.transform, tabLabels[i], 11, TextAnchor.MiddleCenter, TextWhite, FontStyle.Bold);
            Flex(label.gameObject, flexH: 1);

            var indicator = Panel(tabBtnGo.transform, Color.clear);
            indicator.name = "TabIndicator";
            Size(indicator.gameObject, prefH: 3);

            _navTabButtons[i] = btn;
            _navTabIndicators[i] = indicator;
        }
    }

    // ==========================================
    // PÁGINA 1: Inicio / Dashboard
    // ==========================================
    GameObject BuildPageInicio(Transform parent)
    {
        var page = Panel(parent, Color.clear);
        page.name = "Page_Inicio";
        Stretch(page.rectTransform, Vector2.zero, Vector2.one);

        var hLayout = page.gameObject.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 14;
        hLayout.childControlWidth = true;
        hLayout.childControlHeight = true;
        hLayout.childForceExpandWidth = true;
        hLayout.childForceExpandHeight = true;

        // Columna Izquierda: Tarjeta Hero y Accesos a Módulos
        var leftCol = Column(page.transform, 10);
        Flex(leftCol.gameObject, flexW: 2.2f);

        // Hero Card
        var heroCard = Panel(leftCol.transform, CardBg);
        Size(heroCard.gameObject, prefH: 140);
        var heroV = heroCard.gameObject.AddComponent<VerticalLayoutGroup>();
        heroV.padding = new RectOffset(20, 20, 14, 14);
        heroV.spacing = 4;
        heroV.childControlWidth = true;
        heroV.childControlHeight = true;
        heroV.childForceExpandWidth = true;
        heroV.childForceExpandHeight = false;

        var badge = CreateBadge(heroCard.transform, "BUCLE DE VIDEOJUEGO & ANIMACIÓN CON 8 SPRITES (LOOP ANIMATION)", AccentFlame);
        Size(badge.gameObject, prefH: 22);

        var heroTitle = CreateText(heroCard.transform, "Controlador Integral: Sprites, Fuentes, Scripts, Bucle de Juego y 8 Sprites", 18, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(heroTitle.gameObject, prefH: 26);

        var heroDesc = CreateText(heroCard.transform,
            "Aplicación interactiva con bucle de videojuego configurable en tiempo real, ciclo de animación continua por cuadros con 8 sprites (Fuego, Caminata, Gema 3D y Orbe de Energía), arrastre táctil y tipografía dinámica.",
            12, TextAnchor.MiddleLeft, TextMuted);
        Size(heroDesc.gameObject, prefH: 36);

        // Grid 2x2 de Módulos Principales
        var rowModules1 = Row(leftCol.transform, 10);
        Flex(rowModules1.gameObject, flexH: 1);

        CreateModuleCard(rowModules1.transform, "🎬 Bucle & 8 Sprites",
            "Bucle de juego (Play, Pause, Step, TimeScale, FPS) y animación con 8 sprites en bucle continuo (Fuego, Caminata, Gema, Orbe).",
            AccentFlame, "Abrir Módulo", () => _navigationController.NavigateTo(5));

        CreateModuleCard(rowModules1.transform, "🎯 Arrastrar Sprites",
            "Mueve sprites con ratón o táctil. Genera nuevos sprites, resetea origen, cambia apariencias y bloquea arrastre.",
            AccentPrimary, "Abrir Módulo", () => _navigationController.NavigateTo(1));

        var rowModules2 = Row(leftCol.transform, 10);
        Flex(rowModules2.gameObject, flexH: 1);

        CreateModuleCard(rowModules2.transform, "🔤 Control de Fuentes",
            "Ajusta tamaño (+/-), estilo (Bold/Italic), alineación, familia tipográfica, paleta de colores y efectos visuales.",
            AccentSuccess, "Abrir Módulo", () => _navigationController.NavigateTo(2));

        CreateModuleCard(rowModules2.transform, "🧩 Fijar Scripts & Nav",
            "Fija y desacopla componentes (AddComponent) en tiempo real con inspector en vivo y navegación con historial.",
            AccentPurple, "Abrir Módulo", () => _navigationController.NavigateTo(3));

        // Columna Derecha: Métricas y Consola Rápida
        var rightCol = Column(page.transform, 12);
        Flex(rightCol.gameObject, flexW: 1.1f);

        var statusCard = Panel(rightCol.transform, SidebarBg);
        Flex(statusCard.gameObject, flexH: 1);
        var statusV = statusCard.gameObject.AddComponent<VerticalLayoutGroup>();
        statusV.padding = new RectOffset(16, 16, 16, 16);
        statusV.spacing = 8;
        statusV.childControlWidth = true;
        statusV.childControlHeight = true;
        statusV.childForceExpandWidth = true;
        statusV.childForceExpandHeight = false;

        var statusHead = CreateText(statusCard.transform, "📊 Estado del Sistema", 15, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(statusHead.gameObject, prefH: 24);

        _resolutionInfoText = CreateText(statusCard.transform, "Cargando métricas...", 11, TextAnchor.UpperLeft, TextMuted);
        Flex(_resolutionInfoText.gameObject, flexH: 1);

        var div = Panel(statusCard.transform, BorderColor);
        Size(div.gameObject, prefH: 1);

        var quickHead = CreateText(statusCard.transform, "⚡ Atajos Rápidos", 13, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(quickHead.gameObject, prefH: 20);

        var b0 = CreateButton(statusCard.transform, "🎬 Bucle y 8 Sprites Animados", AccentFlame, () => _navigationController.NavigateTo(5));
        Size(b0.gameObject, prefH: 36);

        var b1 = CreateButton(statusCard.transform, "🎯 Probar Arrastre de Sprite", AccentPrimary, () => _navigationController.NavigateTo(1));
        Size(b1.gameObject, prefH: 36);

        var b2 = CreateButton(statusCard.transform, "🔤 Editar Tipografía", AccentSuccess, () => _navigationController.NavigateTo(2));
        Size(b2.gameObject, prefH: 36);

        var b3 = CreateButton(statusCard.transform, "🧩 Fijar Scripts a Objetos", AccentPurple, () => _navigationController.NavigateTo(3));
        Size(b3.gameObject, prefH: 36);

        return page.gameObject;
    }

    // ==========================================
    // PÁGINA 2: Arrastrar un Sprite (Sandbox + Botonera)
    // ==========================================
    GameObject BuildPageArrastrarSprite(Transform parent)
    {
        var page = Panel(parent, Color.clear);
        page.name = "Page_ArrastrarSprite";
        Stretch(page.rectTransform, Vector2.zero, Vector2.one);

        var hLayout = page.gameObject.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 14;
        hLayout.childControlWidth = true;
        hLayout.childControlHeight = true;
        hLayout.childForceExpandWidth = true;
        hLayout.childForceExpandHeight = true;

        // Columna Izquierda: Botones de Configuración y Control
        var leftCol = Column(page.transform, 10);
        Flex(leftCol.gameObject, flexW: 1.15f);

        var headerCard = Panel(leftCol.transform, CardBg);
        Size(headerCard.gameObject, prefH: 75);
        var headV = headerCard.gameObject.AddComponent<VerticalLayoutGroup>();
        headV.padding = new RectOffset(16, 16, 10, 10);
        headV.spacing = 4;
        headV.childControlWidth = true;
        headV.childControlHeight = true;
        headV.childForceExpandWidth = true;
        headV.childForceExpandHeight = false;

        var headTitle = CreateText(headerCard.transform, "🎯 Control de Arrastre de Sprites", 16, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(headTitle.gameObject, prefH: 22);
        var headDesc = CreateText(headerCard.transform, "Usa los botones para controlar el comportamiento y generar nuevos sprites arrastrables.", 11, TextAnchor.MiddleLeft, TextMuted);
        Size(headDesc.gameObject, prefH: 26);

        // Panel de Botones de Acción
        var controlsPanel = Panel(leftCol.transform, SidebarBg);
        Flex(controlsPanel.gameObject, flexH: 1);
        var ctlV = controlsPanel.gameObject.AddComponent<VerticalLayoutGroup>();
        ctlV.padding = new RectOffset(14, 14, 12, 12);
        ctlV.spacing = 8;
        ctlV.childControlWidth = true;
        ctlV.childControlHeight = true;
        ctlV.childForceExpandWidth = true;
        ctlV.childForceExpandHeight = false;

        var sec1Title = CreateText(controlsPanel.transform, "🕹️ Acciones de Posición", 13, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(sec1Title.gameObject, prefH: 18);

        // Fila: Resetear y Centrar
        var rowPos = Row(controlsPanel.transform, 8);
        Size(rowPos.gameObject, prefH: 36);

        var btnReset = CreateButton(rowPos.transform, "🔄 Reset Origen", ButtonNormal, () =>
        {
            if (_primaryDraggableSprite != null) _primaryDraggableSprite.ResetPosition();
        });
        Flex(btnReset.gameObject, flexW: 1);

        var btnCenter = CreateButton(rowPos.transform, "🎯 Centrar", AccentPrimary, () =>
        {
            if (_primaryDraggableSprite != null) _primaryDraggableSprite.CenterInContainer();
        });
        Flex(btnCenter.gameObject, flexW: 1);

        var btnRandom = CreateButton(rowPos.transform, "🎲 Aleatorio", ButtonNormal, () =>
        {
            if (_primaryDraggableSprite != null) _primaryDraggableSprite.RandomizePosition();
        });
        Flex(btnRandom.gameObject, flexW: 1);

        // Botón Bloquear / Desbloquear Arrastre
        var btnToggleLock = CreateButton(controlsPanel.transform, "🔓 Bloquear / Desbloquear Arrastre", AccentWarning, () =>
        {
            if (_primaryDraggableSprite != null)
            {
                bool isLocked = _primaryDraggableSprite.ToggleLock();
                if (_spriteStateBadge != null)
                {
                    _spriteStateBadge.text = isLocked ? "🔒 BLOQUEADO" : "🔓 ARRASTRE ACTIVO";
                    _spriteStateBadge.color = isLocked ? AccentDanger : AccentSuccess;
                }
            }
        });
        Size(btnToggleLock.gameObject, prefH: 36);

        // Separador
        var div1 = Panel(controlsPanel.transform, BorderColor);
        Size(div1.gameObject, prefH: 1);

        var sec2Title = CreateText(controlsPanel.transform, "🔍 Escala y Rotación", 13, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(sec2Title.gameObject, prefH: 18);

        var rowTransform = Row(controlsPanel.transform, 8);
        Size(rowTransform.gameObject, prefH: 36);

        var btnScaleMinus = CreateButton(rowTransform.transform, "➖ Reducir", ButtonNormal, () =>
        {
            if (_primaryDraggableSprite != null) _primaryDraggableSprite.ScaleDelta(-0.2f);
        });
        Flex(btnScaleMinus.gameObject, flexW: 1);

        var btnScalePlus = CreateButton(rowTransform.transform, "➕ Aumentar", ButtonNormal, () =>
        {
            if (_primaryDraggableSprite != null) _primaryDraggableSprite.ScaleDelta(0.2f);
        });
        Flex(btnScalePlus.gameObject, flexW: 1);

        var btnRotate = CreateButton(rowTransform.transform, "🔄 Rotar 45°", AccentPurple, () =>
        {
            if (_primaryDraggableSprite != null) _primaryDraggableSprite.RotateDelta(45f);
        });
        Flex(btnRotate.gameObject, flexW: 1);

        // Separador
        var div2 = Panel(controlsPanel.transform, BorderColor);
        Size(div2.gameObject, prefH: 1);

        var sec3Title = CreateText(controlsPanel.transform, "🎨 Cambiar Apariencia y Color", 13, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(sec3Title.gameObject, prefH: 18);

        // Fila de formas de Sprite
        var rowShapes = Row(controlsPanel.transform, 6);
        Size(rowShapes.gameObject, prefH: 32);

        CreateButton(rowShapes.transform, "💎 Gema", ButtonNormal, () => _primaryDraggableSprite?.SetSprite(_diamondSprite, "Diamante"));
        CreateButton(rowShapes.transform, "⭐ Estrella", ButtonNormal, () => _primaryDraggableSprite?.SetSprite(_starSprite, "Estrella"));
        CreateButton(rowShapes.transform, "🛡️ Escudo", ButtonNormal, () => _primaryDraggableSprite?.SetSprite(_shieldSprite, "Escudo"));
        CreateButton(rowShapes.transform, "⏺ Círculo", ButtonNormal, () => _primaryDraggableSprite?.SetSprite(_circleSprite, "Círculo"));

        // Fila de colores de Sprite
        var rowColors = Row(controlsPanel.transform, 6);
        Size(rowColors.gameObject, prefH: 30);

        CreateColorButton(rowColors.transform, AccentPrimary, () => _primaryDraggableSprite?.SetColor(AccentPrimary));
        CreateColorButton(rowColors.transform, AccentSuccess, () => _primaryDraggableSprite?.SetColor(AccentSuccess));
        CreateColorButton(rowColors.transform, AccentWarning, () => _primaryDraggableSprite?.SetColor(AccentWarning));
        CreateColorButton(rowColors.transform, AccentDanger, () => _primaryDraggableSprite?.SetColor(AccentDanger));
        CreateColorButton(rowColors.transform, AccentPurple, () => _primaryDraggableSprite?.SetColor(AccentPurple));
        CreateColorButton(rowColors.transform, AccentCyan, () => _primaryDraggableSprite?.SetColor(AccentCyan));
        CreateColorButton(rowColors.transform, TextWhite, () => _primaryDraggableSprite?.SetColor(TextWhite));

        // Separador
        var div3 = Panel(controlsPanel.transform, BorderColor);
        Size(div3.gameObject, prefH: 1);

        // Generar múltiples Sprites
        var btnSpawn = CreateButton(controlsPanel.transform, "➕ Generar Nuevo Sprite Arrastrable", AccentSuccess, SpawnExtraDraggableSprite);
        Size(btnSpawn.gameObject, prefH: 38);

        var btnClearSpawns = CreateButton(controlsPanel.transform, "🗑️ Limpiar Sprites Extra", ButtonNormal, ClearSpawnedSprites);
        Size(btnClearSpawns.gameObject, prefH: 32);

        // Columna Derecha: Sandbox Interactivo (Área de Arrastre)
        var rightCol = Column(page.transform, 10);
        Flex(rightCol.gameObject, flexW: 1.8f);

        // Barra de Información de Coordenadas
        var infoBar = Panel(rightCol.transform, CardBg);
        Size(infoBar.gameObject, prefH: 42);
        var infoH = infoBar.gameObject.AddComponent<HorizontalLayoutGroup>();
        infoH.padding = new RectOffset(14, 14, 6, 6);
        infoH.spacing = 10;
        infoH.childControlWidth = false;
        infoH.childControlHeight = true;
        infoH.childForceExpandWidth = false;
        infoH.childForceExpandHeight = true;
        infoH.childAlignment = TextAnchor.MiddleLeft;

        _spriteStateBadge = CreateText(infoBar.transform, "🔓 ARRASTRE ACTIVO", 12, TextAnchor.MiddleLeft, AccentSuccess, FontStyle.Bold);
        Size(_spriteStateBadge.gameObject, prefW: 160);

        CreateSpacer(infoBar.transform, flexW: 1);

        _spriteCoordsText = CreateText(infoBar.transform, "Posición: (0, 0) px", 12, TextAnchor.MiddleRight, AccentCyan, FontStyle.Bold);
        Size(_spriteCoordsText.gameObject, prefW: 240);

        // Área Sandbox
        var sandbox = Panel(rightCol.transform, SandboxBg);
        sandbox.name = "SpriteSandboxArea";
        _spriteSandboxArea = sandbox.rectTransform;
        Flex(sandbox.gameObject, flexH: 1);

        // Guías de cuadrícula de fondo
        var gridLineH = Panel(sandbox.transform, new Color(0.18f, 0.22f, 0.32f, 0.35f));
        Stretch(gridLineH.rectTransform, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f));
        gridLineH.rectTransform.sizeDelta = new Vector2(0, 2);

        var gridLineV = Panel(sandbox.transform, new Color(0.18f, 0.22f, 0.32f, 0.35f));
        Stretch(gridLineV.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 1f));
        gridLineV.rectTransform.sizeDelta = new Vector2(2, 0);

        // Zona de Destino / Drop Zone decorativa en la esquina superior derecha
        var dropZone = Panel(sandbox.transform, new Color(0.14f, 0.35f, 0.25f, 0.45f));
        dropZone.name = "DropZone";
        var dzRt = dropZone.rectTransform;
        dzRt.anchorMin = new Vector2(0.75f, 0.70f);
        dzRt.anchorMax = new Vector2(0.95f, 0.92f);
        dzRt.offsetMin = Vector2.zero;
        dzRt.offsetMax = Vector2.zero;

        var dzText = CreateText(dropZone.transform, "🎯 ZONA DE ENCAJE\n(Drop Target)", 11, TextAnchor.MiddleCenter, AccentSuccess, FontStyle.Bold);
        Stretch(dzText.rectTransform, Vector2.zero, Vector2.one);

        // Sprite Principal Arrastrable
        var spriteObj = Panel(sandbox.transform, AccentPrimary);
        spriteObj.name = "PrimaryDraggableSprite";
        spriteObj.sprite = _diamondSprite;
        var sRt = spriteObj.rectTransform;
        sRt.sizeDelta = new Vector2(100, 100);
        sRt.anchoredPosition = Vector2.zero;

        // Texto informativo dentro del sprite
        var sLabel = CreateText(spriteObj.transform, "✦\nARRASTRAR", 12, TextAnchor.MiddleCenter, TextWhite, FontStyle.Bold);
        Stretch(sLabel.rectTransform, Vector2.zero, Vector2.one);

        // Vincular Componente DraggableSprite
        _primaryDraggableSprite = spriteObj.gameObject.AddComponent<DraggableSprite>();
        _primaryDraggableSprite.containmentArea = _spriteSandboxArea;
        _primaryDraggableSprite.spriteImage = spriteObj;
        _primaryDraggableSprite.OnPositionChanged = pos =>
        {
            if (_spriteCoordsText != null)
            {
                _spriteCoordsText.text = $"Posición: ({pos.x:F0}, {pos.y:F0}) px";
            }
        };
        _primaryDraggableSprite.OnStatusMessage = msg => AddLog(msg);

        return page.gameObject;
    }

    void SpawnExtraDraggableSprite()
    {
        if (_spriteSandboxArea == null) return;

        Color[] colors = { AccentSuccess, AccentWarning, AccentDanger, AccentPurple, AccentCyan, AccentFlame };
        Sprite[] shapes = { _starSprite, _shieldSprite, _circleSprite, _diamondSprite };

        Color chosenColor = colors[UnityEngine.Random.Range(0, colors.Length)];
        Sprite chosenShape = shapes[UnityEngine.Random.Range(0, shapes.Length)];

        var extraGo = Panel(_spriteSandboxArea, chosenColor);
        extraGo.name = "SpawnedSprite_" + (_spawnedSprites.Count + 1);
        extraGo.sprite = chosenShape;

        var rt = extraGo.rectTransform;
        rt.sizeDelta = new Vector2(80, 80);
        float rx = UnityEngine.Random.Range(-200f, 200f);
        float ry = UnityEngine.Random.Range(-150f, 150f);
        rt.anchoredPosition = new Vector2(rx, ry);

        var label = CreateText(extraGo.transform, $"#{_spawnedSprites.Count + 1}", 12, TextAnchor.MiddleCenter, TextWhite, FontStyle.Bold);
        Stretch(label.rectTransform, Vector2.zero, Vector2.one);

        var dragComp = extraGo.gameObject.AddComponent<DraggableSprite>();
        dragComp.containmentArea = _spriteSandboxArea;
        dragComp.spriteImage = extraGo;
        dragComp.OnStatusMessage = msg => AddLog(msg);

        _spawnedSprites.Add(dragComp);
        AddLog($"✨ Nuevo sprite arrastrable #{_spawnedSprites.Count} instanciado.");
    }

    void ClearSpawnedSprites()
    {
        foreach (var s in _spawnedSprites)
        {
            if (s != null) Destroy(s.gameObject);
        }
        _spawnedSprites.Clear();
        AddLog("🗑️ Todos los sprites extra eliminados.");
    }

    // ==========================================
    // PÁGINA 3: Controlar las Fuentes (Estudio Tipográfico)
    // ==========================================
    GameObject BuildPageControlFuentes(Transform parent)
    {
        var page = Panel(parent, Color.clear);
        page.name = "Page_ControlFuentes";
        Stretch(page.rectTransform, Vector2.zero, Vector2.one);

        var hLayout = page.gameObject.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 14;
        hLayout.childControlWidth = true;
        hLayout.childControlHeight = true;
        hLayout.childForceExpandWidth = true;
        hLayout.childForceExpandHeight = true;

        // Columna Izquierda: Botonera de Control de Fuentes
        var leftCol = Column(page.transform, 10);
        Flex(leftCol.gameObject, flexW: 1.3f);

        var headerCard = Panel(leftCol.transform, CardBg);
        Size(headerCard.gameObject, prefH: 70);
        var headV = headerCard.gameObject.AddComponent<VerticalLayoutGroup>();
        headV.padding = new RectOffset(16, 16, 10, 10);
        headV.spacing = 4;
        headV.childControlWidth = true;
        headV.childControlHeight = true;
        headV.childForceExpandWidth = true;
        headV.childForceExpandHeight = false;

        var headTitle = CreateText(headerCard.transform, "🔤 Control Tipográfico Interactivo", 16, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(headTitle.gameObject, prefH: 22);
        var headDesc = CreateText(headerCard.transform, "Modifica dinámicamente familias, tamaños, estilos, colores y efectos del texto mediante botones.", 11, TextAnchor.MiddleLeft, TextMuted);
        Size(headDesc.gameObject, prefH: 24);

        // Panel de Controles de Fuente
        var controlsPanel = Panel(leftCol.transform, SidebarBg);
        Flex(controlsPanel.gameObject, flexH: 1);
        var ctlV = controlsPanel.gameObject.AddComponent<VerticalLayoutGroup>();
        ctlV.padding = new RectOffset(14, 14, 12, 12);
        ctlV.spacing = 8;
        ctlV.childControlWidth = true;
        ctlV.childControlHeight = true;
        ctlV.childForceExpandWidth = true;
        ctlV.childForceExpandHeight = false;

        // 1. Familia Tipográfica
        var secFont = CreateText(controlsPanel.transform, "1. Familia Tipográfica (Font)", 13, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(secFont.gameObject, prefH: 18);

        var rowFont = Row(controlsPanel.transform, 8);
        Size(rowFont.gameObject, prefH: 34);

        var btnPrevFont = CreateButton(rowFont.transform, "◀ Anterior", ButtonNormal, () => _fontController?.PreviousFont());
        Flex(btnPrevFont.gameObject, flexW: 1);

        var btnNextFont = CreateButton(rowFont.transform, "Siguiente ▶", AccentPrimary, () => _fontController?.NextFont());
        Flex(btnNextFont.gameObject, flexW: 1);

        // 2. Control de Tamaño
        var secSize = CreateText(controlsPanel.transform, "2. Tamaño de Fuente (Font Size)", 13, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(secSize.gameObject, prefH: 18);

        var rowSizes = Row(controlsPanel.transform, 6);
        Size(rowSizes.gameObject, prefH: 34);

        CreateButton(rowSizes.transform, "➖ -2px", ButtonNormal, () => _fontController?.DecreaseFontSize(2));
        CreateButton(rowSizes.transform, "14px", ButtonNormal, () => _fontController?.SetFontSize(14));
        CreateButton(rowSizes.transform, "20px", ButtonNormal, () => _fontController?.SetFontSize(20));
        CreateButton(rowSizes.transform, "28px", ButtonNormal, () => _fontController?.SetFontSize(28));
        CreateButton(rowSizes.transform, "38px", ButtonNormal, () => _fontController?.SetFontSize(38));
        CreateButton(rowSizes.transform, "➕ +2px", ButtonNormal, () => _fontController?.IncreaseFontSize(2));

        // 3. Estilo Tipográfico
        var secStyle = CreateText(controlsPanel.transform, "3. Estilo Tipográfico (Font Style)", 13, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(secStyle.gameObject, prefH: 18);

        var rowStyles = Row(controlsPanel.transform, 6);
        Size(rowStyles.gameObject, prefH: 34);

        CreateButton(rowStyles.transform, "Normal", ButtonNormal, () => _fontController?.SetFontStyle(FontStyle.Normal));
        CreateButton(rowStyles.transform, "<b>Negrita</b>", AccentPrimary, () => _fontController?.ToggleBold());
        CreateButton(rowStyles.transform, "<i>Cursiva</i>", AccentPurple, () => _fontController?.ToggleItalic());
        CreateButton(rowStyles.transform, "<b><i>Ambos</i></b>", AccentSuccess, () => _fontController?.SetFontStyle(FontStyle.BoldAndItalic));

        // 4. Alineación
        var secAlign = CreateText(controlsPanel.transform, "4. Alineación del Texto", 13, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(secAlign.gameObject, prefH: 18);

        var rowAlign = Row(controlsPanel.transform, 6);
        Size(rowAlign.gameObject, prefH: 34);

        CreateButton(rowAlign.transform, "⬅ Izquierda", ButtonNormal, () => _fontController?.SetAlignment(TextAnchor.UpperLeft));
        CreateButton(rowAlign.transform, "⏺ Centro", AccentPrimary, () => _fontController?.SetAlignment(TextAnchor.MiddleCenter));
        CreateButton(rowAlign.transform, "Derecha ➡", ButtonNormal, () => _fontController?.SetAlignment(TextAnchor.UpperRight));

        // 5. Paleta de Colores de Texto
        var secColor = CreateText(controlsPanel.transform, "5. Color de Texto", 13, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(secColor.gameObject, prefH: 18);

        var rowColors = Row(controlsPanel.transform, 6);
        Size(rowColors.gameObject, prefH: 30);

        CreateColorButton(rowColors.transform, TextWhite, () => _fontController?.SetTextColor(TextWhite));
        CreateColorButton(rowColors.transform, AccentPrimary, () => _fontController?.SetTextColor(AccentPrimary));
        CreateColorButton(rowColors.transform, AccentSuccess, () => _fontController?.SetTextColor(AccentSuccess));
        CreateColorButton(rowColors.transform, AccentWarning, () => _fontController?.SetTextColor(AccentWarning));
        CreateColorButton(rowColors.transform, AccentDanger, () => _fontController?.SetTextColor(AccentDanger));
        CreateColorButton(rowColors.transform, AccentPurple, () => _fontController?.SetTextColor(AccentPurple));
        CreateColorButton(rowColors.transform, AccentCyan, () => _fontController?.SetTextColor(AccentCyan));

        // 6. Efectos Visuales y Transformación
        var secEffects = CreateText(controlsPanel.transform, "6. Efectos y Formato", 13, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(secEffects.gameObject, prefH: 18);

        var rowEffects = Row(controlsPanel.transform, 6);
        Size(rowEffects.gameObject, prefH: 34);

        CreateButton(rowEffects.transform, "✨ Sombra", ButtonNormal, () => _fontController?.ToggleShadow());
        CreateButton(rowEffects.transform, "🖋️ Contorno", ButtonNormal, () => _fontController?.ToggleOutline());
        CreateButton(rowEffects.transform, "MAYÚSCULAS", ButtonNormal, () => _fontController?.SetTextCase(1));
        CreateButton(rowEffects.transform, "minúsculas", ButtonNormal, () => _fontController?.SetTextCase(2));

        // Columna Derecha: Tarjeta de Muestra en Vivo
        var rightCol = Column(page.transform, 10);
        Flex(rightCol.gameObject, flexW: 1.6f);

        // Tarjeta de Vista Previa de Texto
        var previewCard = Panel(rightCol.transform, CardBg);
        Flex(previewCard.gameObject, flexH: 1.6f);
        var prevV = previewCard.gameObject.AddComponent<VerticalLayoutGroup>();
        prevV.padding = new RectOffset(20, 20, 16, 16);
        prevV.spacing = 10;
        prevV.childControlWidth = true;
        prevV.childControlHeight = true;
        prevV.childForceExpandWidth = true;
        prevV.childForceExpandHeight = false;

        var prevHeader = Row(previewCard.transform, 10);
        Size(prevHeader.gameObject, prefH: 26);
        var prevTitle = CreateText(prevHeader.transform, "👁️ Vista Previa en Tiempo Real", 16, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Flex(prevTitle.gameObject, flexW: 1);
        var prevBadge = CreateBadge(prevHeader.transform, "TEXTO INTERACTIVO", AccentSuccess);
        Size(prevBadge.gameObject, prefW: 150, prefH: 24);

        // Caja de Muestra de Texto
        var sampleBox = Panel(previewCard.transform, new Color(0.06f, 0.08f, 0.12f, 1f));
        Flex(sampleBox.gameObject, flexH: 1);
        var sBoxV = sampleBox.gameObject.AddComponent<VerticalLayoutGroup>();
        sBoxV.padding = new RectOffset(20, 20, 16, 16);
        sBoxV.spacing = 6;
        sBoxV.childControlWidth = true;
        sBoxV.childControlHeight = true;
        sBoxV.childForceExpandWidth = true;
        sBoxV.childForceExpandHeight = true;

        _samplePreviewText = CreateText(sampleBox.transform,
            "Tipografía Dinámica en Unity\n\n" +
            "El rápido zorro marrón salta sobre el perro perezoso.\n" +
            "0123456789 - ¡ÁÉÍÓÚ ñ &%$#@!\n\n" +
            "Esta muestra se actualiza inmediatamente al presionar cualquier botón de tamaño, estilo, color o familia tipográfica.",
            20, TextAnchor.MiddleCenter, TextWhite, FontStyle.Normal);
        Flex(_samplePreviewText.gameObject, flexH: 1);

        // Vincular FontController
        _fontController = _samplePreviewText.gameObject.AddComponent<FontController>();
        _fontController.targetText = _samplePreviewText;
        _fontController.OnStatusMessage = msg => AddLog(msg);
        _fontController.OnFontPropertiesUpdated = UpdateFontMetricsDisplay;

        // Ficha de Métricas de Fuente
        var metricsCard = Panel(rightCol.transform, SidebarBg);
        Size(metricsCard.gameObject, prefH: 110);
        var metV = metricsCard.gameObject.AddComponent<VerticalLayoutGroup>();
        metV.padding = new RectOffset(16, 16, 12, 12);
        metV.spacing = 4;
        metV.childControlWidth = true;
        metV.childControlHeight = true;
        metV.childForceExpandWidth = true;
        metV.childForceExpandHeight = false;

        var metTitle = CreateText(metricsCard.transform, "📋 Inspector de Propiedades Tipográficas", 14, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(metTitle.gameObject, prefH: 20);

        _fontMetricsText = CreateText(metricsCard.transform, "Fuente: Estándar | Tamaño: 20px | Estilo: Normal | Alineación: Centro", 12, TextAnchor.UpperLeft, TextMuted);
        Flex(_fontMetricsText.gameObject, flexH: 1);

        UpdateFontMetricsDisplay();

        return page.gameObject;
    }

    void UpdateFontMetricsDisplay()
    {
        if (_fontController == null || _fontMetricsText == null) return;
        _fontMetricsText.text =
            $"• <b>Familia:</b> {_fontController.CurrentFontName}\n" +
            $"• <b>Tamaño:</b> {_fontController.CurrentFontSize} px  |  <b>Estilo:</b> {_fontController.CurrentFontStyle}\n" +
            $"• <b>Alineación:</b> {_fontController.CurrentAlignment}  |  <b>Color:</b> #{ColorUtility.ToHtmlStringRGBA(_fontController.CurrentColor)}";
    }

    // ==========================================
    // PÁGINA 4: Fijar Scripts a Objetos de Juego & Inspector
    // ==========================================
    GameObject BuildPageFijarScripts(Transform parent)
    {
        var page = Panel(parent, Color.clear);
        page.name = "Page_FijarScripts";
        Stretch(page.rectTransform, Vector2.zero, Vector2.one);

        var hLayout = page.gameObject.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 14;
        hLayout.childControlWidth = true;
        hLayout.childControlHeight = true;
        hLayout.childForceExpandWidth = true;
        hLayout.childForceExpandHeight = true;

        // Columna Izquierda: Botones de Fijación de Scripts
        var leftCol = Column(page.transform, 10);
        Flex(leftCol.gameObject, flexW: 1.2f);

        var headerCard = Panel(leftCol.transform, CardBg);
        Size(headerCard.gameObject, prefH: 75);
        var headV = headerCard.gameObject.AddComponent<VerticalLayoutGroup>();
        headV.padding = new RectOffset(16, 16, 10, 10);
        headV.spacing = 4;
        headV.childControlWidth = true;
        headV.childControlHeight = true;
        headV.childForceExpandWidth = true;
        headV.childForceExpandHeight = false;

        var headTitle = CreateText(headerCard.transform, "🧩 Fijación de Scripts a GameObjects", 16, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(headTitle.gameObject, prefH: 22);
        var headDesc = CreateText(headerCard.transform, "Añade y desacopla scripts en tiempo de ejecución (AddComponent) a objetos de juego mediante botones.", 11, TextAnchor.MiddleLeft, TextMuted);
        Size(headDesc.gameObject, prefH: 26);

        // Panel de Botones de Fijación
        var bindPanel = Panel(leftCol.transform, SidebarBg);
        Flex(bindPanel.gameObject, flexH: 1);
        var bV = bindPanel.gameObject.AddComponent<VerticalLayoutGroup>();
        bV.padding = new RectOffset(14, 14, 12, 12);
        bV.spacing = 6;
        bV.childControlWidth = true;
        bV.childControlHeight = true;
        bV.childForceExpandWidth = true;
        bV.childForceExpandHeight = false;

        var sec1 = CreateText(bindPanel.transform, "📎 Fijar Scripts (AddComponent)", 13, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(sec1.gameObject, prefH: 18);

        var btnAttachDrag = CreateButton(bindPanel.transform, "🎯 Fijar 'DraggableSprite'", AccentPrimary, () =>
        {
            if (_scriptBinder != null && _testTargetGameObject != null)
            {
                var ds = _scriptBinder.AttachDraggableSprite(_testTargetGameObject);
                if (ds != null)
                {
                    ds.containmentArea = _testTargetGameObject.transform.parent as RectTransform;
                }
            }
        });
        Size(btnAttachDrag.gameObject, prefH: 34);

        var btnAttachFont = CreateButton(bindPanel.transform, "🔤 Fijar 'FontController'", AccentSuccess, () =>
        {
            if (_scriptBinder != null && _testTargetGameObject != null)
            {
                _scriptBinder.AttachFontController(_testTargetGameObject);
            }
        });
        Size(btnAttachFont.gameObject, prefH: 34);

        var btnAttachAnim = CreateButton(bindPanel.transform, "🎬 Fijar 'SpriteAnimationLoop'", AccentFlame, () =>
        {
            if (_scriptBinder != null && _testTargetGameObject != null)
            {
                var anim = _scriptBinder.AttachSpriteAnimationLoop(_testTargetGameObject);
                if (anim != null)
                {
                    anim.SetSpriteFrames(_fireSprites, "Fuego Mágico (8 Sprites)");
                }
            }
        });
        Size(btnAttachAnim.gameObject, prefH: 34);

        var btnAttachLoop = CreateButton(bindPanel.transform, "⚡ Fijar 'GameLoopController'", AccentCyan, () =>
        {
            if (_scriptBinder != null && _testTargetGameObject != null)
            {
                _scriptBinder.AttachGameLoopController(_testTargetGameObject);
            }
        });
        Size(btnAttachLoop.gameObject, prefH: 34);

        // Separador
        var div = Panel(bindPanel.transform, BorderColor);
        Size(div.gameObject, prefH: 1);

        var sec2 = CreateText(bindPanel.transform, "🗑️ Desacoplar Scripts (Destroy)", 13, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(sec2.gameObject, prefH: 18);

        var rowRemove1 = Row(bindPanel.transform, 6);
        Size(rowRemove1.gameObject, prefH: 32);
        CreateButton(rowRemove1.transform, "Remover Drag", ButtonNormal, () => _scriptBinder?.RemoveComponentByName("DraggableSprite", _testTargetGameObject));
        CreateButton(rowRemove1.transform, "Remover Font", ButtonNormal, () => _scriptBinder?.RemoveComponentByName("FontController", _testTargetGameObject));

        var rowRemove2 = Row(bindPanel.transform, 6);
        Size(rowRemove2.gameObject, prefH: 32);
        CreateButton(rowRemove2.transform, "Remover Anim", ButtonNormal, () => _scriptBinder?.RemoveComponentByName("SpriteAnimationLoop", _testTargetGameObject));
        CreateButton(rowRemove2.transform, "Remover GameLoop", ButtonNormal, () => _scriptBinder?.RemoveComponentByName("GameLoopController", _testTargetGameObject));

        // Separador
        var div2 = Panel(bindPanel.transform, BorderColor);
        Size(div2.gameObject, prefH: 1);

        var sec3 = CreateText(bindPanel.transform, "⚙️ Alternar Estado de Scripts", 13, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(sec3.gameObject, prefH: 18);

        var btnToggleDrag = CreateButton(bindPanel.transform, "Alternar DraggableSprite", AccentWarning, () =>
        {
            _scriptBinder?.ToggleComponentEnabled("DraggableSprite", _testTargetGameObject);
        });
        Size(btnToggleDrag.gameObject, prefH: 32);

        var btnToggleAnim = CreateButton(bindPanel.transform, "Alternar SpriteAnimationLoop", AccentPurple, () =>
        {
            _scriptBinder?.ToggleComponentEnabled("SpriteAnimationLoop", _testTargetGameObject);
        });
        Size(btnToggleAnim.gameObject, prefH: 32);

        // Columna Derecha: Objeto Objetivo en Vivo e Inspector de Componentes
        var rightCol = Column(page.transform, 10);
        Flex(rightCol.gameObject, flexW: 1.6f);

        // Tarjeta de Demostración del Objeto Objetivo
        var targetCard = Panel(rightCol.transform, CardBg);
        Flex(targetCard.gameObject, flexH: 1.2f);
        var tCardV = targetCard.gameObject.AddComponent<VerticalLayoutGroup>();
        tCardV.padding = new RectOffset(18, 18, 14, 14);
        tCardV.spacing = 8;
        tCardV.childControlWidth = true;
        tCardV.childControlHeight = true;
        tCardV.childForceExpandWidth = true;
        tCardV.childForceExpandHeight = false;

        var tHead = Row(targetCard.transform, 10);
        Size(tHead.gameObject, prefH: 26);
        var tTitle = CreateText(tHead.transform, "🎮 Objeto de Juego Seleccionado: [ObjetoDePrueba]", 15, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Flex(tTitle.gameObject, flexW: 1);
        var tBadge = CreateBadge(tHead.transform, "TARGET GAMEOBJECT", AccentCyan);
        Size(tBadge.gameObject, prefW: 150, prefH: 24);

        // Contenedor visual del GameObject objetivo
        var testArea = Panel(targetCard.transform, SandboxBg);
        Flex(testArea.gameObject, flexH: 1);

        // GameObject de Prueba
        _testTargetGameObject = new GameObject("ObjetoDePrueba", typeof(RectTransform), typeof(Image));
        _testTargetGameObject.transform.SetParent(testArea.transform, false);
        var tImg = _testTargetGameObject.GetComponent<Image>();
        tImg.sprite = _shieldSprite;
        tImg.color = AccentPurple;
        var tRt = _testTargetGameObject.GetComponent<RectTransform>();
        tRt.sizeDelta = new Vector2(120, 120);
        tRt.anchoredPosition = Vector2.zero;

        var tLabel = CreateText(_testTargetGameObject.transform, "OBJETO\nDE PRUEBA", 12, TextAnchor.MiddleCenter, TextWhite, FontStyle.Bold);
        Stretch(tLabel.rectTransform, Vector2.zero, Vector2.one);

        _scriptBinder.targetGameObject = _testTargetGameObject;

        // Tarjeta de Inspector de Componentes Fijados
        var inspectorCard = Panel(rightCol.transform, SidebarBg);
        Flex(inspectorCard.gameObject, flexH: 1.4f);
        var insV = inspectorCard.gameObject.AddComponent<VerticalLayoutGroup>();
        insV.padding = new RectOffset(16, 16, 12, 12);
        insV.spacing = 6;
        insV.childControlWidth = true;
        insV.childControlHeight = true;
        insV.childForceExpandWidth = true;
        insV.childForceExpandHeight = false;

        var insTitle = CreateText(inspectorCard.transform, "📋 Inspector en Vivo de Componentes Fijados", 14, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(insTitle.gameObject, prefH: 20);

        _attachedScriptsText = CreateText(inspectorCard.transform, "Cargando componentes...", 12, TextAnchor.UpperLeft, TextMuted);
        Flex(_attachedScriptsText.gameObject, flexH: 1);

        RefreshAttachedScriptsDisplay();

        return page.gameObject;
    }

    void RefreshAttachedScriptsDisplay()
    {
        if (_testTargetGameObject == null || _attachedScriptsText == null || _scriptBinder == null) return;

        var list = _scriptBinder.GetAttachedComponentsInfo(_testTargetGameObject);
        var lines = new List<string>();

        lines.Add($"<b>GameObject:</b> <color=#1FDFDF>{_testTargetGameObject.name}</color>  |  <b>Total Componentes:</b> {list.Count}\n");

        foreach (var info in list)
        {
            string statusIcon = info.IsEnabled ? "🟢" : "🔴";
            string typeColor = info.IsCustomScript ? "#3D82FF" : "#9AA8C2";
            string badge = info.IsCustomScript ? "<b>[SCRIPT PERSONALIZADO]</b>" : "[NATIVO]";
            lines.Add($"{statusIcon} <color={typeColor}>{info.TypeName}</color>  {badge}  -  {(info.IsEnabled ? "Habilitado" : "Deshabilitado")}");
        }

        _attachedScriptsText.text = string.Join("\n", lines);
    }

    // ==========================================
    // PÁGINA 5: Navegación Funcional & Dispositivos
    // ==========================================
    GameObject BuildPageNavegacionYDispositivos(Transform parent)
    {
        var page = Panel(parent, Color.clear);
        page.name = "Page_Navegacion";
        Stretch(page.rectTransform, Vector2.zero, Vector2.one);

        var hLayout = page.gameObject.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 14;
        hLayout.childControlWidth = true;
        hLayout.childControlHeight = true;
        hLayout.childForceExpandWidth = true;
        hLayout.childForceExpandHeight = true;

        // Columna Izquierda: Botonera de Navegación e Historial
        var leftCol = Column(page.transform, 10);
        Flex(leftCol.gameObject, flexW: 1.2f);

        var navCard = Panel(leftCol.transform, CardBg);
        Flex(navCard.gameObject, flexH: 1);
        var nV = navCard.gameObject.AddComponent<VerticalLayoutGroup>();
        nV.padding = new RectOffset(16, 16, 14, 14);
        nV.spacing = 8;
        nV.childControlWidth = true;
        nV.childControlHeight = true;
        nV.childForceExpandWidth = true;
        nV.childForceExpandHeight = false;

        var navTitle = CreateText(navCard.transform, "🧭 Navegación Funcional en Unity", 16, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(navTitle.gameObject, prefH: 22);

        var navDesc = CreateText(navCard.transform, "Sistema de rutas con historial de navegación (Back/Forward), migas de pan y control de páginas.", 11, TextAnchor.MiddleLeft, TextMuted);
        Size(navDesc.gameObject, prefH: 26);

        // Fila de Botones de Navegación Principal
        var rowNavBtns = Row(navCard.transform, 8);
        Size(rowNavBtns.gameObject, prefH: 40);

        var bBack = CreateButton(rowNavBtns.transform, "◀ Atrás", ButtonNormal, () => _navigationController.GoBack());
        Flex(bBack.gameObject, flexW: 1);

        var bHome = CreateButton(rowNavBtns.transform, "🏠 Inicio", AccentPrimary, () => _navigationController.GoHome());
        Flex(bHome.gameObject, flexW: 1);

        var bFwd = CreateButton(rowNavBtns.transform, "Adelante ▶", ButtonNormal, () => _navigationController.GoForward());
        Flex(bFwd.gameObject, flexW: 1);

        // Separador
        var div1 = Panel(navCard.transform, BorderColor);
        Size(div1.gameObject, prefH: 1);

        var secJump = CreateText(navCard.transform, "🎯 Salto Directo a Módulos", 13, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(secJump.gameObject, prefH: 18);

        CreateButton(navCard.transform, "1. 🏠 Ir a Página de Inicio", ButtonNormal, () => _navigationController.NavigateTo(0));
        CreateButton(navCard.transform, "2. 🎯 Ir a Arrastrar Sprite", AccentPrimary, () => _navigationController.NavigateTo(1));
        CreateButton(navCard.transform, "3. 🔤 Ir a Control de Fuentes", AccentSuccess, () => _navigationController.NavigateTo(2));
        CreateButton(navCard.transform, "4. 🧩 Ir a Fijar Scripts", AccentPurple, () => _navigationController.NavigateTo(3));
        CreateButton(navCard.transform, "5. 🎬 Ir a Bucle y 8 Sprites", AccentFlame, () => _navigationController.NavigateTo(5));

        // Separador
        var div2 = Panel(navCard.transform, BorderColor);
        Size(div2.gameObject, prefH: 1);

        // Selector de Resoluciones y Dispositivos
        var secDev = CreateText(navCard.transform, "📱 Simulación de Dispositivos", 13, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(secDev.gameObject, prefH: 18);

        var rowDevices = Row(navCard.transform, 6);
        Size(rowDevices.gameObject, prefH: 34);

        CreateButton(rowDevices.transform, "💻 PC (16:9)", ButtonNormal, () => ApplyDevicePreset(0));
        CreateButton(rowDevices.transform, "📱 Móvil (9:16)", ButtonNormal, () => ApplyDevicePreset(1));
        CreateButton(rowDevices.transform, "📟 Tablet (4:3)", ButtonNormal, () => ApplyDevicePreset(3));

        // Columna Derecha: Consola de Eventos en Vivo
        var rightCol = Column(page.transform, 10);
        Flex(rightCol.gameObject, flexW: 1.5f);

        var consoleCard = Panel(rightCol.transform, CardBg);
        Flex(consoleCard.gameObject, flexH: 1);
        var conV = consoleCard.gameObject.AddComponent<VerticalLayoutGroup>();
        conV.padding = new RectOffset(16, 16, 14, 14);
        conV.spacing = 8;
        conV.childControlWidth = true;
        conV.childControlHeight = true;
        conV.childForceExpandWidth = true;
        conV.childForceExpandHeight = false;

        var conHead = Row(consoleCard.transform, 8);
        Size(conHead.gameObject, prefH: 26);
        var conTitle = CreateText(conHead.transform, "📜 Consola de Eventos y Notificaciones", 15, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Flex(conTitle.gameObject, flexW: 1);
        var conBadge = CreateBadge(conHead.transform, "LIVE LOGS", AccentSuccess);
        Size(conBadge.gameObject, prefW: 85, prefH: 22);

        var conBox = Panel(consoleCard.transform, new Color(0.05f, 0.06f, 0.09f, 1f));
        Flex(conBox.gameObject, flexH: 1);
        var cBoxV = conBox.gameObject.AddComponent<VerticalLayoutGroup>();
        cBoxV.padding = new RectOffset(12, 12, 10, 10);
        cBoxV.spacing = 4;
        cBoxV.childControlWidth = true;
        cBoxV.childControlHeight = true;
        cBoxV.childForceExpandWidth = true;
        cBoxV.childForceExpandHeight = true;

        _logConsoleContent = CreateText(conBox.transform, "", 11, TextAnchor.UpperLeft, new Color(0.8f, 0.85f, 0.95f));
        Flex(_logConsoleContent.gameObject, flexH: 1);

        // Botón limpiar logs
        var btnClearLogs = CreateButton(consoleCard.transform, "🗑️ Limpiar Registro", ButtonNormal, () =>
        {
            _logEntries.Clear();
            AddLog("Registro de consola reiniciado.");
        });
        Size(btnClearLogs.gameObject, prefH: 32);

        return page.gameObject;
    }

    // ==========================================
    // PÁGINA 6: Bucle de Videojuego y Animación con Ocho (8) Sprites
    // ==========================================
    GameObject BuildPageBucleYAnimacion(Transform parent)
    {
        var page = Panel(parent, Color.clear);
        page.name = "Page_BucleYAnimacion";
        Stretch(page.rectTransform, Vector2.zero, Vector2.one);

        var hLayout = page.gameObject.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 14;
        hLayout.childControlWidth = true;
        hLayout.childControlHeight = true;
        hLayout.childForceExpandWidth = true;
        hLayout.childForceExpandHeight = true;

        // -------------------------------------------------------------
        // Columna Izquierda: Controlador del Bucle de Videojuego (Game Loop)
        // -------------------------------------------------------------
        var leftCol = Column(page.transform, 10);
        Flex(leftCol.gameObject, flexW: 1.25f);

        var loopHeadCard = Panel(leftCol.transform, CardBg);
        Size(loopHeadCard.gameObject, prefH: 70);
        var lHeadV = loopHeadCard.gameObject.AddComponent<VerticalLayoutGroup>();
        lHeadV.padding = new RectOffset(16, 16, 10, 10);
        lHeadV.spacing = 4;
        lHeadV.childControlWidth = true;
        lHeadV.childControlHeight = true;
        lHeadV.childForceExpandWidth = true;
        lHeadV.childForceExpandHeight = false;

        var lHeadTitle = CreateText(loopHeadCard.transform, "⚡ Bucle de Videojuego (Game Loop)", 16, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(lHeadTitle.gameObject, prefH: 22);
        var lHeadDesc = CreateText(loopHeadCard.transform, "Gestiona el ciclo de ejecución, escala de tiempo (TimeScale) y telemetría de rendimiento.", 11, TextAnchor.MiddleLeft, TextMuted);
        Size(lHeadDesc.gameObject, prefH: 24);

        // Panel de Transporte del Bucle
        var loopPanel = Panel(leftCol.transform, SidebarBg);
        Flex(loopPanel.gameObject, flexH: 1);
        var lpV = loopPanel.gameObject.AddComponent<VerticalLayoutGroup>();
        lpV.padding = new RectOffset(14, 14, 12, 12);
        lpV.spacing = 8;
        lpV.childControlWidth = true;
        lpV.childControlHeight = true;
        lpV.childForceExpandWidth = true;
        lpV.childForceExpandHeight = false;

        // Estado del Bucle
        var rowState = Row(loopPanel.transform, 8);
        Size(rowState.gameObject, prefH: 28);
        var lblLoopState = CreateText(rowState.transform, "Estado del Bucle:", 12, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(lblLoopState.gameObject, prefW: 120);

        var stateBadgeImg = CreateBadge(rowState.transform, "🟢 EJECUCIÓN (RUNNING)", AccentSuccess, out _loopStateBadge);
        Flex(stateBadgeImg.gameObject, flexW: 1);

        // Botones de Transporte del Bucle
        var rowTransport = Row(loopPanel.transform, 6);
        Size(rowTransport.gameObject, prefH: 36);

        var btnPlayLoop = CreateButton(rowTransport.transform, "▶ Reanudar", AccentSuccess, () =>
        {
            _gameLoopController?.Play();
            UpdateLoopStateUI(GameLoopController.GameLoopState.Running);
        });
        Flex(btnPlayLoop.gameObject, flexW: 1);

        var btnPauseLoop = CreateButton(rowTransport.transform, "⏸ Pausar", AccentWarning, () =>
        {
            _gameLoopController?.Pause();
            UpdateLoopStateUI(GameLoopController.GameLoopState.Paused);
        });
        Flex(btnPauseLoop.gameObject, flexW: 1);

        var btnStepLoop = CreateButton(rowTransport.transform, "⏯ Paso a Paso", AccentCyan, () =>
        {
            _gameLoopController?.StepFrame();
            UpdateLoopStateUI(GameLoopController.GameLoopState.Stepping);
        });
        Flex(btnStepLoop.gameObject, flexW: 1);

        var btnResetLoop = CreateButton(rowTransport.transform, "⏹ Reset", ButtonNormal, () =>
        {
            _gameLoopController?.StopAndReset();
            UpdateLoopStateUI(GameLoopController.GameLoopState.Stopped);
        });
        Flex(btnResetLoop.gameObject, flexW: 1);

        // Separador
        var divLoop1 = Panel(loopPanel.transform, BorderColor);
        Size(divLoop1.gameObject, prefH: 1);

        // Escala de Tiempo (TimeScale Multiplier)
        var rowScaleTitle = Row(loopPanel.transform, 8);
        Size(rowScaleTitle.gameObject, prefH: 20);
        var lblTimeScale = CreateText(rowScaleTitle.transform, "Velocidad de Tiempo (TimeScale):", 12, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Flex(lblTimeScale.gameObject, flexW: 1);
        _loopTimeScaleText = CreateText(rowScaleTitle.transform, "1.00x", 12, TextAnchor.MiddleRight, AccentCyan, FontStyle.Bold);
        Size(_loopTimeScaleText.gameObject, prefW: 60);

        var rowTimeScales = Row(loopPanel.transform, 6);
        Size(rowTimeScales.gameObject, prefH: 32);
        CreateButton(rowTimeScales.transform, "0.25x", ButtonNormal, () => SetGameTimeScale(0.25f));
        CreateButton(rowTimeScales.transform, "0.50x", ButtonNormal, () => SetGameTimeScale(0.50f));
        CreateButton(rowTimeScales.transform, "1.00x", AccentPrimary, () => SetGameTimeScale(1.00f));
        CreateButton(rowTimeScales.transform, "2.00x", ButtonNormal, () => SetGameTimeScale(2.00f));
        CreateButton(rowTimeScales.transform, "4.00x", ButtonNormal, () => SetGameTimeScale(4.00f));

        // Limitador de Cuadros por Segundo (Target Frame Rate)
        var secFpsLimit = CreateText(loopPanel.transform, "Límite de Tasa de Refresco (Target FPS):", 12, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(secFpsLimit.gameObject, prefH: 18);

        var rowFpsLimits = Row(loopPanel.transform, 6);
        Size(rowFpsLimits.gameObject, prefH: 32);
        CreateButton(rowFpsLimits.transform, "15 FPS", ButtonNormal, () => _gameLoopController?.ApplyTargetFramerate(15));
        CreateButton(rowFpsLimits.transform, "30 FPS", ButtonNormal, () => _gameLoopController?.ApplyTargetFramerate(30));
        CreateButton(rowFpsLimits.transform, "60 FPS", AccentSuccess, () => _gameLoopController?.ApplyTargetFramerate(60));
        CreateButton(rowFpsLimits.transform, "120 FPS", ButtonNormal, () => _gameLoopController?.ApplyTargetFramerate(120));
        CreateButton(rowFpsLimits.transform, "Máx", ButtonNormal, () => _gameLoopController?.ApplyTargetFramerate(-1));

        // Separador
        var divLoop2 = Panel(loopPanel.transform, BorderColor);
        Size(divLoop2.gameObject, prefH: 1);

        // Telemetría en Vivo del Bucle de Videojuego
        var secTele = CreateText(loopPanel.transform, "📊 Telemetría en Vivo del Bucle", 13, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(secTele.gameObject, prefH: 18);

        var teleBox = Panel(loopPanel.transform, SandboxBg);
        Size(teleBox.gameObject, prefH: 95);
        var teleV = teleBox.gameObject.AddComponent<VerticalLayoutGroup>();
        teleV.padding = new RectOffset(12, 12, 8, 8);
        teleV.spacing = 3;
        teleV.childControlWidth = true;
        teleV.childControlHeight = true;
        teleV.childForceExpandWidth = true;
        teleV.childForceExpandHeight = false;

        _loopTelemetryFps = CreateText(teleBox.transform, "• FPS Actual: 60.0 FPS", 11, TextAnchor.MiddleLeft, AccentSuccess, FontStyle.Bold);
        Size(_loopTelemetryFps.gameObject, prefH: 18);

        _loopTelemetryDelta = CreateText(teleBox.transform, "• DeltaTime (ms/cuadro): 16.6 ms", 11, TextAnchor.MiddleLeft, TextMuted);
        Size(_loopTelemetryDelta.gameObject, prefH: 18);

        _loopTelemetryFrames = CreateText(teleBox.transform, "• Total Cuadros Procesados: 0", 11, TextAnchor.MiddleLeft, TextMuted);
        Size(_loopTelemetryFrames.gameObject, prefH: 18);

        _loopTelemetryTime = CreateText(teleBox.transform, "• Tiempo de Juego: 0.0s  |  Real: 0.0s", 11, TextAnchor.MiddleLeft, TextMuted);
        Size(_loopTelemetryTime.gameObject, prefH: 18);

        // Conectar callbacks de telemetría
        _gameLoopController.OnTelemetryUpdate = OnLoopTelemetryReceived;

        // -------------------------------------------------------------
        // Columna Central / Derecha: Animación con Ocho (8) Sprites
        // -------------------------------------------------------------
        var rightCol = Column(page.transform, 10);
        Flex(rightCol.gameObject, flexW: 1.85f);

        var animCard = Panel(rightCol.transform, CardBg);
        Flex(animCard.gameObject, flexH: 1);
        var animV = animCard.gameObject.AddComponent<VerticalLayoutGroup>();
        animV.padding = new RectOffset(18, 18, 14, 14);
        animV.spacing = 8;
        animV.childControlWidth = true;
        animV.childControlHeight = true;
        animV.childForceExpandWidth = true;
        animV.childForceExpandHeight = false;

        // Barra de Título del Módulo de Animación 8 Sprites
        var aHead = Row(animCard.transform, 10);
        Size(aHead.gameObject, prefH: 28);

        _animSequenceTitle = CreateText(aHead.transform, "🔥 Fuego Mágico (8 Sprites en Bucle)", 16, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Flex(_animSequenceTitle.gameObject, flexW: 1);

        var aBadgeImg = CreateBadge(aHead.transform, "CUADRO ACTIVO: #1 / 8", AccentFlame, out _animActiveFrameBadge);
        Size(aBadgeImg.gameObject, prefW: 175, prefH: 24);

        // Selector de Secuencias de 8 Sprites
        var rowSequences = Row(animCard.transform, 6);
        Size(rowSequences.gameObject, prefH: 34);

        CreateButton(rowSequences.transform, "🔥 Fuego (8 Sprites)", AccentFlame, () => Select8SpriteSequence(0));
        CreateButton(rowSequences.transform, "🏃‍♂️ Caminata (8 Sprites)", AccentPrimary, () => Select8SpriteSequence(1));
        CreateButton(rowSequences.transform, "💎 Gema 3D (8 Sprites)", AccentPurple, () => Select8SpriteSequence(2));
        CreateButton(rowSequences.transform, "⚡ Orbe Energía (8 Sprites)", AccentCyan, () => Select8SpriteSequence(3));

        // Área Sandbox de Reproducción y Arrastre del Sprite Animado
        var animSandbox = Panel(animCard.transform, SandboxBg);
        animSandbox.name = "AnimSandbox";
        Flex(animSandbox.gameObject, flexH: 1);

        // Guías de cuadrícula en el sandbox
        var aGridH = Panel(animSandbox.transform, new Color(0.18f, 0.22f, 0.32f, 0.35f));
        Stretch(aGridH.rectTransform, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f));
        aGridH.rectTransform.sizeDelta = new Vector2(0, 2);

        var aGridV = Panel(animSandbox.transform, new Color(0.18f, 0.22f, 0.32f, 0.35f));
        Stretch(aGridV.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 1f));
        aGridV.rectTransform.sizeDelta = new Vector2(2, 0);

        // Contenedor del Sprite Animado
        _animatedSpriteImg = Panel(animSandbox.transform, Color.white);
        _animatedSpriteImg.name = "Animated8SpriteObject";
        _animatedSpriteImg.sprite = _fireSprites[0];
        var aRt = _animatedSpriteImg.rectTransform;
        aRt.sizeDelta = new Vector2(140, 140);
        aRt.anchoredPosition = Vector2.zero;

        // Vincular Componente SpriteAnimationLoop
        _spriteAnimationLoop = _animatedSpriteImg.gameObject.AddComponent<SpriteAnimationLoop>();
        _spriteAnimationLoop.targetImage = _animatedSpriteImg;
        _spriteAnimationLoop.animationFrames = _fireSprites;
        _spriteAnimationLoop.sequenceName = "Fuego Mágico (8 Sprites)";
        _spriteAnimationLoop.framesPerSecond = 8f;
        _spriteAnimationLoop.loopMode = SpriteAnimationLoop.AnimationLoopMode.Loop;

        // Conectar callbacks de animación
        _spriteAnimationLoop.OnFrameChanged = OnAnimationFrameChanged;
        _spriteAnimationLoop.OnStatusMessage = msg => AddLog(msg);
        _spriteAnimationLoop.OnLoopCompleted = count =>
        {
            if (_animLoopCountText != null)
            {
                _animLoopCountText.text = $"Bucles Completados: {count}";
            }
        };

        // Hacer también el sprite animado arrastrable táctil / ratón
        _animDraggableSprite = _animatedSpriteImg.gameObject.AddComponent<DraggableSprite>();
        _animDraggableSprite.containmentArea = animSandbox.rectTransform;
        _animDraggableSprite.spriteImage = _animatedSpriteImg;
        _animDraggableSprite.OnStatusMessage = msg => AddLog(msg);

        // -------------------------------------------------------------
        // Tira de Fotogramas (Storyboard Frame Strip de los 8 Sprites)
        // -------------------------------------------------------------
        var secStrip = CreateText(animCard.transform, "🎞️ Tira de Fotogramas (8 Sprites de la Secuencia) - Haz clic para saltar a un cuadro:", 12, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(secStrip.gameObject, prefH: 18);

        var stripRow = Row(animCard.transform, 6);
        Size(stripRow.gameObject, prefH: 70);

        for (int i = 0; i < 8; i++)
        {
            int frameIdx = i;
            var frameBox = Panel(stripRow.transform, ButtonNormal);
            frameBox.name = "FrameThumb_" + i;
            Flex(frameBox.gameObject, flexW: 1);

            var frameBtn = frameBox.gameObject.AddComponent<Button>();
            frameBtn.targetGraphic = frameBox;
            frameBtn.onClick.AddListener(() => _spriteAnimationLoop?.SetFrame(frameIdx));

            var fbV = frameBox.gameObject.AddComponent<VerticalLayoutGroup>();
            fbV.padding = new RectOffset(4, 4, 3, 3);
            fbV.spacing = 2;
            fbV.childControlWidth = true;
            fbV.childControlHeight = true;
            fbV.childForceExpandWidth = true;
            fbV.childForceExpandHeight = true;

            // Thumbnail Image
            var thumbImg = Panel(frameBox.transform, Color.white);
            thumbImg.sprite = _fireSprites[i];
            Flex(thumbImg.gameObject, flexH: 1);

            // Label #1 to #8
            var lbl = CreateText(frameBox.transform, $"#{i + 1}", 10, TextAnchor.MiddleCenter, (i == 0) ? AccentFlame : TextMuted, FontStyle.Bold);
            Size(lbl.gameObject, prefH: 14);

            _storyboardThumbnails[i] = thumbImg;
            _storyboardBorders[i] = frameBox;
            _storyboardLabels[i] = lbl;
        }

        // -------------------------------------------------------------
        // Controles de Reproducción y Modos de Bucle
        // -------------------------------------------------------------
        var rowAnimControls = Row(animCard.transform, 6);
        Size(rowAnimControls.gameObject, prefH: 34);

        var bPlayAnim = CreateButton(rowAnimControls.transform, "▶ Play", AccentSuccess, () => _spriteAnimationLoop?.Play());
        Flex(bPlayAnim.gameObject, flexW: 1);

        var bPauseAnim = CreateButton(rowAnimControls.transform, "⏸ Pausa", AccentWarning, () => _spriteAnimationLoop?.Pause());
        Flex(bPauseAnim.gameObject, flexW: 1);

        var bStopAnim = CreateButton(rowAnimControls.transform, "⏹ Stop (Reset)", ButtonNormal, () => _spriteAnimationLoop?.Stop());
        Flex(bStopAnim.gameObject, flexW: 1.1f);

        var bPrevFrame = CreateButton(rowAnimControls.transform, "◀ Cuadro Ant.", ButtonNormal, () => _spriteAnimationLoop?.StepBackward());
        Flex(bPrevFrame.gameObject, flexW: 1.1f);

        var bNextFrame = CreateButton(rowAnimControls.transform, "Cuadro Sig. ▶", ButtonNormal, () => _spriteAnimationLoop?.StepForward());
        Flex(bNextFrame.gameObject, flexW: 1.1f);

        // Modos de Bucle y Velocidad
        var rowModesAndFps = Row(animCard.transform, 6);
        Size(rowModesAndFps.gameObject, prefH: 34);

        CreateButton(rowModesAndFps.transform, "🔁 Bucle (Loop)", AccentFlame, () => _spriteAnimationLoop?.SetLoopMode(SpriteAnimationLoop.AnimationLoopMode.Loop));
        CreateButton(rowModesAndFps.transform, "🔀 Ping-Pong", ButtonNormal, () => _spriteAnimationLoop?.SetLoopMode(SpriteAnimationLoop.AnimationLoopMode.PingPong));
        CreateButton(rowModesAndFps.transform, "1️⃣ Una Vez", ButtonNormal, () => _spriteAnimationLoop?.SetLoopMode(SpriteAnimationLoop.AnimationLoopMode.Once));
        CreateButton(rowModesAndFps.transform, "🔄 Inverso", ButtonNormal, () => _spriteAnimationLoop?.SetLoopMode(SpriteAnimationLoop.AnimationLoopMode.Reverse));

        CreateSpacer(rowModesAndFps.transform, flexW: 0.5f);

        _animLoopCountText = CreateText(rowModesAndFps.transform, "Bucles: 0", 11, TextAnchor.MiddleRight, TextMuted);
        Size(_animLoopCountText.gameObject, prefW: 80);

        _animFpsText = CreateText(rowModesAndFps.transform, "Velocidad: 8 FPS", 11, TextAnchor.MiddleRight, AccentCyan, FontStyle.Bold);
        Size(_animFpsText.gameObject, prefW: 120);

        CreateButton(rowModesAndFps.transform, "4 FPS", ButtonNormal, () => SetAnimFps(4));
        CreateButton(rowModesAndFps.transform, "8 FPS", AccentPrimary, () => SetAnimFps(8));
        CreateButton(rowModesAndFps.transform, "12 FPS", ButtonNormal, () => SetAnimFps(12));
        CreateButton(rowModesAndFps.transform, "24 FPS", ButtonNormal, () => SetAnimFps(24));
        CreateButton(rowModesAndFps.transform, "60 FPS", ButtonNormal, () => SetAnimFps(60));

        // Resaltar el primer frame inicial
        HighlightActiveFrame(0);

        return page.gameObject;
    }

    void Select8SpriteSequence(int sequenceIndex)
    {
        if (_spriteAnimationLoop == null) return;

        Sprite[] selectedSet = _fireSprites;
        string name = "Fuego Mágico (8 Sprites)";
        Color themeColor = AccentFlame;

        switch (sequenceIndex)
        {
            case 0:
                selectedSet = _fireSprites;
                name = "🔥 Fuego Mágico (8 Sprites en Bucle)";
                themeColor = AccentFlame;
                break;
            case 1:
                selectedSet = _walkSprites;
                name = "🏃‍♂️ Ciclo de Caminata (8 Sprites en Bucle)";
                themeColor = AccentPrimary;
                break;
            case 2:
                selectedSet = _gemSprites;
                name = "💎 Gema 3D Giratoria (8 Sprites en Bucle)";
                themeColor = AccentPurple;
                break;
            case 3:
                selectedSet = _energySprites;
                name = "⚡ Orbe de Energía (8 Sprites en Bucle)";
                themeColor = AccentCyan;
                break;
        }

        _spriteAnimationLoop.SetSpriteFrames(selectedSet, name);
        if (_animSequenceTitle != null) _animSequenceTitle.text = name;

        // Actualizar miniaturas de la tira
        for (int i = 0; i < 8; i++)
        {
            if (_storyboardThumbnails[i] != null && i < selectedSet.Length)
            {
                _storyboardThumbnails[i].sprite = selectedSet[i];
            }
        }

        HighlightActiveFrame(0);
        AddLog($"🎬 Secuencia cambiada a: {name}");
    }

    void OnAnimationFrameChanged(int frameIndex, Sprite sprite)
    {
        if (_animActiveFrameBadge != null)
        {
            _animActiveFrameBadge.text = $"CUADRO ACTIVO: #{frameIndex + 1} / 8";
        }

        HighlightActiveFrame(frameIndex);
    }

    void HighlightActiveFrame(int activeIndex)
    {
        for (int i = 0; i < 8; i++)
        {
            bool isActive = (i == activeIndex);
            if (_storyboardBorders[i] != null)
            {
                _storyboardBorders[i].color = isActive ? new Color(0.24f, 0.51f, 0.98f, 1f) : ButtonNormal;
            }
            if (_storyboardLabels[i] != null)
            {
                _storyboardLabels[i].color = isActive ? AccentFlame : TextMuted;
            }
        }
    }

    void SetAnimFps(float fps)
    {
        if (_spriteAnimationLoop != null)
        {
            _spriteAnimationLoop.SetFramesPerSecond(fps);
        }
        if (_animFpsText != null)
        {
            _animFpsText.text = $"Velocidad: {fps:F0} FPS";
        }
    }

    void SetGameTimeScale(float scale)
    {
        _gameLoopController?.SetTimeScale(scale);
        if (_loopTimeScaleText != null)
        {
            _loopTimeScaleText.text = $"{scale:F2}x";
        }
    }

    void UpdateLoopStateUI(GameLoopController.GameLoopState state)
    {
        if (_loopStateBadge == null) return;

        switch (state)
        {
            case GameLoopController.GameLoopState.Running:
                _loopStateBadge.text = "🟢 EJECUCIÓN (RUNNING)";
                _loopStateBadge.color = AccentSuccess;
                break;
            case GameLoopController.GameLoopState.Paused:
                _loopStateBadge.text = "⏸️ PAUSADO (PAUSED)";
                _loopStateBadge.color = AccentWarning;
                break;
            case GameLoopController.GameLoopState.Stepping:
                _loopStateBadge.text = "⏯️ PASO A PASO (STEPPING)";
                _loopStateBadge.color = AccentCyan;
                break;
            case GameLoopController.GameLoopState.Stopped:
                _loopStateBadge.text = "⏹️ DETENIDO (STOPPED)";
                _loopStateBadge.color = AccentDanger;
                break;
        }
    }

    void OnLoopTelemetryReceived(GameLoopController.GameLoopTelemetry tele)
    {
        if (_loopTelemetryFps != null)
        {
            _loopTelemetryFps.text = $"• FPS Actual: <b>{tele.Fps:F1} FPS</b>";
        }
        if (_loopTelemetryDelta != null)
        {
            _loopTelemetryDelta.text = $"• DeltaTime (ms/cuadro): <b>{tele.DeltaTimeMs:F1} ms</b>";
        }
        if (_loopTelemetryFrames != null)
        {
            _loopTelemetryFrames.text = $"• Total Cuadros Procesados: <b>{tele.TotalFrames}</b>";
        }
        if (_loopTelemetryTime != null)
        {
            _loopTelemetryTime.text = $"• Tiempo de Juego: <b>{tele.GameTime:F1}s</b>  |  Real: <b>{tele.RealTime:F1}s</b>";
        }
    }

    // ==========================================
    // Footer / Barra Inferior de Navegación (6 Dots)
    // ==========================================
    void BuildFooter(Transform parent)
    {
        var footerPanel = Panel(parent, CardHeaderBg);
        footerPanel.name = "FooterNavBar";
        Size(footerPanel.gameObject, prefH: 58);

        var hLayout = footerPanel.gameObject.AddComponent<HorizontalLayoutGroup>();
        hLayout.padding = new RectOffset(16, 16, 6, 6);
        hLayout.spacing = 14;
        hLayout.childControlWidth = false;
        hLayout.childControlHeight = true;
        hLayout.childForceExpandWidth = false;
        hLayout.childForceExpandHeight = true;
        hLayout.childAlignment = TextAnchor.MiddleLeft;

        // Botón Anterior
        var btnPrev = CreateIconButton(footerPanel.transform, "◀ Anterior", ButtonNormal, 115, 40, () => _navigationController.PreviousPage());
        btnPrev.name = "BtnNavPrev";
        _navigationController.btnPrev = btnPrev;

        // Indicadores circulares (Dots) de página (6 Dots)
        var dotsContainer = Row(footerPanel.transform, 6);
        Size(dotsContainer.gameObject, prefW: 130, prefH: 40);
        dotsContainer.childAlignment = TextAnchor.MiddleCenter;

        var dotIndicators = new Image[6];
        for (int i = 0; i < 6; i++)
        {
            int pageIdx = i;
            var dot = Panel(dotsContainer.transform, TextDimmed);
            dot.name = "Dot_" + i;
            dot.sprite = _circleSprite;
            Size(dot.gameObject, prefW: 10, prefH: 10);
            dotIndicators[i] = dot;
        }
        _navigationController.dotIndicators = dotIndicators;

        // Botón Siguiente
        var btnNext = CreateIconButton(footerPanel.transform, "Siguiente ▶", AccentPrimary, 115, 40, () => _navigationController.NextPage());
        btnNext.name = "BtnNavNext";
        _navigationController.btnNext = btnNext;

        // Línea divisoria
        var div = Panel(footerPanel.transform, BorderColor);
        Size(div.gameObject, prefW: 1, prefH: 30);

        // Barra de estado interactiva
        _statusText = CreateText(footerPanel.transform, "💡 Interactúa con los botones para probar el bucle de videojuego, los 8 sprites animados, arrastre y fuentes.", 12, TextAnchor.MiddleLeft, TextMuted, FontStyle.Italic);
        Flex(_statusText.gameObject, flexW: 1);

        // Botón Inicio Rápido
        var btnHome = CreateIconButton(footerPanel.transform, "🏠 Inicio", ButtonNormal, 90, 40, () => _navigationController.GoHome());
        btnHome.name = "BtnNavHome";
        _navigationController.btnHome = btnHome;
    }

    // ==========================================
    // Helpers de Control y Métricas
    // ==========================================
    void ApplyDevicePreset(int index)
    {
        if (index < 0 || index >= _devicePresets.Length) return;
        var p = _devicePresets[index];

        _canvasScaler.referenceResolution = p.Resolution;
        _canvasScaler.matchWidthOrHeight = (p.Resolution.y > p.Resolution.x) ? 0f : 0.5f;

        AddLog($"📱 Pantalla adaptada a: {p.Name} ({p.Resolution.x}x{p.Resolution.y})");
        if (_statusText != null) _statusText.text = $"📱 Simulación: {p.Name} ({p.AspectRatio})";
    }

    void UpdateLiveMetrics()
    {
        if (_resolutionInfoText == null) return;

        float width = Screen.width;
        float height = Screen.height;
        float ratio = height > 0 ? (width / height) : 1f;
        string orientation = (width >= height) ? "Horizontal (Landscape)" : "Vertical (Portrait)";
        float scale = _canvas != null ? _canvas.scaleFactor : 1f;

        _resolutionInfoText.text =
            $"• <b>Resolución Actual:</b> {(int)width} × {(int)height} px\n" +
            $"• <b>Relación de Aspecto:</b> {ratio:F2}:1 ({orientation})\n" +
            $"• <b>Factor de Escala UI:</b> {scale:F2}x\n" +
            $"• <b>Páginas Registradas:</b> {_navigationController.PageCount}\n" +
            $"• <b>Página Actual:</b> #{_navigationController.CurrentPageIndex + 1}\n" +
            $"• <b>FPS en Vivo:</b> {(_gameLoopController != null ? _gameLoopController.CurrentFps : 60f):F0} FPS\n" +
            $"• <b>Historial Back:</b> {(_navigationController.CanGoBack ? "Disponible" : "Vacío")}";
    }

    void AddLog(string message)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        string entry = $"<color=#3D82FF>[{timestamp}]</color> {message}";
        _logEntries.Insert(0, entry);

        if (_logEntries.Count > 14)
        {
            _logEntries.RemoveAt(_logEntries.Count - 1);
        }

        if (_logConsoleContent != null)
        {
            _logConsoleContent.text = string.Join("\n", _logEntries);
        }
    }

    // ==========================================
    // Generadores Visuales y Helpers UI
    // ==========================================
    void CreateModuleCard(Transform parent, string title, string description, Color accent, string btnLabel, Action onAction)
    {
        var card = Panel(parent, CardBg);
        card.name = "Card_" + title;
        Flex(card.gameObject, flexW: 1, flexH: 1);

        var cardV = card.gameObject.AddComponent<VerticalLayoutGroup>();
        cardV.padding = new RectOffset(16, 16, 14, 14);
        cardV.spacing = 6;
        cardV.childControlWidth = true;
        cardV.childControlHeight = true;
        cardV.childForceExpandWidth = true;
        cardV.childForceExpandHeight = false;

        var topBadge = CreateBadge(card.transform, "MÓDULO INTERACTIVO", accent);
        Size(topBadge.gameObject, prefH: 20);

        var t = CreateText(card.transform, title, 14, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(t.gameObject, prefH: 20);

        var d = CreateText(card.transform, description, 11, TextAnchor.UpperLeft, TextMuted);
        Flex(d.gameObject, flexH: 1);

        var b = CreateButton(card.transform, btnLabel, accent, onAction);
        Size(b.gameObject, prefH: 32);
    }

    Image CreateBadge(Transform parent, string text, Color color, out Text labelText)
    {
        var badge = Panel(parent, new Color(color.r, color.g, color.b, 0.22f));
        badge.name = "Badge_" + text;
        labelText = CreateText(badge.transform, text, 10, TextAnchor.MiddleCenter, color, FontStyle.Bold);
        Stretch(labelText.rectTransform, Vector2.zero, Vector2.one);
        return badge;
    }

    Image CreateBadge(Transform parent, string text, Color color)
    {
        return CreateBadge(parent, text, color, out _);
    }

    Button CreateIconButton(Transform parent, string label, Color bg, float width, float height, Action onClick)
    {
        var btn = CreateButton(parent, label, bg, onClick);
        Size(btn.gameObject, prefW: width, prefH: height);
        return btn;
    }

    Button CreateButton(Transform parent, string label, Color bg, Action onClick = null)
    {
        var img = Panel(parent, bg);
        img.name = "Btn_" + label;
        var btn = img.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;

        var colors = btn.colors;
        colors.normalColor = bg;
        colors.highlightedColor = new Color(Mathf.Min(bg.r + 0.12f, 1f), Mathf.Min(bg.g + 0.12f, 1f), Mathf.Min(bg.b + 0.12f, 1f), 1f);
        colors.pressedColor = new Color(Mathf.Max(bg.r - 0.1f, 0f), Mathf.Max(bg.g - 0.1f, 0f), Mathf.Max(bg.b - 0.1f, 0f), 1f);
        btn.colors = colors;

        if (onClick != null)
        {
            btn.onClick.AddListener(() => onClick());
        }

        var t = CreateText(img.transform, label, 12, TextAnchor.MiddleCenter, TextWhite, FontStyle.Bold);
        Stretch(t.rectTransform, Vector2.zero, Vector2.one);
        return btn;
    }

    Button CreateColorButton(Transform parent, Color color, Action onClick)
    {
        var img = Panel(parent, color);
        img.name = "ColorBtn";
        Size(img.gameObject, prefW: 28, prefH: 28);
        var btn = img.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        if (onClick != null) btn.onClick.AddListener(() => onClick());
        return btn;
    }

    Image Panel(Transform parent, Color color)
    {
        var go = new GameObject("Panel", typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.sprite = _roundedSprite;
        img.type = Image.Type.Sliced;
        img.color = color;
        return img;
    }

    VerticalLayoutGroup Column(Transform parent, float spacing)
    {
        var go = new GameObject("Column", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var v = go.AddComponent<VerticalLayoutGroup>();
        v.spacing = spacing;
        v.childControlWidth = true;
        v.childControlHeight = true;
        v.childForceExpandWidth = true;
        v.childForceExpandHeight = false;
        return v;
    }

    HorizontalLayoutGroup Row(Transform parent, float spacing)
    {
        var go = new GameObject("Row", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var h = go.AddComponent<HorizontalLayoutGroup>();
        h.spacing = spacing;
        h.childControlWidth = true;
        h.childControlHeight = true;
        h.childForceExpandWidth = false;
        h.childForceExpandHeight = true;
        h.childAlignment = TextAnchor.MiddleLeft;
        return h;
    }

    Text CreateText(Transform parent, string txt, int size, TextAnchor anchor, Color color, FontStyle style = FontStyle.Normal)
    {
        var go = new GameObject("Text", typeof(Text));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.text = txt;
        t.fontSize = size;
        t.fontStyle = style;
        t.alignment = anchor;
        t.color = color;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Truncate;
        return t;
    }

    GameObject CreateSpacer(Transform parent, float flexW = -1, float flexH = -1)
    {
        var sp = new GameObject("Spacer", typeof(RectTransform));
        sp.transform.SetParent(parent, false);
        Flex(sp, flexW, flexH);
        return sp;
    }

    static void Stretch(RectTransform rt, Vector2 min, Vector2 max)
    {
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void Size(GameObject go, float prefW = -1, float prefH = -1)
    {
        var le = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
        if (prefW >= 0) le.preferredWidth = prefW;
        if (prefH >= 0) le.preferredHeight = prefH;
    }

    static void Flex(GameObject go, float flexW = -1, float flexH = -1)
    {
        var le = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
        if (flexW >= 0) le.flexibleWidth = flexW;
        if (flexH >= 0) le.flexibleHeight = flexH;
    }

    static void EnsureEventSystem()
    {
        if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() != null) return;
        var go = new GameObject("EventSystem", typeof(EventSystem));
        go.AddComponent<InputSystemUIInputModule>();
    }

    // ==========================================
    // Generadores Procedurales de Texturas y Formas Básicas
    // ==========================================
    void GenerateProceduralSprites()
    {
        _roundedSprite = MakeRoundedTexture(64, 16);
        _circleSprite  = MakeCircleTexture(64);
        _diamondSprite = MakeDiamondTexture(64);
        _starSprite    = MakeStarTexture(64);
        _shieldSprite  = MakeShieldTexture(64);
    }

    // ==========================================
    // Generadores Procedurales de los 4 Sets de Ocho (8) Sprites
    // ==========================================
    void Generate8SpriteSets()
    {
        // 1. 🔥 Fuego / Hoguera Mágica (8 cuadros de llama oscilante y ascuas)
        for (int i = 0; i < 8; i++)
        {
            _fireSprites[i] = MakeFlameFrameTexture(64, i, 8);
        }

        // 2. 🏃‍♂️ Ciclo de Caminata / Walk Cycle (8 cuadros de movimiento)
        for (int i = 0; i < 8; i++)
        {
            _walkSprites[i] = MakeWalkFrameTexture(64, i, 8);
        }

        // 3. 💎 Gema / Moneda 3D Giratoria (8 fases de rotación)
        for (int i = 0; i < 8; i++)
        {
            _gemSprites[i] = MakeGem3DFrameTexture(64, i, 8);
        }

        // 4. ⚡ Orbe de Energía / Pulso Mágico (8 fases de pulso y plasma)
        for (int i = 0; i < 8; i++)
        {
            _energySprites[i] = MakeEnergyFrameTexture(64, i, 8);
        }
    }

    /// <summary>
    /// Genera 1 cuadro de llama de fuego de 8 cuadros (animación en bucle estilo tutorial).
    /// </summary>
    static Sprite MakeFlameFrameTexture(int size, int frameIndex, int totalFrames)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        float progress = (float)frameIndex / totalFrames;
        float angle = progress * Mathf.PI * 2f;

        float flameHeightOffset = Mathf.Sin(angle) * 4f;
        float flameTipCurve = Mathf.Cos(angle) * 7f;
        float flameSecondaryCurve = Mathf.Sin(angle * 2f) * 3f;

        Vector2 baseCenter = new Vector2(size * 0.5f, size * 0.20f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float ny = (float)y / size;
                if (ny < 0.12f || ny > 0.88f)
                {
                    tex.SetPixel(x, y, Color.clear);
                    continue;
                }

                // Curva de la llama según la altura
                float curve = (ny - 0.15f) * (flameTipCurve + flameSecondaryCurve * (1f - ny));
                float cx = size * 0.5f + curve;
                float distH = Mathf.Abs(x - cx);

                // Ancho de la llama según la altura
                float maxHalfWidth = (1f - Mathf.Pow((ny - 0.15f) / (0.75f + flameHeightOffset * 0.01f), 1.3f)) * (size * 0.36f);

                if (distH <= maxHalfWidth && maxHalfWidth > 0f)
                {
                    float factor = 1f - (distH / maxHalfWidth);
                    float coreFactor = Mathf.Clamp01((1f - (distH / (maxHalfWidth * 0.45f))) * (1.2f - ny));

                    // Color degradado: Núcleo amarillo brillante -> Cuerpo naranja -> Borde rojo fuego
                    Color col;
                    if (coreFactor > 0.4f)
                    {
                        col = Color.Lerp(new Color(1f, 0.6f, 0.1f, 1f), new Color(1f, 0.95f, 0.4f, 1f), (coreFactor - 0.4f) / 0.6f);
                    }
                    else
                    {
                        col = Color.Lerp(new Color(0.9f, 0.15f, 0.05f, 0.95f), new Color(1f, 0.6f, 0.1f, 1f), factor);
                    }

                    // Transparencia suave en los bordes
                    float alpha = Mathf.Clamp01((maxHalfWidth - distH) * 1.5f);
                    col.a *= alpha;
                    tex.SetPixel(x, y, col);
                }
                else
                {
                    // Ascuas flotantes animadas
                    float emberY1 = ((progress + 0.2f) % 1f) * size * 0.8f + size * 0.15f;
                    float emberX1 = size * 0.5f + Mathf.Sin(progress * 8f) * 12f;
                    float distE1 = Vector2.Distance(new Vector2(x, y), new Vector2(emberX1, emberY1));

                    float emberY2 = ((progress + 0.65f) % 1f) * size * 0.75f + size * 0.2f;
                    float emberX2 = size * 0.5f + Mathf.Cos(progress * 6f) * 14f;
                    float distE2 = Vector2.Distance(new Vector2(x, y), new Vector2(emberX2, emberY2));

                    if (distE1 < 2.2f)
                    {
                        tex.SetPixel(x, y, new Color(1f, 0.85f, 0.2f, 1f - distE1 / 2.2f));
                    }
                    else if (distE2 < 1.8f)
                    {
                        tex.SetPixel(x, y, new Color(1f, 0.5f, 0.1f, 1f - distE2 / 1.8f));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    }

    /// <summary>
    /// Genera 1 cuadro del ciclo de caminata (Walk Cycle) de 8 fotogramas.
    /// </summary>
    static Sprite MakeWalkFrameTexture(int size, int frameIndex, int totalFrames)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, Color.clear);

        float phase = (float)frameIndex / totalFrames * Mathf.PI * 2f;
        float bob = Mathf.Abs(Mathf.Sin(phase * 2f)) * 3f; // Rebote vertical del cuerpo
        float legSwing1 = Mathf.Sin(phase) * 14f;
        float legSwing2 = -Mathf.Sin(phase) * 14f;
        float armSwing1 = -Mathf.Sin(phase) * 12f;
        float armSwing2 = Mathf.Sin(phase) * 12f;

        Vector2 headCenter = new Vector2(size * 0.5f, size * 0.72f + bob);
        Vector2 bodyCenter = new Vector2(size * 0.5f, size * 0.50f + bob);
        Vector2 hipCenter  = new Vector2(size * 0.5f, size * 0.38f + bob);

        // Dibujar Cabeza (Círculo cian/azul)
        DrawCircleOnTex(tex, headCenter, 8f, AccentCyan);

        // Dibujar Torso (Cuerpo atlético)
        DrawCapsuleOnTex(tex, headCenter - new Vector2(0, 8), hipCenter, 6f, AccentPrimary);

        // Dibujar Brazo Trasero
        Vector2 armBackEnd = bodyCenter + new Vector2(armSwing2, -10f);
        DrawCapsuleOnTex(tex, bodyCenter, armBackEnd, 3f, new Color(0.18f, 0.38f, 0.75f, 0.85f));

        // Dibujar Pierna Trasera
        Vector2 legBackEnd = hipCenter + new Vector2(legSwing2, -16f);
        DrawCapsuleOnTex(tex, hipCenter, legBackEnd, 4f, new Color(0.15f, 0.35f, 0.70f, 0.9f));

        // Dibujar Pierna Delantera
        Vector2 legFrontEnd = hipCenter + new Vector2(legSwing1, -16f);
        DrawCapsuleOnTex(tex, hipCenter, legFrontEnd, 4.5f, AccentSuccess);

        // Dibujar Brazo Delantero
        Vector2 armFrontEnd = bodyCenter + new Vector2(armSwing1, -10f);
        DrawCapsuleOnTex(tex, bodyCenter, armFrontEnd, 3.5f, TextWhite);

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    }

    /// <summary>
    /// Genera 1 cuadro de Gema 3D giratoria de 8 fases (rotación 360°).
    /// </summary>
    static Sprite MakeGem3DFrameTexture(int size, int frameIndex, int totalFrames)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, Color.clear);

        float angle = (float)frameIndex / totalFrames * Mathf.PI * 2f;
        float widthScale = Mathf.Abs(Mathf.Cos(angle)) * 0.7f + 0.3f;
        float shineOffset = Mathf.Sin(angle) * (size * 0.25f);

        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float halfH = size * 0.38f;
        float halfW = size * 0.36f * widthScale;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Abs(x - center.x);
                float dy = Mathf.Abs(y - center.y);

                // Forma de rombo facetado
                if ((dx / Mathf.Max(halfW, 1f) + dy / halfH) <= 1.0f)
                {
                    // Lado izquierdo vs derecho según rotación
                    bool isLeftFacet = (x < center.x);
                    Color facetBase = isLeftFacet ? AccentPurple : new Color(0.78f, 0.50f, 1.0f, 1f);

                    // Destello de brillo especular
                    float shineDist = Mathf.Abs((x - center.x) - shineOffset) + Mathf.Abs(y - (center.y + halfH * 0.3f));
                    if (shineDist < 6f)
                    {
                        facetBase = Color.Lerp(facetBase, TextWhite, (6f - shineDist) / 6f);
                    }

                    tex.SetPixel(x, y, facetBase);
                }
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    }

    /// <summary>
    /// Genera 1 cuadro del Orbe de Energía / Pulso Mágico de 8 cuadros.
    /// </summary>
    static Sprite MakeEnergyFrameTexture(int size, int frameIndex, int totalFrames)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        float progress = (float)frameIndex / totalFrames;
        float angle = progress * Mathf.PI * 2f;

        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float pulseRadius = size * 0.22f + Mathf.Sin(angle) * 5f;
        float ringRadius  = size * 0.36f + Mathf.Cos(angle) * 4f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);

                if (dist <= pulseRadius)
                {
                    // Núcleo brillante
                    float coreFactor = 1f - (dist / pulseRadius);
                    Color coreCol = Color.Lerp(AccentCyan, TextWhite, Mathf.Pow(coreFactor, 2f));
                    tex.SetPixel(x, y, coreCol);
                }
                else if (Mathf.Abs(dist - ringRadius) < 3.5f)
                {
                    // Anillo exterior de plasma
                    float ringFactor = 1f - (Mathf.Abs(dist - ringRadius) / 3.5f);
                    Color ringCol = new Color(AccentCyan.r, AccentCyan.g, AccentCyan.b, ringFactor * 0.8f);
                    tex.SetPixel(x, y, ringCol);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }

        // Partículas orbitales de energía (4 chispas girando)
        for (int p = 0; p < 4; p++)
        {
            float sparkAngle = angle + (p * Mathf.PI * 0.5f);
            Vector2 sparkPos = center + new Vector2(Mathf.Cos(sparkAngle), Mathf.Sin(sparkAngle)) * ringRadius;
            DrawCircleOnTex(tex, sparkPos, 2.5f, TextWhite);
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    }

    static void DrawCircleOnTex(Texture2D tex, Vector2 center, float radius, Color color)
    {
        int minX = Mathf.Max(0, (int)(center.x - radius - 1));
        int maxX = Mathf.Min(tex.width - 1, (int)(center.x + radius + 1));
        int minY = Mathf.Max(0, (int)(center.y - radius - 1));
        int maxY = Mathf.Min(tex.height - 1, (int)(center.y + radius + 1));

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), center);
                if (d <= radius)
                {
                    float alpha = Mathf.Clamp01(radius - d + 0.5f);
                    Color c = color;
                    c.a *= alpha;
                    tex.SetPixel(x, y, c);
                }
            }
        }
    }

    static void DrawCapsuleOnTex(Texture2D tex, Vector2 p1, Vector2 p2, float radius, Color color)
    {
        int steps = Mathf.Max(6, (int)Vector2.Distance(p1, p2));
        for (int i = 0; i <= steps; i++)
        {
            Vector2 p = Vector2.Lerp(p1, p2, (float)i / steps);
            DrawCircleOnTex(tex, p, radius, color);
        }
    }

    static Sprite MakeRoundedTexture(int size = 64, int radius = 16)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        float b = size * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float sd = RoundedSdf(x + 0.5f - b, y + 0.5f - b, b, b, radius);
                float a = Mathf.Clamp01(0.5f - sd);
                tex.SetPixel(x, y, new Color(1, 1, 1, a));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
    }

    static Sprite MakeCircleTexture(int size = 64)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        float radius = size * 0.5f;
        Vector2 center = new Vector2(radius, radius);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                float a = Mathf.Clamp01(radius - dist + 0.5f);
                tex.SetPixel(x, y, new Color(1, 1, 1, a));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    }

    static Sprite MakeDiamondTexture(int size = 64)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        float half = size * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Mathf.Abs(x + 0.5f - half) + Mathf.Abs(y + 0.5f - half);
                float a = Mathf.Clamp01(half - d + 0.5f);
                tex.SetPixel(x, y, new Color(1, 1, 1, a));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    }

    static Sprite MakeStarTexture(int size = 64)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float rOuter = size * 0.46f;
        float rInner = size * 0.20f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 p = new Vector2(x + 0.5f, y + 0.5f) - center;
                float angle = Mathf.Atan2(p.y, p.x) + Mathf.PI * 0.5f;
                float dist = p.magnitude;
                float arms = 5f;
                float mod = (angle % (2f * Mathf.PI / arms) + 2f * Mathf.PI) % (2f * Mathf.PI / arms);
                float halfArm = Mathf.PI / arms;
                float t = Mathf.Abs(mod - halfArm) / halfArm;
                float maxR = Mathf.Lerp(rInner, rOuter, 1f - t);
                float a = Mathf.Clamp01(maxR - dist + 0.5f);
                tex.SetPixel(x, y, new Color(1, 1, 1, a));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    }

    static Sprite MakeShieldTexture(int size = 64)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        float half = size * 0.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = Mathf.Abs((x + 0.5f - half) / half);
                float ny = (y + 0.5f) / size;
                bool inShield = ny >= 0.5f ? (nx <= 0.85f) : (nx <= (ny / 0.5f) * 0.85f);
                tex.SetPixel(x, y, new Color(1, 1, 1, inShield ? 1f : 0f));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    }

    static float RoundedSdf(float px, float py, float bx, float by, float r)
    {
        float qx = Mathf.Abs(px) - bx + r;
        float qy = Mathf.Abs(py) - by + r;
        float outside = Mathf.Sqrt(Mathf.Max(qx, 0) * Mathf.Max(qx, 0) + Mathf.Max(qy, 0) * Mathf.Max(qy, 0));
        return Mathf.Min(Mathf.Max(qx, qy), 0f) + outside - r;
    }
}
