using System.Web;
using GLAA.Web.Controllers;

namespace GLAA.Web.Helpers
{
    public class SessionHelper : ISessionHelper
    {
        public static string LicenceId = "LicenceId";
        public static string CurrentUserIsAdmin = "CurrentUserIsAdmin";

        private readonly HttpSessionStateBase session;

        public SessionHelper(HttpSessionStateBase session)
        {
            this.session = session;
        }

        public object Get(string key)
        {
            return session[key];
        }

        public void Set(string key, object value)
        {
            session[key] = value;
        }

        public int GetInt(string key)
        {
            return GetInt(session, key);
        }

        public bool GetBool(string key)
        {
            return GetBool(session, key);
        }

        public void SetSubmittedPage(FormSection section, int id)
        {
            Set("LastSubmittedPageSection", section.ToString());
            Set("LastSubmittedPageId", id);
        }

        public void SetLoadedPage(int id)
        {
            Set("LastLoadedPageId", id);
        }

        public int GetLoadedPage()
        {
            return GetInt("LastLoadedPageId");
        }

        public int GetCurrentPaId()
        {
            return GetInt("PaId");
        }

        public bool GetCurrentPaIsDirector()
        {
            return GetBool("IsDirector");
        }

        public void SetCurrentPaStatus(int id, bool isDirector)
        {
            Set("PaId", id);
            Set("IsDirector", isDirector);
        }

        public void ClearCurrentPaStatus()
        {
            SetCurrentPaStatus(0, false);
        }

        public void SetCurrentLicenceId(int id)
        {
            Set(LicenceId, id);
        }

        public int GetCurrentLicenceId()
        {
            return GetInt(LicenceId);
        }

        public int GetCurrentAbrId()
        {
            return GetInt("AbrId");
        }

        public int GetCurrentNamedIndividualId()
        {
            return GetInt("NamedIndividualId");
        }

        public void SetCurrentAbrId(int id)
        {
            Set("AbrId", id);
        }

        public void SetCurrentNamedIndividualId(int id)
        {
            Set("NamedIndividualId", id);
        }

        public void ClearCurrentAbrId()
        {
            SetCurrentAbrId(0);
        }

        public void ClearCurrentNamedIndividualId()
        {
            SetCurrentNamedIndividualId(0);
        }

        public int GetCurrentDopId()
        {
            return GetInt("DopId");
        }

        public bool GetCurrentDopIsPa()
        {
            return GetBool("IsPa");
        }

        public void SetCurrentDopStatus(int id, bool isPa)
        {
            Set("DopId", id);
            Set("IsPa", isPa);
        }

        public void ClearCurrentDopStatus()
        {
            SetCurrentDopStatus(0, false);
        }

        // TODO: Fix this when we add proper roles
        public void SetCurrentUserIsAdmin(bool isAdmin)
        {
            Set(CurrentUserIsAdmin, isAdmin);
        }

        public bool GetCurrentUserIsAdmin()
        {
            return GetBool(CurrentUserIsAdmin);
        }

        public static int GetInt(HttpSessionStateBase session, string key)
        {
            var value = 0;
            var sessionValue = session[key];
            if (sessionValue != null)
            {
                int.TryParse(sessionValue.ToString(), out value);
            }
            return value;
        }

        public static bool GetBool(HttpSessionStateBase session, string key)
        {
            var value = false;
            var sessionValue = session[key];
            if (sessionValue != null)
            {
                bool.TryParse(sessionValue.ToString(), out value);
            }
            return value;
        }

        public static int GetCurrentLicenceId(HttpSessionStateBase session)
        {
            return GetInt(session, LicenceId);
        }

        public static bool GetCurrentUserIsAdmin(HttpSessionStateBase session)
        {
            return GetBool(session, CurrentUserIsAdmin);
        }
    }
}