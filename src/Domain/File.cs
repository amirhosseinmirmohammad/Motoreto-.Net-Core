using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class File
    {
        public File () { }
        public int Id { get; set; }

        [DisplayName("سایز فایل")]
        [Display(Name = "سایز فایل")]
        public int FileSize { get; set; }

        [DisplayName("زمان آپلود فایل")]
        [Display(Name = "زمان آپلود فایل")]
        public DateTime UploadDate { get; set; }

        [DisplayName("پسوند فایل")]
        [Display(Name = "پسوند فایل")]
        public string Extension { get; set; }

        [DisplayName("آدرس فایل")]
        [Display(Name = "آدرس فایل")]
        public string FullPath { get; set; }

        [DisplayName("نام فایل همراه پسوند")]
        [Display(Name = "نام فایل همراه پسوند")]
        public string FileName { get; set; }

        [DisplayName("نام فایل بدون پسوند")]
        [Display(Name = "نام فایل بدون پسوند")]
        public string FileNameWithoutExtension { get; set; }

        [DisplayName("محل ذخیره فایل")]
        [Display(Name = "محل ذخیره فایل")]
        public string Directory { get; set; }

        public int? BlogId { get; set; }

        public virtual Blog Blog { get; set; }
    }
}
