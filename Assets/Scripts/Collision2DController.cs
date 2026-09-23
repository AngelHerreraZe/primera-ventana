using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controlador Integral de Físicas y Detección de Colisiones 2D en Unity.
/// Implementa los conceptos del tutorial de colisiones 2D:
/// 1. Rigidbody2D y Collider2D para bloqueo físico absoluto (el personaje no puede atravesar paredes ni piso).
/// 2. Máquina de Estados reactiva a colisiones y triggers (OnCollisionEnter2D, OnCollisionStay2D, OnCollisionExit2D, OnTriggerEnter2D).
/// 3. Alineación milimétrica del colisionador con el Sprite gráfico sin holguras ni superposiciones.
/// 4. Telemetría de contactos (punto de impacto, normal, velocidad relativa) y presets de simulación física.
/// </summary>
public class Collision2DController : MonoBehaviour
{
    // ==========================================
    // Enumeraciones de Estado y Tipos
    // ==========================================
    public enum CharacterState
    {
        Idle,
        Walking,
        JumpingInAir,
        Grounded,
        WallTouchLeft,
        WallTouchRight,
        CeilingImpact,
        Bounced,
        TriggerCollected,
        HazardDamaged,
        PushingObject
    }

    public enum ColliderShapeType
    {
        Box2D,
        Circle2D,
        Capsule2D
    }

    public enum ScenePreset
    {
        HabitacionSolida,
        PlataformasYCajas,
        CircuitoTriggers,
        ParqueTrampolines
    }

    // ==========================================
    // Configuración Física del Personaje
    // ==========================================
    [Header("Configuración de Movimiento y Físicas")]
    public float moveSpeed = 8f;
    public float jumpForce = 13f;
    public float dashForce = 18f;
    public float gravityScale = 3.5f;
    public float mass = 1.0f;
    public float linearDrag = 0.5f;
    public float bounciness = 0.0f;
    public float friction = 0.4f;
    public CollisionDetectionMode2D collisionMode = CollisionDetectionMode2D.Continuous;

    [Header("Alineación del Colisionador al Sprite")]
    public ColliderShapeType activeColliderShape = ColliderShapeType.Box2D;
    public Vector2 colliderSize = new Vector2(1.2f, 1.2f);
    public Vector2 colliderOffset = Vector2.zero;
    public bool autoFitOnSpriteChange = true;
    public bool showColliderWireframe = true;

    // ==========================================
    // Eventos y Notificaciones hacia la UI
    // ==========================================
    public Action<CharacterState, string> OnStateChanged;
    public Action<CollisionEventData> OnCollisionRegistered;
    public Action<TriggerEventData> OnTriggerRegistered;
    public Action<string> OnStatusLog;
    public Action<TelemetryData> OnTelemetryUpdated;

    [System.Serializable]
    public struct CollisionEventData
    {
        public string objectName;
        public string objectTag;
        public Vector2 contactPoint;
        public Vector2 contactNormal;
        public float relativeVelocity;
        public string impactType;
        public float timestamp;
    }

    [System.Serializable]
    public struct TriggerEventData
    {
        public string triggerName;
        public string triggerType;
        public int scoreGained;
        public float timestamp;
    }

    public struct TelemetryData
    {
        public Vector2 position;
        public Vector2 velocity;
        public CharacterState currentState;
        public bool isGrounded;
        public bool isTouchingWall;
        public bool isTouchingCeiling;
        public int totalCollisions;
        public int totalTriggers;
        public int score;
        public int health;
    }

    // ==========================================
    // Referencias de Objetos en el Mundo Físico
    // ==========================================
    private GameObject _worldRoot;
    private GameObject _playerObject;
    private Rigidbody2D _playerRb;
    private Collider2D _playerCollider;
    private SpriteRenderer _playerSpriteRenderer;
    private PhysicsMaterial2D _physicsMaterial;

    // Entorno físico y objetos
    private readonly List<GameObject> _spawnedObstacles = new List<GameObject>();
    private readonly List<GameObject> _spawnedTriggers = new List<GameObject>();
    private GameObject _floorObject;
    private GameObject _leftWallObject;
    private GameObject _rightWallObject;
    private GameObject _ceilingObject;

    // Estado Interno
    private CharacterState _currentState = CharacterState.Idle;
    private bool _isGrounded = false;
    private bool _isTouchingLeftWall = false;
    private bool _isTouchingRightWall = false;
    private bool _isTouchingCeiling = false;
    private float _horizontalInput = 0f;
    private bool _jumpRequested = false;
    private bool _dashRequested = false;
    private int _totalCollisionsCount = 0;
    private int _totalTriggersCount = 0;
    private int _score = 0;
    private int _health = 100;
    private float _lastStateChangeTime = 0f;
    private Vector2 _initialPlayerPosition = new Vector2(0f, -1.5f);
    private ScenePreset _currentPreset = ScenePreset.HabitacionSolida;

    // Overlay visual del colisionador
    private LineRenderer _wireframeLineRenderer;

    // Getters públicos
    public CharacterState CurrentState => _currentState;
    public bool IsGrounded => _isGrounded;
    public int Score => _score;
    public int Health => _health;
    public ScenePreset CurrentPreset => _currentPreset;
    public GameObject PlayerObject => _playerObject;
    public Rigidbody2D PlayerRigidbody => _playerRb;
    public Collider2D PlayerCollider => _playerCollider;

    void Awake()
    {
        SetupWorldSimulation();
    }

    void Start()
    {
        LoadPreset(ScenePreset.HabitacionSolida);
        OnStatusLog?.Invoke("🛡️ Sistema de Colisiones 2D inicializado con Rigidbody2D y Colliders alineados.");
    }

    void Update()
    {
        HandleKeyboardInput();
        UpdateColliderWireframe();
        BroadcastTelemetry();
    }

    void FixedUpdate()
    {
        ProcessPhysicsMovement();
    }

    // ==========================================
    // Inicialización del Entorno de Simulación 2D
    // ==========================================
    private void SetupWorldSimulation()
    {
        if (_worldRoot != null) return;

        _worldRoot = new GameObject("Physics2D_WorldSimulation");
        _worldRoot.transform.SetParent(transform, false);

        // Material físico 2D para controlar rebote y fricción
        _physicsMaterial = new PhysicsMaterial2D("PlayerPhysicMaterial")
        {
            bounciness = bounciness,
            friction = friction
        };

        // 1. Crear Personaje Principal con Rigidbody2D y Collider2D
        CreatePlayerCharacter();

        // 2. Crear los límites sólidos fundamentales (Piso, Paredes y Techo)
        CreateBoundaries();
    }

    private void CreatePlayerCharacter()
    {
        _playerObject = new GameObject("PlayerCharacter_2D");
        _playerObject.transform.SetParent(_worldRoot.transform, false);
        _playerObject.transform.position = _initialPlayerPosition;
        _playerObject.tag = "Player";

        // SpriteRenderer
        _playerSpriteRenderer = _playerObject.AddComponent<SpriteRenderer>();
        _playerSpriteRenderer.sprite = GenerateHeroSprite();
        _playerSpriteRenderer.sortingOrder = 5;
        _playerSpriteRenderer.color = new Color(0.12f, 0.75f, 0.88f, 1f); // Cian vibrante

        // Rigidbody2D con física realista y bloqueo de rotación Z
        _playerRb = _playerObject.AddComponent<Rigidbody2D>();
        _playerRb.bodyType = RigidbodyType2D.Dynamic;
        _playerRb.mass = mass;
        _playerRb.linearDamping = linearDrag;
        _playerRb.gravityScale = gravityScale;
        _playerRb.collisionDetectionMode = collisionMode;
        _playerRb.freezeRotation = true; // Evita que ruede o se incline al colisionar
        _playerRb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _playerRb.sharedMaterial = _physicsMaterial;

        // Configurar Colisionador inicial alineado al sprite
        SetupPlayerCollider(activeColliderShape);

        // Receptor de eventos de colisión y triggers
        var hook = _playerObject.AddComponent<Collision2DHook>();
        hook.Initialize(this);

        // LineRenderer para dibujar el Wireframe del colisionador en tiempo de ejecución
        SetupWireframeDrawer();
    }

    private void CreateBoundaries()
    {
        // Dimensiones del cuadrilátero cerrado de la arena (Ancho: 16m, Alto: 10m)
        float arenaHalfWidth = 8f;
        float arenaHalfHeight = 5f;
        float wallThickness = 1.0f;

        // 1. PISO SÓLIDO (Floor)
        _floorObject = CreateSolidWall("Piso_Solido", new Vector2(0, -arenaHalfHeight), new Vector2(arenaHalfWidth * 2 + wallThickness * 2, wallThickness), "Floor", new Color(0.18f, 0.23f, 0.33f, 1f));

        // 2. PARED IZQUIERDA (Left Wall)
        _leftWallObject = CreateSolidWall("Pared_Izquierda", new Vector2(-arenaHalfWidth - wallThickness * 0.5f, 0), new Vector2(wallThickness, arenaHalfHeight * 2), "Wall", new Color(0.24f, 0.28f, 0.38f, 1f));

        // 3. PARED DERECHA (Right Wall)
        _rightWallObject = CreateSolidWall("Pared_Derecha", new Vector2(arenaHalfWidth + wallThickness * 0.5f, 0), new Vector2(wallThickness, arenaHalfHeight * 2), "Wall", new Color(0.24f, 0.28f, 0.38f, 1f));

        // 4. TECHO SÓLIDO (Ceiling)
        _ceilingObject = CreateSolidWall("Techo_Solido", new Vector2(0, arenaHalfHeight + wallThickness * 0.5f), new Vector2(arenaHalfWidth * 2 + wallThickness * 2, wallThickness), "Ceiling", new Color(0.18f, 0.23f, 0.33f, 1f));
    }

    private GameObject CreateSolidWall(string name, Vector2 position, Vector2 size, string tag, Color color)
    {
        var wall = new GameObject(name);
        wall.transform.SetParent(_worldRoot.transform, false);
        wall.transform.position = position;
        wall.tag = tag;

        var sr = wall.AddComponent<SpriteRenderer>();
        sr.sprite = GenerateBoxSprite();
        sr.color = color;
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = size;
        sr.sortingOrder = 2;

        var col = wall.AddComponent<BoxCollider2D>();
        col.size = size;
        col.offset = Vector2.zero;

        // Las paredes son estáticas (Rigidbody no necesario o Static)
        return wall;
    }

    // ==========================================
    // Sistema de Colisionadores y Alineación Precisa
    // ==========================================
    public void SetupPlayerCollider(ColliderShapeType shape)
    {
        activeColliderShape = shape;

        // Eliminar colisionador anterior si existe
        if (_playerCollider != null)
        {
            Destroy(_playerCollider);
        }

        switch (shape)
        {
            case ColliderShapeType.Box2D:
                var box = _playerObject.AddComponent<BoxCollider2D>();
                box.size = colliderSize;
                box.offset = colliderOffset;
                box.sharedMaterial = _physicsMaterial;
                _playerCollider = box;
                break;

            case ColliderShapeType.Circle2D:
                var circle = _playerObject.AddComponent<CircleCollider2D>();
                circle.radius = Mathf.Min(colliderSize.x, colliderSize.y) * 0.5f;
                circle.offset = colliderOffset;
                circle.sharedMaterial = _physicsMaterial;
                _playerCollider = circle;
                break;

            case ColliderShapeType.Capsule2D:
                var capsule = _playerObject.AddComponent<CapsuleCollider2D>();
                capsule.size = colliderSize;
                capsule.offset = colliderOffset;
                capsule.direction = CapsuleDirection2D.Vertical;
                capsule.sharedMaterial = _physicsMaterial;
                _playerCollider = capsule;
                break;
        }

        OnStatusLog?.Invoke($"📐 Colisionador configurado como '{shape}' alineado al Sprite.");
    }

    /// <summary>
    /// Auto-alinea automáticamente el tamaño y centro del colisionador a los límites exactos del Sprite.
    /// Garantiza cumplimiento estricto del criterio de alineación sin solapamientos.
    /// </summary>
    public void AutoFitColliderToSprite()
    {
        if (_playerSpriteRenderer == null || _playerSpriteRenderer.sprite == null) return;

        var bounds = _playerSpriteRenderer.sprite.bounds;
        colliderSize = new Vector2(bounds.size.x, bounds.size.y);
        colliderOffset = bounds.center;

        if (_playerCollider is BoxCollider2D box)
        {
            box.size = colliderSize;
            box.offset = colliderOffset;
        }
        else if (_playerCollider is CircleCollider2D circle)
        {
            circle.radius = Mathf.Min(colliderSize.x, colliderSize.y) * 0.5f;
            circle.offset = colliderOffset;
        }
        else if (_playerCollider is CapsuleCollider2D capsule)
        {
            capsule.size = colliderSize;
            capsule.offset = colliderOffset;
        }

        OnStatusLog?.Invoke($"🎯 Colisionador auto-alineado exactamente al sprite ({colliderSize.x:F2}m x {colliderSize.y:F2}m, Offset: {colliderOffset}).");
    }

    public void SetColliderSize(Vector2 size)
    {
        colliderSize = new Vector2(Mathf.Max(0.2f, size.x), Mathf.Max(0.2f, size.y));
        if (_playerCollider is BoxCollider2D box) box.size = colliderSize;
        else if (_playerCollider is CircleCollider2D circle) circle.radius = Mathf.Min(colliderSize.x, colliderSize.y) * 0.5f;
        else if (_playerCollider is CapsuleCollider2D capsule) capsule.size = colliderSize;
    }

    public void SetColliderOffset(Vector2 offset)
    {
        colliderOffset = offset;
        if (_playerCollider != null) _playerCollider.offset = offset;
    }

    // ==========================================
    // Control de Movimiento por Físicas
    // ==========================================
    private void HandleKeyboardInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(h) > 0.01f)
        {
            _horizontalInput = h;
        }
        else if (!IsButtonPressed)
        {
            _horizontalInput = 0f;
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            RequestJump();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift) || Input.GetKeyDown(KeyCode.J))
        {
            RequestDash();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetPlayerPosition();
        }
    }

    public bool IsButtonPressed { get; set; } = false;

    public void SetHorizontalMovement(float value)
    {
        _horizontalInput = Mathf.Clamp(value, -1f, 1f);
    }

    public void RequestJump()
    {
        if (_isGrounded)
        {
            _jumpRequested = true;
        }
        else
        {
            OnStatusLog?.Invoke("⚠️ No se puede saltar en el aire (requiere contacto con el piso).");
        }
    }

    public void RequestDash()
    {
        _dashRequested = true;
    }

    public void ResetPlayerPosition()
    {
        if (_playerObject != null && _playerRb != null)
        {
            _playerObject.transform.position = _initialPlayerPosition;
            _playerRb.linearVelocity = Vector2.zero;
            _playerRb.angularVelocity = 0f;
            ChangeState(CharacterState.Idle, "Posición reseteada");
            OnStatusLog?.Invoke("🔄 Personaje reposicionado al punto de origen seguro.");
        }
    }

    private void ProcessPhysicsMovement()
    {
        if (_playerRb == null) return;

        // 1. Movimiento Horizontal mediante velocidad física
        Vector2 currentVel = _playerRb.linearVelocity;
        float targetVelX = _horizontalInput * moveSpeed;
        _playerRb.linearVelocity = new Vector2(targetVelX, currentVel.y);

        // 2. Orientación visual del Sprite (Flip X)
        if (_horizontalInput > 0.05f) _playerSpriteRenderer.flipX = false;
        else if (_horizontalInput < -0.05f) _playerSpriteRenderer.flipX = true;

        // 3. Salto mediante Impulso Físico
        if (_jumpRequested)
        {
            _playerRb.linearVelocity = new Vector2(_playerRb.linearVelocity.x, jumpForce);
            _jumpRequested = false;
            _isGrounded = false;
            ChangeState(CharacterState.JumpingInAir, "Salto con Impulso Físico");
        }

        // 4. Dash Lateral con Fuerza Física
        if (_dashRequested)
        {
            float direction = _playerSpriteRenderer.flipX ? -1f : 1f;
            _playerRb.AddForce(new Vector2(direction * dashForce, 2f), ForceMode2D.Impulse);
            _dashRequested = false;
            ChangeState(CharacterState.Walking, "Dash Lateral Rápido");
        }

        // 5. Actualización de estado en movimiento si no hay colisiones dominantes
        if (_isGrounded && Mathf.Abs(_horizontalInput) > 0.1f && _currentState != CharacterState.PushingObject)
        {
            if (_currentState != CharacterState.Walking)
            {
                ChangeState(CharacterState.Walking, "Caminando sobre el piso");
            }
        }
        else if (_isGrounded && Mathf.Abs(_horizontalInput) <= 0.1f && _currentState != CharacterState.Grounded && _currentState != CharacterState.Idle)
        {
            if (Time.time - _lastStateChangeTime > 0.25f)
            {
                ChangeState(CharacterState.Idle, "En reposo sobre el piso");
            }
        }
        else if (!_isGrounded && _playerRb.linearVelocity.y < -0.2f && _currentState != CharacterState.JumpingInAir)
        {
            ChangeState(CharacterState.JumpingInAir, "Cayendo en el aire");
        }
    }

    // ==========================================
    // Manejo de Eventos de Colisión Sólida (Criterio 1)
    // ==========================================
    public void HandleCollisionEnter(Collision2D collision)
    {
        _totalCollisionsCount++;
        string tag = collision.gameObject.tag;
        string objName = collision.gameObject.name;

        // Analizar puntos y vectores normales de contacto
        Vector2 contactPoint = Vector2.zero;
        Vector2 contactNormal = Vector2.up;

        if (collision.contactCount > 0)
        {
            var contact = collision.GetContact(0);
            contactPoint = contact.point;
            contactNormal = contact.normal;
        }

        float relVel = collision.relativeVelocity.magnitude;

        // Determinar superficie de contacto según la normal:
        // Normal apuntando hacia ARRIBA (y > 0.5) -> PISO SÓLIDO
        if (contactNormal.y > 0.5f)
        {
            _isGrounded = true;
            ChangeState(CharacterState.Grounded, $"Aterrizaje sólido en '{objName}'");
            ApplySquashEffect(new Vector2(1.25f, 0.8f));
        }
        // Normal apuntando hacia ABAJO (y < -0.5) -> TECHO SÓLIDO
        else if (contactNormal.y < -0.5f)
        {
            _isTouchingCeiling = true;
            ChangeState(CharacterState.CeilingImpact, $"Choque contra techo '{objName}' (bloqueo vertical)");
            ApplySquashEffect(new Vector2(1.2f, 0.85f));
        }
        // Normal apuntando hacia la DERECHA (x > 0.5) -> Pared Izquierda
        else if (contactNormal.x > 0.5f)
        {
            _isTouchingLeftWall = true;
            ChangeState(CharacterState.WallTouchLeft, $"Contacto con pared izquierda '{objName}' (no traspasable)");
            ApplySquashEffect(new Vector2(0.85f, 1.15f));
        }
        // Normal apuntando hacia la IZQUIERDA (x < -0.5) -> Pared Derecha
        else if (contactNormal.x < -0.5f)
        {
            _isTouchingRightWall = true;
            ChangeState(CharacterState.WallTouchRight, $"Contacto con pared derecha '{objName}' (no traspasable)");
            ApplySquashEffect(new Vector2(0.85f, 1.15f));
        }

        // Caso especial: Trampolín / Rebote
        if (tag == "BouncyPad")
        {
            ChangeState(CharacterState.Bounced, "¡Rebote elástico en trampolín!");
            ApplySquashEffect(new Vector2(1.4f, 0.6f));
        }
        else if (tag == "DynamicBox")
        {
            ChangeState(CharacterState.PushingObject, $"Empujando caja física '{objName}'");
        }

        // Registrar evento para telemetría
        var ev = new CollisionEventData
        {
            objectName = objName,
            objectTag = tag,
            contactPoint = contactPoint,
            contactNormal = contactNormal,
            relativeVelocity = relVel,
            impactType = _currentState.ToString(),
            timestamp = Time.time
        };
        OnCollisionRegistered?.Invoke(ev);
        OnStatusLog?.Invoke($"💥 Colisión sólida: [{objName}] Normal: {contactNormal:F2} | Vel: {relVel:F1} m/s | Estado -> {_currentState}");
    }

    public void HandleCollisionStay(Collision2D collision)
    {
        if (collision.contactCount > 0)
        {
            var contact = collision.GetContact(0);
            if (contact.normal.y > 0.5f) _isGrounded = true;
            if (contact.normal.x > 0.5f) _isTouchingLeftWall = true;
            if (contact.normal.x < -0.5f) _isTouchingRightWall = true;
            if (contact.normal.y < -0.5f) _isTouchingCeiling = true;
        }
    }

    public void HandleCollisionExit(Collision2D collision)
    {
        string tag = collision.gameObject.tag;

        if (tag == "Floor" || collision.gameObject.name.Contains("Piso") || collision.gameObject.name.Contains("Plataforma"))
        {
            _isGrounded = false;
        }
        if (collision.gameObject.name.Contains("Izquierda") || tag == "Wall")
        {
            _isTouchingLeftWall = false;
        }
        if (collision.gameObject.name.Contains("Derecha") || tag == "Wall")
        {
            _isTouchingRightWall = false;
        }
        if (tag == "Ceiling" || collision.gameObject.name.Contains("Techo"))
        {
            _isTouchingCeiling = false;
        }

        if (!_isGrounded && _currentState == CharacterState.Grounded)
        {
            ChangeState(CharacterState.JumpingInAir, "Dejando superficie sólida");
        }
    }

    // ==========================================
    // Manejo de Eventos de Zonas de Disparo (Triggers)
    // ==========================================
    public void HandleTriggerEnter(Collider2D other)
    {
        _totalTriggersCount++;
        string tag = other.gameObject.tag;
        string objName = other.gameObject.name;

        int scoreDelta = 0;
        string type = "Trigger";

        if (tag == "CollectibleCoin")
        {
            scoreDelta = 100;
            _score += scoreDelta;
            type = "Moneda / Gema";
            ChangeState(CharacterState.TriggerCollected, $"¡Gema recogida! (+{scoreDelta} pts)");
            StartCoroutine(AnimateCollectAndRespawn(other.gameObject));
        }
        else if (tag == "HazardLava")
        {
            _health = Mathf.Max(0, _health - 25);
            type = "Zona de Lava / Peligro";
            ChangeState(CharacterState.HazardDamaged, "¡Impacto con Lava! Salud -25%");
            // Rebote de daño hacia arriba
            _playerRb.linearVelocity = new Vector2(-_horizontalInput * 6f, 8f);
            if (_health <= 0)
            {
                OnStatusLog?.Invoke("💀 Personaje derrotado por lava. Reiniciando...");
                _health = 100;
                ResetPlayerPosition();
            }
        }
        else if (tag == "SpeedPad")
        {
            type = "Acelerador Mágico";
            ChangeState(CharacterState.Walking, "¡Turbo Acelerador activado!");
            _playerRb.AddForce(new Vector2(15f, 5f), ForceMode2D.Impulse);
        }

        var ev = new TriggerEventData
        {
            triggerName = objName,
            triggerType = type,
            scoreGained = scoreDelta,
            timestamp = Time.time
        };
        OnTriggerRegistered?.Invoke(ev);
        OnStatusLog?.Invoke($"⭐ Trigger activado: [{objName}] Tipo: {type} | Puntuación total: {_score} pts");
    }

    public void HandleTriggerExit(Collider2D other)
    {
        // Limpieza de triggers
    }

    private IEnumerator AnimateCollectAndRespawn(GameObject item)
    {
        var col = item.GetComponent<Collider2D>();
        var sr = item.GetComponent<SpriteRenderer>();
        if (col != null) col.enabled = false;

        // Efecto de desvanecimiento y elevación
        Vector3 startPos = item.transform.position;
        float elapsed = 0f;
        while (elapsed < 0.35f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / 0.35f;
            item.transform.position = startPos + new Vector3(0, t * 0.8f, 0);
            if (sr != null) sr.color = new Color(1f, 0.9f, 0.2f, 1f - t);
            yield return null;
        }

        if (sr != null) sr.enabled = false;

        // Reaparición tras 3 segundos
        yield return new WaitForSeconds(3.0f);

        item.transform.position = startPos;
        if (sr != null)
        {
            sr.enabled = true;
            sr.color = Color.white;
        }
        if (col != null) col.enabled = true;
    }

    // ==========================================
    // Cambio de Estado y Retroalimentación Visual
    // ==========================================
    private void ChangeState(CharacterState newState, string reason)
    {
        _currentState = newState;
        _lastStateChangeTime = Time.time;

        // Cambiar color/tinte del sprite según el nuevo estado del objeto
        if (_playerSpriteRenderer != null)
        {
            switch (newState)
            {
                case CharacterState.Idle:
                    _playerSpriteRenderer.color = new Color(0.12f, 0.75f, 0.88f, 1f); // Cian
                    break;
                case CharacterState.Walking:
                    _playerSpriteRenderer.color = new Color(0.24f, 0.51f, 0.98f, 1f); // Azul
                    break;
                case CharacterState.JumpingInAir:
                    _playerSpriteRenderer.color = new Color(0.63f, 0.38f, 0.96f, 1f); // Violeta
                    break;
                case CharacterState.Grounded:
                    _playerSpriteRenderer.color = new Color(0.13f, 0.77f, 0.53f, 1f); // Verde
                    break;
                case CharacterState.WallTouchLeft:
                case CharacterState.WallTouchRight:
                    _playerSpriteRenderer.color = new Color(0.96f, 0.62f, 0.15f, 1f); // Ámbar
                    break;
                case CharacterState.CeilingImpact:
                    _playerSpriteRenderer.color = new Color(1.00f, 0.45f, 0.10f, 1f); // Fuego naranja
                    break;
                case CharacterState.Bounced:
                    _playerSpriteRenderer.color = new Color(0.94f, 0.27f, 0.85f, 1f); // Magenta
                    break;
                case CharacterState.TriggerCollected:
                    _playerSpriteRenderer.color = new Color(1.00f, 0.85f, 0.20f, 1f); // Oro brillante
                    break;
                case CharacterState.HazardDamaged:
                    _playerSpriteRenderer.color = new Color(0.94f, 0.27f, 0.38f, 1f); // Rosa / Rojo peligro
                    break;
                case CharacterState.PushingObject:
                    _playerSpriteRenderer.color = new Color(0.40f, 0.80f, 0.95f, 1f);
                    break;
            }
        }

        OnStateChanged?.Invoke(newState, reason);
    }

    private void ApplySquashEffect(Vector2 scaleFactor)
    {
        if (_playerObject == null) return;
        StopCoroutine(nameof(SquashRoutine));
        StartCoroutine(SquashRoutine(scaleFactor));
    }

    private IEnumerator SquashRoutine(Vector2 targetScale)
    {
        float dur = 0.12f;
        float elapsed = 0f;
        Vector3 orig = Vector3.one;

        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dur;
            _playerObject.transform.localScale = Vector3.Lerp(orig, new Vector3(targetScale.x, targetScale.y, 1f), t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dur;
            _playerObject.transform.localScale = Vector3.Lerp(new Vector3(targetScale.x, targetScale.y, 1f), orig, t);
            yield return null;
        }
        _playerObject.transform.localScale = orig;
    }

    // ==========================================
    // Visualizador de Wireframe de Colisionador en Tiempo Real (Criterio 2)
    // ==========================================
    private void SetupWireframeDrawer()
    {
        var wireframeGo = new GameObject("ColliderWireframeOverlay");
        wireframeGo.transform.SetParent(_playerObject.transform, false);

        _wireframeLineRenderer = wireframeGo.AddComponent<LineRenderer>();
        _wireframeLineRenderer.useWorldSpace = false;
        _wireframeLineRenderer.startWidth = 0.04f;
        _wireframeLineRenderer.endWidth = 0.04f;
        _wireframeLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _wireframeLineRenderer.startColor = new Color(0.13f, 0.95f, 0.55f, 0.95f); // Verde neón brillante
        _wireframeLineRenderer.endColor = new Color(0.13f, 0.95f, 0.55f, 0.95f);
        _wireframeLineRenderer.sortingOrder = 10;
        _wireframeLineRenderer.loop = true;
    }

    private void UpdateColliderWireframe()
    {
        if (_wireframeLineRenderer == null || !showColliderWireframe || _playerCollider == null)
        {
            if (_wireframeLineRenderer != null) _wireframeLineRenderer.enabled = false;
            return;
        }

        _wireframeLineRenderer.enabled = true;

        if (_playerCollider is BoxCollider2D box)
        {
            _wireframeLineRenderer.positionCount = 4;
            Vector2 half = box.size * 0.5f;
            Vector2 off = box.offset;
            _wireframeLineRenderer.SetPosition(0, new Vector3(off.x - half.x, off.y - half.y, 0));
            _wireframeLineRenderer.SetPosition(1, new Vector3(off.x + half.x, off.y - half.y, 0));
            _wireframeLineRenderer.SetPosition(2, new Vector3(off.x + half.x, off.y + half.y, 0));
            _wireframeLineRenderer.SetPosition(3, new Vector3(off.x - half.x, off.y + half.y, 0));
        }
        else if (_playerCollider is CircleCollider2D circle)
        {
            int segments = 24;
            _wireframeLineRenderer.positionCount = segments;
            float r = circle.radius;
            Vector2 off = circle.offset;
            for (int i = 0; i < segments; i++)
            {
                float a = (float)i / segments * Mathf.PI * 2f;
                _wireframeLineRenderer.SetPosition(i, new Vector3(off.x + Mathf.Cos(a) * r, off.y + Mathf.Sin(a) * r, 0));
            }
        }
        else if (_playerCollider is CapsuleCollider2D capsule)
        {
            int segments = 20;
            _wireframeLineRenderer.positionCount = segments;
            Vector2 sz = capsule.size * 0.5f;
            Vector2 off = capsule.offset;
            for (int i = 0; i < segments; i++)
            {
                float a = (float)i / segments * Mathf.PI * 2f;
                _wireframeLineRenderer.SetPosition(i, new Vector3(off.x + Mathf.Cos(a) * sz.x, off.y + Mathf.Sin(a) * sz.y, 0));
            }
        }
    }

    // ==========================================
    // Configuración de Presets de Escenario
    // ==========================================
    public void LoadPreset(ScenePreset preset)
    {
        _currentPreset = preset;
        ClearDynamicObjects();

        switch (preset)
        {
            case ScenePreset.HabitacionSolida:
                // Escenario básico: Demuestra que no atraviesa paredes, piso ni techo
                AddPlatform(new Vector2(0, -2f), new Vector2(5f, 0.6f), new Color(0.24f, 0.51f, 0.98f, 1f));
                AddObstacleBox(new Vector2(3f, -4f), new Vector2(1.2f, 1.2f), true);
                break;

            case ScenePreset.PlataformasYCajas:
                // Plataformas escalonadas y cajas físicas dinámicas
                AddPlatform(new Vector2(-4f, -2.5f), new Vector2(4f, 0.5f), new Color(0.24f, 0.51f, 0.98f, 1f));
                AddPlatform(new Vector2(4f, -1.0f), new Vector2(4f, 0.5f), new Color(0.24f, 0.51f, 0.98f, 1f));
                AddPlatform(new Vector2(0f, 1.5f), new Vector2(3.5f, 0.5f), new Color(0.13f, 0.77f, 0.53f, 1f));
                AddObstacleBox(new Vector2(-4f, -1.8f), new Vector2(1f, 1f), true);
                AddObstacleBox(new Vector2(4f, -0.3f), new Vector2(1f, 1f), true);
                break;

            case ScenePreset.CircuitoTriggers:
                // Monedas, lava y aceleradores
                AddPlatform(new Vector2(-3f, -2f), new Vector2(4f, 0.5f), new Color(0.24f, 0.51f, 0.98f, 1f));
                AddPlatform(new Vector2(3f, 0.5f), new Vector2(4f, 0.5f), new Color(0.24f, 0.51f, 0.98f, 1f));
                AddCollectibleCoin(new Vector2(-3f, -1f));
                AddCollectibleCoin(new Vector2(0f, 2f));
                AddCollectibleCoin(new Vector2(3f, 1.5f));
                AddHazardLavaZone(new Vector2(0f, -4.5f), new Vector2(6f, 0.8f));
                break;

            case ScenePreset.ParqueTrampolines:
                // Resortes elásticos de alta restitución
                AddBouncyPad(new Vector2(-4f, -4.2f), new Vector2(3f, 0.6f), 1.3f);
                AddBouncyPad(new Vector2(4f, -4.2f), new Vector2(3f, 0.6f), 1.3f);
                AddPlatform(new Vector2(0f, 0.5f), new Vector2(3f, 0.5f), new Color(0.63f, 0.38f, 0.96f, 1f));
                AddCollectibleCoin(new Vector2(0f, 2.5f));
                break;
        }

        ResetPlayerPosition();
        OnStatusLog?.Invoke($"🏟️ Escenario cargado: '{preset}'");
    }

    private void ClearDynamicObjects()
    {
        foreach (var obj in _spawnedObstacles) if (obj != null) Destroy(obj);
        foreach (var obj in _spawnedTriggers) if (obj != null) Destroy(obj);
        _spawnedObstacles.Clear();
        _spawnedTriggers.Clear();
    }

    public void AddPlatform(Vector2 position, Vector2 size, Color color)
    {
        var plat = new GameObject("Plataforma_Solida");
        plat.transform.SetParent(_worldRoot.transform, false);
        plat.transform.position = position;
        plat.tag = "Floor";

        var sr = plat.AddComponent<SpriteRenderer>();
        sr.sprite = GenerateBoxSprite();
        sr.color = color;
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = size;
        sr.sortingOrder = 3;

        var col = plat.AddComponent<BoxCollider2D>();
        col.size = size;

        _spawnedObstacles.Add(plat);
    }

    public void AddObstacleBox(Vector2 position, Vector2 size, bool dynamicRigid)
    {
        var box = new GameObject("Caja_Obstaculo");
        box.transform.SetParent(_worldRoot.transform, false);
        box.transform.position = position;
        box.tag = "DynamicBox";

        var sr = box.AddComponent<SpriteRenderer>();
        sr.sprite = GenerateCrateSprite();
        sr.sortingOrder = 4;

        var col = box.AddComponent<BoxCollider2D>();
        col.size = size;

        if (dynamicRigid)
        {
            var rb = box.AddComponent<Rigidbody2D>();
            rb.mass = 2.0f;
            rb.linearDamping = 1.0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        _spawnedObstacles.Add(box);
    }

    public void AddCollectibleCoin(Vector2 position)
    {
        var coin = new GameObject("Moneda_Trigger");
        coin.transform.SetParent(_worldRoot.transform, false);
        coin.transform.position = position;
        coin.tag = "CollectibleCoin";

        var sr = coin.AddComponent<SpriteRenderer>();
        sr.sprite = GenerateCoinSprite();
        sr.sortingOrder = 4;

        var col = coin.AddComponent<CircleCollider2D>();
        col.radius = 0.45f;
        col.isTrigger = true; // Activa OnTriggerEnter2D en lugar de colisión física sólida

        _spawnedTriggers.Add(coin);
    }

    public void AddHazardLavaZone(Vector2 position, Vector2 size)
    {
        var lava = new GameObject("Zona_Lava");
        lava.transform.SetParent(_worldRoot.transform, false);
        lava.transform.position = position;
        lava.tag = "HazardLava";

        var sr = lava.AddComponent<SpriteRenderer>();
        sr.sprite = GenerateBoxSprite();
        sr.color = new Color(1.0f, 0.25f, 0.15f, 0.85f);
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = size;
        sr.sortingOrder = 3;

        var col = lava.AddComponent<BoxCollider2D>();
        col.size = size;
        col.isTrigger = true;

        _spawnedTriggers.Add(lava);
    }

    public void AddBouncyPad(Vector2 position, Vector2 size, float bouncinessFactor)
    {
        var pad = new GameObject("Trampolin_Bouncy");
        pad.transform.SetParent(_worldRoot.transform, false);
        pad.transform.position = position;
        pad.tag = "BouncyPad";

        var sr = pad.AddComponent<SpriteRenderer>();
        sr.sprite = GenerateBoxSprite();
        sr.color = new Color(0.94f, 0.27f, 0.85f, 1f); // Magenta
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = size;
        sr.sortingOrder = 3;

        var col = pad.AddComponent<BoxCollider2D>();
        col.size = size;

        var mat = new PhysicsMaterial2D("BouncyMat")
        {
            bounciness = bouncinessFactor,
            friction = 0.1f
        };
        col.sharedMaterial = mat;

        _spawnedObstacles.Add(pad);
    }

    // ==========================================
    // Parámetros Físicos Dinámicos
    // ==========================================
    public void SetGravity(float value)
    {
        gravityScale = value;
        if (_playerRb != null) _playerRb.gravityScale = value;
        OnStatusLog?.Invoke($"⚙️ Gravedad ajustada a: {value:F1}");
    }

    public void SetBounciness(float value)
    {
        bounciness = Mathf.Clamp01(value);
        if (_physicsMaterial != null) _physicsMaterial.bounciness = bounciness;
        OnStatusLog?.Invoke($"⚙️ Rebote (Bounciness) ajustado a: {bounciness:F2}");
    }

    public void SetFriction(float value)
    {
        friction = Mathf.Clamp01(value);
        if (_physicsMaterial != null) _physicsMaterial.friction = friction;
        OnStatusLog?.Invoke($"⚙️ Fricción ajustada a: {friction:F2}");
    }

    public void SetCollisionDetectionMode(CollisionDetectionMode2D mode)
    {
        collisionMode = mode;
        if (_playerRb != null) _playerRb.collisionDetectionMode = mode;
        OnStatusLog?.Invoke($"⚙️ Detección de colisión cambiada a: {mode}");
    }

    // ==========================================
    // Telemetría en Tiempo Real
    // ==========================================
    private void BroadcastTelemetry()
    {
        if (_playerObject == null || _playerRb == null) return;

        var telem = new TelemetryData
        {
            position = _playerObject.transform.position,
            velocity = _playerRb.linearVelocity,
            currentState = _currentState,
            isGrounded = _isGrounded,
            isTouchingWall = _isTouchingLeftWall || _isTouchingRightWall,
            isTouchingCeiling = _isTouchingCeiling,
            totalCollisions = _totalCollisionsCount,
            totalTriggers = _totalTriggersCount,
            score = _score,
            health = _health
        };

        OnTelemetryUpdated?.Invoke(telem);
    }

    // ==========================================
    // Generadores Procedurales de Texturas para Físicas 2D
    // ==========================================
    private Sprite GenerateHeroSprite()
    {
        int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float radius = size * 0.42f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist <= radius)
                {
                    // Casco y ojos del personaje
                    float normDist = dist / radius;
                    Color bodyColor = Color.Lerp(Color.white, new Color(0.12f, 0.75f, 0.88f, 1f), normDist);

                    // Visor / Ojos
                    if (y >= size * 0.45f && y <= size * 0.65f && x >= size * 0.35f && x <= size * 0.75f)
                    {
                        bodyColor = new Color(0.08f, 0.12f, 0.18f, 1f); // Visor oscuro
                    }

                    tex.SetPixel(x, y, bodyColor);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 50f, 0, SpriteMeshType.FullRect);
    }

    private Sprite GenerateBoxSprite()
    {
        int size = 32;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool isBorder = (x == 0 || x == size - 1 || y == 0 || y == size - 1);
                tex.SetPixel(x, y, isBorder ? new Color(0.35f, 0.45f, 0.6f, 1f) : Color.white);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f, 0, SpriteMeshType.FullRect, new Vector4(2, 2, 2, 2));
    }

    private Sprite GenerateCrateSprite()
    {
        int size = 48;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
        Color wood = new Color(0.72f, 0.48f, 0.28f, 1f);
        Color woodDark = new Color(0.50f, 0.32f, 0.18f, 1f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool border = (x < 3 || x >= size - 3 || y < 3 || y >= size - 3);
                bool diagonal = (Mathf.Abs(x - y) < 3 || Mathf.Abs((size - 1 - x) - y) < 3);
                tex.SetPixel(x, y, (border || diagonal) ? woodDark : wood);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 48f, 0, SpriteMeshType.FullRect);
    }

    private Sprite GenerateCoinSprite()
    {
        int size = 40;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float radius = size * 0.42f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), center);
                if (d <= radius)
                {
                    Color gold = (d <= radius * 0.7f) ? new Color(1.0f, 0.95f, 0.4f, 1f) : new Color(0.96f, 0.72f, 0.15f, 1f);
                    tex.SetPixel(x, y, gold);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 40f, 0, SpriteMeshType.FullRect);
    }
}

/// <summary>
/// Componente Puente acoplado al GameObject del Personaje para recibir y propagar eventos nativos de Physics2D.
/// </summary>
public class Collision2DHook : MonoBehaviour
{
    private Collision2DController _controller;

    public void Initialize(Collision2DController controller)
    {
        _controller = controller;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        _controller?.HandleCollisionEnter(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        _controller?.HandleCollisionStay(collision);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        _controller?.HandleCollisionExit(collision);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        _controller?.HandleTriggerEnter(other);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        _controller?.HandleTriggerExit(other);
    }
}
