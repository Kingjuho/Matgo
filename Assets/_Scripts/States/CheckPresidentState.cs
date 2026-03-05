using System.Collections;
using UnityEngine;

public class CheckPresidentState : GameStateBase
{
    public CheckPresidentState(GameManager manager) : base(manager) { }

    public override IEnumerator Execute()
    {
        // TODO: 선을 잡은 유저의 턴으로 넘겨줘야 함
        GameManager.ChangeState(GameManager.StatePlayerTurn);
        yield break;
    }
}
