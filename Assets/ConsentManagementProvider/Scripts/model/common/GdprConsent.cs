using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text;

namespace ConsentManagementProviderLib
{
    public class GdprConsent
    {
        [JsonInclude] public string uuid;
        [JsonInclude] public string euconsent;
        [JsonInclude] public Dictionary<string, object> TCData;
        [JsonInclude] public Dictionary<string, SpVendorGrant> grants;
        [JsonInclude] public List<string> acceptedCategories;
        [JsonInclude] public bool applies;
        [JsonInclude] public string webConsentPayload;

        public string ToFullString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"UUID: {uuid}");
            sb.AppendLine($"EUConsent: {euconsent}");
            sb.AppendLine($"Applies: {applies}");
            sb.AppendLine($"WebConsentPayload: {webConsentPayload}");

            if(TCData != null)
            {
                sb.AppendLine("TCData:");
//                 foreach (var kvp in TCData)
//                     sb.AppendLine($"    {kvp.Key}: {kvp.Value}");
            }

            if(grants != null)
            {
                sb.AppendLine("Grants:");
                foreach (var grant in grants)
                {
                    sb.AppendLine($"    Vendor: {grant.Key}");
                    sb.AppendLine($"    VendorGrant: {grant.Value.vendorGrant}");

                    if(grant.Value.purposeGrants != null)
                    {
                        sb.AppendLine("    Purpose Grants:");
                        foreach (var purposeGrant in grant.Value.purposeGrants)
                            sb.AppendLine($"        {purposeGrant.Key}: {purposeGrant.Value}");
                    }
                }
            }

            if(acceptedCategories != null)
            {
                sb.AppendLine("Accepted Categories:");
                foreach (var category in acceptedCategories)
                    sb.AppendLine($"    {category}");
            }

            return sb.ToString();
        }

    }
}