﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MvcMiniProfiler.Data;

namespace MvcMiniProfiler
{
    partial class MiniProfiler : IDbProfiler
    {
        /// <summary>
        /// Contains information about queries executed during this profiling session.
        /// </summary>
        internal SqlProfiler SqlProfiler { get; private set; }

        /// <summary>
        /// Returns all currently open commands on this connection
        /// </summary>
        public SqlTiming[] GetInProgressCommands()
        {
            return SqlProfiler == null ? null : SqlProfiler.GetInProgressCommands();
        }

        /// <summary>
        /// Milliseconds, to one decimal place, that this MiniProfiler was executing sql.
        /// </summary>
        public decimal DurationMillisecondsInSql
        {
            get { return GetTimingHierarchy().Sum(t => t.HasSqlTimings ? t.SqlTimings.Sum(s => s.DurationMilliseconds) : 0); }
        }

        /// <summary>
        /// Returns all <see cref="SqlTiming"/> results contained in all child <see cref="Timing"/> steps.
        /// </summary>
        public List<SqlTiming> GetSqlTimings()
        {
            return GetTimingHierarchy().Where(t => t.HasSqlTimings).SelectMany(t => t.SqlTimings).ToList();
        }

        /// <summary>
        /// Returns true when we have profiled queries.
        /// </summary>
        public bool HasSqlTimings { get; set; }

        /// <summary>
        /// Returns true when any child Timings have duplicate queries.
        /// </summary>
        public bool HasDuplicateSqlTimings { get; set; }

        /// <summary>
        /// Contains any sql statements that are executed, along with how many times those statements are executed.
        /// </summary>
        private readonly Dictionary<string, int> _sqlExecutionCounts = new Dictionary<string, int>();

        /// <summary>
        /// Adds <paramref name="stats"/> to the current <see cref="Timing"/>.
        /// </summary>
        internal void AddSqlTiming(SqlTiming stats)
        {
            if (Head == null)
                return;

            int count;

            stats.IsDuplicate = _sqlExecutionCounts.TryGetValue(stats.RawCommandString, out count);
            _sqlExecutionCounts[stats.RawCommandString] = count + 1;

            HasSqlTimings = true;
            if (stats.IsDuplicate)
            {
                HasDuplicateSqlTimings = true;
            }

            Head.AddSqlTiming(stats);
        }


        // IDbProfiler methods

        void IDbProfiler.ExecuteStart(System.Data.Common.DbCommand profiledDbCommand, ExecuteType executeType)
        {
            SqlProfiler.ExecuteStart(profiledDbCommand, executeType);
        }

        void IDbProfiler.ExecuteFinish(System.Data.Common.DbCommand profiledDbCommand, ExecuteType executeType, System.Data.Common.DbDataReader reader)
        {
            SqlProfiler.ExecuteFinish(profiledDbCommand, executeType, reader);
        }

        void IDbProfiler.ExecuteFinish(System.Data.Common.DbCommand profiledDbCommand, ExecuteType executeType)
        {
            SqlProfiler.ExecuteFinish(profiledDbCommand, executeType);
        }

        void IDbProfiler.ReaderFinish(System.Data.Common.DbDataReader reader)
        {
            SqlProfiler.ReaderFinish(reader);
        }

        bool _isActive;
        bool IDbProfiler.IsActive { get { return _isActive; } }
        internal bool IsActive { set { _isActive = value; } }

    }
}
