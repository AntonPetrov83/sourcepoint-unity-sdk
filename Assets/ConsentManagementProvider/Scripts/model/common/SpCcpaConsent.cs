using System.Text.Json.Serialization;

namespace ConsentManagementProviderLib
{
    public class SpCcpaConsent
    {
        [JsonInclude] public CcpaConsent consents;

        public SpCcpaConsent(CcpaConsent consents)
        {
            this.consents = consents;
        }
    }
}