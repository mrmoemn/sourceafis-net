using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using SourceAFIS.Meta;
using SourceAFIS.Tuning;
using SourceAFIS.Tuning.Reports;

namespace DatabaseAnalyzer
{
    sealed class DatabaseAnalyzer
    {
        Options Options = new Options();
        TestDatabase TestDatabase = new TestDatabase();
        ExtractorBenchmark ExtractorBenchmark = new ExtractorBenchmark();
        MatcherBenchmark MatcherBenchmark = new MatcherBenchmark();
        Optimizer Optimizer = new Optimizer();

        public DatabaseAnalyzer()
        {
            Options.TestDatabase = TestDatabase;
            Options.ExtractorBenchmark = ExtractorBenchmark;
            ExtractorBenchmark.Database = TestDatabase;
            MatcherBenchmark.TestDatabase = TestDatabase;
            Optimizer.ExtractorBenchmark = ExtractorBenchmark;
            Optimizer.MatcherBenchmark = MatcherBenchmark;
        }

        void Run()
        {
            Options.Load("DatabaseAnalyzerConfiguration.xml");
            switch (Options.Action)
            {
                case "extractor-benchmark":
                    RunExtractorBenchmark();
                    break;
                case "matcher-benchmark":
                    RunMatcherBenchmark();
                    break;
                case "optimizer":
                    RunOptimizer();
                    break;
            }
        }

        void RunExtractorBenchmark()
        {
            Console.WriteLine("Running extractor benchmark");
            ExtractorReport report = ExtractorBenchmark.Run();
            Console.WriteLine("Saving extractor report");
            report.Save("Extractor");
            MatcherBenchmark.TestDatabase = TestDatabase = report.Templates;
        }

        void RunMatcherBenchmark()
        {
            string dbPath = Path.Combine("Extractor", "Templates.dat");
            if (File.Exists(dbPath))
                TestDatabase.Load(dbPath);
            else
                RunExtractorBenchmark();
            
            Console.WriteLine("Running matcher benchmark");
            MatcherReport report = MatcherBenchmark.Run();
            Console.WriteLine("Saving matcher report");
            report.Save("Matcher");
        }

        void RunOptimizer()
        {
            Optimizer.Mutations.OnMutation += delegate(ParameterValue initial, ParameterValue mutated)
            {
                Console.WriteLine("Mutated {0}, {1} -> {2}", initial.FieldPath, initial.Value.Double, mutated.Value.Double);
            };
            Optimizer.NicheSlot.OnChange += delegate()
            {
                Console.WriteLine("NicheSlot has improved");
                Optimizer.NicheSlot.Save("Optimizer");
            };
            Console.WriteLine("Running optimizer");
            Optimizer.Run();
        }

        static void Main(string[] args)
        {
            DatabaseAnalyzer instance = new DatabaseAnalyzer();
            instance.Run();
        }
    }
}
