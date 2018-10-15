namespace Tutorial.Tests.LinqToObjects
{
    using System.Collections.Generic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Tutorial.LinqToObjects;

    [TestClass]
    public class LinkedListSequenceTests
    {
        [TestMethod]
        public void EmptyTest()
        {
            IteratorPattern.LinkedListNode<int> head = null;
            IteratorPattern.LinkedListSequence<int> sequence = new IteratorPattern.LinkedListSequence<int>(head);
            List<int> list = new List<int>();
            foreach (int value in sequence)
            {
                list.Add(value);
            }

            Assert.IsFalse(list.Any());
        }

        [TestMethod]
        public void SingleTest()
        {
            IteratorPattern.LinkedListNode<int> head = new IteratorPattern.LinkedListNode<int>(1);
            IteratorPattern.LinkedListSequence<int> sequence = new IteratorPattern.LinkedListSequence<int>(head);
            List<int> list = new List<int>();
            foreach (int value in sequence)
            {
                list.Add(value);
            }

            Assert.AreEqual(1, list.Single());
        }

        [TestMethod]
        public void MultipleTest()
        {
            IteratorPattern.LinkedListNode<int> head = new IteratorPattern.LinkedListNode<int>(0, new IteratorPattern.LinkedListNode<int>(1, new IteratorPattern.LinkedListNode<int>(2, new IteratorPattern.LinkedListNode<int>(3))));
            IteratorPattern.LinkedListSequence<int> sequence = new IteratorPattern.LinkedListSequence<int>(head);
            List<int> list = new List<int>();
            foreach (int value in sequence)
            {
                list.Add(value);
            }

            EnumerableAssert.AreSequentialEqual(new int[] { 0, 1, 2, 3 }, list);
        }
    }
}
