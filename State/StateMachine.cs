
namespace FiniteStateMachine
{
    public class StateMachine<T>
    {
        private State<T> currentState;
        private State<T> beforeState;

        public void Execute()
        {
            if (currentState == null)
                return;

            if (!currentState.IsDone())
                currentState.Update(this);
        }
        public void SetDefaultState(State<T> newState)
        {
            beforeState = null;
            currentState = newState;
            currentState.Start();
        }
        public void ChangeState(State<T> newState)
        {
            currentState.End();
            newState.Start();
            beforeState = currentState;
            currentState = newState;
        }

        public State<T> GetBeforeState()
        {
            return beforeState;
        }
        public State<T> GetCurrentState()
        {
            return currentState;
        }
    }
}