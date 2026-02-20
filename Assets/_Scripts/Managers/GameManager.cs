using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 싱글톤
    public static GameManager Instance { get; private set; }

    // 컴포넌트
    [SerializeField] CardDealer _cardDealer;    
    
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
        yield return StartCoroutine(_cardDealer.DistributeCardsSequence());

        ChangeState(GameState.CheckPresident);
    }

    /** 플레이어 턴 루틴 **/
    private IEnumerator PlayerTurnRoutine()
    {
        Debug.Log("플레이어의 턴! 카드를 선택하세요.");

        // TODO: 플레이어가 카드를 클릭하거나 QWER 키를 누를 때까지 무한 대기
        // (입력이 들어오면 ChangeState(GameState.PlayHandCard) 호출)
        yield return null;
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
