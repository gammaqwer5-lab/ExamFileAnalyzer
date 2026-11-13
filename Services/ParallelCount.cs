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
        public static async Task ProcessAsync(string[] paths, int workerCount)
        {
            var channel = Channel.CreateBounded<string>(10);
            Task[] workers = Enumerable
           .Range(1, workerCount)
            .Select(async workerId =>
            {
                await foreach (var task in channel.Reader.ReadAllAsync())
                {
                    await WorkerAsync(workerId, task);
                }
            })
           .ToArray();
            foreach (var file in paths)
            {
                await channel.Writer.WriteAsync(file);
            }
            channel.Writer.TryComplete();
            await Task.WhenAll(workers);
        }


        static async Task WorkerAsync(int workerId,string file)
        {
            try
            {
                Console.WriteLine(
                    $"[WORKER {workerId}] Начал: {Path.GetFileName(file)}");

                var result = await AnalyzeAsync(
                    workerId,
                    file);

                Console.WriteLine(
                    $"[WORKER {workerId}] Завершил: {Path.GetFileName(file)} Слов: {result.Words} Линий: {result.Lines} Символов: {result.Characters}");
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
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
