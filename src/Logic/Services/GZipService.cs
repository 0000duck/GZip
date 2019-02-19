using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using Common.Abstractions.Services;
using Common.Helpers;
using Common.Structs;
using Newtonsoft.Json;

namespace Logic.Services
{
    public class GZipService : ICompressionService
    {
        private static readonly int ThreadNumber = Environment.ProcessorCount;
        private readonly Thread[] _threadPool = new Thread[ThreadNumber];
        private BlockOfArchive[] _blocksOfArchive;
        private readonly long _sizeOfBlock = 1024 * 1024; // 1mb
        private readonly string _dataDirectoryName = "data"; //дирректория хранения блоков архива
        private readonly string _gzipFileInfoName = "gzip.info"; //информационный файл, необходим для распаковки архива
        private string _nameOfFile(string path) => path.Split('\\').Last(); //получение имени конечного файла из пути
        private QueueWrapper _queueOfBlocks;
        private string _sourceFileName;
        private string _pathOfDestinationFile;

        public void Compression(string pathOfSourceFile, string pathOfDestinationFile)
        {
            try
            {
                using (var sourceFile = File.OpenRead(pathOfSourceFile))
                {
                    _pathOfDestinationFile = pathOfDestinationFile;
                    _sourceFileName = _nameOfFile(sourceFile.Name);

                    if (!Directory.Exists($@"{pathOfSourceFile}\{_sourceFileName}\{_dataDirectoryName}"))
                        Directory.CreateDirectory($@"{pathOfDestinationFile}\{_sourceFileName}\{_dataDirectoryName}");

                    var remainingFileSize = sourceFile.Length;
                    var blockCount = (int)(sourceFile.Length % _sizeOfBlock > 0 ? sourceFile.Length / _sizeOfBlock + 1 : sourceFile.Length % _sizeOfBlock);
                    _queueOfBlocks = new QueueWrapper(SystemUsageHelper.GetAvailableRam(), (int)(_sizeOfBlock / (1024 * 1024)));
                    _blocksOfArchive = new BlockOfArchive[blockCount];

                    for (int i = 0; i < blockCount; i++)
                    {
                        var lenghtOfCurrentBlock = remainingFileSize - _sizeOfBlock > 0 ? _sizeOfBlock : remainingFileSize;
                        var buffer = new byte[lenghtOfCurrentBlock];
                        var bytesRead = 0;
                        while (bytesRead < buffer.Length)
                            bytesRead = sourceFile.Read(buffer, 0, buffer.Length);

                        var indexOfBlock = i;
                        _queueOfBlocks.Enqueue(new KeyValuePair<int, byte[]>(indexOfBlock, buffer));
                        TryStartNewCompressThread();
                        remainingFileSize -= lenghtOfCurrentBlock;
                    }
                    CompletionQueue();
                    CreateInfoFile($@"{pathOfDestinationFile}\{_sourceFileName}\{_gzipFileInfoName}", sourceFile.Length, sourceFile.Name);
                    WaitingForAllThreads();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Ошибка архивации. {Environment.NewLine}{e.Message}");
                throw;
            }
        }

        public void Decompression(string pathToArchiveDirectory)
        {
            var archiveData = GetBlockCountFromInfoFile(pathToArchiveDirectory);
            try
            {
                using (var fileStream = File.Create($@"{pathToArchiveDirectory}\{_nameOfFile(archiveData.SourceFileName)}"))
                {
                    for (int i = 0; i < archiveData.Blocks.Length; i++)
                    {
                        using (var decompressionStream = new GZipStream(File.OpenRead($@"{pathToArchiveDirectory}\{_dataDirectoryName}\{i}.gz"), CompressionMode.Decompress))
                        {
                            var buffer = new byte[archiveData.Blocks[i].Size];
                            decompressionStream. Read(buffer, 0, buffer.Length);
                            fileStream.Write(buffer, 0, buffer.Length);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Ошибка распаковки. {Environment.NewLine}{e.Message}");
                throw;
            }
        }

        /// <summary>
        /// Завершение сфармированной очереди
        /// </summary>
        private void CompletionQueue()
        {
            while (_queueOfBlocks.CanDequeue())
            {
                TryStartNewCompressThread();
            }
        }

        /// <summary>
        /// Запуск нового потока на сжатие блока из очереди, при условии, что в пуле потоков есть свободный поток
        /// </summary>
        private void TryStartNewCompressThread()
        {
            var index = Array.IndexOf(_threadPool, null);
            if (index >= 0)
            {
                var queueItem = _queueOfBlocks.Dequeue();
                _threadPool[index] = new Thread(() => Compression(queueItem.Key, queueItem.Value));
                _threadPool[index].Start();
                return;
            }

            var stopedThread = _threadPool.FirstOrDefault(x => x != null && x.ThreadState == ThreadState.Stopped);
            if (stopedThread != null)
            {
                var queueItem = _queueOfBlocks.Dequeue();
                index = Array.IndexOf(_threadPool, stopedThread);
                _threadPool[index] = new Thread(() => Compression(queueItem.Key, queueItem.Value));
                _threadPool[index].Start();
            }
        }

        /// <summary>
        /// Сжатие блока данных
        /// </summary>
        /// <param name="blockIndex"></param>
        /// <param name="buffer"></param>
        private void Compression(int blockIndex, byte[] buffer)
        {
            try
            {
                using (var gZipStream = new GZipStream(File.OpenWrite($@"{_pathOfDestinationFile}\{_sourceFileName}\{_dataDirectoryName}\{blockIndex}.gz"), CompressionMode.Compress))
                {
                    gZipStream.Write(buffer, 0, buffer.Length);
                    _blocksOfArchive[blockIndex] = new BlockOfArchive(blockIndex, buffer.Length);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        private void WaitingForAllThreads()
        {
            while (_threadPool.Any(x => x.ThreadState != ThreadState.Stopped))
                Thread.Sleep(100);
        }

        private void CreateInfoFile(string path, long totalSize, string fileName)
        {
            using (StreamWriter infoFile = File.CreateText(path))
            {
                infoFile.WriteLine(JsonConvert.SerializeObject(new ArchiveData(fileName, _blocksOfArchive, totalSize)));
            }
        }

        private ArchiveData GetBlockCountFromInfoFile(string path)
        {
            try
            {
                return JsonConvert.DeserializeObject<ArchiveData>(File.ReadAllText($@"{path}\{_gzipFileInfoName}"));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Архив поврежден{Environment.NewLine}{e.Message}");
                throw;
            }
        }
    }
}
