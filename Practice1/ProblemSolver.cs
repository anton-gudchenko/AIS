using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls.Primitives;

namespace Practice1
{
    public interface IProblemState<T>
    {
        T? Operation { get; set; }

        IProblemState<T>? PreviousState { get; set; }
    }

    public interface IStateResolver<State, T> where State : IProblemState<T>
    {
        bool IsCorrect(State state);

        bool IsSolved(State state);

        IEnumerable<State> ResolveNextStates(State state);
    }

    public interface ISolutionTracker<State, T> where State : IProblemState<T>
    {
        void StatePulled(State state);

        void StateDiscarded(State state);

        void StateSolved(State state);
    }

    public abstract class AbstractSolutionTracker<State, T> : ISolutionTracker<State, T> where State : IProblemState<T>
    {
        public virtual void StatePulled(State state)
        {
            
        }

        public virtual void StateDiscarded(State state)
        {
            
        }

        public virtual void StateSolved(State state)
        {
            
        }
    }

    public static class ProblemSolver
    {
        public static List<T> Solve<State, T>(State initialState, Func<IStateResolver<State, T>> resolverFactory, ISolutionTracker<State, T>? tracker = null) where State : IProblemState<T>
        {
            Stack<State> stack = new();
            stack.Push(initialState);

            var resolver = resolverFactory();
            IProblemState<T>? result = null;

            while (stack.Count > 0)
            {
                State state = stack.Pop();
                tracker?.StatePulled(state);
                if (!resolver.IsCorrect(state))
                {
                    tracker?.StateDiscarded(state);
                    continue;
                }
                if (resolver.IsSolved(state))
                {
                    result = state;
                    tracker?.StateSolved(state);
                    break;
                }
                foreach (var nextState in resolver.ResolveNextStates(state).Reverse())
                {
                    stack.Push(nextState);
                }
            }

            List<T> solution = [];
            while (result != null && result.PreviousState != null)
            {
                solution.Add(result.Operation!);
                result = result.PreviousState;
            }
            solution.Reverse();
            return solution;
        }
    }
}
