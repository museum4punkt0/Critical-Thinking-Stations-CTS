using System;

namespace Gemelo.Components.Cts.Database.Models
{
    public class CtsUser
    {
        public int CtsUserID { get; set; }

        public bool IsActive { get; set; } = true;

        public string Rfid { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.Now;

        public DateTime LastUpdateTime { get; set; } = DateTime.Now;

        public string DetailsAsJson { get; set; }
    }
}
