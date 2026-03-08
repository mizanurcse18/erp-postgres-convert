using System;
using System.Linq;
using System.Collections.Generic;

using Microsoft.EntityFrameworkCore.ChangeTracking;


using Newtonsoft.Json;
using Core.AppContexts;
using Core.Extensions;

namespace DAL.Core.Entities
{
    public class AUDIT_ENTRY
    {
        public AUDIT_ENTRY(EntityEntry entry)
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

        public AUDIT_LOG ToAudit()
        {
            var currUser = AppContexts.User;
            var audit = new AUDIT_LOG
            {
                TABLE_NAME = TableName,
                AUDIT_DATE = DateTime.Now,
                KEY_VALUES = JsonConvert.SerializeObject(KeyValues),
                OLD_VALUES = OldValues.Count.IsZero() ? null : JsonConvert.SerializeObject(OldValues),
                NEW_VALUES = NewValues.Count.IsZero() ? null : JsonConvert.SerializeObject(NewValues),
                ROW_STATE = RowState,
                COMPANY_ID = currUser.CompanyID,
                CREATED_BY = currUser.UserID,
                CREATED_DATE = DateTime.Now,
                CREATED_IP = currUser.IPAddress ?? AppContexts.GetIPAddress()
            };

            return audit;
        }
    }
}
