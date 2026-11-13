using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Channels;
using System.Text.Json;
using FileAnalyzer.Services;
using System.Threading.Tasks;

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

        foreach (string file in paths)
        {
            string content = await File.ReadAllTextAsync(file);

            var countedFile = TextCounter.Count(content);
            Console.WriteLine($"{Path.GetFileName(file)}: Слов: {countedFile.Words} Линий: {countedFile.Lines} Символов: {countedFile.Characters}");

        }
        // TODO 3: Реализовать параллельный анализ файлов.

        ParallelCount.ProcessAsync(paths,3);
        // TODO 4: Ограничить число одновременно обрабатываемых файлов.
        // TODO 5: Сравнить последовательный и параллельный режимы с Stopwatch.
        // TODO 6: Обрабатывать ошибки отдельных файлов, не прекращая весь отчёт.
        // TODO 7: Сохранять итоговый отчёт через корректно закрываемый поток.

        // TODO 8: Построить pipeline через bounded Channel и фиксированных worker-ов.
        // TODO 9: Добавить JSON-отчёт.
        // TODO 10: Сохранять параметры анализа в HKCU.

        await Task.CompletedTask;
    }
}
