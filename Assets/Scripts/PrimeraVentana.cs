using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

/// <summary>
/// Aplicación Interactiva Responsiva para Unity.
/// Incluye:
/// 1. Adaptación de tamaño de pantalla para diferentes dispositivos (Móvil, Tablet, Desktop, UltraWide).
/// 2. Sistema completo de botones de navegación (Pestañas, Anterior/Siguiente, Acciones y Enlaces).
/// 3. Demostración y visualización en vivo de Puntos de Anclaje (RectTransform Anchors en esquinas, bordes y centro).
/// </summary>
public class PrimeraVentana : MonoBehaviour
{
    // ==========================================
    // Paleta de Colores Moderna (Dark Glass Theme)
    // ==========================================
    static readonly Color BgDark          = new Color(0.07f, 0.09f, 0.13f, 0.95f);
    static readonly Color CardBg          = new Color(0.12f, 0.15f, 0.22f, 0.92f);
    static readonly Color CardHeaderBg    = new Color(0.16f, 0.20f, 0.29f, 1f);
    static readonly Color SidebarBg       = new Color(0.10f, 0.12f, 0.18f, 0.96f);
    static readonly Color AccentPrimary   = new Color(0.24f, 0.51f, 0.98f, 1f); // Azul vibrante
    static readonly Color AccentSuccess   = new Color(0.13f, 0.77f, 0.53f, 1f); // Verde esmeralda
    static readonly Color AccentWarning   = new Color(0.96f, 0.62f, 0.15f, 1f); // Ámbar
    static readonly Color AccentDanger    = new Color(0.94f, 0.27f, 0.38f, 1f); // Rosa/Rojo
    static readonly Color AccentPurple    = new Color(0.63f, 0.38f, 0.96f, 1f); // Púrpura
    static readonly Color TextWhite       = new Color(0.98f, 0.99f, 1.00f, 1f);
    static readonly Color TextMuted       = new Color(0.60f, 0.65f, 0.75f, 1f);
    static readonly Color TextDimmed      = new Color(0.40f, 0.44f, 0.54f, 1f);
    static readonly Color BorderColor     = new Color(0.22f, 0.28f, 0.38f, 0.7f);
    static readonly Color ButtonNormal    = new Color(0.18f, 0.23f, 0.33f, 1f);
    static readonly Color ButtonHover     = new Color(0.26f, 0.33f, 0.46f, 1f);
    static readonly Color AnchorPinColor  = new Color(1.00f, 0.32f, 0.48f, 0.95f);

    // ==========================================
    // Estado y Referencias
    // ==========================================
    Sprite _roundedSprite;
    Sprite _circleSprite;
    Canvas _canvas;
    CanvasScaler _canvasScaler;
    Text _statusText;
    Text _resolutionInfoText;
    Text _anchorDetailsText;
    RectTransform _anchorTargetPin;
    RectTransform _anchorSandboxArea;
    GameObject[] _pages;
    Button[] _navTabButtons;
    Image[] _navTabIndicators;
    Image[] _dotIndicators;
    int _currentPageIndex = 0;
    int _interactiveCounter = 0;
    Text _counterValueText;
    Text _logConsoleContent;
    readonly List<string> _logEntries = new List<string>();
    bool _showCornerPins = true;
    GameObject _cornerPinsRoot;

    // Presets de dispositivos
    struct DevicePreset
    {
        public string Name;
        public string Description;
        public Vector2 Resolution;
        public string AspectRatio;
        public string Icon;
    }

    readonly DevicePreset[] _devicePresets = new DevicePreset[]
    {
        new DevicePreset { Name = "Escritorio / Laptop", Description = "Resolución Full HD estándar de PC", Resolution = new Vector2(1920, 1080), AspectRatio = "16:9", Icon = "💻" },
        new DevicePreset { Name = "Móvil Vertical", Description = "Teléfono inteligente moderno (Portrait)", Resolution = new Vector2(1080, 1920), AspectRatio = "9:16", Icon = "📱" },
        new DevicePreset { Name = "Móvil Horizontal", Description = "Smartphone en modo panorámico (Landscape)", Resolution = new Vector2(1920, 1080), AspectRatio = "16:9", Icon = "📲" },
        new DevicePreset { Name = "Tablet / iPad", Description = "Tableta en formato 4:3 o 3:2", Resolution = new Vector2(2048, 1536), AspectRatio = "4:3", Icon = "📟" },
        new DevicePreset { Name = "Monitor UltraWide", Description = "Pantalla ultra-ancha panorámica", Resolution = new Vector2(2560, 1080), AspectRatio = "21:9", Icon = "🖥️" },
    };

    // Puntos de Anclaje Presets
    struct AnchorPreset
    {
        public string Name;
        public string ShortName;
        public Vector2 Min;
        public Vector2 Max;
        public Vector2 Pivot;
        public string Code;
        public string Desc;
    }

    readonly AnchorPreset[] _anchorPresets = new AnchorPreset[]
    {
        new AnchorPreset { Name = "Superior Izquierda", ShortName = "↖ Sup-Izq", Min = new Vector2(0f, 1f), Max = new Vector2(0f, 1f), Pivot = new Vector2(0f, 1f), Code = "Min(0,1) Max(0,1)", Desc = "Ideal para barras de estado, logos y botones de menú superior." },
        new AnchorPreset { Name = "Superior Centro", ShortName = "⬆ Sup-Cen", Min = new Vector2(0.5f, 1f), Max = new Vector2(0.5f, 1f), Pivot = new Vector2(0.5f, 1f), Code = "Min(0.5,1) Max(0.5,1)", Desc = "Ideal para títulos principales, marcadores de puntuación y notificaciones." },
        new AnchorPreset { Name = "Superior Derecha", ShortName = "↗ Sup-Der", Min = new Vector2(1f, 1f), Max = new Vector2(1f, 1f), Pivot = new Vector2(1f, 1f), Code = "Min(1,1) Max(1,1)", Desc = "Ideal para botón de cerrar (X), perfil de usuario y ajustes." },
        new AnchorPreset { Name = "Medio Izquierda", ShortName = "⬅ Med-Izq", Min = new Vector2(0f, 0.5f), Max = new Vector2(0f, 0.5f), Pivot = new Vector2(0f, 0.5f), Code = "Min(0,0.5) Max(0,0.5)", Desc = "Ideal para barras laterales acoplables y paneles de herramientas." },
        new AnchorPreset { Name = "Centro", ShortName = "⏺ Centro", Min = new Vector2(0.5f, 0.5f), Max = new Vector2(0.5f, 0.5f), Pivot = new Vector2(0.5f, 0.5f), Code = "Min(0.5,0.5) Max(0.5,0.5)", Desc = "Ideal para cuadros de diálogo modales, popups y ventanas de alerta." },
        new AnchorPreset { Name = "Medio Derecha", ShortName = "➡️ Med-Der", Min = new Vector2(1f, 0.5f), Max = new Vector2(1f, 0.5f), Pivot = new Vector2(1f, 0.5f), Code = "Min(1,0.5) Max(1,0.5)", Desc = "Ideal para paneles de inspección, inventario y chat lateral." },
        new AnchorPreset { Name = "Inferior Izquierda", ShortName = "↙ Inf-Izq", Min = new Vector2(0f, 0f), Max = new Vector2(0f, 0f), Pivot = new Vector2(0f, 0f), Code = "Min(0,0) Max(0,0)", Desc = "Ideal para minimapas, controles virtuales (joysticks) y chat de juego." },
        new AnchorPreset { Name = "Inferior Centro", ShortName = "⬇ Inf-Cen", Min = new Vector2(0.5f, 0f), Max = new Vector2(0.5f, 0f), Pivot = new Vector2(0.5f, 0f), Code = "Min(0.5,0) Max(0.5,0)", Desc = "Ideal para barra de navegación principal, botones de acción y paginadores." },
        new AnchorPreset { Name = "Inferior Derecha", ShortName = "↘ Inf-Der", Min = new Vector2(1f, 0f), Max = new Vector2(1f, 0f), Pivot = new Vector2(1f, 0f), Code = "Min(1,0) Max(1,0)", Desc = "Ideal para botones de confirmación, FABs (+), y créditos." },
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoBootstrap()
    {
        if (UnityEngine.Object.FindAnyObjectByType<PrimeraVentana>() != null) return;
        var go = new GameObject("AplicacionInteractiva");
        go.AddComponent<PrimeraVentana>();
    }

    void Awake()
    {
        _roundedSprite = MakeRoundedTexture(64, 16);
        _circleSprite = MakeCircleTexture(64);
        EnsureEventSystem();
        BuildUI();
    }

    void Start()
    {
        AddLog("Aplicación interactiva inicializada correctamente.");
        AddLog("Sistema CanvasScaler activo con escala adaptable a múltiples pantallas.");
        ShowPage(0);
        SelectAnchorPreset(4); // Centro por defecto
    }

    void Update()
    {
        UpdateLiveMetrics();
    }

    // ==========================================
    // Construcción General de la Interfaz
    // ==========================================
    void BuildUI()
    {
        // 1. Canvas y Canvas Scaler adaptado para diferentes dispositivos
        var canvasGo = new GameObject("InteractiveCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);
        _canvas = canvasGo.GetComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 10;

        _canvasScaler = canvasGo.GetComponent<CanvasScaler>();
        _canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        _canvasScaler.referenceResolution = new Vector2(1920, 1080);
        _canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        _canvasScaler.matchWidthOrHeight = 0.5f; // Balance entre ancho y alto

        // 2. Fondo general de pantalla con gradiente visual
        var background = Panel(canvasGo.transform, BgDark);
        background.name = "BackgroundLayer";
        Stretch(background.rectTransform, Vector2.zero, Vector2.one);

        // 3. Contenedor Maestro centrado y adaptable
        var masterContainer = Panel(canvasGo.transform, Color.clear);
        masterContainer.name = "MasterContainer";
        Stretch(masterContainer.rectTransform, new Vector2(0.02f, 0.03f), new Vector2(0.98f, 0.97f));
        
        var masterVGroup = masterContainer.gameObject.AddComponent<VerticalLayoutGroup>();
        masterVGroup.padding = new RectOffset(0, 0, 0, 0);
        masterVGroup.spacing = 14;
        masterVGroup.childControlWidth = true;
        masterVGroup.childControlHeight = true;
        masterVGroup.childForceExpandWidth = true;
        masterVGroup.childForceExpandHeight = false;

        // 4. Header / Barra Superior con Logo y Navegación Principal
        BuildHeader(masterContainer.transform);

        // 5. Área de Páginas / Contenido Central
        var contentArea = Panel(masterContainer.transform, Color.clear);
        contentArea.name = "ContentArea";
        Flex(contentArea.gameObject, flexH: 1);

        _pages = new GameObject[4];
        _pages[0] = BuildPageInicio(contentArea.transform);
        _pages[1] = BuildPageDispositivos(contentArea.transform);
        _pages[2] = BuildPageAnclajes(contentArea.transform);
        _pages[3] = BuildPageExplorador(contentArea.transform);

        // 6. Footer / Barra Inferior con Paginación, Botones de Navegación y Estado
        BuildFooter(masterContainer.transform);

        // 7. Pines Visuales de Anclaje en las Esquinas de la Pantalla (Overlay)
        BuildCornerAnchorPins(canvasGo.transform);
    }

    // ==========================================
    // 1. Header y Barra de Navegación Superior
    // ==========================================
    void BuildHeader(Transform parent)
    {
        var headerPanel = Panel(parent, CardHeaderBg);
        headerPanel.name = "HeaderNavBar";
        Size(headerPanel.gameObject, prefH: 76);

        var hLayout = headerPanel.gameObject.AddComponent<HorizontalLayoutGroup>();
        hLayout.padding = new RectOffset(20, 20, 10, 10);
        hLayout.spacing = 16;
        hLayout.childControlWidth = false;
        hLayout.childControlHeight = true;
        hLayout.childForceExpandWidth = false;
        hLayout.childForceExpandHeight = true;
        hLayout.childAlignment = TextAnchor.MiddleLeft;

        // Logo e Identificador de la Aplicación
        var logoBox = Panel(headerPanel.transform, AccentPrimary);
        logoBox.name = "AppLogo";
        Size(logoBox.gameObject, prefW: 56, prefH: 56);
        var logoText = CreateText(logoBox.transform, "✦", 28, TextAnchor.MiddleCenter, TextWhite, FontStyle.Bold);
        Stretch(logoText.rectTransform, Vector2.zero, Vector2.one);

        var titleCol = Column(headerPanel.transform, 2);
        Size(titleCol.gameObject, prefW: 280, prefH: 56);
        titleCol.childAlignment = TextAnchor.MiddleLeft;
        
        var titleText = CreateText(titleCol.transform, "APLICACIÓN INTERACTIVA", 18, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(titleText.gameObject, prefH: 26);
        var subTitleText = CreateText(titleCol.transform, "Escena Multi-Dispositivo & Puntos de Anclaje", 12, TextAnchor.MiddleLeft, TextMuted);
        Size(subTitleText.gameObject, prefH: 20);

        // Espaciador flexible
        var spacer = CreateSpacer(headerPanel.transform, flexW: 1);

        // Botones de Navegación (Pestañas Principales)
        string[] tabLabels = { "🏠 Inicio", "📱 Dispositivos", "⚓ Puntos de Anclaje", "🧭 Explorador" };
        _navTabButtons = new Button[tabLabels.Length];
        _navTabIndicators = new Image[tabLabels.Length];

        for (int i = 0; i < tabLabels.Length; i++)
        {
            int pageIdx = i;
            var tabBtnGo = Panel(headerPanel.transform, ButtonNormal);
            tabBtnGo.name = "TabBtn_" + i;
            Size(tabBtnGo.gameObject, prefW: 170, prefH: 48);

            var btn = tabBtnGo.gameObject.AddComponent<Button>();
            btn.targetGraphic = tabBtnGo;

            var tabVGroup = tabBtnGo.gameObject.AddComponent<VerticalLayoutGroup>();
            tabVGroup.padding = new RectOffset(10, 10, 6, 4);
            tabVGroup.spacing = 4;
            tabVGroup.childControlWidth = true;
            tabVGroup.childControlHeight = true;
            tabVGroup.childForceExpandWidth = true;
            tabVGroup.childForceExpandHeight = true;

            var label = CreateText(tabBtnGo.transform, tabLabels[i], 14, TextAnchor.MiddleCenter, TextWhite, FontStyle.Bold);
            Flex(label.gameObject, flexH: 1);

            // Indicador de pestaña activa (línea inferior)
            var indicator = Panel(tabBtnGo.transform, Color.clear);
            indicator.name = "ActiveIndicator";
            Size(indicator.gameObject, prefH: 4);
            _navTabIndicators[i] = indicator;

            btn.onClick.AddListener(() =>
            {
                ShowPage(pageIdx);
                AddLog("Navegación: Cambiado a página '" + tabLabels[pageIdx] + "'");
            });

            _navTabButtons[i] = btn;
        }

        // Botón de alternar Pines de Anclaje en pantalla
        var pinToggleBtn = CreateIconButton(headerPanel.transform, "📌 Pines", AccentPurple, 110, 48, () =>
        {
            _showCornerPins = !_showCornerPins;
            if (_cornerPinsRoot != null) _cornerPinsRoot.SetActive(_showCornerPins);
            AddLog("Visibilidad de pines de esquina: " + (_showCornerPins ? "ACTIVADOS" : "DESACTIVADOS"));
        });
        pinToggleBtn.name = "ToggleCornerPinsBtn";
    }

    // ==========================================
    // 2. Página 1: Inicio (Dashboard General)
    // ==========================================
    GameObject BuildPageInicio(Transform parent)
    {
        var page = Panel(parent, Color.clear);
        page.name = "Page_Inicio";
        Stretch(page.rectTransform, Vector2.zero, Vector2.one);

        var hLayout = page.gameObject.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 16;
        hLayout.childControlWidth = true;
        hLayout.childControlHeight = true;
        hLayout.childForceExpandWidth = true;
        hLayout.childForceExpandHeight = true;

        // Columna Principal Izquierda (Bienvenida y Tarjetas de Pilares)
        var leftCol = Column(page.transform, 14);
        Flex(leftCol.gameObject, flexW: 2.2f);

        // Tarjeta Hero de Bienvenida
        var heroCard = Panel(leftCol.transform, CardBg);
        heroCard.name = "HeroCard";
        Size(heroCard.gameObject, prefH: 170);

        var heroV = heroCard.gameObject.AddComponent<VerticalLayoutGroup>();
        heroV.padding = new RectOffset(24, 24, 20, 20);
        heroV.spacing = 8;
        heroV.childControlWidth = true;
        heroV.childControlHeight = true;
        heroV.childForceExpandWidth = true;
        heroV.childForceExpandHeight = false;

        var heroBadge = CreateBadge(heroCard.transform, "✨ ESCENA TOTALMENTE CONFIGURADA", AccentSuccess);
        Size(heroBadge.gameObject, prefH: 26);

        var heroTitle = CreateText(heroCard.transform, "Bienvenido a la Experiencia Interactiva", 22, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(heroTitle.gameObject, prefH: 32);

        var heroDesc = CreateText(heroCard.transform,
            "Esta escena ha sido estructurada con soporte nativo para múltiples dispositivos, navegación por pestañas y demostración interactiva de anclajes (Anchors).",
            14, TextAnchor.MiddleLeft, TextMuted);
        Size(heroDesc.gameObject, prefH: 42);

        // Fila de 3 Tarjetas de Características Principales
        var featuresRow = Row(leftCol.transform, 12);
        Flex(featuresRow.gameObject, flexH: 1);

        CreateFeatureCard(featuresRow.transform, "📱 Multi-Dispositivo",
            "Escalado adaptable mediante CanvasScaler. Soporte automático para resoluciones de móviles, tablets y pantallas panorámicas.",
            AccentPrimary, "Explorar Dispositivos", () => ShowPage(1));

        CreateFeatureCard(featuresRow.transform, "🧭 Navegación Fluida",
            "Botones interactivos con respuesta visual, control por pestañas superiores, avance/retroceso y barra de migas de pan.",
            AccentSuccess, "Probar Navegación", () => ShowPage(3));

        CreateFeatureCard(featuresRow.transform, "⚓ Puntos de Anclaje",
            "Demostración técnica de los 9 presets de RectTransform (esquinas, centro y bordes) con pines visuales en vivo.",
            AccentDanger, "Ver Anclajes", () => ShowPage(2));

        // Columna Derecha (Panel de Métricas en Vivo y Enlaces Rápidos)
        var rightCol = Column(page.transform, 14);
        Flex(rightCol.gameObject, flexW: 1.0f);

        // Tarjeta de Estado del Sistema
        var statusCard = Panel(rightCol.transform, SidebarBg);
        statusCard.name = "StatusCard";
        Flex(statusCard.gameObject, flexH: 1);

        var statusV = statusCard.gameObject.AddComponent<VerticalLayoutGroup>();
        statusV.padding = new RectOffset(20, 20, 20, 20);
        statusV.spacing = 12;
        statusV.childControlWidth = true;
        statusV.childControlHeight = true;
        statusV.childForceExpandWidth = true;
        statusV.childForceExpandHeight = false;

        var statusHead = CreateText(statusCard.transform, "📊 Métricas en Tiempo Real", 16, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(statusHead.gameObject, prefH: 26);

        _resolutionInfoText = CreateText(statusCard.transform, "Cargando resolución...", 13, TextAnchor.UpperLeft, TextMuted);
        Flex(_resolutionInfoText.gameObject, flexH: 1);

        var div = Panel(statusCard.transform, BorderColor);
        Size(div.gameObject, prefH: 1);

        var quickHead = CreateText(statusCard.transform, "⚡ Acciones Rápidas", 15, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(quickHead.gameObject, prefH: 24);

        var btnAction1 = CreateButton(statusCard.transform, "🎯 Simular Móvil Vertical", AccentPrimary, () =>
        {
            ShowPage(1);
            ApplyDevicePreset(1);
        });
        Size(btnAction1.gameObject, prefH: 42);

        var btnAction2 = CreateButton(statusCard.transform, "📍 Probar Anclaje Central", AccentPurple, () =>
        {
            ShowPage(2);
            SelectAnchorPreset(4);
        });
        Size(btnAction2.gameObject, prefH: 42);

        return page.gameObject;
    }

    // ==========================================
    // 3. Página 2: Tamaño de Pantalla para Diferentes Dispositivos
    // ==========================================
    GameObject BuildPageDispositivos(Transform parent)
    {
        var page = Panel(parent, Color.clear);
        page.name = "Page_Dispositivos";
        Stretch(page.rectTransform, Vector2.zero, Vector2.one);

        var hLayout = page.gameObject.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 16;
        hLayout.childControlWidth = true;
        hLayout.childControlHeight = true;
        hLayout.childForceExpandWidth = true;
        hLayout.childForceExpandHeight = true;

        // Columna Izquierda: Selector de Dispositivos y Explicación
        var leftCol = Column(page.transform, 12);
        Flex(leftCol.gameObject, flexW: 1.3f);

        var headerCard = Panel(leftCol.transform, CardBg);
        Size(headerCard.gameObject, prefH: 110);
        var headV = headerCard.gameObject.AddComponent<VerticalLayoutGroup>();
        headV.padding = new RectOffset(20, 20, 16, 16);
        headV.spacing = 6;
        headV.childControlWidth = true;
        headV.childControlHeight = true;
        headV.childForceExpandWidth = true;
        headV.childForceExpandHeight = false;

        var headTitle = CreateText(headerCard.transform, "📱 Selector de Dispositivos y Formatos", 18, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(headTitle.gameObject, prefH: 26);
        var headDesc = CreateText(headerCard.transform, "El CanvasScaler ajusta automáticamente las proporciones de los elementos para mantener la legibilidad y estética sin importar el aspecto.", 13, TextAnchor.MiddleLeft, TextMuted);
        Size(headDesc.gameObject, prefH: 42);

        // Lista interactiva de presets de dispositivos
        var presetsContainer = Panel(leftCol.transform, SidebarBg);
        Flex(presetsContainer.gameObject, flexH: 1);
        var presetsV = presetsContainer.gameObject.AddComponent<VerticalLayoutGroup>();
        presetsV.padding = new RectOffset(16, 16, 16, 16);
        presetsV.spacing = 10;
        presetsV.childControlWidth = true;
        presetsV.childControlHeight = true;
        presetsV.childForceExpandWidth = true;
        presetsV.childForceExpandHeight = false;

        for (int i = 0; i < _devicePresets.Length; i++)
        {
            int presetIdx = i;
            var p = _devicePresets[i];

            var devItem = Panel(presetsContainer.transform, ButtonNormal);
            devItem.name = "DeviceItem_" + i;
            Size(devItem.gameObject, prefH: 64);

            var devBtn = devItem.gameObject.AddComponent<Button>();
            devBtn.targetGraphic = devItem;

            var devH = devItem.gameObject.AddComponent<HorizontalLayoutGroup>();
            devH.padding = new RectOffset(16, 16, 10, 10);
            devH.spacing = 14;
            devH.childControlWidth = false;
            devH.childControlHeight = true;
            devH.childForceExpandWidth = false;
            devH.childForceExpandHeight = true;
            devH.childAlignment = TextAnchor.MiddleLeft;

            var iconText = CreateText(devItem.transform, p.Icon, 26, TextAnchor.MiddleCenter, TextWhite);
            Size(iconText.gameObject, prefW: 36);

            var infoV = Column(devItem.transform, 2);
            Size(infoV.gameObject, prefW: 260);
            var nameT = CreateText(infoV.transform, p.Name, 15, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
            Size(nameT.gameObject, prefH: 22);
            var resT = CreateText(infoV.transform, p.Resolution.x + "x" + p.Resolution.y + " (" + p.AspectRatio + ")", 12, TextAnchor.MiddleLeft, AccentPrimary);
            Size(resT.gameObject, prefH: 18);

            CreateSpacer(devItem.transform, flexW: 1);

            var badge = CreateBadge(devItem.transform, "Probar", AccentPrimary);
            Size(badge.gameObject, prefW: 70, prefH: 30);

            devBtn.onClick.AddListener(() => ApplyDevicePreset(presetIdx));
        }

        // Columna Derecha: Panel de Simulación Visual y Especificaciones
        var rightCol = Column(page.transform, 12);
        Flex(rightCol.gameObject, flexW: 1.5f);

        var simCard = Panel(rightCol.transform, CardBg);
        Flex(simCard.gameObject, flexH: 1);

        var simV = simCard.gameObject.AddComponent<VerticalLayoutGroup>();
        simV.padding = new RectOffset(20, 20, 20, 20);
        simV.spacing = 12;
        simV.childControlWidth = true;
        simV.childControlHeight = true;
        simV.childForceExpandWidth = true;
        simV.childForceExpandHeight = false;

        var simTitle = CreateText(simCard.transform, "📐 Configuración del CanvasScaler Activo", 17, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(simTitle.gameObject, prefH: 26);

        var scalerDetails = CreateText(simCard.transform,
            "• UI Scale Mode: Scale With Screen Size\n" +
            "• Resolución de Referencia: 1920 × 1080 px\n" +
            "• Screen Match Mode: Match Width Or Height (0.5)\n" +
            "• Render Mode: Screen Space - Overlay\n\n" +
            "Ventajas:\n" +
            "✓ Los textos no se pixelan ni se deforman.\n" +
            "✓ Las tarjetas mantienen márgenes relativos y proporcionales.\n" +
            "✓ La navegación y botones se adaptan tanto a teléfonos verticales como a monitores ultra-panorámicos.",
            13, TextAnchor.UpperLeft, TextMuted);
        Size(scalerDetails.gameObject, prefH: 180);

        var matchControlBox = Panel(simCard.transform, SidebarBg);
        Size(matchControlBox.gameObject, prefH: 120);
        var matchV = matchControlBox.gameObject.AddComponent<VerticalLayoutGroup>();
        matchV.padding = new RectOffset(16, 16, 12, 12);
        matchV.spacing = 8;
        matchV.childControlWidth = true;
        matchV.childControlHeight = true;
        matchV.childForceExpandWidth = true;
        matchV.childForceExpandHeight = false;

        var matchTitle = CreateText(matchControlBox.transform, "🎛️ Control Rápido de Match (Ancho / Alto)", 14, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(matchTitle.gameObject, prefH: 20);

        var matchBtnsRow = Row(matchControlBox.transform, 10);
        Size(matchBtnsRow.gameObject, prefH: 42);

        var btnMatchW = CreateButton(matchBtnsRow.transform, "Ancho (0.0)", ButtonNormal, () =>
        {
            _canvasScaler.matchWidthOrHeight = 0f;
            AddLog("CanvasScaler: Match ajustado a Ancho (0.0)");
        });
        Flex(btnMatchW.gameObject, flexW: 1);

        var btnMatchHalf = CreateButton(matchBtnsRow.transform, "Equilibrado (0.5)", AccentPrimary, () =>
        {
            _canvasScaler.matchWidthOrHeight = 0.5f;
            AddLog("CanvasScaler: Match ajustado a Equilibrado (0.5)");
        });
        Flex(btnMatchHalf.gameObject, flexW: 1);

        var btnMatchH = CreateButton(matchBtnsRow.transform, "Alto (1.0)", ButtonNormal, () =>
        {
            _canvasScaler.matchWidthOrHeight = 1f;
            AddLog("CanvasScaler: Match ajustado a Alto (1.0)");
        });
        Flex(btnMatchH.gameObject, flexW: 1);

        return page.gameObject;
    }

    // ==========================================
    // 4. Página 3: Demostración Interactiva de Puntos de Anclaje
    // ==========================================
    GameObject BuildPageAnclajes(Transform parent)
    {
        var page = Panel(parent, Color.clear);
        page.name = "Page_Anclajes";
        Stretch(page.rectTransform, Vector2.zero, Vector2.one);

        var hLayout = page.gameObject.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 16;
        hLayout.childControlWidth = true;
        hLayout.childControlHeight = true;
        hLayout.childForceExpandWidth = true;
        hLayout.childForceExpandHeight = true;

        // Columna Izquierda: Cuadrícula de 9 Botones de Anclaje y Explicación
        var leftCol = Column(page.transform, 12);
        Flex(leftCol.gameObject, flexW: 1.1f);

        var anchorHeader = Panel(leftCol.transform, CardBg);
        Size(anchorHeader.gameObject, prefH: 90);
        var aHeadV = anchorHeader.gameObject.AddComponent<VerticalLayoutGroup>();
        aHeadV.padding = new RectOffset(16, 16, 12, 12);
        aHeadV.spacing = 4;
        aHeadV.childControlWidth = true;
        aHeadV.childControlHeight = true;
        aHeadV.childForceExpandWidth = true;
        aHeadV.childForceExpandHeight = false;

        var aTitle = CreateText(anchorHeader.transform, "⚓ Puntos de Anclaje (RectTransform)", 17, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(aTitle.gameObject, prefH: 24);
        var aDesc = CreateText(anchorHeader.transform, "Haz clic en cualquier cuadrante para desplazar el pin interactivo y ver sus parámetros.", 12, TextAnchor.MiddleLeft, TextMuted);
        Size(aDesc.gameObject, prefH: 36);

        // Cuadrícula 3x3 de Botones de Anclaje
        var gridContainer = Panel(leftCol.transform, SidebarBg);
        Flex(gridContainer.gameObject, flexH: 1);
        var gridV = gridContainer.gameObject.AddComponent<VerticalLayoutGroup>();
        gridV.padding = new RectOffset(16, 16, 16, 16);
        gridV.spacing = 10;
        gridV.childControlWidth = true;
        gridV.childControlHeight = true;
        gridV.childForceExpandWidth = true;
        gridV.childForceExpandHeight = true;

        // Fila Superior (0, 1, 2)
        var rowTop = Row(gridContainer.transform, 10);
        Flex(rowTop.gameObject, flexH: 1);
        CreateAnchorGridBtn(rowTop.transform, 0);
        CreateAnchorGridBtn(rowTop.transform, 1);
        CreateAnchorGridBtn(rowTop.transform, 2);

        // Fila Media (3, 4, 5)
        var rowMid = Row(gridContainer.transform, 10);
        Flex(rowMid.gameObject, flexH: 1);
        CreateAnchorGridBtn(rowMid.transform, 3);
        CreateAnchorGridBtn(rowMid.transform, 4);
        CreateAnchorGridBtn(rowMid.transform, 5);

        // Fila Inferior (6, 7, 8)
        var rowBot = Row(gridContainer.transform, 10);
        Flex(rowBot.gameObject, flexH: 1);
        CreateAnchorGridBtn(rowBot.transform, 6);
        CreateAnchorGridBtn(rowBot.transform, 7);
        CreateAnchorGridBtn(rowBot.transform, 8);

        // Columna Derecha: Sandbox Visual donde se mueve el Pin y Ficha Técnica
        var rightCol = Column(page.transform, 12);
        Flex(rightCol.gameObject, flexW: 1.5f);

        // Área Sandbox (Representación del Canvas / Pantalla)
        var sandboxPanel = Panel(rightCol.transform, new Color(0.08f, 0.10f, 0.15f, 1f));
        sandboxPanel.name = "AnchorSandboxArea";
        _anchorSandboxArea = sandboxPanel.rectTransform;
        Flex(sandboxPanel.gameObject, flexH: 1.6f);

        // Cuadrícula de guía decorativa en el sandbox
        var centerCrossH = Panel(sandboxPanel.transform, new Color(0.2f, 0.25f, 0.35f, 0.4f));
        Stretch(centerCrossH.rectTransform, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f));
        centerCrossH.rectTransform.sizeDelta = new Vector2(0, 2);

        var centerCrossV = Panel(sandboxPanel.transform, new Color(0.2f, 0.25f, 0.35f, 0.4f));
        Stretch(centerCrossV.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 1f));
        centerCrossV.rectTransform.sizeDelta = new Vector2(2, 0);

        // Pin Objetivo Móvil dentro del Sandbox
        var pinObj = Panel(sandboxPanel.transform, AnchorPinColor);
        pinObj.name = "TargetAnchorPin";
        _anchorTargetPin = pinObj.rectTransform;
        _anchorTargetPin.sizeDelta = new Vector2(140, 50);
        var pinLabel = CreateText(pinObj.transform, "🎯 PIN ACTIVO", 13, TextAnchor.MiddleCenter, TextWhite, FontStyle.Bold);
        Stretch(pinLabel.rectTransform, Vector2.zero, Vector2.one);

        // Ficha Técnica de Parámetros del Anclaje
        var detailsCard = Panel(rightCol.transform, CardBg);
        Flex(detailsCard.gameObject, flexH: 1.0f);
        var detailsV = detailsCard.gameObject.AddComponent<VerticalLayoutGroup>();
        detailsV.padding = new RectOffset(18, 18, 14, 14);
        detailsV.spacing = 6;
        detailsV.childControlWidth = true;
        detailsV.childControlHeight = true;
        detailsV.childForceExpandWidth = true;
        detailsV.childForceExpandHeight = false;

        var detailsTitle = CreateText(detailsCard.transform, "📋 Inspector de Valores de Anclaje", 15, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(detailsTitle.gameObject, prefH: 22);

        _anchorDetailsText = CreateText(detailsCard.transform, "Selecciona un anclaje...", 13, TextAnchor.UpperLeft, TextMuted);
        Flex(_anchorDetailsText.gameObject, flexH: 1);

        return page.gameObject;
    }

    void CreateAnchorGridBtn(Transform parent, int presetIndex)
    {
        var preset = _anchorPresets[presetIndex];
        var btn = CreateButton(parent, preset.ShortName, ButtonNormal, () => SelectAnchorPreset(presetIndex));
        Flex(btn.gameObject, flexW: 1, flexH: 1);
    }

    void SelectAnchorPreset(int index)
    {
        if (index < 0 || index >= _anchorPresets.Length) return;
        var p = _anchorPresets[index];

        if (_anchorTargetPin != null)
        {
            _anchorTargetPin.anchorMin = p.Min;
            _anchorTargetPin.anchorMax = p.Max;
            _anchorTargetPin.pivot = p.Pivot;
            
            // Márgenes de separación según la esquina
            float offsetX = Mathf.Lerp(16f, -16f, p.Pivot.x);
            float offsetY = Mathf.Lerp(16f, -16f, p.Pivot.y);
            if (p.Pivot == new Vector2(0.5f, 0.5f)) { offsetX = 0; offsetY = 0; }

            _anchorTargetPin.anchoredPosition = new Vector2(offsetX, offsetY);
        }

        if (_anchorDetailsText != null)
        {
            _anchorDetailsText.text =
                "<b>Nombre:</b> <color=#3D82FF>" + p.Name + "</color>\n" +
                "<b>Anchor Min:</b> (" + p.Min.x + ", " + p.Min.y + ")  |  <b>Anchor Max:</b> (" + p.Max.x + ", " + p.Max.y + ")\n" +
                "<b>Pivot:</b> (" + p.Pivot.x + ", " + p.Pivot.y + ")\n" +
                "<b>Uso recomendado:</b> " + p.Desc;
        }

        AddLog("Punto de Anclaje seleccionado: " + p.Name + " (" + p.Code + ")");
    }

    // ==========================================
    // 5. Página 4: Explorador Interactivo y Controles
    // ==========================================
    GameObject BuildPageExplorador(Transform parent)
    {
        var page = Panel(parent, Color.clear);
        page.name = "Page_Explorador";
        Stretch(page.rectTransform, Vector2.zero, Vector2.one);

        var hLayout = page.gameObject.AddComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 16;
        hLayout.childControlWidth = true;
        hLayout.childControlHeight = true;
        hLayout.childForceExpandWidth = true;
        hLayout.childForceExpandHeight = true;

        // Columna Izquierda: Controles Interactivos (Contador, Botones de Estado)
        var leftCol = Column(page.transform, 14);
        Flex(leftCol.gameObject, flexW: 1.2f);

        // Tarjeta Contador Interactivo
        var counterCard = Panel(leftCol.transform, CardBg);
        Size(counterCard.gameObject, prefH: 150);
        var countV = counterCard.gameObject.AddComponent<VerticalLayoutGroup>();
        countV.padding = new RectOffset(20, 20, 16, 16);
        countV.spacing = 10;
        countV.childControlWidth = true;
        countV.childControlHeight = true;
        countV.childForceExpandWidth = true;
        countV.childForceExpandHeight = false;

        var countTitle = CreateText(counterCard.transform, "🔢 Contador Interactivo de Clics", 16, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(countTitle.gameObject, prefH: 22);

        var countRow = Row(counterCard.transform, 14);
        Size(countRow.gameObject, prefH: 52);

        var btnMinus = CreateButton(countRow.transform, "➖ Restar (-1)", ButtonNormal, () =>
        {
            _interactiveCounter--;
            UpdateCounterDisplay();
            AddLog("Contador interactivo: " + _interactiveCounter);
        });
        Flex(btnMinus.gameObject, flexW: 1);

        _counterValueText = CreateText(countRow.transform, "0", 26, TextAnchor.MiddleCenter, AccentPrimary, FontStyle.Bold);
        Size(_counterValueText.gameObject, prefW: 70);

        var btnPlus = CreateButton(countRow.transform, "➕ Sumar (+1)", AccentPrimary, () =>
        {
            _interactiveCounter++;
            UpdateCounterDisplay();
            AddLog("Contador interactivo: " + _interactiveCounter);
        });
        Flex(btnPlus.gameObject, flexW: 1);

        // Tarjeta de Acciones de Prueba
        var actionsCard = Panel(leftCol.transform, SidebarBg);
        Flex(actionsCard.gameObject, flexH: 1);
        var actV = actionsCard.gameObject.AddComponent<VerticalLayoutGroup>();
        actV.padding = new RectOffset(20, 20, 16, 16);
        actV.spacing = 10;
        actV.childControlWidth = true;
        actV.childControlHeight = true;
        actV.childForceExpandWidth = true;
        actV.childForceExpandHeight = false;

        var actTitle = CreateText(actionsCard.transform, "🕹️ Botones de Prueba de Eventos", 16, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(actTitle.gameObject, prefH: 22);

        CreateButton(actionsCard.transform, "🔔 Lanzar Notificación Éxito", AccentSuccess, () =>
        {
            AddLog("¡Éxito! Notificación de evento disparada correctamente.");
            _statusText.text = "✅ Acción confirmada satisfactoriamente.";
        });

        CreateButton(actionsCard.transform, "⚠️ Lanzar Advertencia del Sistema", AccentWarning, () =>
        {
            AddLog("Advertencia: Se ha pulsado el botón de prueba de alertas.");
            _statusText.text = "⚠️ Alerta interactiva emitida.";
        });

        CreateButton(actionsCard.transform, "🗑️ Limpiar Registro de Consola", ButtonNormal, () =>
        {
            _logEntries.Clear();
            AddLog("Registro de consola reiniciado.");
        });

        // Columna Derecha: Registro de Consola de Eventos en Vivo
        var rightCol = Column(page.transform, 14);
        Flex(rightCol.gameObject, flexW: 1.5f);

        var consoleCard = Panel(rightCol.transform, CardBg);
        Flex(consoleCard.gameObject, flexH: 1);
        var conV = consoleCard.gameObject.AddComponent<VerticalLayoutGroup>();
        conV.padding = new RectOffset(20, 20, 16, 16);
        conV.spacing = 8;
        conV.childControlWidth = true;
        conV.childControlHeight = true;
        conV.childForceExpandWidth = true;
        conV.childForceExpandHeight = false;

        var conHeadRow = Row(consoleCard.transform, 10);
        Size(conHeadRow.gameObject, prefH: 26);
        var conTitle = CreateText(conHeadRow.transform, "📜 Consola de Interacciones en Vivo", 16, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Flex(conTitle.gameObject, flexW: 1);
        var conBadge = CreateBadge(conHeadRow.transform, "LIVE", AccentSuccess);
        Size(conBadge.gameObject, prefW: 60, prefH: 24);

        var conBox = Panel(consoleCard.transform, new Color(0.06f, 0.07f, 0.10f, 1f));
        Flex(conBox.gameObject, flexH: 1);
        var conBoxV = conBox.gameObject.AddComponent<VerticalLayoutGroup>();
        conBoxV.padding = new RectOffset(14, 14, 12, 12);
        conBoxV.spacing = 4;
        conBoxV.childControlWidth = true;
        conBoxV.childControlHeight = true;
        conBoxV.childForceExpandWidth = true;
        conBoxV.childForceExpandHeight = true;

        _logConsoleContent = CreateText(conBox.transform, "", 12, TextAnchor.UpperLeft, new Color(0.75f, 0.82f, 0.95f));
        Flex(_logConsoleContent.gameObject, flexH: 1);

        return page.gameObject;
    }

    // ==========================================
    // 6. Footer / Barra Inferior de Navegación
    // ==========================================
    void BuildFooter(Transform parent)
    {
        var footerPanel = Panel(parent, CardHeaderBg);
        footerPanel.name = "FooterNavBar";
        Size(footerPanel.gameObject, prefH: 64);

        var hLayout = footerPanel.gameObject.AddComponent<HorizontalLayoutGroup>();
        hLayout.padding = new RectOffset(20, 20, 8, 8);
        hLayout.spacing = 16;
        hLayout.childControlWidth = false;
        hLayout.childControlHeight = true;
        hLayout.childForceExpandWidth = false;
        hLayout.childForceExpandHeight = true;
        hLayout.childAlignment = TextAnchor.MiddleLeft;

        // Botón Anterior
        var btnPrev = CreateIconButton(footerPanel.transform, "◀ Anterior", ButtonNormal, 130, 44, () =>
        {
            int prevIdx = (_currentPageIndex - 1 + _pages.Length) % _pages.Length;
            ShowPage(prevIdx);
        });
        btnPrev.name = "BtnNavPrev";

        // Indicadores circulares (Dots) de página
        var dotsContainer = Row(footerPanel.transform, 8);
        Size(dotsContainer.gameObject, prefW: 100, prefH: 44);
        dotsContainer.childAlignment = TextAnchor.MiddleCenter;
        _dotIndicators = new Image[_pages.Length];

        for (int i = 0; i < _pages.Length; i++)
        {
            int pageIdx = i;
            var dot = Panel(dotsContainer.transform, TextDimmed);
            dot.name = "Dot_" + i;
            dot.sprite = _circleSprite;
            Size(dot.gameObject, prefW: 12, prefH: 12);
            _dotIndicators[i] = dot;
        }

        // Botón Siguiente
        var btnNext = CreateIconButton(footerPanel.transform, "Siguiente ▶", AccentPrimary, 130, 44, () =>
        {
            int nextIdx = (_currentPageIndex + 1) % _pages.Length;
            ShowPage(nextIdx);
        });
        btnNext.name = "BtnNavNext";

        // Línea divisoria
        var div = Panel(footerPanel.transform, BorderColor);
        Size(div.gameObject, prefW: 1, prefH: 36);

        // Barra de estado interactiva
        _statusText = CreateText(footerPanel.transform, "💡 Interactúa con los botones para navegar o probar anclajes.", 13, TextAnchor.MiddleLeft, TextMuted, FontStyle.Italic);
        Flex(_statusText.gameObject, flexW: 1);

        // Botón de Inicio Rápido
        var btnHome = CreateIconButton(footerPanel.transform, "🏠 Inicio", ButtonNormal, 100, 44, () => ShowPage(0));
        btnHome.name = "BtnNavHome";
    }

    // ==========================================
    // 7. Pines Visuales de Anclaje de Esquina (Overlay)
    // ==========================================
    void BuildCornerAnchorPins(Transform canvasTransform)
    {
        _cornerPinsRoot = new GameObject("CornerAnchorPinsOverlay", typeof(RectTransform));
        _cornerPinsRoot.transform.SetParent(canvasTransform, false);
        var rootRt = _cornerPinsRoot.GetComponent<RectTransform>();
        Stretch(rootRt, Vector2.zero, Vector2.one);

        // 1. Esquina Superior Izquierda (0, 1)
        CreateCornerPinBadge(_cornerPinsRoot.transform, "⚓ [0, 1] Sup-Izq", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14, -14));

        // 2. Esquina Superior Derecha (1, 1)
        CreateCornerPinBadge(_cornerPinsRoot.transform, "⚓ [1, 1] Sup-Der", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-14, -14));

        // 3. Esquina Inferior Izquierda (0, 0)
        CreateCornerPinBadge(_cornerPinsRoot.transform, "⚓ [0, 0] Inf-Izq", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(14, 14));

        // 4. Esquina Inferior Derecha (1, 0)
        CreateCornerPinBadge(_cornerPinsRoot.transform, "⚓ [1, 0] Inf-Der", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-14, 14));
    }

    void CreateCornerPinBadge(Transform parent, string label, Vector2 anchor, Vector2 pivot, Vector2 offset)
    {
        var pinGo = Panel(parent, new Color(0.94f, 0.27f, 0.38f, 0.85f));
        pinGo.name = "Pin_" + label;
        var rt = pinGo.rectTransform;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.sizeDelta = new Vector2(120, 24);
        rt.anchoredPosition = offset;

        var t = CreateText(pinGo.transform, label, 11, TextAnchor.MiddleCenter, TextWhite, FontStyle.Bold);
        Stretch(t.rectTransform, Vector2.zero, Vector2.one);
    }

    // ==========================================
    // Control de Navegación y Páginas
    // ==========================================
    public void ShowPage(int index)
    {
        if (index < 0 || index >= _pages.Length) return;
        _currentPageIndex = index;

        for (int i = 0; i < _pages.Length; i++)
        {
            if (_pages[i] != null)
                _pages[i].SetActive(i == index);

            // Actualizar botones de pestaña superior
            if (_navTabIndicators != null && i < _navTabIndicators.Length && _navTabIndicators[i] != null)
            {
                _navTabIndicators[i].color = (i == index) ? AccentPrimary : Color.clear;
            }
            if (_navTabButtons != null && i < _navTabButtons.Length && _navTabButtons[i] != null)
            {
                var img = _navTabButtons[i].GetComponent<Image>();
                if (img != null) img.color = (i == index) ? CardBg : ButtonNormal;
            }

            // Actualizar dots inferiores
            if (_dotIndicators != null && i < _dotIndicators.Length && _dotIndicators[i] != null)
            {
                _dotIndicators[i].color = (i == index) ? AccentPrimary : TextDimmed;
                _dotIndicators[i].rectTransform.sizeDelta = (i == index) ? new Vector2(24, 12) : new Vector2(12, 12);
            }
        }

        string pageName = index switch
        {
            0 => "Inicio (Vista General)",
            1 => "Dispositivos y Pantalla",
            2 => "Puntos de Anclaje",
            3 => "Explorador Interactivo",
            _ => "Página " + index
        };

        if (_statusText != null)
        {
            _statusText.text = "📍 Página actual: " + pageName;
        }
    }

    void ApplyDevicePreset(int index)
    {
        if (index < 0 || index >= _devicePresets.Length) return;
        var p = _devicePresets[index];

        // Ajustar CanvasScaler para simular el comportamiento de pantalla
        _canvasScaler.referenceResolution = p.Resolution;
        
        // Si la pantalla es más alta que ancha (retrato), dar prioridad al ancho
        if (p.Resolution.y > p.Resolution.x)
        {
            _canvasScaler.matchWidthOrHeight = 0f; // Match Width
        }
        else
        {
            _canvasScaler.matchWidthOrHeight = 0.5f; // Equilibrado
        }

        AddLog("Preset aplicado: " + p.Name + " (" + p.Resolution.x + "x" + p.Resolution.y + ")");
        if (_statusText != null)
        {
            _statusText.text = "📱 Simulación configurada para: " + p.Name + " (" + p.AspectRatio + ")";
        }
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
            "• <b>Resolución Actual:</b> " + (int)width + " × " + (int)height + " px\n" +
            "• <b>Relación de Aspecto:</b> " + ratio.ToString("F2") + ":1\n" +
            "• <b>Orientación:</b> " + orientation + "\n" +
            "• <b>Factor de Escala UI:</b> " + scale.ToString("F2") + "x\n" +
            "• <b>DPI de Pantalla:</b> " + (Screen.dpi > 0 ? Screen.dpi.ToString("F0") : "Estándar") + "\n" +
            "• <b>Match Width/Height:</b> " + (_canvasScaler != null ? _canvasScaler.matchWidthOrHeight.ToString("F2") : "0.50");
    }

    void UpdateCounterDisplay()
    {
        if (_counterValueText != null)
        {
            _counterValueText.text = _interactiveCounter.ToString();
        }
    }

    void AddLog(string message)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        string entry = "<color=#3D82FF>[" + timestamp + "]</color> " + message;
        _logEntries.Insert(0, entry);

        if (_logEntries.Count > 10)
        {
            _logEntries.RemoveAt(_logEntries.Count - 1);
        }

        if (_logConsoleContent != null)
        {
            _logConsoleContent.text = string.Join("\n", _logEntries);
        }
    }

    // ==========================================
    // Helpers de Componentes Visuales
    // ==========================================
    void CreateFeatureCard(Transform parent, string title, string description, Color accent, string btnLabel, Action onAction)
    {
        var card = Panel(parent, CardBg);
        card.name = "Card_" + title;
        Flex(card.gameObject, flexW: 1, flexH: 1);

        var cardV = card.gameObject.AddComponent<VerticalLayoutGroup>();
        cardV.padding = new RectOffset(20, 20, 18, 18);
        cardV.spacing = 10;
        cardV.childControlWidth = true;
        cardV.childControlHeight = true;
        cardV.childForceExpandWidth = true;
        cardV.childForceExpandHeight = false;

        var topBadge = CreateBadge(card.transform, "CARACTERÍSTICA", accent);
        Size(topBadge.gameObject, prefH: 22);

        var t = CreateText(card.transform, title, 16, TextAnchor.MiddleLeft, TextWhite, FontStyle.Bold);
        Size(t.gameObject, prefH: 24);

        var d = CreateText(card.transform, description, 13, TextAnchor.UpperLeft, TextMuted);
        Flex(d.gameObject, flexH: 1);

        var b = CreateButton(card.transform, btnLabel, accent, onAction);
        Size(b.gameObject, prefH: 40);
    }

    Image CreateBadge(Transform parent, string text, Color color)
    {
        var badge = Panel(parent, new Color(color.r, color.g, color.b, 0.25f));
        badge.name = "Badge_" + text;
        var t = CreateText(badge.transform, text, 11, TextAnchor.MiddleCenter, color, FontStyle.Bold);
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

        var t = CreateText(img.transform, label, 14, TextAnchor.MiddleCenter, TextWhite, FontStyle.Bold);
        Stretch(t.rectTransform, Vector2.zero, Vector2.one);
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

    // ==========================================
    // Layout y Dimensionamiento Helpers
    // ==========================================
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
    // Generadores Procedurales de Texturas / Sprites
    // ==========================================
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

    static float RoundedSdf(float px, float py, float bx, float by, float r)
    {
        float qx = Mathf.Abs(px) - bx + r;
        float qy = Mathf.Abs(py) - by + r;
        float outside = Mathf.Sqrt(Mathf.Max(qx, 0) * Mathf.Max(qx, 0) + Mathf.Max(qy, 0) * Mathf.Max(qy, 0));
        return Mathf.Min(Mathf.Max(qx, qy), 0f) + outside - r;
    }
}
