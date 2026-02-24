using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Player : MonoBehaviour
{
    [Header("기본 정보")]
    public string playerName;
    public long money = 500000;

    [Header("화투 목록")]
    public List<Card> handCards = new List<Card>();     // 들고 있는 패
    public List<Card> capturedCards = new List<Card>(); // 먹은 패

    [Header("입력 상태")]
    public Card selectedCard = null;    // 선택한 패
    public bool hasPlayed = false;      // 해당 턴의 패 선택 여부

    [Header("게임 상태")]
    public int currentScore = 0;    // 현재 점수
    public int multiplier = 1;      // 배당
    public int goCount = 0;         // 고 횟수

    [Header("획득 패 앵커")]
    public Transform gwangAnchor;      // 광 (왼쪽)
    public Transform yeolggeutAnchor;  // 열끗 (가운데 위)
    public Transform ddeeAnchor;       // 띠 (가운데 아래)
    public Transform peeAnchor;        // 피 (오른쪽)

    // 획득 패 겹침 간격
    Vector3 colOffset = new Vector3(0.2f, 0, 0);
    Vector3 rowOffset = new Vector3(0, 0.5f, 0);

    /** 턴 시작 시 초기화 **/
    public virtual void StartTurn() 
    {
        selectedCard = null;
        hasPlayed = false;
    }

    /** 카드 획득 **/
    public virtual void CaptureCard(Card card)
    {
        capturedCards.Add(card);
    }

    /** 배당 증가 **/
    public void addMultiplier()
    {
        // 변칙적인 배당 증가는 없으므로 2배로 고정
        multiplier *= 2;
        Debug.Log($"[{playerName}] 배당이 {multiplier}배로 증가");
    }

    /** 패 지우기 (턴 시작 시 등) **/
    public void RemoveHandCard(Card card)
    {
        if (handCards.Contains(card))
            handCards.Remove(card);
    }

    /** 손패 정렬 **/
    public void SortHandCards()
    {
        handCards = handCards.OrderBy(card => (int)card.Month).ToList();
    }

    /** 먹은 패 정렬 **/
    public void OrganizeCapturedCards()
    {
        // 광, 열끗, 띠 장수 카운팅
        int gwangCount = 0;
        int yeolCount = 0;
        int ddeeCount = 0;

        // 피 카운팅
        int peeCountRow0 = 0;       // 1번째 줄
        int peeCountRow1 = 0;       // 2번째 줄
        int peeCountRow2 = 0;       // 3번째 줄
        int currentPeeScore = 0;    // 현재 총 점수

        foreach (Card c in capturedCards)
        {
            Vector3 targetPos = Vector3.zero;
            int order = 0;

            if (c.Type == CardType.Gwang)
            {
                targetPos = gwangAnchor.position + (colOffset * gwangCount);
                order = gwangCount;
                gwangCount++;
            }
            else if (c.Type == CardType.Yeolggeut)
            {
                targetPos = yeolggeutAnchor.position + (colOffset * yeolCount);
                order = yeolCount;
                yeolCount++;
            }
            else if (c.Type == CardType.Ddee)
            {
                targetPos = ddeeAnchor.position + (colOffset * ddeeCount);
                order = ddeeCount;
                ddeeCount++;
            }
            else if (c.Type == CardType.Pee || c.Type == CardType.Ssangpee)
            {
                // 0~10점: 1번 줄, 11~20점: 2번 줄, 21점 이상: 3번 줄
                int row = Mathf.Min(currentPeeScore / 10, 2);
                int col = 0;

                if (row == 0) 
                { 
                    col = peeCountRow0; 
                    peeCountRow0++; 
                }
                else if (row == 1) 
                { 
                    col = peeCountRow1; 
                    peeCountRow1++; 
                }
                else 
                { 
                    col = peeCountRow2; 
                    peeCountRow2++; 
                }

                // 위치 = 피 앵커 + (가로 오프셋 * 열) + (세로 오프셋 * 행)
                targetPos = peeAnchor.position + (colOffset * col) + (rowOffset * row);
                // 윗줄이 아랫줄보다 랜더링 더 높게
                order = (row * 10) + col;

                int score = (c.Type == CardType.Ssangpee) ? 2 : 1;
                currentPeeScore += score;
            }

            // 레이어 설정 및 애니메이션 실행
            c.SetSortingOrder("TableCards", order);
            AnimationManager.Instance.MoveCard(c, targetPos, Quaternion.identity, 0.3f);
        }
    }
}
