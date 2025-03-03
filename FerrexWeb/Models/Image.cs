using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FerrexWeb.Models
{
    public class Image
    {
        [Key]
        public int id_image { get; set; }
        public string url { get; set; }
        public string title { get; set; }
    }
}
