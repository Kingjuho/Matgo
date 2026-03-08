using System;
using System.Collections;
using UnityEngine;

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

    /** 팝업 입력 대기 공통 헬퍼 **/
    protected IEnumerator WaitForPopupResult<T>
    (
        Func<Action<T>, bool> tryShowPopup,
        Action<T> onResolved,
        T fallbackValue
    )
    {
        bool isResolved = false;
        T result = fallbackValue;

        bool didOpen = tryShowPopup != null && tryShowPopup(value =>
        {
            result = value;
            isResolved = true;
        });

        if (!didOpen)
        {
            onResolved?.Invoke(fallbackValue);
            yield break;
        }

        yield return new WaitUntil(() => isResolved);
        onResolved?.Invoke(result);
    }
}
