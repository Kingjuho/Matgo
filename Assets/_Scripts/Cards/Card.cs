using UnityEngine;
using DG.Tweening;

public class Card : MonoBehaviour
{
    [Header("카드 정의")]
    private CardMonth _month;
    private CardType _type;
    private SpecialFeature _feature;

    [Header("스프라이트")]
    public Sprite frontImage;      // 앞면 이미지
    public Sprite backImage;       // 뒷면 이미지
    private SpriteRenderer _spriteRenderer;

    // 앞/뒷면 여부
    private bool _isFront = true;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /** 초기화 함수 **/
    public void Initialize(
        CardMonth month, 
        CardType type, 
        Sprite sprite,
        SpecialFeature feature = SpecialFeature.None
    )
    {
        _month = month;
        _type = type;
        _feature = feature;
        frontImage = sprite;

        _spriteRenderer.sprite = frontImage;
    }

    /** 카드 뒤집기 **/
    public void Flip(bool showFront)
    {
        _isFront = showFront;

        // 0.2초동안 Y축으로 90도 회전 -> 이미지 교체 -> 원상복구
        transform.DOScaleX(0, 0.2f).OnComplete(() =>
        {
            _spriteRenderer.sprite = _isFront ? frontImage : backImage;
            transform.DOScaleX(1, 0.2f);
        });
    }
}
