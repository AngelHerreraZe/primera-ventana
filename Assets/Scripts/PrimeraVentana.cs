using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

/// <summary>
/// Aplicación Interactiva Responsiva y Modular para Unity.
/// Cumple con todos los criterios:
/// 1. Arrastre de Sprites funcional mediante eventos y botones de control.
/// 2. Control interactivo de fuentes tipográficas (familias, tamaños, estilos, colores y efectos).
/// 3. Fijación de scripts a GameObjects en tiempo de ejecución (AddComponent) y navegación funcional con historial, migas de pan y pestañas.
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
    static readonly Color TextWhite       = new Color(0.98f, 0.99f, 1.00f, 1f);
    static readonly Color TextMuted       = new Color(0.60f, 0.65f, 0.75f, 1f);
    static readonly Color TextDimmed      = new Color(0.40f, 0.44f, 0.54f, 1f);
    static readonly Color BorderColor     = new Color(0.22f, 0.28f, 0.38f, 0.7f);
    static readonly Color ButtonNormal    = new Color(0.18f, 0.23f, 0.33f, 1f);
    static readonly Color ButtonHover     = new Color(0.26f, 0.33f, 0.46f, 1f);

    // ==========================================
    // Sprites Procedurales
    // ==========================================
    Sprite _roundedSprite;
    Sprite _circleSprite;
    Sprite _diamondSprite;
    Sprite _starSprite;
    Sprite _shieldSprite;

    // ==========================================
    // Componentes del Sistema Principal
    // ==========================================
    Canvas _canvas;
    CanvasScaler _canvasScaler;
    NavigationController _navigationController;
    ScriptBinder _scriptBinder;

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

    // Puntos de Anclaje
    RectTransform _anchorTargetPin;
    Text _anchorDetailsText;

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
        EnsureEventSystem();
        BuildUI();
    }

    void Start()
    {
        AddLog("🚀 Sistema interactivo de Unity inicializado correctamente.");
        AddLog("✓ Módulo 'Arrastrar Sprite' configurado con soporte para eventos y botones.");
        AddLog("✓ Módulo 'Controlar Fuentes' vinculado con FontController.");
        AddLog("✓ Módulo 'Fijar Scripts y Navegación' activo con NavigationController y ScriptBinder.");

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

        _navigationController.OnStatusLog = msg => AddLog(msg);
        _scriptBinder.OnStatusLog = msg => AddLog(msg);
        _scriptBinder.OnComponentsChanged = RefreshAttachedScriptsDisplay;

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

        // 5. Header / Barra Superior con Logo, Breadcrumbs y Pestañas
        BuildHeader(masterContainer.transform);

        // 6. Área de Contenido Central (Páginas)
        var contentArea = Panel(masterContainer.transform, Color.clear);
        contentArea.name = "ContentArea";
        Flex(contentArea.gameObject, flexH: 1);

        // Construcción de las 5 Páginas Principales
        var page0 = BuildPageInicio(contentArea.transform);
        var page1 = BuildPageArrastrarSprite(contentArea.transform);
        var page2 = BuildPageControlFuentes(contentArea.transform);
        var page3 = BuildPageFijarScripts(contentArea.transform);
        var page4 = BuildPageNavegacionYDispositivos(contentArea.transform);

        // Registrar páginas en NavigationController
        _navigationController.RegisterPage("inicio", "Inicio", "🏠", page0, _navTabButtons[0], _navTabIndicators[0]);
        _navigationController.RegisterPage("sprites", "Arrastrar Sprite", "🎯", page1, _navTabButtons[1], _navTabIndicators[1]);
        _navigationController.RegisterPage("fuentes", "Control de Fuentes", "🔤", page2, _navTabButtons[2], _navTabIndicators[2]);
        _navigationController.RegisterPage("scripts", "Fijar Scripts", "🧩", page3, _navTabButtons[3], _navTabIndicators[3]);
        _navigationController.RegisterPage("navegacion", "Navegación & Pantalla", "🧭", page4, _navTabButtons[4], _navTabIndicators[4]);

        // 7. Footer / Barra Inferior de Navegación y Estado
        BuildFooter(masterContainer.transform);
    }

    // ==========================================
    // 1. Header y Barra de Navegación Superior
    // ==========================================
    Button[] _navTabButtons = new Button[5];
    Image[] _navTabIndicators = new Image[5];

    void BuildHeader(Transform parent)
    {
        var headerPanel = Panel(parent, CardHeaderBg);
        headerPanel.name = "HeaderNavBar";
        Size(headerPanel.gameObject, prefH: 72);

        var hLayout = headerPanel.gameObject.AddComponent<HorizontalLayoutGroup>();
        hLayout.padding = new RectOffset(16, 16, 8, 8);
        hLayout.spacing = 12;
        hLayout.childControlWidth = false;
        hLayout.childControlHeight = true;
        hLayout.childForceExpandWidth = false;
        hLayout.childForceExpandHeight = true;
        hLayout.childAlignment = TextAnchor.MiddleLeft;

        // Logo
        var logoBox = Panel(headerPanel.transform, AccentPrimary);
        logoBox.name = "AppLogo";
        Size(logoBox.gameObject, prefW: 48, prefH: 48);
        var logoText = CreateText(logoBox.transform, "✦", 24, TextAnchor.MiddleCenter, TextWhite, FontStyle.Bold);
        Stretch(logoText.rectTransform, Vector2.zero, Vector2.one);

        // Título y Migas de Pan (Breadcrumbs)
        var titleCol = Column(headerPanel.transform, 2);
        Size(titleCol.gameObject, prefW: 270, prefH: 54);
        titleCol.childAlignment = TextAnchor.MiddleLeft;

        var titleText = CreateText(titleCol.transform, "UNITY INTERACTIVO", 16, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(titleText.gameObject, prefH: 22);

        _breadcrumbsText = CreateText(titleCol.transform, "Inicio", 11, TextAnchor.MiddleLeft, AccentCyan);
        Size(_breadcrumbsText.gameObject, prefH: 18);
        _navigationController.txtBreadcrumbs = _breadcrumbsText;

        // Botones de Historial Rápido (Back / Forward)
        var btnBack = CreateIconButton(headerPanel.transform, "◀", ButtonNormal, 42, 42, () => _navigationController.GoBack());
        btnBack.name = "BtnQuickBack";
        _navigationController.btnBack = btnBack;

        var btnForward = CreateIconButton(headerPanel.transform, "▶", ButtonNormal, 42, 42, () => _navigationController.GoForward());
        btnForward.name = "BtnQuickForward";
        _navigationController.btnForward = btnForward;

        CreateSpacer(headerPanel.transform, flexW: 1);

        // Pestañas Principales (5 Pestañas)
        string[] tabLabels = { "🏠 Inicio", "🎯 Arrastrar Sprite", "🔤 Control Fuentes", "🧩 Fijar Scripts", "🧭 Navegación" };

        for (int i = 0; i < tabLabels.Length; i++)
        {
            var tabBtnGo = Panel(headerPanel.transform, ButtonNormal);
            tabBtnGo.name = "TabBtn_" + i;
            Size(tabBtnGo.gameObject, prefW: 155, prefH: 46);

            var btn = tabBtnGo.gameObject.AddComponent<Button>();
            btn.targetGraphic = tabBtnGo;

            var tabVGroup = tabBtnGo.gameObject.AddComponent<VerticalLayoutGroup>();
            tabVGroup.padding = new RectOffset(8, 8, 4, 3);
            tabVGroup.spacing = 2;
            tabVGroup.childControlWidth = true;
            tabVGroup.childControlHeight = true;
            tabVGroup.childForceExpandWidth = true;
            tabVGroup.childForceExpandHeight = true;

            var label = CreateText(tabBtnGo.transform, tabLabels[i], 12, TextAnchor.MiddleCenter, TextWhite, FontStyle.Bold);
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
        var leftCol = Column(page.transform, 12);
        Flex(leftCol.gameObject, flexW: 2.2f);

        // Hero Card
        var heroCard = Panel(leftCol.transform, CardBg);
        Size(heroCard.gameObject, prefH: 155);
        var heroV = heroCard.gameObject.AddComponent<VerticalLayoutGroup>();
        heroV.padding = new RectOffset(20, 20, 16, 16);
        heroV.spacing = 6;
        heroV.childControlWidth = true;
        heroV.childControlHeight = true;
        heroV.childForceExpandWidth = true;
        heroV.childForceExpandHeight = false;

        var badge = CreateBadge(heroCard.transform, "SISTEMA INTEGRAL DE BOTONES Y COMPONENTES EN UNITY", AccentSuccess);
        Size(badge.gameObject, prefH: 24);

        var heroTitle = CreateText(heroCard.transform, "Controlador Interactivo de Sprites, Fuentes, Scripts y Navegación", 19, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(heroTitle.gameObject, prefH: 28);

        var heroDesc = CreateText(heroCard.transform,
            "Selecciona cualquiera de los módulos interactivos para experimentar el arrastre táctil de sprites, el formateo en vivo de fuentes tipográficas, la fijación dinámica de scripts a GameObjects y la navegación multinivel.",
            13, TextAnchor.MiddleLeft, TextMuted);
        Size(heroDesc.gameObject, prefH: 40);

        // Fila con 3 Tarjetas de Módulos Principales
        var rowModules = Row(leftCol.transform, 12);
        Flex(rowModules.gameObject, flexH: 1);

        CreateModuleCard(rowModules.transform, "🎯 Arrastrar Sprite",
            "Mueve sprites con el ratón o táctil. Botones para generar nuevos sprites, resetear posición, cambiar apariencia, color y bloquear.",
            AccentPrimary, "Abrir Módulo", () => _navigationController.NavigateTo(1));

        CreateModuleCard(rowModules.transform, "🔤 Controlar Fuentes",
            "Ajusta tamaño (+/-), estilo (Bold/Italic), alineación, familia tipográfica, paleta de colores y efectos de sombra/contorno con botones.",
            AccentSuccess, "Abrir Módulo", () => _navigationController.NavigateTo(2));

        CreateModuleCard(rowModules.transform, "🧩 Fijar Scripts",
            "Fija y desacopla scripts (AddComponent) a objetos de juego en tiempo real con inspector en vivo y botones de control.",
            AccentPurple, "Abrir Módulo", () => _navigationController.NavigateTo(3));

        // Columna Derecha: Métricas y Consola Rápida
        var rightCol = Column(page.transform, 12);
        Flex(rightCol.gameObject, flexW: 1.1f);

        var statusCard = Panel(rightCol.transform, SidebarBg);
        Flex(statusCard.gameObject, flexH: 1);
        var statusV = statusCard.gameObject.AddComponent<VerticalLayoutGroup>();
        statusV.padding = new RectOffset(16, 16, 16, 16);
        statusV.spacing = 10;
        statusV.childControlWidth = true;
        statusV.childControlHeight = true;
        statusV.childForceExpandWidth = true;
        statusV.childForceExpandHeight = false;

        var statusHead = CreateText(statusCard.transform, "📊 Estado del Sistema", 15, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(statusHead.gameObject, prefH: 24);

        _resolutionInfoText = CreateText(statusCard.transform, "Cargando métricas...", 12, TextAnchor.UpperLeft, TextMuted);
        Flex(_resolutionInfoText.gameObject, flexH: 1);

        var div = Panel(statusCard.transform, BorderColor);
        Size(div.gameObject, prefH: 1);

        var quickHead = CreateText(statusCard.transform, "⚡ Atajos Rápidos", 14, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(quickHead.gameObject, prefH: 22);

        var b1 = CreateButton(statusCard.transform, "🎯 Probar Arrastre de Sprite", AccentPrimary, () => _navigationController.NavigateTo(1));
        Size(b1.gameObject, prefH: 38);

        var b2 = CreateButton(statusCard.transform, "🔤 Editar Tipografía", AccentSuccess, () => _navigationController.NavigateTo(2));
        Size(b2.gameObject, prefH: 38);

        var b3 = CreateButton(statusCard.transform, "🧩 Fijar Scripts a Objetos", AccentPurple, () => _navigationController.NavigateTo(3));
        Size(b3.gameObject, prefH: 38);

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

        Color[] colors = { AccentSuccess, AccentWarning, AccentDanger, AccentPurple, AccentCyan };
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
        bV.spacing = 8;
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
        Size(btnAttachDrag.gameObject, prefH: 38);

        var btnAttachFont = CreateButton(bindPanel.transform, "🔤 Fijar 'FontController'", AccentSuccess, () =>
        {
            if (_scriptBinder != null && _testTargetGameObject != null)
            {
                _scriptBinder.AttachFontController(_testTargetGameObject);
            }
        });
        Size(btnAttachFont.gameObject, prefH: 38);

        var btnAttachNav = CreateButton(bindPanel.transform, "🧭 Fijar 'NavigationController'", AccentPurple, () =>
        {
            if (_scriptBinder != null && _testTargetGameObject != null)
            {
                _scriptBinder.AttachNavigationController(_testTargetGameObject);
            }
        });
        Size(btnAttachNav.gameObject, prefH: 38);

        // Separador
        var div = Panel(bindPanel.transform, BorderColor);
        Size(div.gameObject, prefH: 1);

        var sec2 = CreateText(bindPanel.transform, "🗑️ Desacoplar Scripts (Destroy)", 13, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(sec2.gameObject, prefH: 18);

        var rowRemove = Row(bindPanel.transform, 6);
        Size(rowRemove.gameObject, prefH: 34);

        CreateButton(rowRemove.transform, "Remover Drag", ButtonNormal, () => _scriptBinder?.RemoveComponentByName("DraggableSprite", _testTargetGameObject));
        CreateButton(rowRemove.transform, "Remover Font", ButtonNormal, () => _scriptBinder?.RemoveComponentByName("FontController", _testTargetGameObject));
        CreateButton(rowRemove.transform, "Remover Nav", ButtonNormal, () => _scriptBinder?.RemoveComponentByName("NavigationController", _testTargetGameObject));

        // Separador
        var div2 = Panel(bindPanel.transform, BorderColor);
        Size(div2.gameObject, prefH: 1);

        var sec3 = CreateText(bindPanel.transform, "⚙️ Alternar Estado de Scripts", 13, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(sec3.gameObject, prefH: 18);

        var btnToggleDrag = CreateButton(bindPanel.transform, "Alternar Habilitación de DraggableSprite", AccentWarning, () =>
        {
            _scriptBinder?.ToggleComponentEnabled("DraggableSprite", _testTargetGameObject);
        });
        Size(btnToggleDrag.gameObject, prefH: 34);

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
    // Footer / Barra Inferior de Navegación
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

        // Indicadores circulares (Dots) de página
        var dotsContainer = Row(footerPanel.transform, 6);
        Size(dotsContainer.gameObject, prefW: 110, prefH: 40);
        dotsContainer.childAlignment = TextAnchor.MiddleCenter;

        var dotIndicators = new Image[5];
        for (int i = 0; i < 5; i++)
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
        _statusText = CreateText(footerPanel.transform, "💡 Interactúa con los botones para probar el arrastre, fuentes, scripts o navegación.", 12, TextAnchor.MiddleLeft, TextMuted, FontStyle.Italic);
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
            $"• <b>Historial Back:</b> {(_navigationController.CanGoBack ? "Disponible" : "Vacío")}";
    }

    void AddLog(string message)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        string entry = $"<color=#3D82FF>[{timestamp}]</color> {message}";
        _logEntries.Insert(0, entry);

        if (_logEntries.Count > 12)
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
        cardV.padding = new RectOffset(18, 18, 16, 16);
        cardV.spacing = 8;
        cardV.childControlWidth = true;
        cardV.childControlHeight = true;
        cardV.childForceExpandWidth = true;
        cardV.childForceExpandHeight = false;

        var topBadge = CreateBadge(card.transform, "MÓDULO INTERACTIVO", accent);
        Size(topBadge.gameObject, prefH: 22);

        var t = CreateText(card.transform, title, 15, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(t.gameObject, prefH: 22);

        var d = CreateText(card.transform, description, 12, TextAnchor.UpperLeft, TextMuted);
        Flex(d.gameObject, flexH: 1);

        var b = CreateButton(card.transform, btnLabel, accent, onAction);
        Size(b.gameObject, prefH: 36);
    }

    Image CreateBadge(Transform parent, string text, Color color)
    {
        var badge = Panel(parent, new Color(color.r, color.g, color.b, 0.22f));
        badge.name = "Badge_" + text;
        var t = CreateText(badge.transform, text, 10, TextAnchor.MiddleCenter, color, FontStyle.Bold);
        Stretch(t.rectTransform, Vector2.zero, Vector2.one);
        return badge;
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
    // Generadores Procedurales de Texturas y Formas
    // ==========================================
    void GenerateProceduralSprites()
    {
        _roundedSprite = MakeRoundedTexture(64, 16);
        _circleSprite  = MakeCircleTexture(64);
        _diamondSprite = MakeDiamondTexture(64);
        _starSprite    = MakeStarTexture(64);
        _shieldSprite  = MakeShieldTexture(64);
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
