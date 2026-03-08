using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace API.Core
{
    public static class ApiCoreUtility
    {
        public static ObjectResult ResponseAPPVersion()
        {
            return new ObjectResult(new
            {
                status = "upgrade_app",
                msg = "এ্যাপ স্টোর থেকে আপনার এ্যাপটি আপডেট করে নিন",
                userID = string.Empty,
                userToken = "",
                jwt = "",
                userName = string.Empty,
                changePassword = "0",
                role = "",
                home_location_text = "",
                contactNumber = string.Empty,
                longitude = string.Empty,
                latitude = string.Empty,
                fullName = string.Empty,
            });
        }
        
    }
}
