using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Card : MonoBehaviour
{
    [Header("카드 정의")]
    public CardMonth Month { get; private set; }
    public CardType Type { get; private set; }
    public SpecialFeature Feature { get; private set; }

    [Header("UI")]
    public Image cardImage;         // 뒷면 이미지
    public Sprite frontSprite;      // 앞면 이미지

    // 앞/뒷면 여부
    private bool _isFront = true;

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

        cardImage.sprite = frontSprite;
        cardImage.color = Color.white;
    }

    /** 카드 뒤집기(초기화 용) **/
    public void FlipInstant(bool showFront)
    {
        _isFront = showFront;
        UpdateVisual();
        transform.localScale = Vector3.one;
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
            transform.DOScaleX(1, 0.2f);
        });
    }

    /** 카드의 앞/뒷면을 그려주는 함수 **/
    private void UpdateVisual()
    {
        if (_isFront)
        {
            cardImage.sprite = frontSprite;
            cardImage.color = Color.white;
        }
        else
        {
            cardImage.sprite = null;
            cardImage.color = new Color(0.8f, 0.2f, 0.2f);
        }
    }
}
