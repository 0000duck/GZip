using System.Collections.Generic;
using System.Linq;
using Common.Exceptions;

namespace Common.Structs
{
    public struct QueueWrapper
    {
        private readonly object _lock;

        public QueueWrapper(float availableRam, int sizeOfBlock)
        {
            QueueOfBlocks = new Queue<KeyValuePair<int, byte[]>>();
            MaxNumberOfQueueItems = (int)(( availableRam / sizeOfBlock)*0.99); //Загрузка очереди на 99% доступной памяти
            _lock = new object();
            QueueIsActivate = false;
        }
        public Queue<KeyValuePair<int, byte[]>> QueueOfBlocks { get; set; }
        public int MaxNumberOfQueueItems { get; }
        public int CurrentNumberOfQueueItems => QueueOfBlocks.Count;
        /// <summary>
        /// Признак, указывающий, что очередь  активирована заполнением
        /// </summary>
        /// <remarks>Флаг необходим для проверки перед попыткой получить объект очереди</remarks>
        public bool QueueIsActivate { get; private set; }

        public bool CanDequeue() => CurrentNumberOfQueueItems > 0;
        public bool CanEnqueue() => MaxNumberOfQueueItems >= CurrentNumberOfQueueItems;

        public bool TryEnqueue(KeyValuePair<int, byte[]> item)
        {
            if (CanEnqueue())
            {
                QueueOfBlocks.Enqueue(item);
                QueueIsActivate = true;
                return true;
            }
            return false;
        } 

        public KeyValuePair<int, byte[]> Dequeue()
        {
            lock (_lock)
            {
                if (CanDequeue())
                    return QueueOfBlocks.Dequeue();
                throw new EndQueueException();
            }
        } 
    }
}