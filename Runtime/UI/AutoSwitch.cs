using System;
using UnityEngine;

public class AutoSwitch : MonoBehaviour
{
    [SerializeField] private BOButton btn;
    [SerializeField] private Animator animator;
    
    private Action<bool> _action;
    private Func<bool> _currentState;
    public Func<bool> CurrentState => _currentState;
    
    void OnEnable()
    {
        if (_currentState != null)
            Switch(_currentState());
    }
    
    void Switch(bool on)
    {
        animator.SetBool("auto", on);
    }

    public void ChangeAutoState(bool on)
    {
        _action?.Invoke(on);
        Switch(on);
    }
    
    public void Initialize(Func<bool> state, Action<bool> action)
    {
        _action = action;
        _currentState = state;
        btn.onClick.AddListener(() =>
        {
            var changedState = !this._currentState();
            ChangeAutoState(changedState);
        });
        
        Switch(this._currentState());
    }
}

