﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QLyTV.Models
{
    public class LoginRequestModel
    {
        public string Email { get; set; }

        public string Password { get; set; }

        public int RoleId { get; set; }
    }
}