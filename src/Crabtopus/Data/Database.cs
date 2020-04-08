using System;
using LiteDB;

namespace Crabtopus.Data
{
    internal class Database : IDisposable
    {
        private readonly LiteDatabase _db;

        private bool _disposedValue;

        public Database()
        {
            _db = new LiteDatabase("filename=crabtopus.db;log=255");
        }

        public ILiteCollection<T> Set<T>() where T : IEntity
        {
            return _db.GetCollection<T>(typeof(T).Name);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _db?.Dispose();
                }

                _disposedValue = true;
            }
        }
    }
}
