using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI; // new Input System (activeInputHandler = 1)

// Ventana responsiva construida por código: 1 título, 4 iconos, 3 botones y
// 3 enlaces a sitios web por el costado. Se auto-instancia al pulsar Play en
// cualquier escena, así no hay que tocar YAML de escena ni conectar nada.
// ponytail: todo en un archivo, se arma en runtime; para una UI de producción
// se movería a un prefab/escena editable.
public class PrimeraVentana : MonoBehaviour
{
    // Paleta
    static readonly Color Card    = new Color(0.97f, 0.97f, 0.98f);
    static readonly Color Ink     = new Color(0.12f, 0.14f, 0.19f);
    static readonly Color Accent  = new Color(0.31f, 0.42f, 1.00f);
    static readonly Color SideBg  = new Color(0.93f, 0.945f, 0.97f);
    static readonly Color[] IconColors =
    {
        new Color(1.00f, 0.42f, 0.42f), // rojo
        new Color(0.31f, 0.80f, 0.77f), // turquesa
        new Color(1.00f, 0.85f, 0.24f), // amarillo
        new Color(0.42f, 0.80f, 0.47f), // verde
    };

    Sprite _rounded;   // sprite blanco 9-slice reutilizado y tintado
    Text _status;      // etiqueta que confirma la interacción

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (FindFirstObjectByType<PrimeraVentana>() != null) return;
        new GameObject("PrimeraVentana").AddComponent<PrimeraVentana>();
    }

    void Awake()
    {
        _rounded = MakeRounded();
        EnsureEventSystem();
        Build();
    }

    // ---------- Construcción de la ventana ----------
    void Build()
    {
        // Canvas + escalador que adapta la UI a distintas resoluciones.
        var canvasGo = new GameObject("Canvas",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(transform, false);
        canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // Fondo atenuado a pantalla completa.
        var backdrop = Panel(canvasGo.transform, new Color(0.06f, 0.07f, 0.10f, 0.85f));
        Stretch(backdrop.rectTransform, Vector2.zero, Vector2.one);

        // Tarjeta central: anclada con márgenes relativos => crece con la pantalla.
        var card = Panel(canvasGo.transform, Card);
        Stretch(card.rectTransform, new Vector2(0.06f, 0.08f), new Vector2(0.94f, 0.92f));
        var cardH = card.gameObject.AddComponent<HorizontalLayoutGroup>();
        cardH.padding = new RectOffset(24, 24, 24, 24);
        cardH.spacing = 20;
        cardH.childControlWidth = cardH.childControlHeight = true;
        cardH.childForceExpandWidth = false;
        cardH.childForceExpandHeight = true;

        BuildMainColumn(card.transform);
        BuildSideColumn(card.transform);
    }

    void BuildMainColumn(Transform parent)
    {
        var col = Column(parent, 16);
        Flex(col.gameObject, flexW: 1);

        var title = CreateText(col.transform, "Mi Primera Ventana", 34, TextAnchor.MiddleLeft, Ink, FontStyle.Bold);
        Size(title.gameObject, prefH: 56);

        // Fila de 4 iconos.
        var iconsRow = Row(col.transform, 12);
        iconsRow.childForceExpandHeight = false; // iconos cuadrados, no estirados
        Size(iconsRow.gameObject, prefH: 72);
        string[] glyphs = { "★", "♥", "♪", "✓" };
        for (int i = 0; i < 4; i++) CreateIcon(iconsRow.transform, glyphs[i], IconColors[i]);

        // Empuja el resto hacia abajo.
        var spacer = new GameObject("Spacer", typeof(RectTransform));
        spacer.transform.SetParent(col.transform, false);
        Flex(spacer, flexH: 1);

        _status = CreateText(col.transform, "Pulsa un botón…", 16, TextAnchor.MiddleLeft,
            new Color(0.45f, 0.48f, 0.55f), FontStyle.Italic);
        Size(_status.gameObject, prefH: 24);

        // Fila de 3 botones que se reparten el ancho.
        var btnRow = Row(col.transform, 12);
        Size(btnRow.gameObject, prefH: 52);
        for (int i = 1; i <= 3; i++)
        {
            int n = i; // captura para el closure
            var b = CreateButton(btnRow.transform, "Botón " + n, Accent);
            Flex(b.gameObject, flexW: 1);
            b.onClick.AddListener(() => _status.text = "Botón " + n + " presionado");
        }
    }

    void BuildSideColumn(Transform parent)
    {
        var col = Column(parent, 10);
        Size(col.gameObject, prefW: 220);
        Flex(col.gameObject, flexW: 0);

        var head = CreateText(col.transform, "Enlaces", 18, TextAnchor.MiddleLeft, Ink, FontStyle.Bold);
        Size(head.gameObject, prefH: 30);

        CreateLink(col.transform, "Unity",         "https://unity.com");
        CreateLink(col.transform, "Documentación", "https://docs.unity3d.com");
        CreateLink(col.transform, "GitHub",        "https://github.com");

        var tail = new GameObject("Spacer", typeof(RectTransform));
        tail.transform.SetParent(col.transform, false);
        Flex(tail, flexH: 1);
    }

    // ---------- Helpers de widgets ----------
    Image Panel(Transform parent, Color color)
    {
        var go = new GameObject("Panel", typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.sprite = _rounded;
        img.type = Image.Type.Sliced;
        img.pixelsPerUnitMultiplier = 1f;
        img.color = color;
        return img;
    }

    VerticalLayoutGroup Column(Transform parent, float spacing)
    {
        var go = new GameObject("Column", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var v = go.AddComponent<VerticalLayoutGroup>();
        v.spacing = spacing;
        v.childControlWidth = v.childControlHeight = true;
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
        h.childControlWidth = h.childControlHeight = true;
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
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        return t;
    }

    void CreateIcon(Transform parent, string glyph, Color color)
    {
        var chip = Panel(parent, color);
        chip.name = "Icon";
        Size(chip.gameObject, prefW: 64, prefH: 64);
        var t = CreateText(chip.transform, glyph, 28, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
        Stretch(t.rectTransform, Vector2.zero, Vector2.one);
    }

    Button CreateButton(Transform parent, string label, Color bg)
    {
        var img = Panel(parent, bg);
        img.name = "Button";
        var btn = img.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        var t = CreateText(img.transform, label, 18, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold);
        Stretch(t.rectTransform, Vector2.zero, Vector2.one);
        return btn;
    }

    void CreateLink(Transform parent, string label, string url)
    {
        var img = Panel(parent, SideBg);
        img.name = "Link";
        Size(img.gameObject, prefH: 44);
        var btn = img.gameObject.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() => Application.OpenURL(url));
        var t = CreateText(img.transform, "↗  " + label, 15, TextAnchor.MiddleLeft, Accent, FontStyle.Bold);
        Stretch(t.rectTransform, new Vector2(0, 0), new Vector2(1, 1));
        t.rectTransform.offsetMin = new Vector2(14, 0);
        t.rectTransform.offsetMax = new Vector2(-8, 0);
    }

    // ---------- Helpers de layout ----------
    static void Stretch(RectTransform rt, Vector2 min, Vector2 max)
    {
        rt.anchorMin = min; rt.anchorMax = max;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
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
        if (FindFirstObjectByType<EventSystem>() != null) return;
        var go = new GameObject("EventSystem", typeof(EventSystem));
        go.AddComponent<InputSystemUIInputModule>(); // requerido con el nuevo Input System
    }

    // ---------- Sprite de rectángulo redondeado (blanco, 9-slice) ----------
    static Sprite MakeRounded(int size = 64, int radius = 20)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        float b = size * 0.5f;
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float sd = RoundedSdf(x + 0.5f - b, y + 0.5f - b, b, b, radius);
            float a = Mathf.Clamp01(0.5f - sd); // antialias de ~1px en el borde
            tex.SetPixel(x, y, new Color(1, 1, 1, a));
        }
        tex.Apply();
        var sp = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f),
            100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        return sp;
    }

    // SDF de rectángulo redondeado: <0 dentro, 0 en el borde, >0 fuera.
    static float RoundedSdf(float px, float py, float bx, float by, float r)
    {
        float qx = Mathf.Abs(px) - bx + r;
        float qy = Mathf.Abs(py) - by + r;
        float outside = Mathf.Sqrt(Mathf.Max(qx, 0) * Mathf.Max(qx, 0) + Mathf.Max(qy, 0) * Mathf.Max(qy, 0));
        return Mathf.Min(Mathf.Max(qx, qy), 0f) + outside - r;
    }
}
