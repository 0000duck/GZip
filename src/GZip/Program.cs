using System;
using Common.Abstractions.Services;
using Logic.Services;

namespace GZip
{
    class Program
    {
        static void Main()
        {
            Run(new GZipService());
        }

        private static void Run(ICompressionService compressionService)
        {
            var run = true; 
            while (run)
            {
                Console.WriteLine("Выберите действие: 1 - сжатие, 2 - распаковка, 3 - выход");
                switch (Console.ReadLine())
                {
                    case "1":
                        Compress(compressionService);
                        break;
                    case "2":
                        Decompress(compressionService);
                        break;
                    case "3":
                        run = false;
                        break;
                    default:
                        Console.WriteLine("Введено некорректное значение");
                        break;
                }
            }
        }

        private static void Compress(ICompressionService compressionService)
        {
            try
            {
                Console.WriteLine("Укажите путь к исходному файлу, включая полное имя файла и расширение");
                var pathOfSourceFile = Console.ReadLine();
                Console.WriteLine("Укажите путь назначения");
                var pathOfDestenationFile = Console.ReadLine();
                Console.WriteLine("Сжатие...");
                var startTime = System.Diagnostics.Stopwatch.StartNew();
                compressionService.Compression(pathOfSourceFile, pathOfDestenationFile);
                var resultTime = startTime.Elapsed;
                Console.WriteLine($"Файл успешно сжат. Время сжатия: \"{resultTime.Hours:00}:{resultTime.Minutes:00}:{resultTime.Seconds:00}.{resultTime.Milliseconds:000}\"");
                Console.WriteLine("Для продолжения нажмите Enter");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void Decompress(ICompressionService compressionService)
        {
            try
            {
                Console.WriteLine("Укажите путь к файлу gz.info архива");
                var pathOfDestenationFileDecompression = Console.ReadLine();
                Console.WriteLine("Распаковка...");
                compressionService.Decompression(pathOfDestenationFileDecompression);
                Console.WriteLine("Файл успешно распакован");
                Console.WriteLine("Для продолжения нажмите Enter");
                Console.Read();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
