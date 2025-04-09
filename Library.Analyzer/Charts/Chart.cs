using System.Collections.Generic;
using Library.Analyzer.Automata;
using System.IO;
using Library.Analyzer.Grammars;

namespace Library.Analyzer.Charts
{
    public class Chart : IChart
    {
        private List<EarleySet> _earleySets;
        
        public IReadOnlyList<IEarleySet> EarleySets { get { return _earleySets; } }

        public Chart()
        {
            _earleySets = new List<EarleySet>();
        }

        public bool Enqueue(int index, IState state)
        {
            IEarleySet earleySet = GetEarleySet(index);
            return earleySet.Enqueue(state);
        }

        public int Count
        {
            get { return EarleySets.Count; }
        }

        public bool Contains(int index, StateType stateType, IDottedRule dottedRule, int origin)
        {
            var earleySet = GetEarleySet(index);
            return earleySet.Contains(stateType, dottedRule, origin);
        }

        private EarleySet GetEarleySet(int index)
        {
            EarleySet earleySet;
            if (_earleySets.Count <= index)
            {
                earleySet = new EarleySet(index);
                _earleySets.Add(earleySet);

                //попробовать вывести тут earleysets
                //string filePath = "C:/Users/denst/OneDrive/Рабочий стол/earley_sets.txt";

                
                //using (StreamWriter writer = new StreamWriter(filePath, true)) // true для добавления в конец файла
                //{
                //     //writer.WriteLine("Set started");
                //    foreach (var setic in _earleySets)
                //    {
                //        //writer.WriteLine("Predictions: ");
                //        if (setic._predictions != null)
                //        {
                //            foreach (var item in setic._predictions)
                //            {
                //                //writer.WriteLine(item.ToString());
                //            }
                //        }


                //       // writer.WriteLine("Completions: ");
                //        if (setic._completions != null)
                //        {
                //            foreach (var item in setic._completions)
                //            {
                //                //writer.WriteLine(item.ToString());
                //            }
                //        }

                //        //writer.WriteLine("Transition: ");
                //        if (setic._transitions != null)
                //        {
                //            foreach (var item in setic._transitions)
                //            {
                //                //writer.WriteLine(item.ToString());
                //            }
                //        }

                //       // writer.WriteLine("Scans: ");
                //        if (setic._scans != null)
                //        {
                //            foreach (var item in setic._scans)
                //            {
                //               // writer.WriteLine(item.ToString());
                //            }
                //        }
                //    }
                //    //writer.WriteLine("Set ended");
                //}
                
                // Используйте StreamWriter для записи информации в файл
                
            }
            else
            {
                earleySet = _earleySets[index];
            }

            return earleySet;
        }
    }
}