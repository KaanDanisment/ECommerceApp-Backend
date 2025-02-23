using Core.Entities.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Entities
{
    public class Image : IEntity
    {
        public int Id { get; set; }
        public string FileUrl {  get; set; }
        public int ProductId { get; set; }

        [JsonIgnore]
        public Product Product { get; set; }
    }
}
