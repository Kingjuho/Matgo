using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDealer : MonoBehaviour
{
    [Header("덱")]
    public Deck deck;

    [Header("2D 앵커 포인트")]
    public Transform deckAnchor;            // 덱
    public Transform[] aiHandAnchors;       // AI 패 (10장)
    public Transform[] playerHandAnchors;   // 플레이어 패 (10장)
    public Transform[] tableAnchors;        // 테이블 (12칸)

    [Header("간격 설정")]
    public float handSpacing = 0.8f;                            // 손패 겹치는 간격 (X축)
    public Vector2 tableSpacing = new Vector2(1.2f, 1.5f);      // 바닥패 간격 (X, Y축)
    public int tableColumns = 5;                                // 줄당 장 수
    public Vector2 stackOffset = new Vector2(0.15f, -0.2f);     // 바닥패의 같은 월 패가 겹칠 때 간격

    [Header("배분 속도")]
    public float dealSpeed = 0.2f;
    public float dealInterval = 0.1f;

    // 현재 각 영역에 나눠준 카드 개수
    private int _aiCount = 0;
    private int _playerCount = 0;

    // 바닥패 상태 관리
    private Dictionary<CardMonth, List<Card>> _tableCards = new Dictionary<CardMonth, List<Card>>();

    // 서로 다른 월의 개수
    private int _uniqueMonthCount = 0;

    // 배분 영역 열거형
    private enum Target { AI, Player, Table }

    /** 전체 패 분배 코루틴 **/
    public IEnumerator DistributeCardsSequence()
    {
        yield return new WaitForSeconds(0.5f); // 덱 생성 및 세팅 대기시간

        Debug.Log("[Dealer] 패 분배 시작");

        // 1라운드: AI(5) -> Player(5) -> Table(4)
        yield return StartCoroutine(Deal(Target.AI, 5));
        yield return StartCoroutine(Deal(Target.Player, 5));
        yield return StartCoroutine(Deal(Target.Table, 4));

        // 2라운드: AI(5) -> Player(5) -> Table(4)
        yield return StartCoroutine(Deal(Target.AI, 5));
        yield return StartCoroutine(Deal(Target.Player, 5));
        yield return StartCoroutine(Deal(Target.Table, 4));

        Debug.Log("[Dealer] 패 분배 완료");
    }

    /** 패 분배 코루틴 **/
    IEnumerator Deal(Target target, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Card card = deck.Draw();
            if (card == null) yield break;

            // 출발지: 덱
            card.transform.position = deckAnchor.position;
            card.transform.rotation = deckAnchor.rotation;

            // 포지션, 회전값, 앞뒷면 여부, 레이어
            Vector3 targetPos = Vector3.zero;
            Quaternion targetRot = Quaternion.identity;
            bool isFaceUp = true;
            int orderInLayer = 0;

            // 목적지 계산
            switch (target)
            {
                case Target.AI:
                    // 앵커 배열 범위 초과 방지
                    int aiIndex = Mathf.Min(_aiCount, aiHandAnchors.Length - 1);
                    targetPos = aiHandAnchors[aiIndex].position;
                    targetRot = aiHandAnchors[aiIndex].rotation;

                    isFaceUp = false;
                    orderInLayer = _aiCount;
                    _aiCount++;

                    GameManager.Instance.computerPlayer.handCards.Add(card);
                    break;

                case Target.Player:
                    int pIndex = Mathf.Min(_playerCount, playerHandAnchors.Length - 1);
                    targetPos = playerHandAnchors[pIndex].position;
                    targetRot = playerHandAnchors[pIndex].rotation;

                    isFaceUp = true;
                    orderInLayer = _playerCount;
                    _playerCount++;

                    GameManager.Instance.humanPlayer.handCards.Add(card);
                    break;

                case Target.Table:
                    targetPos = CalculateTablePosition(card, out orderInLayer);
                    isFaceUp = true;
                    break;
            }

            // 렌더링 순서 세팅 (카드가 겹칠 때 뒤에 온 놈이 위로 보이게)
            card.GetComponent<SpriteRenderer>().sortingOrder = orderInLayer;

            // 애니메이션 재생
            card.transform.DOMove(targetPos, dealSpeed).SetEase(Ease.OutCubic);

            if (isFaceUp) card.Flip(true);

            yield return new WaitForSeconds(dealInterval);
        }
    }

    /** 바닥패 위치 및 렌더링 순서 계산 함수 **/
    private Vector3 CalculateTablePosition(Card card, out int sortingOrder)
    {
        CardMonth month = card.Month;

        // 1. 해당 월(Month)의 리스트가 없으면 새로 생성
        if (!_tableCards.ContainsKey(month))
        {
            _tableCards[month] = new List<Card>();
            _uniqueMonthCount++;
        }

        // 이 카드가 해당 월의 몇 번째 카드인지 확인
        List<Card> monthGroup = _tableCards[month];
        int stackIndex = monthGroup.Count;

        // 2. 이 월(Month)이 몇 번째 슬롯인지 확인
        int slotIndex = GetMonthSlotIndex(month);

        // 미리 지정해둔 앵커의 위치를 바로 가져옴(에러 발생 시 0)
        if (slotIndex >= tableAnchors.Length) slotIndex = 0;
        Vector3 basePos = tableAnchors[slotIndex].position;

        // 3. 겹침 오프셋(Stack Offset) 추가
        Vector3 finalPos = basePos + new Vector3(stackOffset.x * stackIndex, stackOffset.y * stackIndex, 0);

        // 4. 리스트에 카드 등록
        monthGroup.Add(card);

        // 5. 렌더링 순서 (겹친 놈이 위로 오게)
        sortingOrder = (slotIndex * 10) + stackIndex;

        return finalPos;
    }

    /** 해당 월이 몇 번째 슬롯인지 찾아주는 헬퍼 함수 **/
    private int GetMonthSlotIndex(CardMonth targetMonth)
    {
        int index = 0;
        foreach (var month in _tableCards.Keys)
        {
            if (month == targetMonth) return index;
            index++;
        }
        return 0; // 에러 대비
    }
}
