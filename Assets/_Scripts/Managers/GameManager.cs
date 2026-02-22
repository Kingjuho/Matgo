using System.Collections;
using System.Collections.Generic;
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

    public GameState currentState;          // 현재 상태
    private bool _isPlayerTurn = true;      // 플레이어 턴 여부

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
                break;
            case GameState.FlipDeckCard:
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

        Debug.Log($"턴 종료: {humanPlayer.selectedCard.Month}월 카드가 제출 대기 중입니다.");
        ChangeState(GameState.PlayHandCard);
    }

    ///** 공통: 손패 처리 루틴 **/
    //private IEnumerator PlayHandCardRoutine()
    //{
    //    ChangeState(GameState.FlipDeckCard);
    //}

    ///** 공통: 덱에서 화투를 1장 뽑은 후 처리 루틴 **/
    //private IEnumerator FlipDeckCardRoutine()
    //{
    //    ChangeState(GameState.ResolveMatch);
    //}

    ///** 공통: 판정 루틴 (쪽, 따닥, 뻑 등) **/
    //private IEnumerator ResolveMatchRoutine()
    //{
    //    ChangeState(GameState.CheckScore);
    //}
}
