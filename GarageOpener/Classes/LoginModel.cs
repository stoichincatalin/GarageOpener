using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GarageOpener.Classes
{
    public class LoginModel
    {
        public string User { get; set; }
        public string Parola { get; set; }

        public string Address { get; set; }

        public LoginModel(string address)
        {
            this.Address = address;
        }

        public LoginModel()
        {
            User = "";
            Parola = "";
            Address = "";
        }
    }
}