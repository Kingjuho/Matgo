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
    public Sprite backSprite;               // 뒷면 이미지
    private SpriteRenderer _spriteRenderer; // 스프라이트 렌더러 컴포넌트

    [Header("힌트 UI")]
    public SpriteRenderer topArrowIcon;     // 상단 힌트
    public SpriteRenderer bottomRightIcon;  // 우하단 힌트(굳, 흔들기, 폭탄)

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

    /** 카드 힌트 표시 함수 **/
    public void ShowHint(HintType type)
    {
        // 초기화
        topArrowIcon.gameObject.SetActive(false);
        bottomRightIcon.gameObject.SetActive(false);

        switch (type)
        {
            case HintType.Basic:
                topArrowIcon.sprite = null; // TODO: 파란색 화살표 스프라이트 넣기
                topArrowIcon.gameObject.SetActive(true);
                break;
            case HintType.Good1:
                topArrowIcon.sprite = null; // TODO: 회색 화살표 스프라이트 넣기
                topArrowIcon.gameObject.SetActive(true);
                break;
            case HintType.Good2:
                bottomRightIcon.sprite = null; // TODO: [굳] 스프라이트
                bottomRightIcon.gameObject.SetActive(true);
                break;
            case HintType.Shake:
                bottomRightIcon.sprite = null; // TODO: [종] 스프라이트
                bottomRightIcon.gameObject.SetActive(true);
                break;
            case HintType.Bomb:
                bottomRightIcon.sprite = null; // TODO: [폭탄] 스프라이트
                bottomRightIcon.gameObject.SetActive(true);
                break;
        }
    }
}
