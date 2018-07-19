using System;

namespace RenderTest.Web.Models
{
    public class TestTableModel
    {
        public Guid Id { get; set; }
        public string MyStringField { get; set; }
        public DateTime MyDateField { get; set; }
        public bool MyBoolField { get; set; }
        public int MyIntField { get; set; }
        public decimal MyMoneyField { get; set; }
    }
}