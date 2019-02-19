namespace Common.Abstractions.Services
{
    public interface ICompressionService
    {
        /// <summary>
        /// Сжатие файла
        /// </summary>
        /// <param name="pathOfSourceFile">путь к исходному файлу</param>
        /// <param name="pathOfDestinationFile">путь к дирректории архива</param>
        void Compression(string pathOfSourceFile, string pathOfDestinationFile);

        /// <summary>
        /// Распаковка файла
        /// </summary>
        /// <param name="pathToArchiveDirectory">Путь к корневому файлу архива (gz.info)</param>
        void Decompression(string pathToArchiveDirectory);
    }
}