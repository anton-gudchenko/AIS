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

        bool IsCorrect();

        bool IsSolved();

        IList<IProblemState<T>> GetPossibleStates();
    }

    public interface ISolutionTracker<T>
    {
        void StatePulled(IProblemState<T> state);

        void StateDiscarded(IProblemState<T> state);

        void StateSolved(IProblemState<T> state);
    }

    public abstract class AbstractSolutionTracker<T> : ISolutionTracker<T>
    {
        public virtual void StatePulled(IProblemState<T> state)
        {
            
        }

        public virtual void StateDiscarded(IProblemState<T> state)
        {
            
        }

        public virtual void StateSolved(IProblemState<T> state)
        {
            
        }
    }

    public static class ProblemSolver
    {
        public static List<T> Solve<T>(IProblemState<T> initialState, ISolutionTracker<T>? tracker = null)
        {
            Stack<IProblemState<T>> stack = new();
            stack.Push(initialState);

            IProblemState<T>? result = null;

            while (stack.Count > 0)
            {
                IProblemState<T> state = stack.Pop();
                tracker?.StatePulled(state);
                if (!state.IsCorrect())
                {
                    tracker?.StateDiscarded(state);
                    continue;
                }
                if (state.IsSolved())
                {
                    result = state;
                    tracker?.StateSolved(state);
                    break;
                }
                foreach (var nextState in state.GetPossibleStates().Reverse())
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
