/** 1~12월 열거형 **/
public enum CardMonth
{
    Jan = 1, // 송학
    Feb = 2, // 매조
    Mar = 3, // 벚꽃
    Apr = 4, // 흑싸리
    May = 5, // 난초
    Jun = 6, // 모란
    Jul = 7, // 홍싸리
    Aug = 8, // 공산
    Sep = 9, // 국화
    Oct = 10, // 단풍
    Nov = 11, // 오동
    Dec = 12  // 비
}

/** 카드 타입 열거형 **/
public enum CardType
{
    Pee,        // 피
    Ssangpee,    // 쌍피
    Ddee,       // 띠
    Yeolggeut,  // 열끗
    Gwang       // 광
}

/** 특수 타입 열거형 **/
public enum SpecialFeature
{
    None,
    Godori,     // 고도리
    HongDan,    // 홍단
    ChungDan,   // 청단
    ChoDan,     // 초단
    Bee         // 비
}

/** 카드 힌트 열거형 **/
public enum HintType
{
    None,   // 없음
    Basic,  // 기본 (소지패 1~2장, 바닥패 1장, 이미 먹힌 패 0장)
    Good1,  // 굳은자 1 (소지패 1장, 바닥패 1장, 이미 먹힌 패 2장)
    Good2,  // 굳은자 2 (소지패 2장, 바닥패 0장, 이미 먹힌 패 2장)
    Shake,  // 흔들기
    Bomb    // 폭탄
}
