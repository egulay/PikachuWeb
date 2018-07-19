using Pikachu.Data.Framework.Repository;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RenderTest.Data.Models
{
    [Table("TestTable")]
    public partial class TestTable : EntityBase
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(5)]
        public string MyStringField { get; set; }

        public DateTime MyDateField { get; set; }

        public bool MyBoolField { get; set; }

        public int MyIntField { get; set; }

        [Column(TypeName = "money")]
        public decimal MyMoneyField { get; set; }
    }
}
