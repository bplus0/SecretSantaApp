﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretSantaApp.Models;

namespace SecretSantaApp.ViewModels
{
    public class CustomUserDetailsEditModel : CustomUserDetails
    {
        public bool Saved { get; set; }
    }
}