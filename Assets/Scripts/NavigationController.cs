using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controlador de Navegación Profesional para interfaces y escenas en Unity.
/// Permite gestionar páginas, pestañas, avance/retroceso con pila de historial (Back/Forward), migas de pan (Breadcrumbs) y botones de acceso directo.
/// </summary>
public class NavigationController : MonoBehaviour
{
    [System.Serializable]
    public class NavigationPage
    {
        public string id;
        public string title;
        public string icon;
        public GameObject panelObject;
        public Button tabButton;
        public Image tabIndicator;
    }

    [Header("Configuración de Páginas")]
    public List<NavigationPage> pages = new List<NavigationPage>();

    [Header("Elementos de Navegación UI")]
    public Button btnBack;
    public Button btnForward;
    public Button btnHome;
    public Button btnNext;
    public Button btnPrev;
    public Text txtBreadcrumbs;
    public Text txtCurrentPageTitle;
    public Image[] dotIndicators;

    [Header("Eventos")]
    public Action<int, NavigationPage> OnPageChanged;
    public Action<string> OnBreadcrumbsChanged;
    public Action<bool, bool> OnNavigationHistoryChanged; // canBack, canForward
    public Action<string> OnStatusLog;

    // Pilas de historial para navegación real tipo navegador
    private readonly Stack<int> _backStack = new Stack<int>();
    private readonly Stack<int> _forwardStack = new Stack<int>();
    private readonly List<string> _breadcrumbTrail = new List<string>();
    private int _currentPageIndex = -1;

    public int CurrentPageIndex => _currentPageIndex;
    public int PageCount => pages.Count;
    public bool CanGoBack => _backStack.Count > 0;
    public bool CanGoForward => _forwardStack.Count > 0;

    void Awake()
    {
        BindButtonEvents();
    }

    private void BindButtonEvents()
    {
        if (btnBack != null) btnBack.onClick.AddListener(GoBack);
        if (btnForward != null) btnForward.onClick.AddListener(GoForward);
        if (btnHome != null) btnHome.onClick.AddListener(GoHome);
        if (btnNext != null) btnNext.onClick.AddListener(NextPage);
        if (btnPrev != null) btnPrev.onClick.AddListener(PreviousPage);
    }

    public void RegisterPage(string id, string title, string icon, GameObject panel, Button tabBtn = null, Image tabInd = null)
    {
        var page = new NavigationPage
        {
            id = id,
            title = title,
            icon = icon,
            panelObject = panel,
            tabButton = tabBtn,
            tabIndicator = tabInd
        };

        int idx = pages.Count;
        pages.Add(page);

        if (tabBtn != null)
        {
            tabBtn.onClick.AddListener(() => NavigateTo(idx));
        }
    }

    // ==========================================
    // Métodos Principales de Navegación
    // ==========================================

    /// <summary>
    /// Navega a una página específica por índice, guardando el historial.
    /// </summary>
    public void NavigateTo(int index, bool recordHistory = true)
    {
        if (index < 0 || index >= pages.Count) return;
        if (index == _currentPageIndex) return;

        if (recordHistory && _currentPageIndex >= 0)
        {
            _backStack.Push(_currentPageIndex);
            _forwardStack.Clear(); // Nueva rama en el historial
        }

        ApplyPageTransition(index);
    }

    /// <summary>
    /// Navega a una página buscando su identificador o título.
    /// </summary>
    public void NavigateTo(string pageIdOrTitle)
    {
        for (int i = 0; i < pages.Count; i++)
        {
            if (pages[i].id.Equals(pageIdOrTitle, StringComparison.OrdinalIgnoreCase) ||
                pages[i].title.Equals(pageIdOrTitle, StringComparison.OrdinalIgnoreCase))
            {
                NavigateTo(i);
                return;
            }
        }
    }

    /// <summary>
    /// Retrocede a la página anterior en el historial de navegación.
    /// </summary>
    public void GoBack()
    {
        if (_backStack.Count == 0)
        {
            OnStatusLog?.Invoke("ℹ️ No hay más páginas en el historial hacia atrás.");
            return;
        }

        int prevIndex = _backStack.Pop();
        _forwardStack.Push(_currentPageIndex);
        ApplyPageTransition(prevIndex);
        OnStatusLog?.Invoke($"◀ Retrocediendo a: {pages[prevIndex].title}");
    }

    /// <summary>
    /// Avanza en el historial hacia la página siguiente.
    /// </summary>
    public void GoForward()
    {
        if (_forwardStack.Count == 0)
        {
            OnStatusLog?.Invoke("ℹ️ No hay páginas hacia adelante en el historial.");
            return;
        }

        int nextIndex = _forwardStack.Pop();
        _backStack.Push(_currentPageIndex);
        ApplyPageTransition(nextIndex);
        OnStatusLog?.Invoke($"▶ Avanzando a: {pages[nextIndex].title}");
    }

    /// <summary>
    /// Regresa a la página de Inicio (índice 0).
    /// </summary>
    public void GoHome()
    {
        NavigateTo(0);
        OnStatusLog?.Invoke("🏠 Navegando al Inicio del sistema.");
    }

    /// <summary>
    /// Avanza de forma circular a la siguiente página disponible.
    /// </summary>
    public void NextPage()
    {
        if (pages.Count == 0) return;
        int next = (_currentPageIndex + 1) % pages.Count;
        NavigateTo(next);
    }

    /// <summary>
    /// Retrocede de forma circular a la página anterior.
    /// </summary>
    public void PreviousPage()
    {
        if (pages.Count == 0) return;
        int prev = (_currentPageIndex - 1 + pages.Count) % pages.Count;
        NavigateTo(prev);
    }

    // ==========================================
    // Actualización de Estado y UI
    // ==========================================

    private void ApplyPageTransition(int targetIndex)
    {
        _currentPageIndex = targetIndex;

        // 1. Activar / Desactivar Paneles de Contenido
        for (int i = 0; i < pages.Count; i++)
        {
            if (pages[i].panelObject != null)
            {
                pages[i].panelObject.SetActive(i == targetIndex);
            }

            // Actualizar pestañas superiores
            if (pages[i].tabIndicator != null)
            {
                pages[i].tabIndicator.color = (i == targetIndex) ? new Color(0.24f, 0.51f, 0.98f, 1f) : Color.clear;
            }
            if (pages[i].tabButton != null)
            {
                var img = pages[i].tabButton.GetComponent<Image>();
                if (img != null)
                {
                    img.color = (i == targetIndex) ? new Color(0.16f, 0.20f, 0.29f, 1f) : new Color(0.12f, 0.15f, 0.22f, 0.9f);
                }
            }
        }

        // 2. Actualizar Indicadores de Puntos (Dots)
        if (dotIndicators != null)
        {
            for (int i = 0; i < dotIndicators.Length; i++)
            {
                if (dotIndicators[i] != null)
                {
                    dotIndicators[i].color = (i == targetIndex) ? new Color(0.24f, 0.51f, 0.98f, 1f) : new Color(0.40f, 0.44f, 0.54f, 1f);
                    dotIndicators[i].rectTransform.sizeDelta = (i == targetIndex) ? new Vector2(24, 12) : new Vector2(12, 12);
                }
            }
        }

        // 3. Actualizar Migas de Pan (Breadcrumbs)
        UpdateBreadcrumbs(targetIndex);

        // 4. Actualizar estado de botones Back/Forward
        UpdateNavigationButtonsState();

        // 5. Notificar Eventos
        var currentPage = pages[targetIndex];
        if (txtCurrentPageTitle != null)
        {
            txtCurrentPageTitle.text = $"{currentPage.icon} {currentPage.title}";
        }

        OnPageChanged?.Invoke(targetIndex, currentPage);
    }

    private void UpdateBreadcrumbs(int index)
    {
        var targetPage = pages[index];
        _breadcrumbTrail.Clear();
        _breadcrumbTrail.Add("Inicio");

        if (index > 0)
        {
            _breadcrumbTrail.Add(targetPage.title);
        }

        string trailText = string.Join("  ›  ", _breadcrumbTrail);
        if (txtBreadcrumbs != null)
        {
            txtBreadcrumbs.text = trailText;
        }

        OnBreadcrumbsChanged?.Invoke(trailText);
    }

    private void UpdateNavigationButtonsState()
    {
        if (btnBack != null)
        {
            btnBack.interactable = CanGoBack;
            var img = btnBack.GetComponent<Image>();
            if (img != null) img.color = CanGoBack ? new Color(0.18f, 0.23f, 0.33f, 1f) : new Color(0.12f, 0.14f, 0.18f, 0.5f);
        }

        if (btnForward != null)
        {
            btnForward.interactable = CanGoForward;
            var img = btnForward.GetComponent<Image>();
            if (img != null) img.color = CanGoForward ? new Color(0.18f, 0.23f, 0.33f, 1f) : new Color(0.12f, 0.14f, 0.18f, 0.5f);
        }

        OnNavigationHistoryChanged?.Invoke(CanGoBack, CanGoForward);
    }
}
