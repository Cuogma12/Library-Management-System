using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QLyTV.Models
{
    public partial class DocGia
    {
        public string Role { get; set; } // Vai trò (Reader, Librarian, Admin)
        public bool IsActive { get; set; } // Trạng thái kích hoạt
    }

}