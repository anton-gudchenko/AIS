namespace Practice1
{
    public class RiverCoast : ICloneable
    {
        public int Missionaries { get; set; }
        public int Cannibals { get; set; }

        public object Clone()
        {
            return new RiverCoast
            {
                Missionaries = this.Missionaries,
                Cannibals = this.Cannibals
            };
        }
    }

    public record RiverTransition(int FromCoast, int Missioners, int Cannibals)
    {
    }

    public class MC_ProblemState : IProblemState<RiverTransition>
    {
        public RiverCoast Coast1 { get; private set; }
        public RiverCoast Coast2 { get; private set; }
        public int BoastCoast { get; private set; }
        public IProblemState<RiverTransition>? PreviousState { get; set; }
        public RiverTransition? Operation { get; set; }

        private MC_ProblemState()
        {
            Coast1 ??= new();
            Coast2 ??= new();
        }

        public MC_ProblemState(RiverCoast coast1, RiverCoast coast2)
        {
            Coast1 = coast1;
            Coast2 = coast2;
            BoastCoast = 1;
        }

        public static MC_ProblemState Initialize(int missionaries, int cannibals)
        {
            return new(new()
            {
                Missionaries = missionaries,
                Cannibals = cannibals
            }, new());
        }

        public bool IsCorrect()
        {
            return BoastCoast == 1 ? Coast2.Missionaries >= Coast2.Cannibals : Coast1.Missionaries >= Coast1.Cannibals;
        }

        public bool IsSolved()
        {
            return Coast1.Missionaries == 0 && Coast1.Cannibals == 0;
        }

        public IList<IProblemState<RiverTransition>> GetPossibleStates()
        {
            List<MC_ProblemState> nextStates = [];
            if (BoastCoast == 1)
            {
                if (Coast1.Missionaries > 0)
                    nextStates.Add(new()
                    {
                        Coast1 = new()
                        {
                            Missionaries = this.Coast1.Missionaries - 1,
                            Cannibals = this.Coast1.Cannibals
                        },
                        Coast2 = new()
                        {
                            Missionaries = this.Coast2.Missionaries + 1,
                            Cannibals = this.Coast2.Cannibals
                        },
                        BoastCoast = 2,
                        Operation = new(BoastCoast, 1, 0),
                        PreviousState = this
                    });
                else
                    nextStates.Add(new()
                    {
                        Coast1 = new()
                        {
                            Missionaries = this.Coast1.Missionaries,
                            Cannibals = this.Coast1.Cannibals - 1
                        },
                        Coast2 = new()
                        {
                            Missionaries = this.Coast2.Missionaries,
                            Cannibals = this.Coast2.Cannibals + 1
                        },
                        BoastCoast = 2,
                        Operation = new(BoastCoast, 0, 1),
                        PreviousState = this
                    });
            }
            else
            {
                if (Coast2.Missionaries > 0)
                    nextStates.Add(new()
                    {
                        Coast1 = new()
                        {
                            Missionaries = this.Coast1.Missionaries + 1,
                            Cannibals = this.Coast1.Cannibals
                        },
                        Coast2 = new()
                        {
                            Missionaries = this.Coast2.Missionaries - 1,
                            Cannibals = this.Coast2.Cannibals
                        },
                        BoastCoast = 1,
                        Operation = new(BoastCoast, 1, 0),
                        PreviousState = this
                    });
                if (Coast2.Cannibals > 0)
                    nextStates.Add(new()
                    {
                        Coast1 = new()
                        {
                            Missionaries = this.Coast1.Missionaries,
                            Cannibals = this.Coast1.Cannibals + 1
                        },
                        Coast2 = new()
                        {
                            Missionaries = this.Coast2.Missionaries,
                            Cannibals = this.Coast2.Cannibals - 1
                        },
                        BoastCoast = 1,
                        Operation = new(BoastCoast, 0, 1),
                        PreviousState = this
                    });
            }
            return (IList<IProblemState<RiverTransition>>)nextStates;
        }
    }
}
