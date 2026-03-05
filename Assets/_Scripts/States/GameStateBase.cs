using System.Collections;

public abstract class GameStateBase
{
    protected GameManager GameManager;

    public GameStateBase(GameManager manager) => GameManager = manager;

    /** 상태 진입 **/
    public virtual IEnumerator Enter() { yield break; }
    /** 상태 실행 **/
    public virtual IEnumerator Execute() { yield break; }
    /** 상태 종료 **/
    public virtual void Exit() { }

}
