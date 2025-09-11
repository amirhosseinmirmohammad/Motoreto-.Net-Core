using System.Collections.Generic;

namespace Domain
{
    public class Gallery
    {
        public Gallery()
        {
            Images = new List<Image>();
        }

        public int Id { get; set; }

        public virtual ICollection<Image> Images { get; set; }
    }
}