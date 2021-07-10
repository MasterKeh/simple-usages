using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SUs.KeepLatest.Cli.DataAccess.Models
{
    [Table("KeepLatestItems")]
    public class KeepLatestItem
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string ItemName { get; set; }
        [Required]
        public string ItemUrl { get; set; }
        public string ItemVer { get; set; }
        public string ItemLatestVer { get; set; }
        public DateTime? ItemLatestReleasedAt { get; set; }
    }
}
