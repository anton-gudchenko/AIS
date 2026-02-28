using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Practice1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

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

        public class ProblemState
        {
            public RiverCoast Coast1 { get; private set; }
            public RiverCoast Coast2 { get; private set; }
            public int BoastCoast { get; private set; }

            private ProblemState()
            {

            }

            public ProblemState(RiverCoast coast1, RiverCoast coast2)
            {
                Coast1 = coast1;
                Coast2 = coast2;
                BoastCoast = 1;
            }

            public static ProblemState Initialize(int missionaries, int cannibals)
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

            public void ApplyDeepSearch(Stack<ProblemState> stack)
            {
                if (BoastCoast == 1)
                {
                    if (Coast1.Missionaries > 0)
                        stack.Push(new()
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
                            BoastCoast = 2
                        });
                    if (Coast1.Cannibals > 0)
                        stack.Push(new()
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
                            BoastCoast = 2
                        });
                }
                else
                {
                    if (Coast2.Missionaries > 0)
                        stack.Push(new()
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
                            BoastCoast = 1
                        });
                    if (Coast2.Cannibals > 0)
                        stack.Push(new()
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
                            BoastCoast = 1
                        });
                }
            }
        }
    }
}
