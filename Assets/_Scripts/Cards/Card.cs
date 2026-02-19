using UnityEngine;
using DG.Tweening;

public class Card : MonoBehaviour
{
    [Header("카드 정의")]
    public CardMonth Month { get; private set; }
    public CardType Type { get; private set; }
    public SpecialFeature Feature { get; private set; }

    [Header("UI")]
    public Sprite frontSprite;              // 앞면 이미지
    public Sprite backSprite;              // 뒷면 이미지
    private SpriteRenderer _spriteRenderer; // 스프라이트 렌더러 컴포넌트

    // 앞/뒷면 여부
    private bool _isFront = true;

    // 오리지널 스케일
    private Vector3 _originalScale;

    private void Awake() 
    { 
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _originalScale = transform.localScale;
    }

    /** 초기화 함수 **/
    public void Initialize(
        CardMonth month, 
        CardType type, 
        Sprite sprite,
        SpecialFeature feature = SpecialFeature.None
    )
    {
        Month = month;
        Type = type;
        Feature = feature;
        frontSprite = sprite;
    }

    /** 카드 뒤집기(초기화 용) **/
    public void FlipInstant(bool showFront)
    {
        _isFront = showFront;
        UpdateVisual();
        transform.localScale = _originalScale;
    }

    /** 카드 뒤집기(플레이 용) **/
    public void Flip(bool showFront)
    {
        // 앞면 -> 앞면, 뒷면 -> 뒷면 필터링
        if (_isFront == showFront) return;
        _isFront = showFront;

        // 0.2초동안 Y축으로 90도 회전 -> 이미지 교체 -> 원상복구
        transform.DOScaleX(0, 0.2f).OnComplete(() =>
        {
            UpdateVisual();
            transform.DOScaleX(_originalScale.x, 0.2f);
        });
    }

    /** 카드의 앞/뒷면을 그려주는 함수 **/
    private void UpdateVisual()
    {
        if (_isFront)
        {
            _spriteRenderer.sprite = frontSprite;
            _spriteRenderer.color = Color.white;
        }
        else
        {
            _spriteRenderer.sprite = backSprite;
            _spriteRenderer.color = new Color(0.8f, 0.2f, 0.2f);
        }
    }
}
