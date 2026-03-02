using UnityEngine;

/// <summary>
/// Sprite-based HP bar that sits above an enemy.
/// Attach to an empty child of the enemy (e.g. "HealthBar").
/// Background and fill SpriteRenderers are always created at runtime in Awake() —
/// no Inspector assignment needed. Tune barWidth, barHeight, yOffset, and colours.
///
/// The fill shrinks from right to left and lerps from green (full) to red (empty).
/// LateUpdate counteracts the enemy's horizontal scale-flip so the bar always faces forward.
/// </summary>
public class EnemyHealthBarUI : MonoBehaviour
{
    [Header("Layout")]
    [Tooltip("Total width of the bar in world units.")]
    [SerializeField] private float barWidth  = 1f;
    [Tooltip("Height of the bar in world units.")]
    [SerializeField] private float barHeight = 0.1f;
    [Tooltip("Vertical offset above the enemy's pivot.")]
    [SerializeField] private float yOffset   = 0.7f;

    [Header("Colours")]
    [SerializeField] private Color fullColor       = Color.green;
    [SerializeField] private Color emptyColor      = Color.red;
    [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);

    // ── Runtime ───────────────────────────────────────────────────────────────

    private EnemyHealth    _enemyHealth;
    private SpriteRenderer _backgroundRenderer;
    private SpriteRenderer _fillRenderer;
    private Transform      _fillTransform;

    private static Sprite _sharedSquare;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        transform.localPosition = new Vector3(0f, yOffset, 0f);

        Sprite square = GetSharedSquareSprite();

        // Background — rendered just above enemy sprites (sortingOrder 10).
        var bg = new GameObject("HealthBar_BG");
        bg.transform.SetParent(transform, false);
        bg.transform.localPosition = Vector3.zero;
        bg.transform.localScale    = new Vector3(barWidth, barHeight, 1f);
        _backgroundRenderer        = bg.AddComponent<SpriteRenderer>();
        _backgroundRenderer.sprite = square;
        _backgroundRenderer.color  = backgroundColor;
        _backgroundRenderer.sortingOrder = 10;

        // Fill — rendered on top of the background (sortingOrder 11).
        var fill = new GameObject("HealthBar_Fill");
        fill.transform.SetParent(transform, false);
        fill.transform.localPosition = Vector3.zero;
        fill.transform.localScale    = new Vector3(barWidth, barHeight, 1f);
        _fillRenderer                = fill.AddComponent<SpriteRenderer>();
        _fillRenderer.sprite         = square;
        _fillRenderer.color          = fullColor;
        _fillRenderer.sortingOrder   = 11;

        _fillTransform = fill.transform;
    }

    private void Start()
    {
        _enemyHealth = GetComponentInParent<EnemyHealth>();

        if (_enemyHealth != null)
        {
            _enemyHealth.OnHealthChanged += HandleHealthChanged;
            HandleHealthChanged(_enemyHealth.CurrentHealth, _enemyHealth.MaxHealth);
        }
    }

    private void LateUpdate()
    {
        // Counteract the enemy's horizontal flip so the bar never appears mirrored.
        float parentScaleX = transform.parent != null ? transform.parent.lossyScale.x : 1f;
        float signX        = parentScaleX < 0f ? -1f : 1f;
        Vector3 ls         = transform.localScale;
        transform.localScale = new Vector3(signX * Mathf.Abs(ls.x), ls.y, ls.z);
    }

    private void OnDestroy()
    {
        if (_enemyHealth != null)
            _enemyHealth.OnHealthChanged -= HandleHealthChanged;
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    private void HandleHealthChanged(int current, int max)
    {
        float percent = max > 0 ? (float)current / max : 0f;
        SetFill(Mathf.Clamp01(percent));
    }

    private void SetFill(float percent)
    {
        // Scale the fill horizontally and reposition so its left edge stays fixed.
        float fillWidth   = barWidth * percent;
        Vector3 fillScale = _fillTransform.localScale;
        fillScale.x       = fillWidth;
        _fillTransform.localScale = fillScale;

        // Left-anchor: shift fill centre so left edges of bg and fill align.
        Vector3 fillPos = _fillTransform.localPosition;
        fillPos.x       = -barWidth / 2f + fillWidth / 2f;
        _fillTransform.localPosition = fillPos;

        // Green → yellow → red colour transition.
        _fillRenderer.color = Color.Lerp(emptyColor, fullColor, percent);
    }

    /// <summary>Lazily creates a shared 1×1 white square sprite used by all health bars.</summary>
    private static Sprite GetSharedSquareSprite()
    {
        if (_sharedSquare != null)
            return _sharedSquare;

        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode   = TextureWrapMode.Clamp
        };
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();

        _sharedSquare = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return _sharedSquare;
    }
}
