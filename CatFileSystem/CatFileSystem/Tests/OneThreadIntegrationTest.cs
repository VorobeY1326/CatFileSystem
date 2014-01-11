using System;
using System.Collections.Generic;
using System.Linq;
using CatFileSystem.FS;
using NUnit.Framework;
using File = System.IO.File;

namespace CatFileSystem.Tests
{
    /*
     * Однопоточные тесты на файловую систему, дабы проверить её работоспособность.
     */
    [TestFixture]
    public class OneThreadIntegrationTest
    {
        private FileSystem sut;

        [SetUp]
        public void SetUp()
        {
            File.Delete("test.db");
            sut = new FileSystem("test.db");
            sut.FormatC();
        }

        [Test]
        public void TestCreateFile()
        {
            sut.AddFile("qqq");
            var res = sut.ReadFile("qqq");

            Assert.AreEqual(0, res.Length);
        }

        [Test]
        public void TestWriteFile()
        {
            sut.AddFile("qqq");
            var value = new byte[]{1,2,3,4,5};
            sut.WriteFile("qqq", value);
            var res = sut.ReadFile("qqq");

            Assert.AreEqual(5, res.Length);
            for (int i=0; i<5; i++)
                Assert.AreEqual(value[i], res[i]);
        }

        [Test]
        public void TestWriteWithoutCreate()
        {
            var value = new byte[] { 1, 2, 3, 4, 5 };
            sut.WriteFile("qqq", value);
            var res = sut.ReadFile("qqq");

            Assert.AreEqual(5, res.Length);
            for (int i = 0; i < 5; i++)
                Assert.AreEqual(value[i], res[i]);
        }

        [Test]
        public void TestWriteSeveralBlocks()
        {
            var random = new Random();
            var value = Enumerable.Repeat(0, 50000).Select(a => (byte)random.Next()).ToArray();
            sut.WriteFile("qqq", value);
            var res = sut.ReadFile("qqq");

            Assert.AreEqual(value.Length, res.Length);
            for (int i = 0; i < res.Length; i++)
                Assert.AreEqual(value[i], res[i]);
        }

        [Test]
        public void TestWriteSeveralFiles()
        {
            var count = 20;
            var random = new Random();
            var values = new List<byte[]>();
            
            for (int i = 0; i < count; i++)
                values.Add(Enumerable.Repeat(0, 5000).Select(a => (byte)random.Next()).ToArray());
            
            for (int i = 0; i < count; i++)
                sut.WriteFile("file" + i, values[i]);

            for (int i = 0; i < count; i++)
            {
                var res = sut.ReadFile("file" + i);

                Assert.AreEqual(values[i].Length, res.Length);
                for (int j = 0; j < res.Length; j++)
                    Assert.AreEqual(values[i][j], res[j]);
            }
        }

        [Test]
        public void TestMultipleReadWrites()
        {
            const int filesCount = 20;
            const int stepsCount = 14;
            var random = new Random();

            for (int step = 0; step < stepsCount; step++)
            {
                var values = new List<byte[]>();

                for (int i = 0; i < filesCount; i++)
                    values.Add(Enumerable.Repeat(0, random.Next(0, 5000)).Select(a => (byte)random.Next()).ToArray());

                for (int i = 0; i < filesCount; i++)
                    sut.WriteFile("file" + i, values[i]);

                for (int i = 0; i < filesCount; i++)
                {
                    var res = sut.ReadFile("file" + i);

                    Assert.AreEqual(values[i].Length, res.Length);
                    for (int j = 0; j < res.Length; j++)
                        Assert.AreEqual(values[i][j], res[j]);
                }
            }
        }

        [Test]
        public void TestMultipleWriteDeletes()
        {
            const int filesCount = 20;
            const int stepsCount = 100;
            var random = new Random();

            for (int step = 0; step < stepsCount; step++)
            {
                var values = new List<byte[]>();

                for (int i = 0; i < filesCount; i++)
                    values.Add(Enumerable.Repeat(0, random.Next(0, 5000)).Select(a => (byte)random.Next()).ToArray());

                for (int i = 0; i < filesCount; i++)
                    sut.WriteFile("file" + i, values[i]);

                for (int i = 0; i < filesCount; i++)
                    sut.RemoveFile("file" + i);

                for (int i = 0; i < filesCount; i++)
                {
                    var res = sut.ReadFile("file" + i);
                    Assert.IsNull(res);
                }
            }
        }
    }
}