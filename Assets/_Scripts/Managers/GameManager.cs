using DG.Tweening;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 싱글톤
    public static GameManager Instance { get; private set; }

    // 컴포넌트
    public CardDealer CardDealer;

    [Header("플레이어 객체")]
    public HumanPlayer humanPlayer;
    public ComputerPlayer computerPlayer;

    [Header("해당 턴 판정용 데이터")]
    public Card lastPlayerCard;     // 이번 턴에 손에서 낸 카드
    public Card lastDeckCard;       // 이번 턴에 덱에서 깐 카드

    [Header("현재 상태")]
    public GameState currentState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        ChangeState(GameState.Init);
    }

    /** FSM **/
    public void ChangeState(GameState newState)
    {
        currentState = newState;
        switch (currentState)
        {
            case GameState.Init:
                StartCoroutine(InitRoutine());
                break;
            case GameState.CheckPresident:
                ChangeState(GameState.PlayerTurn);
                break;
            case GameState.PlayerTurn:
                StartCoroutine(PlayerTurnRoutine());
                break;
            case GameState.AITurn:
                // TODO: AI 로직 개발 후 추가
                break;
            case GameState.PlayHandCard:
                StartCoroutine(PlayHandCardRoutine());
                break;
            case GameState.FlipDeckCard:
                StartCoroutine(FlipDeckCardRoutine());
                break;
            case GameState.ResolveMatch:
                break;
            case GameState.CheckScore:
                break;
            case GameState.TurnEnd:
                break;
            case GameState.GameOver:
                break;
        }
    }

    /** 초기화 루틴 **/
    private IEnumerator InitRoutine()
    {
        // 패 분배
        yield return StartCoroutine(CardDealer.DistributeCardsSequence());

        // 손패 정렬
        humanPlayer.SortHandCards();
        CardDealer.RearrangeHand(humanPlayer, CardDealer.playerHandAnchors);

        // 애니메이션 재생 동안 대기
        yield return new WaitForSeconds(0.4f);

        ChangeState(GameState.CheckPresident);
    }

    /** 플레이어 턴 루틴 **/
    private IEnumerator PlayerTurnRoutine()
    {
        Debug.Log("PlayerTurn 시작");

        // 플레이어 초기화
        humanPlayer.StartTurn();

        // 카드 선택까지 대기
        yield return new WaitUntil(() => humanPlayer.hasPlayed);

        ChangeState(GameState.PlayHandCard);
    }

    ///** 공통: 손패 처리 루틴 **/
    private IEnumerator PlayHandCardRoutine()
    {
        Debug.Log("PlayHandCard 시작");

        // 유저가 선택한 카드를 손패에서 제거
        Card playerCard = humanPlayer.selectedCard;
        humanPlayer.handCards.Remove(playerCard);

        // 해당 카드 데이터 저장
        lastPlayerCard = playerCard;

        // 손패가 빠졌으니 재정렬
        CardDealer.RearrangeHand(humanPlayer, CardDealer.playerHandAnchors);

        // 레이어 확인
        int orderInLayer;
        Vector3 targetPos = CardDealer.CalculateTablePosition(playerCard, out orderInLayer);

        // 렌더링 순서 적용(바닥에 깔린 애들보다 위로 오게)
        UnityEngine.Rendering.SortingGroup sg = playerCard.GetComponent<UnityEngine.Rendering.SortingGroup>();
        if (sg != null)
        {
            sg.sortingLayerName = "TableCards";
            sg.sortingOrder = orderInLayer;
        }

        // 애니메이션 재생
        Vector3 baseScale = playerCard.transform.localScale;
        playerCard.transform.DOScale(baseScale * 2.0f, 0.1f).OnComplete(() =>
        {
            playerCard.transform.DOScale(baseScale, 0.15f);
        });

        // 애니메이션 재생 (이동)
        playerCard.transform.DOMove(targetPos, 0.15f).SetEase(Ease.OutQuad);
        playerCard.transform.DORotateQuaternion(Quaternion.identity, 0.15f);

        // 애니메이션 종료까지 대기
        yield return new WaitForSeconds(0.5f);

        ChangeState(GameState.FlipDeckCard);
    }

    /** 공통: 덱에서 화투를 1장 뽑은 후 처리 루틴 **/
    private IEnumerator FlipDeckCardRoutine()
    {
        Debug.Log("FlipDeckCard 시작");

        // 덱에서 1장 드로우
        Card deckCard = CardDealer.deck.Draw();
        // 덱이 텅 비었다면 바로 판정 시작
        if (deckCard == null)
        {
            ChangeState(GameState.ResolveMatch);
            yield break;
        }

        // 해당 카드 데이터 저장
        lastDeckCard = deckCard;

        // 잠시 HandCards로 설정
        UnityEngine.Rendering.SortingGroup sg = deckCard.GetComponent<UnityEngine.Rendering.SortingGroup>();
        if (sg != null)
        {
            sg.sortingLayerName = "HandCards";
            sg.sortingOrder = 100;
        }

        // 카드의 출발 위치를 덱 앵커로 세팅
        deckCard.transform.position = CardDealer.deckAnchor.position;
        deckCard.transform.rotation = CardDealer.deckAnchor.rotation;

        // 카드를 화면 중앙으로 살짝 띄우면서 앞면으로 뒤집음
        Vector3 flipPos = CardDealer.deckAnchor.position + new Vector3(1.5f, 0.5f, 0);
        deckCard.transform.DOMove(flipPos, 0.3f).SetEase(Ease.OutBack);
        deckCard.Flip(true);

        // 0.5초 대기(무슨 패인지 확인)
        yield return new WaitForSeconds(0.5f);

        // 이동할 바닥패 위치 탐색
        int orderInLayer;
        Vector3 targetPos = CardDealer.CalculateTablePosition(deckCard, out orderInLayer);

        // 바닥패로 변경
        if (sg != null)
        {
            sg.sortingLayerName = "TableCards";
            sg.sortingOrder = orderInLayer;
        }

        // 바닥으로 내리 꽂는 애니메이션
        Vector3 baseScale = deckCard.transform.localScale;
        deckCard.transform.DOScale(baseScale * 1.2f, 0.1f).OnComplete(() =>
        {
            deckCard.transform.DOScale(baseScale, 0.2f);
        });

        deckCard.transform.DOMove(targetPos, 0.3f).SetEase(Ease.OutQuad);
        deckCard.transform.DORotateQuaternion(Quaternion.identity, 0.3f);

        // 카드가 바닥에 완전히 꽂힐 때까지 대기
        yield return new WaitForSeconds(0.5f);


        ChangeState(GameState.ResolveMatch);
    }

    ///** 공통: 판정 루틴 (쪽, 따닥, 뻑 등) **/
    //private IEnumerator ResolveMatchRoutine()
    //{
    //    ChangeState(GameState.CheckScore);
    //}
}
