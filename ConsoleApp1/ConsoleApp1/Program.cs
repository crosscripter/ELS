using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Hit
    {
        public string Term { get; set; }
        public int Skip { get; set; }
        public int Index { get; set; }
        // public string Text { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.Write("Usage: source term from to");
                return;
            }

            Console.InputEncoding = new UTF8Encoding(false);
            Console.OutputEncoding = new UTF8Encoding(false);

            var hits = new List<Hit>();
            //var padding = 5;            
            var source = File.ReadAllText(args[0]);
            var terms = args[1].Split(',');
            var fromSkip = int.Parse(args[2]);
            var toSkip = int.Parse(args[3]);
            var locker = new object();
            var totalSearches = (toSkip - fromSkip) * terms.Length;
            var searchesDone = 0;

            var timer = new Stopwatch();
            timer.Start();

            foreach (var term in terms)
            {
                Parallel.For(fromSkip, toSkip + 1, skip =>
                {
                    lock (locker) { searchesDone++; }
                    Console.Title = $"ELISE {searchesDone}/{totalSearches}";
                    var regex = string.Join(".{" + skip + "}", term.ToCharArray());
                    var hit = Regex.Match(source, regex, RegexOptions.Compiled);
                    if (!hit.Success) return;

                    do
                    {                        
                        //var start = hit.Index - padding;
                        //start = start >= 0 ? start : 0;
                        //var stop = hit.Length + padding * 2;
                        //stop = stop < source.Length ? stop : hit.Length;
                        //var context = source.Substring(start, stop);
                        //// var letters = hit.Value.ToCharArray().Select((c, i) => i % (skip + 1) == 0 ? $"[{c}]" : c.ToString());
                        //var text = hit.Value; //  context.Replace(hit.Value, string.Join("", letters));
                        lock (locker)
                            hits.Add(new Hit { Term = term, Skip = skip, Index = hit.Index });//, Text = text });

                        hit = hit.NextMatch();
                    } while (hit.Success);
                });
            }

            timer.Stop();
            Console.Title = $"ELISE {searchesDone} searches, {hits.Count} hit(s) in {timer.Elapsed}";

            Console.WriteLine(string.Join("\n", hits.OrderBy(h => h.Index)
                .ThenBy(h => h.Skip)
                .Select(h => $"{h.Term}\t{h.Skip}\t{h.Index}")));
        }
    }
}
