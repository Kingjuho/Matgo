using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayHandCardState : GameStateBase
{
    public PlayHandCardState(GameManager manager) : base(manager) { }

    public override IEnumerator Execute()
    {
        GameManager.lastPlayerCard = GameManager.currentPlayer.selectedCard;

        // 더미 패 예외 처리
        if (GameManager.lastPlayerCard.Type == CardType.Dummy)
        {
            // 소멸
            GameManager.currentPlayer.handCards.Remove(GameManager.lastPlayerCard);
            Object.Destroy(GameManager.lastPlayerCard.gameObject);
            GameManager.lastPlayerCard = null;

            // 손패 재정렬
            GameManager.currentPlayer.SortHandCards();
            Transform[] d_anchors = (GameManager.currentPlayer == GameManager.humanPlayer) ? GameManager.CardDealer.playerHandAnchors : GameManager.CardDealer.aiHandAnchors;
            GameManager.CardDealer.RearrangeHand(GameManager.currentPlayer, d_anchors);

            GameManager.ChangeState(GameManager.StateFlipDeckCard);
            yield break;
        }

        // 유저가 선택한 패가 폭탄이나 흔들기가 되는지 검사
        HintType hintType = GameManager.currentPlayer.CheckSpecialMoveCondition(GameManager.lastPlayerCard);
        GameManager.isBombThisTurn = false;
        GameManager.isShakeThisTurn = false;

        // 폭탄
        if (hintType == HintType.Bomb)
        {
            GameManager.isBombThisTurn = true;
            GameManager.currentPlayer.bombCount++;

            // 소지패에서 같은 월 3장을 모두 바닥패로 던짐
            CardMonth bombMonth = GameManager.lastPlayerCard.Month;
            List<Card> bombCards = GameManager.currentPlayer.handCards.FindAll(c => c.Month == bombMonth);
            foreach (Card card in bombCards)
            {
                // 유저가 선택한 카드를 손패에서 제거
                GameManager.currentPlayer.handCards.Remove(card);

                // 바닥패 목적지 탐색
                int orderInLayer;
                Vector3 targetPos = GameManager.CardDealer.CalculateTablePosition(card, out orderInLayer);

                // 애니메이션 재생
                GameManager.StartCoroutine(AnimationManager.Instance?.PlayDropCardToTable(card, targetPos, orderInLayer, true));
            }

            // 더미 패 2장 추가
            for (int i = 0; i < 2; i++)
            {
                Card dummy = GameManager.CardDealer.CreateDummyCard();
                GameManager.currentPlayer.handCards.Add(dummy);
            }
        }
        else
        {
            // 흔들기
            if (hintType == HintType.Shake)
            {
                GameManager.isShakeThisTurn = true;
                GameManager.currentPlayer.shakeCount++;

                // 소지패에서 같은 월 3장 검색
                CardMonth shakeMonth = GameManager.lastPlayerCard.Month;
                List<Card> shakeCards = GameManager.currentPlayer.handCards.FindAll(c => c.Month == shakeMonth);

                // 2초간 흔들기 팝업 표시
                UIManager.Instance?.ShowShakePopup(shakeCards);
                yield return new WaitForSeconds(2.0f);
                UIManager.Instance?.HideShakePopup();
            }

            // 유저가 선택한 카드를 손패에서 제거
            GameManager.currentPlayer.handCards.Remove(GameManager.lastPlayerCard);

            // 바닥패 목적지 탐색
            int orderInLayer;
            Vector3 targetPos = GameManager.CardDealer.CalculateTablePosition(GameManager.lastPlayerCard, out orderInLayer);

            // 애니메이션 재생
            yield return GameManager.StartCoroutine(AnimationManager.Instance?.PlayDropCardToTable(GameManager.lastPlayerCard, targetPos, orderInLayer, true));
        }

        // 손패 재정렬
        GameManager.currentPlayer.SortHandCards();
        Transform[] anchors = (GameManager.currentPlayer == GameManager.humanPlayer) ? GameManager.CardDealer.playerHandAnchors : GameManager.CardDealer.aiHandAnchors;
        GameManager.CardDealer.RearrangeHand(GameManager.currentPlayer, anchors);

        GameManager.ChangeState(GameManager.StateFlipDeckCard);
    }
}