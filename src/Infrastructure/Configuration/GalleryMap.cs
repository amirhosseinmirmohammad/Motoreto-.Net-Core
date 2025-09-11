using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Infrastructure.Configuration
{
    public class GalleryMap : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<Gallery>
    {
        public GalleryMap()
        {

            // Primary Key
            this.HasKey(t => t.Id);
        }
    }
}