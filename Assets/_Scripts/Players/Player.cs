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
    public int currentTurnCount = 0;    // 현재 진행 턴 수
    public int currentScore = 0;        // 현재 점수
    public int multiplier = 1;          // 배당
    public int goCount = 0;             // 고 횟수
    public int BbuckCount = 0;          // 뻑 횟수

    [Header("배당 계산용 상태")]
    public int shakeCount = 0;          // 흔든 횟수 (점수 x2)
    public int bombCount = 0;           // 폭탄 횟수 (점수 x2)

    [Header("획득 패 앵커")]
    public Transform gwangAnchor;      // 광 (왼쪽)
    public Transform yeolggeutAnchor;  // 열끗 (가운데 위)
    public Transform ddeeAnchor;       // 띠 (가운데 아래)
    public Transform peeAnchor;        // 피 (오른쪽)

    // 획득 패 겹침 간격
    Vector3 colOffset = new Vector3(0.2f, 0, 0);
    Vector3 rowOffset = new Vector3(0, 0.5f, 0);

    /** 새 게임 시작 시 초기화 **/
    public virtual void StartGame()
    {
        currentTurnCount = 0;
        BbuckCount = 0;
    }

    /** 턴 시작 시 초기화 **/
    public virtual void StartTurn() 
    {
        selectedCard = null;
        hasPlayed = false;
        currentTurnCount++;
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

    /** 상대에게 피를 뺏기는 함수 **/
    public List<Card> LosePees(int count)
    {
        // 반환할 피 리스트
        List<Card> stolen = new List<Card>();
        // 피, 쌍피만 검색해서 리스트에 삽입
        List<Card> myPees = capturedCards.FindAll(c => c.Type == CardType.Pee || c.Type == CardType.Ssangpee);

        // 뺏길 피가 없을 시 빈 리스트 반환
        if (myPees.Count == 0) return stolen;

        if (count == 2)
        {
            // 2장 뺏길 땐 쌍피 우선, 없으면 일반 피 2장
            Card ssangPee = myPees.Find(c => c.Type == CardType.Ssangpee);
            if (ssangPee != null)
            {
                stolen.Add(ssangPee);
            }
            else
            {
                var normalPees = myPees.FindAll(c => c.Type == CardType.Pee);
                stolen.AddRange(normalPees.Take(Mathf.Min(2, normalPees.Count)));
            }    
        }
        else if (count == 1)
        {
            // 일반 피 뺏기, 없으면 아무거나 집어오기
            Card normalPee = myPees.Find(c => c.Type == CardType.Pee);
            if (normalPee != null)
                stolen.Add(normalPee);
            else
                stolen.Add(myPees[0]);
        }

        // 먹은 패 재정렬
        foreach (var c in stolen) capturedCards.Remove(c);
        if (stolen.Count > 0) OrganizeCapturedCards();

        return stolen;
    }

    /** 해당 패의 폭탄/흔들기 가능 여부 검사 **/
    public HintType CheckSpecialMoveCondition(Card selectedCard)
    {
        // 현재 소지패에 매개변수 패와 월이 똑같은 패가 몇 장 있는지 검사
        int handCount = handCards.Count(c => c.Month == selectedCard.Month);

        // 바닥패 중에 매개변수 패와 월이 똑같은 패가 있는지 검사
        var table = GameManager.Instance.CardDealer.TableCards;
        int tableCount = (table.ContainsKey(selectedCard.Month)) ? table[selectedCard.Month].Count : 0;

        if (handCount == 3)
        {
            // 패에 3장, 바닥에 1장이면 폭탄
            if (tableCount > 0) 
                return HintType.Bomb;
            // 아니면 흔들기
            else 
                return HintType.Shake;
        }

        return HintType.None;
    }
}
