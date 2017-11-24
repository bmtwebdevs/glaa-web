using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GLAA.Domain;
using GLAA.Domain.Auth;
using Microsoft.Owin.Security.DataProtection;

namespace GLAA.Web.App_Start
{
    public class AppDataProtectionProvider : IAppDataProtectionProvider
    {
        public IDataProtectionProvider DataProtectionProvider
        {
            get { return Startup.DataProtectionProvider; }
        }
    }
}