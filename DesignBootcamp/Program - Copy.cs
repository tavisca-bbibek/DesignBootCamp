using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DesignBootcamp
{

    public class Document
    {
        public string Id { get; set; }
    }

    public class DocumentDbWithCaching : IDocumentDb
    {
        public DocumentDbWithCaching(IDocumentCache cache, IDocumentDb inner)
        {
            Cache = cache;
            Inner = inner;
        }

        public IDocumentCache Cache { get; }
        public IDocumentDb Inner { get; }

        public Document GetById(string id)
        {
            var result = Cache.Get(id);
            if (result == null)
            {
                result = Inner.GetById(id);
                Cache.Set(id, result);
            }
            return result;
        }
    }


    public class DocumentService
    {
        public DocumentService(IDocumentDb db)
        {
            Db = db;
        }

        public IDocumentDb Db { get; }

        public Document GetDocument(string id)
        {
            return Db.GetById(id);
        }
    }

    
    public interface IDocumentCache
    {
        Document Get(string id);

        void Set(string id, Document doc);
    }

    public interface IAuditLog
    {
        void WriteLog(string log);
    }

    public class AudittableDocumentDb : IDocumentDb
    {
        public AudittableDocumentDb(IDocumentDb inner, IAuditLog log)
        {
            Inner = inner;
            Log = log;
        }

        public IDocumentDb Inner { get; set; }

        public IAuditLog Log { get; set; }
        public Document GetById(string id)
        {
            Log.WriteLog($"Document {id} accessed at {DateTime.Now}");
            return Inner.GetById(id);
        }
    }

    public class ConsoleAuditLog : IAuditLog
    {
        public void WriteLog(string log)
        {
            Console.WriteLine(log);
        }
    }

    public static class DocumentDbExtensions
    {
        public static IDocumentDb WithCaching(this IDocumentDb db, IDocumentCache cache = null)
        {
            return new DocumentDbWithCaching(cache ?? new InMemoryCache(), db);
        }

        public static IDocumentDb WithAuditing(this IDocumentDb db, IAuditLog log = null)
        {
            return new AudittableDocumentDb(db, log ?? new ConsoleAuditLog());
        }


    }

    public interface IDocumentDb
    {
        Document GetById(string id);
    }

    public class SqlDocumentDb : IDocumentDb
    {
        public Document GetById(string id)
        {
            // code here to actually get the document
            Thread.Sleep(5000);
            return new Document { Id = id };
        }
    }

    public class InMemoryCache : IDocumentCache
    {
        private static Dictionary<string, Document> _cache = new Dictionary<string, Document>();
        public Document Get(string id)
        {
            if (_cache.TryGetValue(id, out Document doc) == false)
                return null;
            return doc;
        }

        public void Set(string id, Document doc)
        {
            _cache[id] = doc;
        }
    }
    
    public static class Timer
    {
        public static decimal Measure( Action action)
        {
            decimal timing = decimal.Zero;
            Stopwatch timer = Stopwatch.StartNew();
            try
            {
                action();
            }
            finally
            {
                timing = (decimal)timer.ElapsedTicks / Stopwatch.Frequency * 1000;
                timer = null;
            }
            return timing;

        }
    }
    

}
