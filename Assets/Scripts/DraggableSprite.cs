using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Componente interactivo para hacer que cualquier Sprite o Imagen en Unity sea arrastrable mediante el ratón o pantalla táctil.
/// Implementa interfaces del EventSystem de Unity: IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, etc.
/// Permite ser controlado tanto por interacción directa como mediante botones externos.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class DraggableSprite : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Configuración de Arrastre")]
    [Tooltip("Área límite dentro de la cual el sprite puede ser arrastrado.")]
    public RectTransform containmentArea;

    [Tooltip("Indica si el arrastre está actualmente habilitado o bloqueado.")]
    public bool isLocked = false;

    [Tooltip("Multiplicador de escala al hacer clic o arrastrar.")]
    public float dragScaleFactor = 1.12f;

    [Tooltip("Velocidad de transición suave de posición o escala.")]
    public float smoothSpeed = 12f;

    [Header("Referencias Visuales")]
    public Image spriteImage;
    public Image shadowImage;

    [Header("Eventos y Notificaciones")]
    public Action<Vector2> OnPositionChanged;
    public Action<string> OnStatusMessage;
    public Action OnDragBeginEvent;
    public Action OnDragEndEvent;

    // Variables internas de estado
    private RectTransform _rectTransform;
    private Canvas _rootCanvas;
    private Vector2 _initialPosition;
    private Vector3 _originalScale;
    private Vector3 _targetScale;
    private bool _isDragging = false;
    private bool _isPointerOver = false;
    private Color _baseColor = Color.white;
    private float _currentRotation = 0f;

    public Vector2 CurrentPosition => _rectTransform != null ? _rectTransform.anchoredPosition : Vector2.zero;
    public bool IsDragging => _isDragging;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        if (spriteImage == null) spriteImage = GetComponent<Image>();
        _originalScale = transform.localScale;
        _targetScale = _originalScale;
        _initialPosition = _rectTransform.anchoredPosition;

        if (spriteImage != null)
        {
            _baseColor = spriteImage.color;
        }

        FindRootCanvas();
    }

    void Start()
    {
        if (containmentArea == null && transform.parent != null)
        {
            containmentArea = transform.parent.GetComponent<RectTransform>();
        }
    }

    void Update()
    {
        // Interpolación suave de escala para respuesta táctil / visual premium
        if (transform.localScale != _targetScale)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, Time.unscaledDeltaTime * smoothSpeed);
        }
    }

    private void FindRootCanvas()
    {
        _rootCanvas = GetComponentInParent<Canvas>();
    }

    // ==========================================
    // Implementación de Interfaces de EventSystem
    // ==========================================

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isLocked)
        {
            OnStatusMessage?.Invoke("⚠️ El sprite está bloqueado. Usa el botón 'Desbloquear' para moverlo.");
            return;
        }

        _targetScale = _originalScale * dragScaleFactor;
        transform.SetAsLastSibling(); // Traer al frente visualmente
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_isDragging)
        {
            _targetScale = _isPointerOver ? _originalScale * 1.05f : _originalScale;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isPointerOver = true;
        if (!_isDragging && !isLocked)
        {
            _targetScale = _originalScale * 1.05f;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isPointerOver = false;
        if (!_isDragging)
        {
            _targetScale = _originalScale;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isLocked) return;

        _isDragging = true;
        _targetScale = _originalScale * dragScaleFactor;
        transform.SetAsLastSibling();

        if (shadowImage != null)
        {
            shadowImage.gameObject.SetActive(true);
        }

        OnDragBeginEvent?.Invoke();
        OnStatusMessage?.Invoke("✋ Arrastrando Sprite: Posición libre en coordenadas de pantalla.");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isLocked || !_isDragging) return;

        if (_rootCanvas == null) FindRootCanvas();

        // Conversión precisa de coordenadas de pantalla a posición local dentro del contenedor
        RectTransform parentRect = containmentArea != null ? containmentArea : _rectTransform.parent as RectTransform;

        if (parentRect != null)
        {
            Camera cam = _rootCanvas != null && _rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay ? _rootCanvas.worldCamera : null;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, eventData.position, cam, out Vector2 localPoint))
            {
                // Limitar movimiento dentro de los bordes del contenedor si está definido
                if (containmentArea != null)
                {
                    localPoint = ClampToContainer(localPoint, containmentArea, _rectTransform);
                }

                _rectTransform.localPosition = localPoint;
                OnPositionChanged?.Invoke(_rectTransform.anchoredPosition);
            }
        }
        else
        {
            // Movimiento directo por delta si no hay contenedor padre
            float scaleFactor = _rootCanvas != null ? _rootCanvas.scaleFactor : 1f;
            _rectTransform.anchoredPosition += eventData.delta / scaleFactor;
            OnPositionChanged?.Invoke(_rectTransform.anchoredPosition);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;

        _isDragging = false;
        _targetScale = _isPointerOver ? _originalScale * 1.05f : _originalScale;

        OnDragEndEvent?.Invoke();
        OnStatusMessage?.Invoke($"✅ Sprite colocado en ({_rectTransform.anchoredPosition.x:F0}, {_rectTransform.anchoredPosition.y:F0})");
    }

    private Vector2 ClampToContainer(Vector2 targetPos, RectTransform container, RectTransform element)
    {
        Rect cRect = container.rect;
        Vector2 eSize = element.sizeDelta * element.localScale;

        float minX = cRect.xMin + (eSize.x * element.pivot.x);
        float maxX = cRect.xMax - (eSize.x * (1f - element.pivot.x));
        float minY = cRect.yMin + (eSize.y * element.pivot.y);
        float maxY = cRect.yMax - (eSize.y * (1f - element.pivot.y));

        // Si el contenedor es más pequeño que el elemento, centrar
        if (minX > maxX) minX = maxX = (cRect.xMin + cRect.xMax) * 0.5f;
        if (minY > maxY) minY = maxY = (cRect.yMin + cRect.yMax) * 0.5f;

        return new Vector2(Mathf.Clamp(targetPos.x, minX, maxX), Mathf.Clamp(targetPos.y, minY, maxY));
    }

    // ==========================================
    // Métodos Públicos para Control por Botones
    // ==========================================

    /// <summary>
    /// Restablece la posición del Sprite a su origen inicial o al centro.
    /// </summary>
    public void ResetPosition()
    {
        if (_rectTransform != null)
        {
            _rectTransform.anchoredPosition = _initialPosition;
            _rectTransform.localRotation = Quaternion.identity;
            _currentRotation = 0f;
            transform.localScale = _originalScale;
            _targetScale = _originalScale;
            OnPositionChanged?.Invoke(_initialPosition);
            OnStatusMessage?.Invoke("🔄 Posición y orientación del Sprite restablecidas al origen.");
        }
    }

    /// <summary>
    /// Guarda la posición actual como nueva posición inicial/por defecto.
    /// </summary>
    public void SetCurrentAsInitialPosition()
    {
        if (_rectTransform != null)
        {
            _initialPosition = _rectTransform.anchoredPosition;
            OnStatusMessage?.Invoke($"📌 Nueva posición por defecto guardada: ({_initialPosition.x:F0}, {_initialPosition.y:F0})");
        }
    }

    /// <summary>
    /// Mueve el sprite al centro del área contenedora.
    /// </summary>
    public void CenterInContainer()
    {
        if (_rectTransform != null)
        {
            _rectTransform.anchoredPosition = Vector2.zero;
            OnPositionChanged?.Invoke(Vector2.zero);
            OnStatusMessage?.Invoke("🎯 Sprite centrado en el contenedor.");
        }
    }

    /// <summary>
    /// Cambia el color/tinte del Sprite mediante botones de paleta.
    /// </summary>
    public void SetColor(Color newColor)
    {
        _baseColor = newColor;
        if (spriteImage != null)
        {
            spriteImage.color = newColor;
            OnStatusMessage?.Invoke($"🎨 Color del sprite cambiado a #{ColorUtility.ToHtmlStringRGBA(newColor)}");
        }
    }

    /// <summary>
    /// Asigna una nueva textura/sprite visual al componente.
    /// </summary>
    public void SetSprite(Sprite newSprite, string spriteName = "Nuevo Sprite")
    {
        if (spriteImage != null && newSprite != null)
        {
            spriteImage.sprite = newSprite;
            OnStatusMessage?.Invoke($"🖼️ Apariencia cambiada a: {spriteName}");
        }
    }

    /// <summary>
    /// Alterna el estado de bloqueo de arrastre (Lock / Unlock).
    /// </summary>
    public bool ToggleLock()
    {
        isLocked = !isLocked;
        if (isLocked)
        {
            _isDragging = false;
            _targetScale = _originalScale;
            if (spriteImage != null)
            {
                spriteImage.color = new Color(_baseColor.r * 0.7f, _baseColor.g * 0.7f, _baseColor.b * 0.7f, _baseColor.a);
            }
            OnStatusMessage?.Invoke("🔒 Sprite BLOQUEADO. No se puede arrastrar hasta desbloquearlo.");
        }
        else
        {
            if (spriteImage != null)
            {
                spriteImage.color = _baseColor;
            }
            OnStatusMessage?.Invoke("🔓 Sprite DESBLOQUEADO. Arrastre libre habilitado.");
        }
        return isLocked;
    }

    /// <summary>
    /// Modifica el tamaño / escala del Sprite mediante botones (+ / -).
    /// </summary>
    public void ScaleDelta(float delta)
    {
        Vector3 newScale = _originalScale + Vector3.one * delta;
        float clamped = Mathf.Clamp(newScale.x, 0.5f, 2.5f);
        _originalScale = new Vector3(clamped, clamped, 1f);
        _targetScale = _originalScale;
        transform.localScale = _originalScale;
        OnStatusMessage?.Invoke($"🔍 Escala del sprite: {(clamped * 100f):F0}%");
    }

    /// <summary>
    /// Rota el Sprite en incrementos angulares dados.
    /// </summary>
    public void RotateDelta(float degrees)
    {
        _currentRotation = (_currentRotation + degrees) % 360f;
        transform.localRotation = Quaternion.Euler(0, 0, _currentRotation);
        OnStatusMessage?.Invoke($"🔄 Rotación del sprite: {_currentRotation:F0}°");
    }

    /// <summary>
    /// Posiciona el Sprite en una ubicación aleatoria dentro del área contenedora.
    /// </summary>
    public void RandomizePosition()
    {
        if (containmentArea == null) return;
        Rect r = containmentArea.rect;
        Vector2 size = _rectTransform.sizeDelta;
        float rx = UnityEngine.Random.Range(r.xMin + size.x * 0.6f, r.xMax - size.x * 0.6f);
        float ry = UnityEngine.Random.Range(r.yMin + size.y * 0.6f, r.yMax - size.y * 0.6f);
        _rectTransform.anchoredPosition = new Vector2(rx, ry);
        OnPositionChanged?.Invoke(_rectTransform.anchoredPosition);
        OnStatusMessage?.Invoke($"🎲 Posición aleatoria asignada: ({rx:F0}, {ry:F0})");
    }
}
