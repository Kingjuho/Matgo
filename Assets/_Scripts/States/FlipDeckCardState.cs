using System.Collections;
using UnityEngine;
using DG.Tweening;

public class FlipDeckCardState : GameStateBase
{
    public FlipDeckCardState(GameManager manager) : base(manager) { }

    public override IEnumerator Execute()
    {
        // 덱에서 1장 드로우
        GameManager.lastDeckCard = GameManager.CardDealer.deck.Draw();
        // 덱이 텅 비었다면 바로 판정 시작
        if (GameManager.lastDeckCard == null)
        {
            GameManager.ChangeState(GameManager.StateResolveMatch);
            yield break;
        }

        // 잠시 HandCards로 설정
        GameManager.lastDeckCard.SetSortingOrder(Constants.HandCards, 100);

        // 카드의 출발 위치를 덱 앵커로 세팅
        GameManager.lastDeckCard.transform.position = GameManager.CardDealer.deckAnchor.position;
        GameManager.lastDeckCard.transform.rotation = GameManager.CardDealer.deckAnchor.rotation;

        // 카드를 화면 중앙으로 살짝 띄우면서 앞면으로 뒤집음
        Vector3 flipPos = GameManager.CardDealer.deckAnchor.position + new Vector3(1.5f, 0.5f, 0);
        GameManager.lastDeckCard.transform.DOMove(flipPos, 0.3f).SetEase(Ease.OutBack);
        GameManager.lastDeckCard.Flip(true);

        // 대기
        yield return new WaitForSeconds(0.5f);

        // 바닥패 목적지 탐색
        int orderInLayer;
        Vector3 targetPos = GameManager.CardDealer.CalculateTablePosition(GameManager.lastDeckCard, out orderInLayer);

        // 애니메이션 재생
        yield return GameManager.StartCoroutine(AnimationManager.Instance?.PlayDropCardToTable(GameManager.lastDeckCard, targetPos, orderInLayer, false));

        GameManager.ChangeState(GameManager.StateResolveMatch);
    }
}