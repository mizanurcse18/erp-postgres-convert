using System;
using System.Linq;
using System.Collections.Generic;

using Microsoft.EntityFrameworkCore.ChangeTracking;


using Newtonsoft.Json;
using Core.AppContexts;
using Core.Extensions;

namespace DAL.Core.Entities
{
    public class AuditEntry
    {
        public AuditEntry(EntityEntry entry)
        {
            Entry = entry;
        }

        public EntityEntry Entry { get; }
        public string TableName { get; set; }
        public Dictionary<string, object> KeyValues { get; } = new Dictionary<string, object>();
        public Dictionary<string, object> OldValues { get; } = new Dictionary<string, object>();
        public Dictionary<string, object> NewValues { get; } = new Dictionary<string, object>();
        public string RowState { get; set; }
        public List<PropertyEntry> TemporaryProperties { get; } = new List<PropertyEntry>();

        public bool HasTemporaryProperties => TemporaryProperties.Any();

        public AuditLog ToAudit()
        {
            var currUser = AppContexts.User;
            var audit = new AuditLog
            {
                TableName = TableName,
                AuditDate = DateTime.Now,
                KeyValues = JsonConvert.SerializeObject(KeyValues),
                OldValues = OldValues.Count.IsZero() ? null : JsonConvert.SerializeObject(OldValues),
                NewValues = NewValues.Count.IsZero() ? null : JsonConvert.SerializeObject(NewValues),
                RowState = RowState,
                CompanyID = currUser.CompanyID,
                CreatedBy = currUser.UserID,
                CreatedDate = DateTime.Now,
                CreatedIP = currUser.IPAddress ?? AppContexts.GetIPAddress()
            };

            return audit;
        }
    }
}
