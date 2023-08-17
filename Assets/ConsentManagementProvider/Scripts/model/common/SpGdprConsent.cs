using System.Text.Json.Serialization;

namespace ConsentManagementProviderLib
{
    public class SpGdprConsent
    {
        [JsonInclude] public GdprConsent consents;

        public SpGdprConsent(GdprConsent consents)
        {
            this.consents = consents;
        }
    }
}