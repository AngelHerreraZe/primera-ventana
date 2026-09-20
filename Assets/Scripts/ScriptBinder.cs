using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Utilidad y controlador interactivo para Fijar (Attach / Bind) y Desacoplar Scripts a Objetos de Juego (GameObjects) en Unity.
/// Permite añadir componentes en tiempo de ejecución (AddComponent), inspeccionar scripts adheridos y explicar la fijación mediante Inspector y Código.
/// </summary>
public class ScriptBinder : MonoBehaviour
{
    [Header("Objetos de Juego Objetivo")]
    [Tooltip("Objeto de prueba al cual se le pueden fijar o remover scripts dinámicamente.")]
    public GameObject targetGameObject;

    [Header("Eventos y Notificaciones")]
    public Action<string> OnStatusLog;
    public Action OnComponentsChanged;

    /// <summary>
    /// Fija el script DraggableSprite al GameObject objetivo mediante AddComponent.
    /// </summary>
    public DraggableSprite AttachDraggableSprite(GameObject target = null)
    {
        GameObject go = target != null ? target : targetGameObject;
        if (go == null)
        {
            OnStatusLog?.Invoke("⚠️ Error: No hay ningún GameObject seleccionado para fijar el script.");
            return null;
        }

        var existing = go.GetComponent<DraggableSprite>();
        if (existing != null)
        {
            OnStatusLog?.Invoke($"ℹ️ El script 'DraggableSprite' ya está fijado al objeto [{go.name}].");
            return existing;
        }

        var comp = go.AddComponent<DraggableSprite>();
        OnStatusLog?.Invoke($"✅ Script 'DraggableSprite' FIJADO con éxito a [{go.name}]. ¡Ahora es arrastrable!");
        OnComponentsChanged?.Invoke();
        return comp;
    }

    /// <summary>
    /// Fija el script FontController al GameObject objetivo mediante AddComponent.
    /// </summary>
    public FontController AttachFontController(GameObject target = null)
    {
        GameObject go = target != null ? target : targetGameObject;
        if (go == null)
        {
            OnStatusLog?.Invoke("⚠️ Error: No hay ningún GameObject seleccionado.");
            return null;
        }

        var existing = go.GetComponent<FontController>();
        if (existing != null)
        {
            OnStatusLog?.Invoke($"ℹ️ El script 'FontController' ya está fijado a [{go.name}].");
            return existing;
        }

        // Asegurar que tenga un Text para que FontController funcione
        if (go.GetComponent<Text>() == null)
        {
            go.AddComponent<Text>();
        }

        var comp = go.AddComponent<FontController>();
        OnStatusLog?.Invoke($"✅ Script 'FontController' FIJADO con éxito a [{go.name}]. ¡Tipografía editable!");
        OnComponentsChanged?.Invoke();
        return comp;
    }

    /// <summary>
    /// Fija el script NavigationController al GameObject objetivo mediante AddComponent.
    /// </summary>
    public NavigationController AttachNavigationController(GameObject target = null)
    {
        GameObject go = target != null ? target : targetGameObject;
        if (go == null) return null;

        var existing = go.GetComponent<NavigationController>();
        if (existing != null)
        {
            OnStatusLog?.Invoke($"ℹ️ El script 'NavigationController' ya está fijado a [{go.name}].");
            return existing;
        }

        var comp = go.AddComponent<NavigationController>();
        OnStatusLog?.Invoke($"✅ Script 'NavigationController' FIJADO a [{go.name}].");
        OnComponentsChanged?.Invoke();
        return comp;
    }

    /// <summary>
    /// Remueve/desacopla un componente específico del GameObject en tiempo de ejecución.
    /// </summary>
    public bool RemoveComponentByName(string typeName, GameObject target = null)
    {
        GameObject go = target != null ? target : targetGameObject;
        if (go == null) return false;

        Component comp = go.GetComponent(typeName);
        if (comp != null)
        {
            Destroy(comp);
            OnStatusLog?.Invoke($"🗑️ Script '{typeName}' DESACOPLADO y removido de [{go.name}].");
            OnComponentsChanged?.Invoke();
            return true;
        }

        OnStatusLog?.Invoke($"⚠️ El componente '{typeName}' no se encontraba fijado en [{go.name}].");
        return false;
    }

    /// <summary>
    /// Alterna el estado de activación (enabled) de un componente fijado.
    /// </summary>
    public bool ToggleComponentEnabled(string typeName, GameObject target = null)
    {
        GameObject go = target != null ? target : targetGameObject;
        if (go == null) return false;

        var comp = go.GetComponent(typeName) as MonoBehaviour;
        if (comp != null)
        {
            comp.enabled = !comp.enabled;
            OnStatusLog?.Invoke($"⚙️ Estado del script '{typeName}': {(comp.enabled ? "HABILITADO" : "DESHABILITADO")}");
            OnComponentsChanged?.Invoke();
            return comp.enabled;
        }

        OnStatusLog?.Invoke($"⚠️ No se encontró el script '{typeName}' para alternar.");
        return false;
    }

    /// <summary>
    /// Obtiene un resumen formateado de todos los componentes y scripts actualmente fijados al GameObject objetivo.
    /// </summary>
    public List<ComponentInfo> GetAttachedComponentsInfo(GameObject target = null)
    {
        GameObject go = target != null ? target : targetGameObject;
        var list = new List<ComponentInfo>();
        if (go == null) return list;

        Component[] comps = go.GetComponents<Component>();
        foreach (var c in comps)
        {
            if (c == null) continue;
            string typeName = c.GetType().Name;
            bool isMono = c is MonoBehaviour;
            bool isEnabled = (c as MonoBehaviour)?.enabled ?? true;

            list.Add(new ComponentInfo
            {
                TypeName = typeName,
                IsCustomScript = isMono && !typeName.StartsWith("UI") && !typeName.StartsWith("Image") && !typeName.StartsWith("Text"),
                IsEnabled = isEnabled,
                ComponentRef = c
            });
        }
        return list;
    }

    public struct ComponentInfo
    {
        public string TypeName;
        public bool IsCustomScript;
        public bool IsEnabled;
        public Component ComponentRef;
    }
}
