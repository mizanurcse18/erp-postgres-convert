using System;

using Manager.Core.Mapper;
using DAL.Core.EntityBase;
using Security.DAL.Entities;

namespace Security.Manager.Dto
{
    [AutoMap(typeof(Menu)), Serializable]
    public class MenuDto : EntityBase
    {
        private int menuID;
        public int MenuID
        {
            get => menuID;
            set
            {
                if (PropertyChanged(menuID, value))
                    menuID = value;
            }
        }

        private int parentID;
        public int ParentID
        {
            get => parentID;
            set
            {
                if (PropertyChanged(parentID, value))
                    parentID = value;
            }
        }

        [NonSerialized]
        private int? applicationID;
        public int? ApplicationID
        {
            get => applicationID;
            set
            {
                if (PropertyChanged(applicationID, value))
                    applicationID = value;
            }
        }

        [NonSerialized]
        private string displayName;
        public string DisplayName
        {
            get => displayName;
            set
            {
                if (PropertyChanged(displayName, value))
                    displayName = value;
            }
        }

        [NonSerialized]
        private string description;
        public string Description
        {
            get => description;
            set
            {
                if (PropertyChanged(description, value))
                    description = value;
            }
        }

        [NonSerialized]
        private string routeName;
        public string RouteName
        {
            get => routeName;
            set
            {
                if (PropertyChanged(routeName, value))
                    routeName = value;
            }
        }

        [NonSerialized]
        private string viewName;
        public string ViewName
        {
            get => viewName;
            set
            {
                if (PropertyChanged(viewName, value))
                    viewName = value;
            }
        }

        [NonSerialized]
        private string parameters;
        public string Parameters
        {
            get => parameters;
            set
            {
                if (PropertyChanged(parameters, value))
                    parameters = value;
            }
        }

        private bool isVisible;
        public bool IsVisible
        {
            get => isVisible;
            set
            {
                if (PropertyChanged(isVisible, value))
                    isVisible = value;
            }
        }

        [NonSerialized]
        private decimal sequenceNo;
        public decimal SequenceNo
        {
            get => sequenceNo;
            set
            {
                if (PropertyChanged(sequenceNo, value))
                    sequenceNo = value;
            }
        }

        //public MenuPrivilegeDto MenuPrivilege { get; set; }
    }
}
