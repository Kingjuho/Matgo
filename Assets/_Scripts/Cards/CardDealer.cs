using System.Collections;
using UnityEngine;
using DG.Tweening;

public class CardDealer : MonoBehaviour
{
    [Header("덱")]
    public Deck deck;

    [Header("2D 앵커 포인트")]
    public Transform deckAnchor;        // 덱 위치
    public Transform aiHandAnchor;      // AI 패 시작점
    public Transform playerHandAnchor;  // 플레이어 패 시작점
    public Transform tableAnchor;       // 바닥 패 시작점

    [Header("간격 설정")]
    public float handSpacing = 0.8f;                        // 손패 겹치는 간격 (X축)
    public Vector2 tableSpacing = new Vector2(1.2f, 1.5f);  // 바닥패 간격 (X, Y축)
    public int tableColumns = 5;                            // 줄당 장 수

    [Header("배분 속도")]
    public float dealSpeed = 0.2f;
    public float dealInterval = 0.1f;

    // 현재 각 영역에 나눠준 카드 개수
    private int _aiCount = 0;
    private int _playerCount = 0;
    private int _tableCount = 0;

    // 배분 영역 열거형
    private enum Target { AI, Player, Table }

    private void Start()
    {
        GameStart();
    }

    public void GameStart() { StartCoroutine(DistributeCardsSequence()); }

    /** 전체 패 분배 코루틴 **/
    IEnumerator DistributeCardsSequence()
    {
        yield return new WaitForSeconds(0.5f); // 덱 생성 및 세팅 대기시간

        Debug.Log("[Dealer] 패 분배 시작 (2D World)");

        // 1라운드: AI(4) -> Player(4) -> Table(4)
        yield return StartCoroutine(Deal(Target.AI, 4));
        yield return StartCoroutine(Deal(Target.Player, 4));
        yield return StartCoroutine(Deal(Target.Table, 4));

        // 2라운드: AI(4) -> Player(4) -> Table(4)
        yield return StartCoroutine(Deal(Target.AI, 4));
        yield return StartCoroutine(Deal(Target.Player, 4));
        yield return StartCoroutine(Deal(Target.Table, 4));

        // 3라운드: AI(2) -> Player(2)
        yield return StartCoroutine(Deal(Target.AI, 2));
        yield return StartCoroutine(Deal(Target.Player, 2));

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

            Vector3 targetPos = Vector3.zero;
            bool isFaceUp = true;
            int orderInLayer = 0; // 나중에 겹친 카드가 제대로 위로 올라오게 렌더링 순서 지정

            // 목적지 수학 계산
            switch (target)
            {
                case Target.AI:
                    // 기준점 + (현재 개수 * 간격)
                    targetPos = aiHandAnchor.position + new Vector3(_aiCount * handSpacing, 0, 0);
                    isFaceUp = false;
                    orderInLayer = _aiCount;
                    _aiCount++;
                    break;

                case Target.Player:
                    targetPos = playerHandAnchor.position + new Vector3(_playerCount * handSpacing, 0, 0);
                    isFaceUp = true;
                    orderInLayer = _playerCount;
                    _playerCount++;
                    break;

                case Target.Table:
                    // 그리드(격자) 수학: 몫은 행(Y), 나머지는 열(X)
                    int col = _tableCount % tableColumns;
                    int row = _tableCount / tableColumns;

                    targetPos = tableAnchor.position + new Vector3(col * tableSpacing.x, -row * tableSpacing.y, 0);
                    isFaceUp = true;
                    orderInLayer = _tableCount;
                    _tableCount++;
                    break;
            }

            // 렌더링 순서 세팅 (카드가 겹칠 때 뒤에 온 놈이 위로 보이게)
            card.GetComponent<SpriteRenderer>().sortingOrder = orderInLayer;

            // 아무런 방해 없이 완벽하게 날아가는 애니메이션
            card.transform.DOMove(targetPos, dealSpeed).SetEase(Ease.OutCubic);

            if (isFaceUp) card.Flip(true);

            yield return new WaitForSeconds(dealInterval);
        }
    }
}
