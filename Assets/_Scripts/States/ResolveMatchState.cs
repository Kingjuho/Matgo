using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResolveMatchState : GameStateBase
{
    public ResolveMatchState(GameManager manager) : base(manager) { }

    public override IEnumerator Execute()
    {
        CardMonth playedMonth = GameManager.lastPlayerCard?.Month ?? (CardMonth)0;
        CardMonth deckMonth = GameManager.lastDeckCard.Month;

        // 더미 패
        if (GameManager.lastPlayerCard == null)
        {
            yield return GameManager.StartCoroutine(ProcessSingleMatchRoutine(deckMonth, GameManager.lastDeckCard, false));
        }
        else
        {
            // 특수 판정 (낸 패 == 뒤집은 패)
            if (playedMonth == deckMonth)
            {
                yield return GameManager.StartCoroutine(ProcessSpecialMatchRoutine(playedMonth));
            }
            // 일반 판정 (낸 패 != 뒤집은 패)
            else
            {
                // 낸 패 처리
                yield return GameManager.StartCoroutine(ProcessSingleMatchRoutine(playedMonth, GameManager.lastPlayerCard, true));
                // 덱 패 처리
                yield return GameManager.StartCoroutine(ProcessSingleMatchRoutine(deckMonth, GameManager.lastDeckCard, false));
            }
        }

        // 싹쓸 판정
        if (GameManager.CardDealer.GetTotalTableCardCount() == 0)
            yield return GameManager.StartCoroutine(StealOpponentPeeRoutine(1));

        // 해당 턴의 유저가 획득한 패 정렬
        GameManager.currentPlayer.OrganizeCapturedCards();
        yield return new WaitForSeconds(0.4f);

        // 판정 시간
        yield return new WaitForSeconds(1f);

        GameManager.ChangeState(GameManager.StateCheckScore);
    }

    /** 2장 중 1장 선택 루틴 **/
    private IEnumerator HandleChoiceRoutine(CardMonth month, Card triggerCard)
    {
        var table = GameManager.CardDealer.TableCards;
        List<Card> options = new List<Card>(table[month]);
        options.Remove(triggerCard);    // 플레이어가 낸 카드는 무조건 먹음

        // 상태 변경
        GameManager.isChoosingCard = true;
        GameManager.selectedChoiceCard = null;

        // 컴퓨터 턴엔 UI 띄우지 않음
        if (GameManager.currentPlayer == GameManager.computerPlayer)
        {
            yield return new WaitForSeconds(0.8f);
            // TODO: AI가 자신에게 유리한 패를 고르도록
            GameManager.selectedChoiceCard = options[0];
            GameManager.isChoosingCard = false;
        }
        else
        {
            // UIManager 호출, 콜백
            UIManager.Instance?.ShowChoicePopup(options, (chosenCard) =>
            {
                GameManager.selectedChoiceCard = chosenCard;
                GameManager.isChoosingCard = false;
            });

            // 선택될 때까지 대기
            yield return new WaitUntil(() => !GameManager.isChoosingCard);
        }

        // 낸 패 획득
        GameManager.currentPlayer.CaptureCard(triggerCard);
        table[month].Remove(triggerCard);

        // 선택한 패 획득
        GameManager.currentPlayer.CaptureCard(GameManager.selectedChoiceCard);
        table[month].Remove(GameManager.selectedChoiceCard);
    }

    /** 상대의 피를 뺏는 루틴 **/
    private IEnumerator StealOpponentPeeRoutine(int count)
    {
        Player opponent = (GameManager.currentPlayer == GameManager.humanPlayer) ? GameManager.computerPlayer : GameManager.humanPlayer;

        // 피가 없으면 continue
        List<Card> stolenCards = opponent.LosePees(count);
        if (stolenCards.Count == 0) yield break;

        foreach (Card stolenCard in stolenCards)
        {
            // 현재 유저의 피에 추가
            GameManager.currentPlayer.CaptureCard(stolenCard);

            // 현재 유저의 피 앵커를 타겟으로 설정 및 레이어 정렬
            Vector3 targetPos = GameManager.currentPlayer.peeAnchor.position;
            stolenCard.SetSortingOrder(Constants.TableCards, 100);

            // 애니메이션 재생
            AnimationManager.Instance?.MoveCard(stolenCard, targetPos, Quaternion.identity, 0.4f);
        }

        // 대기
        yield return new WaitForSeconds(0.4f);
    }

    /** 낸 패와 뒤집은 패가 같은 월일 경우 특수 판정 루틴 **/
    private IEnumerator ProcessSpecialMatchRoutine(CardMonth month)
    {
        int totalCount = GameManager.CardDealer.TableCards[month].Count;
        // 쪽 or 따닥
        if (totalCount == 2 || totalCount == 4)
        {
            CaptureAllCardsOfMonth(month);
            yield return GameManager.StartCoroutine(StealOpponentPeeRoutine(1));
        }
        // 뻑
        else if (totalCount == 3)
        {
            GameManager.currentPlayer.BbuckCount++;

            // 첫뻑 확인
            if (GameManager.currentPlayer.currentTurnCount == 1)
            {
                // TODO: 즉시 7점 상당의 돈을 뺏음
            }

            // 쓰리뻑 확인
            if (GameManager.currentPlayer.BbuckCount >= 3)
            {
                // TODO: 즉시 7점으로 승리
            }

            // 뻑 장부에 기록(자뻑 판정을 위함)
            GameManager.bbuckRecords[month] = GameManager.currentPlayer;
        }
    }

    /** 낸 패와 뒤집은 패가 다른 월일 경우 일반 판정 루틴 **/
    private IEnumerator ProcessSingleMatchRoutine(CardMonth month, Card triggerCard, bool checkBomb = false)
    {
        int count = GameManager.CardDealer.TableCards[month].Count;

        // 단순 획득
        if (count == 2)
        {
            CaptureAllCardsOfMonth(month);
        }
        // 1장 선택
        else if (count == 3)
        {
            yield return GameManager.StartCoroutine(HandleChoiceRoutine(month, triggerCard));
        }
        // 뻑 먹기 or 폭탄
        else if (count == 4)
        {
            CaptureAllCardsOfMonth(month);

            int stealCount = 1;
            if (!checkBomb || !GameManager.isBombThisTurn)
            {
                // 뻑 장부 검사
                if (GameManager.bbuckRecords.ContainsKey(month))
                {
                    // 자뻑일 경우 2장
                    if (GameManager.bbuckRecords[month] == GameManager.currentPlayer) stealCount = 2;
                    GameManager.bbuckRecords.Remove(month);
                }
            }

            // 피 뺏기
            yield return GameManager.StartCoroutine(StealOpponentPeeRoutine(stealCount));
        }
    }

    /** 특정 월의 바닥패를 먹는 헬퍼 함수 **/
    private void CaptureAllCardsOfMonth(CardMonth month)
    {
        // 해당 월의 바닥패가 없으면 종료
        var table = GameManager.CardDealer.TableCards;
        if (!table.ContainsKey(month) || table[month].Count == 0) return;

        // 리스트에 저장 후 해당 바닥패 청소
        List<Card> cardsToCapture = new List<Card>(table[month]);
        table[month].Clear();

        // 해당 플레이어에게 지급
        foreach (Card c in cardsToCapture) GameManager.currentPlayer.CaptureCard(c);
    }
}