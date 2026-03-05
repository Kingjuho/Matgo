public enum GameState
{
    Init,           // 초기화 및 패 분배
    CheckPresident, // 총통 체크

    PlayerTurn,     // 플레이어 입력 대기
    AITurn,         // AI 판단 대기

    /** AI, Player 공통 로직 **/
    PlayHandCard,   // 1. 손패를 바닥에 냄 (폭탄 체크 포함)
    FlipDeckCard,   // 2. 중앙 덱에서 한 장 까서 바닥에 냄
    ResolveMatch,   // 3. 바닥패 판정 (뻑, 쪽, 따닥, 피뺏기 등)
    CheckScore,     // 4. 점수 계산 및 고/스톱 판단

    TurnEnd,        // 턴 교체
    GameOver        // 게임 종료 (승패 결과)
}