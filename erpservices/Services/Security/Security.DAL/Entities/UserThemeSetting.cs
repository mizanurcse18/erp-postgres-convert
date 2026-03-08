using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DAL.Core.Attribute;
using DAL.Core.EntityBase;

namespace Security.DAL.Entities
{
    [Table("UserThemeSetting"), Serializable]
    public class UserThemeSetting : EntityBase
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int UserID { get; set; }
        [Loggable]
        public string Settings { get; set; }
        [Loggable]
        public string ShortCuts { get; set; }

    }
}