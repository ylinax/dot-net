﻿using Dapper;
using StackExchange.Profiling;
using System;
using System.Data.Common;
using System.Diagnostics;
using System.Net;
using System.Threading;
using static System.Console;

namespace Samples.Console
{
    /// <summary>
    /// simple sample console application.
    /// </summary>
    public static class Program
    {
        private static MiniProfilerOptions Options;

        /// <summary>
        /// application entry point.
        /// </summary>
        /// <param name="args">application command line arguments.</param>
        public static void Main()
        {
            try
            {
                SetupProfiling();
                //Test();
                TestMultiThreaded();
                Report();

                if (Debugger.IsAttached)
                    ReadKey();
            }
            catch (Exception ex)
            {
                WriteLine(ex);
            }
        }

        /// <summary>
        /// setup the profiling.
        /// </summary>
        public static void SetupProfiling()
        {
            // We're using DefaultProfilerProvider here (not the Web provider) because we're 
            // not using HttpContext.Current to track MiniPofiler.Current
            // In general, DefaultProfilerProvider() should be the go-to for most new applications.
            Options = new MiniProfilerOptions().SetProvider(new DefaultProfilerProvider());
        }

        /// <summary>
        /// test the profiling.
        /// </summary>
        public static void Test()
        {
            var mp = Options.StartProfiler("Test");

            using (mp.Step("Level 1"))
            using (var conn = GetConnection())
            {
                conn.Query<long>("select 1");

                using (mp.Step("Level 2"))
                {
                    conn.Query<long>("select 1");
                }

                using (var wc = new WebClient())
                using (mp.CustomTiming("http", "GET https://google.com"))
                {
                    wc.DownloadString("https://google.com");
                }
            }

            mp.Stop();
        }

        public static void TestMultiThreaded()
        {
            var mp = Options.StartProfiler("Locking");
            Action doWork = () => Thread.Sleep(new Random().Next(1, 50));

            using (mp.Step("outer"))
            {
                System.Threading.Tasks.Parallel.For(0, 5, i =>
                {
                    doWork();

                    using (mp.Step("step " + i))
                    {
                        doWork();

                        using (mp.Step("sub-step" + i))
                        {
                            doWork();
                        }
                    }
                });
            }
        }

        /// <summary>
        /// produce a profiling report.
        /// </summary>
        public static void Report() => WriteLine(MiniProfiler.Current.RenderPlainText());

        /// <summary>
        /// Returns an open connection that will have its queries profiled.
        /// </summary>
        /// <returns>the database connection abstraction</returns>
        public static DbConnection GetConnection()
        {
            DbConnection cnn = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");

            // to get profiling times, we have to wrap whatever connection we're using in a ProfiledDbConnection
            // when MiniProfiler.Current is null, this connection will not record any database timings
            if (MiniProfiler.Current != null)
            {
                cnn = new StackExchange.Profiling.Data.ProfiledDbConnection(cnn, MiniProfiler.Current);
            }

            cnn.Open();
            return cnn;
        }
    }
}
