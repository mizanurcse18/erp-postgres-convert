using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

using Newtonsoft.Json;

using Core.Extensions;
namespace DAL.Core.EntityBase
{
    [Serializable]
    public class EntityBase : IEntityBase
    {
        [NotMapped]
        private string rowEditorStatus;
        [NotMapped, Browsable(false)]
        public string RowEditorStatus
        {
            get => rowEditorStatus;
            set
            {
                rowEditorStatus = value;
                switch (rowEditorStatus)
                {
                    case "inserted":
                    case "insert":
                        SetAdded();
                        break;
                    case "deleted":
                    case "delete":
                        ObjectState = ModelState.Deleted;
                        break;
                    case "updated":
                    case "update":
                        SetModified();
                        break;
                    default:
                        SetUnchanged();
                        break;
                }
            }
        }

        [NotMapped, JsonIgnore]
        public ModelState ObjectState { get; set; } = ModelState.Unchanged;

        [JsonIgnore]
        public bool IsAdded => ObjectState.Equals(ModelState.Added);

        [JsonIgnore]
        public bool IsModified => ObjectState.Equals(ModelState.Modified);

        [JsonIgnore]
        public bool IsDeleted => ObjectState.Equals(ModelState.Deleted);

        [JsonIgnore]
        public bool IsUnchanged => ObjectState.Equals(ModelState.Unchanged);

        [JsonIgnore]
        public bool IsDetached => ObjectState.Equals(ModelState.Detached);

        [JsonIgnore]
        public bool IsArchived => ObjectState.Equals(ModelState.Archived);

        public void SetAdded()
        {
            ObjectState = ModelState.Added;
        }
        public void SetModified()
        {
            ObjectState = ModelState.Modified;
        }
        public void SetDeleted()
        {
            ObjectState = IsAdded ? ModelState.Detached : ModelState.Deleted;
        }
        public void SetDetached()
        {
            ObjectState = ModelState.Detached;
        }
        public void SetArchived()
        {
            ObjectState = IsAdded ? ModelState.Detached : ModelState.Archived;
        }
        public void SetUnchanged()
        {
            ObjectState = ModelState.Unchanged;
        }
        public T Copy<T>()
        {
            return (T)MemberwiseClone();
        }
        protected bool PropertyChanged<T>(T oldValue, T newValue)
        {
            if (!oldValue.NotEquals(newValue)) return false;
            if (IsUnchanged) SetModified();
            return true;
        }
        public override string ToString()
        {
            return $"Name = {GetType().Name}, State = {ObjectState}";
        }
    }
}
