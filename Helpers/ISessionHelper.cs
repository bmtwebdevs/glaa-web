using GLAA.Web.Controllers;

namespace GLAA.Web.Helpers
{
    public interface ISessionHelper
    {
        object Get(string key);
        void Set(string key, object value);
        int GetInt(string key);
        bool GetBool(string key);
        void SetSubmittedPage(FormSection section, int id);
        void SetLoadedPage(int id);
        int GetLoadedPage();
        int GetCurrentPaId();
        bool GetCurrentPaIsDirector();
        void SetCurrentPaStatus(int id, bool isDirector);
        void ClearCurrentPaStatus();
        void SetCurrentLicenceId(int id);
        int GetCurrentLicenceId();
        int GetCurrentAbrId();
        int GetCurrentNamedIndividualId();
        void SetCurrentAbrId(int id);
        void SetCurrentNamedIndividualId(int id);
        void ClearCurrentAbrId();
        void ClearCurrentNamedIndividualId();
        int GetCurrentDopId();
        bool GetCurrentDopIsPa();
        void SetCurrentDopStatus(int id, bool isPa);
        void ClearCurrentDopStatus();
        void SetCurrentUserIsAdmin(bool isAdmin);
        bool GetCurrentUserIsAdmin();
    }
}