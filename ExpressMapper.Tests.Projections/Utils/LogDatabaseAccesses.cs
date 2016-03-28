using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;

namespace ExpressMapper.Tests.Projections.Utils
{
    public class LogDatabaseAccesses : IDisposable
    {
        private readonly DbContext _db;
        private readonly string _whatIn;
        private readonly List<string> _dbLogs = new List<string>();

        public LogDatabaseAccesses(DbContext db)
        {
            _db = db;
            var stackTrace = new StackTrace();
            _whatIn = stackTrace.GetFrame(1).GetMethod().Name;
            db.Database.Log = x => _dbLogs.Add(x);
        }

        public void Dispose()
        {
            Console.WriteLine("Database accesses as logged while in {0}", _whatIn);
            foreach (var log in _dbLogs)
            {
                Console.WriteLine("-- {0}", log);
            }
            _db.Database.Log = null;
        }

    }
}