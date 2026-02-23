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

    /** 공통: 손패 처리 루틴 **/
    private IEnumerator PlayHandCardRoutine()
    {
        Debug.Log("PlayHandCard 시작");

        // 유저가 선택한 카드를 손패에서 제거
        lastPlayerCard = humanPlayer.selectedCard;
        humanPlayer.handCards.Remove(lastPlayerCard);

        // 손패 재정렬
        CardDealer.RearrangeHand(humanPlayer, CardDealer.playerHandAnchors);

        // 바닥패 목적지 탐색
        int orderInLayer;
        Vector3 targetPos = CardDealer.CalculateTablePosition(lastPlayerCard, out orderInLayer);

        // 애니메이션 재생
        yield return StartCoroutine(AnimationManager.Instance.PlayDropCardToTable(lastPlayerCard, targetPos, orderInLayer, true));

        ChangeState(GameState.FlipDeckCard);
    }

    /** 공통: 덱에서 화투를 1장 뽑은 후 처리 루틴 **/
    private IEnumerator FlipDeckCardRoutine()
    {
        Debug.Log("FlipDeckCard 시작");

        // 덱에서 1장 드로우
        lastDeckCard = CardDealer.deck.Draw();
        // 덱이 텅 비었다면 바로 판정 시작
        if (lastDeckCard == null)
        {
            ChangeState(GameState.ResolveMatch);
            yield break;
        }

        // 잠시 HandCards로 설정
        lastDeckCard.SetSortingOrder("HandCards", 100);

        // 카드의 출발 위치를 덱 앵커로 세팅
        lastDeckCard.transform.position = CardDealer.deckAnchor.position;
        lastDeckCard.transform.rotation = CardDealer.deckAnchor.rotation;

        // 카드를 화면 중앙으로 살짝 띄우면서 앞면으로 뒤집음
        Vector3 flipPos = CardDealer.deckAnchor.position + new Vector3(1.5f, 0.5f, 0);
        lastDeckCard.transform.DOMove(flipPos, 0.3f).SetEase(Ease.OutBack);
        lastDeckCard.Flip(true);

        // 대기
        yield return new WaitForSeconds(0.5f);

        // 바닥패 목적지 탐색
        int orderInLayer;
        Vector3 targetPos = CardDealer.CalculateTablePosition(lastDeckCard, out orderInLayer);

        // 애니메이션 재생
        yield return StartCoroutine(AnimationManager.Instance.PlayDropCardToTable(lastDeckCard, targetPos, orderInLayer, false));


        ChangeState(GameState.ResolveMatch);
    }

    ///** 공통: 판정 루틴 (쪽, 따닥, 뻑 등) **/
    //private IEnumerator ResolveMatchRoutine()
    //{
    //    ChangeState(GameState.CheckScore);
    //}
}
