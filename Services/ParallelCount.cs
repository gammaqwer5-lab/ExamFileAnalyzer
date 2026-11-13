using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FileAnalyzer.Services
{
    public class ParallelCount
    {
        public static async Task<(List<FileResult> Results, List<string> Errors)> ProcessAsync(string[] paths, int workerCount)
        {
            var results = new ConcurrentBag<FileResult>();
            var errors = new ConcurrentBag<string>();
            var channel = Channel.CreateBounded<string>(10);

            Task[] workers = Enumerable
            .Range(1, workerCount)
            .Select(async workerId =>
            {
                await foreach (var task in channel.Reader.ReadAllAsync())
                {
                    await WorkerAsync(workerId, task, results, errors);
                }
            })
           .ToArray();

            foreach (var file in paths)
            {
                await channel.Writer.WriteAsync(file);
            }

            channel.Writer.TryComplete();
            await Task.WhenAll(workers);

            return (results.ToList(), errors.ToList());
        }


        static async Task WorkerAsync(int workerId,string file, ConcurrentBag<FileResult> results, ConcurrentBag<string> errors)
        {
            try
            {
                Console.WriteLine(
                    $"[WORKER {workerId}] Начал: {Path.GetFileName(file)}");

                var result = await AnalyzeAsync(
                    workerId,
                    file);

                results.Add(new FileResult(Path.GetFileName(file), result.Lines, result.Words, result.Characters));

                Console.WriteLine(
                    $"[WORKER {workerId}] Завершил: {Path.GetFileName(file)} Слов: {result.Words} Линий: {result.Lines} Символов: {result.Characters}");
            }
            catch (IOException ex)
            {
                errors.Add($"{Path.GetFileName(file)}: {ex.Message}");
                Console.WriteLine(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                errors.Add($"{Path.GetFileName(file)}: {ex.Message}");
                Console.WriteLine(ex.Message);
            }
        }
        public static async Task<(int Lines, int Words, long Characters)> AnalyzeAsync(int workerId, string file)
        {
            string content = await File.ReadAllTextAsync(file);
            return TextCounter.Count(content);
        }
    }
}
