using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FileAnalyzer.Services
{
    public class Report
    {
        public static void SaveTxt(List<FileResult> results, List<string> errors, Stopwatch sw, Stopwatch swPar, float acc)
        {
            string path = Path.Combine(AppContext.BaseDirectory, "report.txt");
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream))
            {
                writer.WriteLine($"Дата: {DateTime.Now}");
                foreach (var r in results)
                    writer.WriteLine($"{r.FileName}: Слов: {r.Words} Линий: {r.Lines} Символов: {r.Characters}");
                writer.WriteLine($"Последовательно: {sw.ElapsedMilliseconds} ms");
                writer.WriteLine($"Параллельно: {swPar.ElapsedMilliseconds} ms");
                writer.WriteLine($"Параллельный в {acc} быстрее");

                if (errors.Count != 0)
                {
                    writer.WriteLine("\nОшибки: ");
                    foreach (string error in errors)
                    { 
                        writer.WriteLine(error);
                    }
                }
            }
            Console.WriteLine($"Отчёт сохранён: {path}");
        }
        public static void SaveJson(List<FileResult> results, List<string> errors, Stopwatch sw, Stopwatch swPar, float acc)
        {
            string path = Path.Combine(AppContext.BaseDirectory, "report.json");
            var report = new
            {
                CreatedAt = DateTime.Now,
                Files = results,
                FileCount = results.Count,
                SequentialMs = sw.ElapsedMilliseconds,
                ParallelMs = swPar.ElapsedMilliseconds,
                Speedup = acc,
            };
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            File.WriteAllText(path, JsonSerializer.Serialize(report, options));
            Console.WriteLine($"Json сохранён: {path}");
        }
    }
}
