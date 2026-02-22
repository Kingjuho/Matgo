using System.Collections.Generic;
using UnityEngine;

public class HumanPlayer : Player
{
    [Header("입력 상태")]
    public Card selectedCard = null;    // 선택한 패
    public bool hasPlayed = false;      // 해당 턴의 패 선택 여부

    private void Start()
    {
        playerName = "플레이어";
    }

    /** 턴 시작 시 초기화 **/
    public void StartTurn()
    {
        selectedCard = null;
        hasPlayed = false;
        EvaluateHints();
    }

    /** 카드가 클릭되었을 때 호출되는 함수 **/
    public void SelectCard(Card card)
    {
        selectedCard = card;
        hasPlayed = true;

        // 선택을 마쳤으니 힌트 삭제
        foreach (var c in handCards) c.ShowHint(HintType.None);
    }

    /** 소지 패 힌트 활성화 함수 **/
    public void EvaluateHints()
    {
        // 초기화
        foreach (var card in handCards) card.ShowHint(HintType.None);

        // 1~12월 순회
        for (int m = 1; m <= 12; m++)
        {
            CardMonth currentMonth = (CardMonth) m;

            // 해당 월의 카드가 각각 어디에 몇 장 있는지 확인
            int handCount = GetCountInList(handCards, currentMonth);
            int tableCount = GetCountOnTable(currentMonth);
            int capturedCount = GetCountInList(capturedCards, currentMonth) + GetCountInList(GameManager.Instance.computerPlayer.capturedCards, currentMonth);

            // 내 패에 없으면 스킵
            if (handCount == 0) continue;

            HintType resultHint = HintType.None;
            // 폭탄/흔들기
            if (handCount == 3)
            {
                if (tableCount == 1)
                    resultHint = HintType.Bomb;
                else
                    resultHint = HintType.Shake;
            }
            // 기본/굳은자 2
            else if (handCount == 2)
            {
                if (capturedCount == 2) 
                    resultHint = HintType.Good2;
                else if (tableCount > 0) 
                    resultHint = HintType.Basic;
            }
            // 기본/굳은자 1
            else if (handCount == 1)
            {
                if (tableCount > 0)
                {
                    if (capturedCount == 2) 
                        resultHint = HintType.Good1;
                    else 
                        resultHint = HintType.Basic;
                }
            }

            // 힌트 아이콘 활성화
            if (resultHint != HintType.None)
            {
                foreach (var card in handCards)
                    if (card.Month == currentMonth) card.ShowHint(resultHint);
            }
        }
    }

    /** 해당 리스트에서 지정한 월의 카드가 몇 장 있는지 탐색 **/
    private int GetCountInList(List<Card> list, CardMonth month)
    {
        int count = 0;
        foreach (var card in list) 
            if (card.Month == month) count++;

        return count;
    }

    /** 바닥패에 해당 월 카드가 몇 장 깔려있는지 탐색 **/
    private int GetCountOnTable(CardMonth month)
    {
        var table = GameManager.Instance.CardDealer.TableCards;
        if (table.ContainsKey(month)) return table[month].Count;
        return 0;
    }
}
