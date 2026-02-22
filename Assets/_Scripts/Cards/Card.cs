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

    [Header("힌트 스프라이트 에셋")]
    public Sprite arrowBlue;
    public Sprite arrowGray;
    public Sprite iconGood;
    public Sprite iconShake;
    public Sprite iconBomb;

    [HideInInspector]
    public Vector3 basePosition;        // 마우스 이벤트에 사용하기 위한 좌표값
    // bool타입 변수
    private bool _isFront = true;       // 앞/뒷면 여부
    private bool _isHovered = false;    // 마우스 호버링 여부

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
                topArrowIcon.sprite = arrowBlue;
                topArrowIcon.gameObject.SetActive(true);
                break;
            case HintType.Good1:
                topArrowIcon.sprite = arrowGray;
                topArrowIcon.gameObject.SetActive(true);
                break;
            case HintType.Good2:
                bottomRightIcon.sprite = iconGood;
                bottomRightIcon.gameObject.SetActive(true);
                break;
            case HintType.Shake:
                bottomRightIcon.sprite = iconShake;
                bottomRightIcon.gameObject.SetActive(true);
                break;
            case HintType.Bomb:
                bottomRightIcon.sprite = iconBomb;
                bottomRightIcon.gameObject.SetActive(true);
                break;
        }
    }

    /** 마우스 클릭 감지 함수 **/
    private void OnMouseDown()
    {
        // 플레이어 턴 검증
        if (GameManager.Instance.currentState != GameState.PlayerTurn) return;
        // 플레이어 손패에 있는 카드인지 검증
        if (!GameManager.Instance.humanPlayer.handCards.Contains(this)) return;

        GameManager.Instance.humanPlayer.SelectCard(this);
    }

    /** 마우스가 카드 안으로 들어오는 것을 감지 **/
    private void OnMouseEnter()
    {
        // 플레이어 손패에 있는 카드인지 검증
        if (!GameManager.Instance.humanPlayer.handCards.Contains(this)) return;

        _isHovered = true;

        // 카드를 위로 살짝 들어올림
        transform.DOKill(); // 버그 방지용
        transform.DOMoveY(basePosition.y + 0.1f, 0.1f).SetEase(Ease.OutQuad);
    }

    /** 마우스가 카드 밖으로 나가는 것을 감지 **/
    private void OnMouseExit()
    {
        if (!_isHovered) return;
        _isHovered = false;

        // 카드를 이미 냈다면 돌아갈 필요 없음
        if (!GameManager.Instance.humanPlayer.handCards.Contains(this)) return;

        // 원위치
        transform.DOKill(); // 버그 방지용
        transform.DOMoveY(basePosition.y, 0.1f).SetEase(Ease.OutQuad);
    }
}
