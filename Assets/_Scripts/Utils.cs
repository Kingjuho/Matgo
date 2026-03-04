using System.Text;

public static class Utils
{
    /** 금액을 '조', '억', '만' 단위로 포맷팅하는 헬퍼 함수 **/
    public static string FormatMoney(long amount)
    {
        if (amount == 0) return "0원";

        long jo = amount / 1000000000000;
        long uk = (amount % 1000000000000) / 100000000;
        long man = (amount % 100000000) / 10000;
        long won = amount % 10000;

        StringBuilder sb = new StringBuilder();

        if (jo > 0) sb.AppendFormat("{0}조 ", jo);
        if (uk > 0) sb.AppendFormat("{0}억 ", uk);
        if (man > 0) sb.AppendFormat("{0}만 ", man);
        if (won > 0) sb.AppendFormat("{0}", won);

        return sb.ToString().TrimEnd() + "원";
    }

    /** 승리시 획득 금액을 계산하는 헬퍼 함수 **/
    public static long CalculateFinalMoney(int score, Player winner, Player loser, long baseStake = 500)
    {
        // 기본 배당(고 배당 포함)
        int finalMultiplier = winner.multiplier;

        // 흔들기, 폭탄 계산
        if (winner.bombCount > 0) finalMultiplier *= 2 * winner.bombCount;
        if (winner.shakeCount > 0) finalMultiplier *= 2 * winner.shakeCount;

        // ~박 계산
        if (loser.isGobak) finalMultiplier *= 2;
        if (loser.isPeebak) finalMultiplier *= 2;
        if (loser.isGwangbak) finalMultiplier *= 2;
        if (winner.isMeongbak) finalMultiplier *= 2;

        // TODO: 나가리, 미션 등 특수 배당 계산

        return score * baseStake * finalMultiplier;
    }
}
