using System.Collections.Generic;
using System;

namespace FiniteStateMachine
{
    public delegate bool TriggerEvent();
    public struct Transition<T>
    {
        public TriggerEvent Condition;
        public State<T> transitionState;
        public Action PostEvent;
        public Transition(TriggerEvent _condition, State<T> _state, Action _postEvent)
        {
            Condition = _condition;
            transitionState = _state;
            PostEvent = _postEvent;
        }
    }

    public abstract class State<T>
    {
        protected T owner;
        protected bool bDone;

        private Action PostEvent;
        private readonly List<Transition<T>> transitions = new List<Transition<T>>();

        public State(T _owner)
        {
            owner = _owner;
        }
        public void Start()
        {
            bDone = false;
            Enter();
        }
        public void Update(StateMachine<T> _stateMachine)
        {
            Execute();
            foreach (var transition in transitions)
            {
                if (transition.Condition())
                {
                    if (transition.transitionState != null)
                        _stateMachine.ChangeState(transition.transitionState);
                    if (transition.PostEvent != null)
                        transition.PostEvent();
                    break;
                }
            }
        }
        public void End()
        {
            if (!bDone)
                Exit();
        }
        public bool IsDone()
        {
            return bDone;
        }
        public void Done()
        {
            bDone = true;
            Exit();
            if (PostEvent != null)
                PostEvent();
        }
        public State<T> MakeTransition(TriggerEvent _condition, State<T> _state, Action _postTransitionEvent = null)
        {
            Transition<T> transition = new Transition<T>(_condition, _state, _postTransitionEvent);
            transitions.Add(transition);
            return this;
        }
        public void SetPostStateEvent(Action _postEvent)
        {
            PostEvent = _postEvent;
        }
        protected abstract void Enter();
        protected abstract void Execute();
        protected abstract void Exit();
    }
}

