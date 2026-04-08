using System.Collections.Generic;

namespace Practice2
{
    public static class TowerOfHanoi
    {
        public record Move(int FromPivot, int ToPivot)
        {
            public override string ToString()
            {
                return (char) ('A' + FromPivot) + " --> " + (char) ('A' + ToPivot);
            }
        }

        private static void RecursiveSolve(List<Move> solution, int n, int fromPivot, int toPivot)
        {
            if (n == 1)
            {
                solution.Add(new(fromPivot, toPivot));
                return;
            }
            int midPivot = 3 - (fromPivot + toPivot);
            RecursiveSolve(solution, n - 1, fromPivot, midPivot);
            solution.Add(new(fromPivot, toPivot));
            RecursiveSolve(solution, n - 1, midPivot, toPivot);
        }

        public static List<Move> RecursiveSolve(int initialDisks)
        {
            if (initialDisks < 1)
            {
                throw new ArgumentException("invalid initial disk count");
            }
            List<Move> solution = [];
            RecursiveSolve(solution, initialDisks, 0, 2);
            return solution;
        }

        private class StackInstance
        {
            public required int N { get; set; }
            public required int FromPivot { get; set; }
            public required int ToPivot { get; set; }
            public int MidPivot => 3 - (FromPivot + ToPivot);
            public int RecursiveCall { get; set; }

            public Move Move => new(FromPivot, ToPivot);
        }

        public static List<Move> LoopSolve(int initialDisks)
        {
            List<Move> solution = [];
            Stack<StackInstance> stack = [];
            stack.Push(new()
            {
                N = initialDisks,
                FromPivot = 0,
                ToPivot = 2
            });
            while (stack.Count > 0)
            {
                var instance = stack.Peek();
                if (instance.N == 1)
                {
                    solution.Add(instance.Move);
                    stack.Pop();
                    continue;
                }
                if (instance.RecursiveCall == 0)
                {
                    stack.Push(new()
                    {
                        N = instance.N - 1,
                        FromPivot = instance.FromPivot,
                        ToPivot = instance.MidPivot
                    });
                    instance.RecursiveCall++;
                }
                else if (instance.RecursiveCall == 1)
                {
                    solution.Add(instance.Move);
                    stack.Push(new()
                    {
                        N = instance.N - 1,
                        FromPivot = instance.MidPivot,
                        ToPivot = instance.ToPivot
                    });
                    instance.RecursiveCall++;
                }
                else
                {
                    stack.Pop();
                }
            }
            return solution;
        }
    }
}
