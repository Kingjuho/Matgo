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
    public HumanPlayer humanPlayer;         // 유저
    public ComputerPlayer computerPlayer;   // AI
    public Player currentPlayer;            // 현재 턴을 진행한 플레이어

    [Header("게임 판정용 데이터")]
    public Dictionary<CardMonth, Player> bbuckRecords = new Dictionary<CardMonth, Player>();

    [Header("상태 머신")]
    public GameStateBase CurrentState { get; private set; }
    public GameStateBase StateInit { get; private set; }
    public GameStateBase StateCheckPresident { get; private set; }
    public GameStateBase StatePlayerTurn { get; private set; }
    public GameStateBase StateAITurn { get; private set; }
    public GameStateBase StatePlayHandCard { get; private set; }
    public GameStateBase StateFlipDeckCard { get; private set; }
    public GameStateBase StateResolveMatch { get; private set; }
    public GameStateBase StateCheckScore { get; private set; }
    public GameStateBase StateTurnEnd { get; private set; }
    public GameStateBase StateGameOver { get; private set; }

    // 해당 턴 판정용 데이터
    public Card lastPlayerCard;             // 이번 턴에 손에서 낸 카드
    public Card lastDeckCard;               // 이번 턴에 덱에서 깐 카드
    public bool isBombThisTurn = false;     // 이번 턴에 폭탄 터트렸는지 여부
    public bool isShakeThisTurn = false;    // 이번 턴에 흔들었는지 여부

    // 선택 대기 상태
    public bool isChoosingCard = false;     // 선택 모드 활성화 여부
    public Card selectedChoiceCard = null;  // 최종 선택 카드

    private void Awake()
    {
        // 인스턴스 유일성 보장
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 상태 클래스 메모리에 저장
        StateInit = new InitState(this);
        StateCheckPresident = new CheckPresidentState(this);
        StatePlayerTurn = new PlayerTurnState(this);
        StateAITurn = new AITurnState(this);
        StatePlayHandCard = new PlayHandCardState(this);
        StateFlipDeckCard = new FlipDeckCardState(this);
        StateResolveMatch = new ResolveMatchState(this);
        StateCheckScore = new CheckScoreState(this);
        StateTurnEnd = new TurnEndState(this);
        StateGameOver = new GameOverState(this);
    }

    private void Start()
    {
        ChangeState(StateInit);
    }

    /** FSM **/
    public void ChangeState(GameStateBase newState)
    {
        CurrentState?.Exit();

        CurrentState = newState;
        StartCoroutine(StateRoutine());
    }

    /** 상태 실행 루틴 **/
    private IEnumerator StateRoutine()
    {
        yield return StartCoroutine(CurrentState.Enter());
        yield return StartCoroutine(CurrentState.Execute());
    }
}
