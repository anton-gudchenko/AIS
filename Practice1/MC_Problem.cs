using System.Text;

namespace Practice1
{
    public record RiverCoast(int Missionaries, int Cannibals)
    {
        public override string ToString()
        {
            return $"Миссионеры({Missionaries}), Людоеды({Cannibals})";
        }
    }

    public record RiverTransition(int FromCoast, int Missioners, int Cannibals)
    {
        public override string ToString()
        {
            if (Missioners == Cannibals && Cannibals == 0)
            {
                if (FromCoast == 1)
                {
                    return ">------>";
                }
                else
                {
                    return "<------<";
                }
            }
            if (Missioners == Cannibals)
            {
                if (FromCoast == 1)
                {
                    return ">>>МЛ>>>";
                }
                else
                {
                    return "<<<МЛ<<<";
                }
            }
            if (FromCoast == 1)
            {
                return ">>>" + (Missioners > Cannibals ? Missioners + "М" : Cannibals + "Л") + ">>>";
            }
            else
            {
                return "<<<" + (Missioners > Cannibals ? Missioners + "М" : Cannibals + "Л") + "<<<";
            }
        }
    }

    public class MC_ProblemState : IProblemState<RiverTransition>
    {
        public RiverCoast Coast1 { get; set; }
        public RiverCoast Coast2 { get; set; }
        public int BoastCoast { get; set; }
        public IProblemState<RiverTransition>? PreviousState { get; set; }
        public RiverTransition? Operation { get; set; }

        public MC_ProblemState()
        {
            Coast1 ??= new(0, 0);
            Coast2 ??= new(0, 0);
        }

        public MC_ProblemState(RiverCoast coast1, RiverCoast coast2)
        {
            Coast1 = coast1;
            Coast2 = coast2;
            BoastCoast = 1;
        }

        public static MC_ProblemState Initialize(int missionaries, int cannibals)
        {
            return new(new(missionaries, cannibals), new(0, 0));
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Coast1, Coast2);
        }

        public override bool Equals(object? obj)
        {
            if (this == obj) return true;
            if (obj is not MC_ProblemState state) return false;
            return Coast1.Equals(state.Coast1) && Coast2.Equals(state.Coast2) && BoastCoast == state.BoastCoast;
        }

        public override string ToString()
        {
            return $"{Coast1} {(Operation != null ? Operation : "--------")} {Coast2}";
        }
    }

    public class MC_StateResolver : IStateResolver<MC_ProblemState, RiverTransition>
    {
        private readonly List<MC_ProblemState> resolvedStates = [];

        public MC_StateResolver(MC_ProblemState initialState)
        {
            resolvedStates.Add(initialState);
        }

        public bool IsCorrect(MC_ProblemState state)
        {
            return state.BoastCoast == 1 ? state.Coast2.Missionaries == 0 || state.Coast2.Missionaries >= state.Coast2.Cannibals : state.Coast1.Missionaries == 0 || state.Coast1.Missionaries >= state.Coast1.Cannibals;
        }

        public bool IsSolved(MC_ProblemState state)
        {
            return state.Coast1.Missionaries == 0 && state.Coast1.Cannibals == 0;
        }

        public IEnumerable<MC_ProblemState> ResolveNextStates(MC_ProblemState state)
        {
            List<MC_ProblemState> nextStates = [];
            if (state.BoastCoast == 1)
            {
                if (state.Coast1.Missionaries > 1)
                    Transfer(state, 2, 2, 0, nextStates);
                if (state.Coast1.Missionaries > 0)
                    Transfer(state, 2, 1, 0, nextStates);
                if (state.Coast1.Cannibals > 1)
                    Transfer(state, 2, 0, 2, nextStates);
                if (state.Coast1.Cannibals > 0)
                    Transfer(state, 2, 0, 1, nextStates);
                if (state.Coast1.Missionaries > 0 && state.Coast1.Cannibals > 0)
                    Transfer(state, 2, 1, 1, nextStates);
            }
            else
            {
                if (state.Coast2.Missionaries > 1)
                    Transfer(state, 1, 2, 0, nextStates);
                if (state.Coast2.Missionaries > 0)
                    Transfer(state, 1, 1, 0, nextStates);
                if (state.Coast2.Cannibals > 1)
                    Transfer(state, 1, 0, 2, nextStates);
                if (state.Coast2.Cannibals > 0)
                    Transfer(state, 1, 0, 1, nextStates);
                if (state.Coast2.Missionaries > 0 && state.Coast2.Cannibals > 0)
                    Transfer(state, 1, 1, 1, nextStates);
            }
            return nextStates.Shuffle();
        }

        private void Transfer(MC_ProblemState state, int destinationCoast, int missioners, int cannibals, List<MC_ProblemState> nextStates)
        {
            MC_ProblemState newState = new()
            {
                BoastCoast = destinationCoast,
                Operation = new(state.BoastCoast, missioners, cannibals),
                PreviousState = state
            };

            int coast1Sign = Math.Sign(state.BoastCoast - destinationCoast), coast2Sign = -coast1Sign;

            newState.Coast1 = new(state.Coast1.Missionaries + missioners * coast1Sign, state.Coast1.Cannibals + cannibals * coast1Sign);
            newState.Coast2 = new(state.Coast2.Missionaries + missioners * coast2Sign, state.Coast2.Cannibals + cannibals * coast2Sign);

            if (!resolvedStates.Contains(newState))
            {
                nextStates.Add(newState);
                resolvedStates.Add(newState);
            }
        }
    }
}
