using System.Collections.Generic;
using System.Linq;

namespace Common.Structs
{
    public struct QueueWrapper
    {
        public QueueWrapper(float availableRam, int sizeOfBlock)
        {
            QueueOfBlocks = new Queue<KeyValuePair<int, byte[]>>();
            MaxNumberOfQueueItems = (int)(( availableRam / sizeOfBlock)*0.99); //Загрузка очереди на 99% доступной памяти
        }
        public Queue<KeyValuePair<int, byte[]>> QueueOfBlocks { get; set; }
        public int MaxNumberOfQueueItems { get; }
        public int CurrentNumberOfQueueItems => QueueOfBlocks.Count;

        public bool CanDequeue() => CurrentNumberOfQueueItems > 0;
        public void Enqueue(KeyValuePair<int, byte[]> item) => QueueOfBlocks.Enqueue(item);
        public KeyValuePair<int, byte[]> Dequeue() => CanDequeue() ? QueueOfBlocks.Dequeue() : new KeyValuePair<int, byte[]>();
    }
}