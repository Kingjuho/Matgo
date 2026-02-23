using UnityEngine;
using DG.Tweening;
using System.Collections;

public class AnimationManager : MonoBehaviour
{
    public static AnimationManager Instance { get; private set; }

    [Header("애니메이션 설정값")]
    public float tableDropDuration = 0.3f;
    public float hoverScaleMultiplier = 1.2f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /** 특정 위치의 화투를 바닥패로 내리꽂는 애니메이션 **/
    public IEnumerator PlayDropCardToTable(Card card, Vector3 targetPos, int orderInLayer, bool isGiantHandCard = false)
    {
        // 레이어 변경
        card.SetSortingOrder("TableCards", orderInLayer);

        // 스케일 확대 비율 설정
        float scaleMult = isGiantHandCard ? 2.0f : hoverScaleMultiplier;

        // 애니메이션 재생
        Vector3 baseScale = card.transform.localScale;
        card.transform.DOScale(baseScale * scaleMult, 0.1f).OnComplete(() =>
        {
            card.transform.DOScale(baseScale, tableDropDuration);
        });

        // 이동 및 회전
        card.transform.DOMove(targetPos, tableDropDuration).SetEase(Ease.OutQuad);
        card.transform.DORotateQuaternion(Quaternion.identity, tableDropDuration);

        // 애니메이션 종료까지 대기
        yield return new WaitForSeconds(tableDropDuration + 0.1f);
    }
}
