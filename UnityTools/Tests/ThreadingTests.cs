namespace UnityTools
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    using NUnit.Framework;

    using UnityTools.Threading;

    [TestFixture]
    public class ThreadingTests
    {
        #region Methods

        [Test]
        public void TaskAllMethodCombinesTasksInParallel()
        {
            const int taskCount = 10;
            const int taskTime = 500; //ms
            const float tolerance = 50; //ms
            const int testLimit = 600; //ms
            UnityTask[] tasks = new UnityTask[taskCount];
            List<Thread> threads = new List<Thread>(taskCount);

            for (int i = 0; i < taskCount; i++)
            {
                UnityTask task = new UnityTask(false);
                tasks[i] = task;
                Thread thread = new Thread( () =>
                {
                    try
                    {
                        Thread.Sleep(taskTime);
                        task.Resolve();
                    }
                    catch (Exception e)
                    {
                        task.Reject(e);
                    }
                });
                thread.Start();
                threads.Add(thread);
            }

            DateTime startTime = DateTime.Now;
            object result = UnityTask.All(tasks).Result; // Result blocks until finished
            TimeSpan totalTime = DateTime.Now - startTime;

            Assert.AreEqual(totalTime.TotalMilliseconds, (float) taskTime, tolerance);
        }

        [Test]
        public void TaskAllSequentialMethodCombinesTasksSequentially()
        {
            const int taskCount = 10;
            const int taskTime = 50; //ms
            const float tolerance = 50; //ms
            int tasksExecuted = 0;
            int accessCounter = 0;
            Func<UnityTask>[] tasks = new Func<UnityTask>[taskCount];
            List<Thread> threads = new List<Thread>(taskCount);

            for (int i = 0; i < taskCount; i++)
            {
                int i_ = i; // Copy i for lambdas

                UnityTask task = new UnityTask(false);
                Thread thread = new Thread( (object count) =>
                {
                    try
                    {
                        int count_ = (int)count;

                        Assert.AreEqual(accessCounter, 0, "Two tasks are executing simulaniously");
                        accessCounter++;

                        Assert.AreEqual(tasksExecuted, count_, "Tasks executed should be equal to the current index");
                        Thread.Sleep(taskTime);
                        Assert.AreEqual(tasksExecuted, count_, "Tasks executed should be equal to the current index");

                        tasksExecuted++;

                        accessCounter--;
                        task.Resolve();
                    }
                    catch (Exception e)
                    {
                        task.Reject(e);
                    }
                });

                tasks[i_] = () => { thread.Start(i_); return task; };

                threads.Add(thread);
            }

            DateTime startTime = DateTime.Now;
            object result = UnityTask.AllSequential(tasks).Result; // Result blocks until finished
            TimeSpan totalTime = DateTime.Now - startTime;

            Assert.AreEqual((float) taskTime * taskCount, totalTime.TotalMilliseconds , tolerance);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TaskCanOnlyRejectOnce()
        {
            UnityTask t = new UnityTask(false);
            t.Then(null, (o) => Console.WriteLine("Task Failed"));

            t.Reject(new Exception());
            t.Reject(new Exception());
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TaskCanOnlyResolveOnce()
        {
            UnityTask t = new UnityTask(false);
            t.Then((o) => Console.WriteLine("Task Fulfilled"));

            t.Resolve();
            t.Resolve();
        }

        [Test]
        public void TaskDoesFail()
        {
            Exception value = null;
            Exception targetValue = new Exception();

            UnityTask t = new UnityTask(false);
            t.Then(null, (ex) => value = ex);

            t.Reject(targetValue);

            Assert.That(value == targetValue);
        }

        [Test]
        public void TaskDoesNotify()
        {
            float value = 0f;
            float targetValue = 1f;

            UnityTask t = new UnityTask(false);
            t.Then(null, null, (p) => value = p);

            t.Notify(1f);

            Assert.That(UnityEngine.Mathf.Approximately(value, targetValue));
        }

        [Test]
        public void TaskDoesResolve()
        {
            object value = null;
            object targetValue = new object();

            UnityTask t = new UnityTask(false);
            t.Then((o) => value = o);

            t.Resolve(targetValue);

            Assert.That(value == targetValue);
        }

        #endregion Methods
    }
}