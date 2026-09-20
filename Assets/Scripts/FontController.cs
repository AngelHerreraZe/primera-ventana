using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Componente interactivo para controlar fuentes tipográficas, tamaños, estilos, colores y efectos en Unity.
/// Permite ser fijado a cualquier GameObject con componente Text y expone métodos públicos para ser controlados mediante botones.
/// </summary>
[RequireComponent(typeof(Text))]
public class FontController : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Componente Text objetivo a controlar.")]
    public Text targetText;

    [Header("Configuración de Fuentes")]
    [Tooltip("Fuentes tipográficas disponibles para alternar.")]
    public Font[] customFonts;

    [Header("Efectos Opcionales")]
    public Shadow shadowEffect;
    public Outline outlineEffect;

    [Header("Eventos y Notificaciones")]
    public Action<string> OnStatusMessage;
    public Action OnFontPropertiesUpdated;

    // Estado interno
    private string _originalRawText = "";
    private int _currentFontIndex = 0;
    private List<Font> _availableFonts = new List<Font>();
    private List<string> _availableFontNames = new List<string>();

    public int CurrentFontSize => targetText != null ? targetText.fontSize : 14;
    public FontStyle CurrentFontStyle => targetText != null ? targetText.fontStyle : FontStyle.Normal;
    public TextAnchor CurrentAlignment => targetText != null ? targetText.alignment : TextAnchor.UpperLeft;
    public Color CurrentColor => targetText != null ? targetText.color : Color.white;
    public string CurrentFontName => _availableFontNames.Count > _currentFontIndex && _currentFontIndex >= 0 ? _availableFontNames[_currentFontIndex] : "Predeterminada";

    void Awake()
    {
        if (targetText == null) targetText = GetComponent<Text>();
        if (targetText != null) _originalRawText = targetText.text;

        InitializeFontsList();
        EnsureEffectsComponents();
    }

    private void InitializeFontsList()
    {
        _availableFonts.Clear();
        _availableFontNames.Clear();

        // 1. Fuente estándar del sistema
        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (defaultFont != null)
        {
            _availableFonts.Add(defaultFont);
            _availableFontNames.Add("Estándar (Sans-Serif)");
        }

        // 2. Fuentes personalizadas configuradas en el Inspector
        if (customFonts != null)
        {
            foreach (var f in customFonts)
            {
                if (f != null && !_availableFonts.Contains(f))
                {
                    _availableFonts.Add(f);
                    _availableFontNames.Add(f.name);
                }
            }
        }

        // 3. Cargar fuentes instaladas en el sistema operativo si están disponibles
        try
        {
            string[] osFonts = Font.GetOSInstalledFontNames();
            if (osFonts != null && osFonts.Length > 0)
            {
                // Agregar fuentes populares si existen en el SO (Arial, Times, Courier, Segoe UI, Verdana)
                string[] desired = { "Arial", "Segoe UI", "Calibri", "Verdana", "Times New Roman", "Courier New", "Comic Sans MS", "Trebuchet MS", "Impact" };
                foreach (var fontName in desired)
                {
                    foreach (var osF in osFonts)
                    {
                        if (osF.Equals(fontName, StringComparison.OrdinalIgnoreCase))
                        {
                            Font loaded = Font.CreateDynamicFontFromOSFont(osF, 16);
                            if (loaded != null && !_availableFonts.Contains(loaded))
                            {
                                _availableFonts.Add(loaded);
                                _availableFontNames.Add(fontName);
                            }
                            break;
                        }
                    }
                }
            }
        }
        catch (Exception)
        {
            // Ignorar si el entorno restringe acceso a fuentes OS
        }

        // Si no hay fuentes, asegurar al menos una
        if (_availableFonts.Count == 0 && targetText != null && targetText.font != null)
        {
            _availableFonts.Add(targetText.font);
            _availableFontNames.Add(targetText.font.name);
        }
    }

    private void EnsureEffectsComponents()
    {
        if (shadowEffect == null) shadowEffect = GetComponent<Shadow>();
        if (outlineEffect == null) outlineEffect = GetComponent<Outline>();
    }

    // ==========================================
    // Métodos Públicos para Control por Botones
    // ==========================================

    /// <summary>
    /// Cambia a la siguiente o anterior fuente disponible en el catálogo.
    /// </summary>
    public void NextFont()
    {
        if (_availableFonts.Count == 0) return;
        _currentFontIndex = (_currentFontIndex + 1) % _availableFonts.Count;
        ApplyFontByIndex(_currentFontIndex);
    }

    public void PreviousFont()
    {
        if (_availableFonts.Count == 0) return;
        _currentFontIndex = (_currentFontIndex - 1 + _availableFonts.Count) % _availableFonts.Count;
        ApplyFontByIndex(_currentFontIndex);
    }

    public void SetFontByName(string fontName)
    {
        for (int i = 0; i < _availableFontNames.Count; i++)
        {
            if (_availableFontNames[i].IndexOf(fontName, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                _currentFontIndex = i;
                ApplyFontByIndex(i);
                return;
            }
        }
    }

    private void ApplyFontByIndex(int index)
    {
        if (index < 0 || index >= _availableFonts.Count || targetText == null) return;
        targetText.font = _availableFonts[index];
        string name = _availableFontNames[index];
        OnStatusMessage?.Invoke($"🔤 Familia tipográfica cambiada a: {name}");
        OnFontPropertiesUpdated?.Invoke();
    }

    /// <summary>
    /// Aumenta o disminuye el tamaño de la fuente en incrementos.
    /// </summary>
    public void IncreaseFontSize(int delta = 2)
    {
        if (targetText == null) return;
        targetText.fontSize = Mathf.Clamp(targetText.fontSize + delta, 10, 72);
        OnStatusMessage?.Invoke($"📏 Tamaño de fuente: {targetText.fontSize} px");
        OnFontPropertiesUpdated?.Invoke();
    }

    public void DecreaseFontSize(int delta = 2)
    {
        if (targetText == null) return;
        targetText.fontSize = Mathf.Clamp(targetText.fontSize - delta, 10, 72);
        OnStatusMessage?.Invoke($"📏 Tamaño de fuente: {targetText.fontSize} px");
        OnFontPropertiesUpdated?.Invoke();
    }

    /// <summary>
    /// Asigna un tamaño exacto predeterminado (Small, Medium, Large, Extra).
    /// </summary>
    public void SetFontSize(int exactSize)
    {
        if (targetText == null) return;
        targetText.fontSize = Mathf.Clamp(exactSize, 10, 72);
        OnStatusMessage?.Invoke($"📏 Tamaño de fuente fijado en: {exactSize} px");
        OnFontPropertiesUpdated?.Invoke();
    }

    /// <summary>
    /// Cambia el estilo tipográfico: Normal, Negrita, Cursiva, Negrita-Cursiva.
    /// </summary>
    public void SetFontStyle(FontStyle style)
    {
        if (targetText == null) return;
        targetText.fontStyle = style;
        string styleName = style switch
        {
            FontStyle.Normal => "Normal",
            FontStyle.Bold => "Negrita (Bold)",
            FontStyle.Italic => "Cursiva (Italic)",
            FontStyle.BoldAndItalic => "Negrita Cursiva (Bold & Italic)",
            _ => style.ToString()
        };
        OnStatusMessage?.Invoke($"✍️ Estilo de texto: {styleName}");
        OnFontPropertiesUpdated?.Invoke();
    }

    public void ToggleBold()
    {
        if (targetText == null) return;
        if (targetText.fontStyle == FontStyle.Normal) SetFontStyle(FontStyle.Bold);
        else if (targetText.fontStyle == FontStyle.Bold) SetFontStyle(FontStyle.Normal);
        else if (targetText.fontStyle == FontStyle.Italic) SetFontStyle(FontStyle.BoldAndItalic);
        else if (targetText.fontStyle == FontStyle.BoldAndItalic) SetFontStyle(FontStyle.Italic);
    }

    public void ToggleItalic()
    {
        if (targetText == null) return;
        if (targetText.fontStyle == FontStyle.Normal) SetFontStyle(FontStyle.Italic);
        else if (targetText.fontStyle == FontStyle.Italic) SetFontStyle(FontStyle.Normal);
        else if (targetText.fontStyle == FontStyle.Bold) SetFontStyle(FontStyle.BoldAndItalic);
        else if (targetText.fontStyle == FontStyle.BoldAndItalic) SetFontStyle(FontStyle.Bold);
    }

    /// <summary>
    /// Cambia la alineación del texto: Izquierda, Centro, Derecha.
    /// </summary>
    public void SetAlignment(TextAnchor alignment)
    {
        if (targetText == null) return;
        targetText.alignment = alignment;
        string alignStr = alignment switch
        {
            TextAnchor.UpperLeft or TextAnchor.MiddleLeft or TextAnchor.LowerLeft => "Izquierda",
            TextAnchor.UpperCenter or TextAnchor.MiddleCenter or TextAnchor.LowerCenter => "Centro",
            TextAnchor.UpperRight or TextAnchor.MiddleRight or TextAnchor.LowerRight => "Derecha",
            _ => alignment.ToString()
        };
        OnStatusMessage?.Invoke($"📐 Alineación: {alignStr}");
        OnFontPropertiesUpdated?.Invoke();
    }

    /// <summary>
    /// Cambia el color del texto mediante paleta de botones.
    /// </summary>
    public void SetTextColor(Color color)
    {
        if (targetText == null) return;
        targetText.color = color;
        OnStatusMessage?.Invoke($"🎨 Color de texto aplicado: #{ColorUtility.ToHtmlStringRGBA(color)}");
        OnFontPropertiesUpdated?.Invoke();
    }

    /// <summary>
    /// Alterna efecto de sombra (Drop Shadow).
    /// </summary>
    public bool ToggleShadow()
    {
        if (shadowEffect == null)
        {
            shadowEffect = gameObject.AddComponent<Shadow>();
            shadowEffect.effectColor = new Color(0, 0, 0, 0.75f);
            shadowEffect.effectDistance = new Vector2(2f, -2f);
            shadowEffect.useGraphicAlpha = true;
            OnStatusMessage?.Invoke("✨ Sombra de texto: ACTIVADA");
            OnFontPropertiesUpdated?.Invoke();
            return true;
        }

        shadowEffect.enabled = !shadowEffect.enabled;
        OnStatusMessage?.Invoke($"✨ Sombra de texto: {(shadowEffect.enabled ? "ACTIVADA" : "DESACTIVADA")}");
        OnFontPropertiesUpdated?.Invoke();
        return shadowEffect.enabled;
    }

    /// <summary>
    /// Alterna efecto de contorno (Outline).
    /// </summary>
    public bool ToggleOutline()
    {
        if (outlineEffect == null)
        {
            outlineEffect = gameObject.AddComponent<Outline>();
            outlineEffect.effectColor = new Color(0.1f, 0.1f, 0.2f, 0.9f);
            outlineEffect.effectDistance = new Vector2(1.5f, -1.5f);
            outlineEffect.useGraphicAlpha = true;
            OnStatusMessage?.Invoke("🖋️ Contorno de texto: ACTIVADO");
            OnFontPropertiesUpdated?.Invoke();
            return true;
        }

        outlineEffect.enabled = !outlineEffect.enabled;
        OnStatusMessage?.Invoke($"🖋️ Contorno de texto: {(outlineEffect.enabled ? "ACTIVADO" : "DESACTIVADO")}");
        OnFontPropertiesUpdated?.Invoke();
        return outlineEffect.enabled;
    }

    /// <summary>
    /// Transforma el contenido del texto a mayúsculas, minúsculas o texto original.
    /// </summary>
    public void SetTextCase(int mode) // 0 = original, 1 = MAYÚSCULAS, 2 = minúsculas
    {
        if (targetText == null) return;
        if (string.IsNullOrEmpty(_originalRawText)) _originalRawText = targetText.text;

        targetText.text = mode switch
        {
            1 => _originalRawText.ToUpper(),
            2 => _originalRawText.ToLower(),
            _ => _originalRawText
        };
        OnStatusMessage?.Invoke($"🔠 Formato de mayúsculas/minúsculas actualizado.");
    }

    /// <summary>
    /// Actualiza el contenido del texto en vivo.
    /// </summary>
    public void SetSampleText(string customText)
    {
        if (targetText == null) return;
        _originalRawText = customText;
        targetText.text = customText;
    }

    public List<string> GetAvailableFontNames() => _availableFontNames;
}
