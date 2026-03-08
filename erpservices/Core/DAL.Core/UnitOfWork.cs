using System;
using System.Collections.Generic;

using Microsoft.EntityFrameworkCore.Storage;

using Core.AppContexts;

namespace DAL.Core
{
    public sealed class UnitOfWork : IDisposable
    {
        readonly List<BaseDbContext> activeDbs;
        readonly List<IDbContextTransaction> activeTrans;
        private bool tranStarted;

        public UnitOfWork()
        {
            activeDbs = AppContexts.GetActiveDbContexts<BaseDbContext>();
            activeTrans = new List<IDbContextTransaction>();
            BeginTrans();
        }

        private void BeginTrans()
        {
            foreach (var context in activeDbs)
            {
                activeTrans.Add(context.Database.BeginTransaction());
            }

            tranStarted = true;
        }

        private void Commit()
        {
            foreach (var trans in activeTrans)
            {
                trans.Commit();
            }

            tranStarted = false;
        }

        private void Rollback()
        {
            if (!tranStarted) return;
            foreach (var trans in activeTrans)
            {
                trans.Rollback();
            }

            tranStarted = false;
        }

        public static void SaveChanges()
        {
            var dbs = AppContexts.GetActiveDbContexts<BaseDbContext>();
            SaveChanges(dbs);
        }

        private static void SaveChanges(List<BaseDbContext> dbs)
        {
            foreach (var context in dbs)
            {
                context.SaveChanges();
            }
        }

        private static void SaveChangesWithAudit(List<BaseDbContext> dbs)
        {
            foreach (var context in dbs)
            {
                context.SaveChangesWithAudit();
            }
        }

        public void CommitChanges()
        {
            try
            {
                SaveChanges(activeDbs);
                Commit();
            }
            catch (Exception)
            {
                Rollback();
                throw;
            }
        }

        public void CommitChangesWithAudit()
        {
            try
            {
                SaveChangesWithAudit(activeDbs);
                Commit();
            }
            catch (Exception ex)
            {
                Rollback();
                throw;
            }
        }

        public void Dispose()
        {
            Rollback();

            foreach (var trans in activeTrans)
            {
                trans.Dispose();
            }
        }
    }
}