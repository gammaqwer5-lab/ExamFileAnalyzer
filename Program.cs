using FileAnalyzer.Services;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Variant02;

internal static class Program
{
    private static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("Зачётный проект 02: Параллельный анализатор файлов");
        Console.WriteLine(new string('=', 64));

        // Стартовый сценарий нужен для проверки структуры проекта.
        // Реализацию индивидуальных требований выполняйте в ProjectService.
        var service = new ProjectService();
        await service.RunAsync();

        Console.WriteLine();
        Console.WriteLine("Нажмите Enter для завершения.");
        Console.ReadLine();
    }
}

public sealed class ProjectService
{
    public async Task RunAsync()
    {
        // TODO: перенесите предметную логику в отдельные методы и классы.
        // TODO: не размещайте весь проект в Main.
        // TODO 1: Найти текстовые файлы в каталоге.
        string dataDirectory = Path.Combine(
            AppContext.BaseDirectory,
            "Data");

        Directory.CreateDirectory(dataDirectory);

        string[] paths = Directory.GetFiles(
            dataDirectory,
            "*.txt");

        if (paths.Length == 0)
        {
            Console.WriteLine("Нет файлов для обработки");
            return;
        }

        // TODO 2: Последовательно подсчитать строки, слова и символы.
        var results = new List<FileResult>();
        var errors = new List<string>();

        var sw = Stopwatch.StartNew();
        foreach (string file in paths)
        {
            try
            {
                string content = await File.ReadAllTextAsync(file);
                var countedFile = TextCounter.Count(content);

                results.Add(new FileResult(Path.GetFileName(file), countedFile.Lines, countedFile.Words, countedFile.Characters));
                Console.WriteLine($"{Path.GetFileName(file)}: Слов: {countedFile.Words} Линий: {countedFile.Lines} Символов: {countedFile.Characters}");
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
        sw.Stop();

        Console.WriteLine($"\nПоследовательно: {sw.ElapsedMilliseconds} ms");

        // TODO 3: Реализовать параллельный анализ файлов.
        var swPar = Stopwatch.StartNew();
        var (resultsPar, errorsPar) = await ParallelCount.ProcessAsync(paths,3);
        swPar.Stop();

        Console.WriteLine($"\nПарралельно: {swPar.ElapsedMilliseconds} ms");
        // TODO 4: Ограничить число одновременно обрабатываемых файлов.
        // TODO 5: Сравнить последовательный и параллельный режимы с Stopwatch.

        float acc = (float)sw.ElapsedMilliseconds / (float)swPar.ElapsedMilliseconds;

        Console.WriteLine($"Параллельный в {acc} быстрее");

        // TODO 6: Обрабатывать ошибки отдельных файлов, не прекращая весь отчёт.
        // TODO 7: Сохранять итоговый отчёт через корректно закрываемый поток.

        var allErrors = errors.Concat(errorsPar).Distinct().ToList();

        Report.SaveTxt(results, allErrors, sw, swPar, acc);

        // TODO 8: Построить pipeline через bounded Channel и фиксированных worker-ов.
        // TODO 9: Добавить JSON-отчёт.
        // TODO 10: Сохранять параметры анализа в HKCU.

        await Task.CompletedTask;
    }
}
