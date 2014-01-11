using System;

namespace CatFileSystem.FS
{
    /*
     * Реализация операции блокировки (входа/выхода из критической секции)
     * Основана на алгоритме булочной:
     * http://en.wikipedia.org/wiki/Lamport's_bakery_algorithm
     */
    public class Locker
    {
        private readonly int maxThreadsCount;
        private readonly bool[] choosing;
        private readonly int[] number;
        private int iAmLocked = 0; // Переменная для проверки корректности алгоритма

        public Locker(int maxThreadsCount)
        {
            this.maxThreadsCount = maxThreadsCount;

            choosing = new bool[maxThreadsCount];
            number = new int[maxThreadsCount];
        }

        public void Lock(int threadId)
        {
            choosing[threadId] = true;

            for (int i = 0; i < maxThreadsCount; i++)
            {
                if (number[i] >= number[threadId])
                    number[threadId] = Math.Max(number[i], number[threadId]);
            }

            number[threadId]++;
            choosing[threadId] = false;

            for (int i = 0; i < maxThreadsCount; i++)
            {
                while (choosing[i])
                {}
                while (number[i] != 0 && ((number[i] < number[threadId]) || (number[i] == number[threadId] && i < threadId)))
                {}
            }

            iAmLocked = threadId;
        }

        public void Unlock(int threadId)
        {
            // Для проверки, что алгоритм правда работает, убеждаемся, что наш поток выходит из секции, а не некто другой
            if (iAmLocked != threadId)
                throw new Exception("Fatal error in Locker mechanism");
            iAmLocked = 0;

            number[threadId] = 0;
        }
    }
}