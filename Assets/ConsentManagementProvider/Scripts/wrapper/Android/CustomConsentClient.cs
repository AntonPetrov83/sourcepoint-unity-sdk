﻿using System;
using UnityEngine;
using ConsentManagementProviderLib.Json;

namespace ConsentManagementProviderLib.Android
{
    internal class CustomConsentClient : AndroidJavaProxy
    {
        static readonly string NativeJavaInterfaceName = "com.sourcepoint.cmplibrary.consent.CustomConsentClient";
        Action<GdprConsent> callback;
        internal GdprConsent customGdprConsent = null;
        
        public CustomConsentClient(Action<GdprConsent> callback) : base(new AndroidJavaClass(NativeJavaInterfaceName)) 
        {
            this.callback = callback;
        }

        void transferCustomConsentToUnity(string spConsentsJson)
        {
            CmpDebugUtil.Log("transferCustomConsentToUnitySide c#-side custom consent ->" + spConsentsJson.ToString());
            SpCustomConsentAndroid parsed = null;
            try
            {
                parsed = JsonUnwrapper.UnwrapSpCustomConsentAndroid(spConsentsJson);
            }
            catch (Exception ex)
            {
                Debug.LogError("Something went wrong while parsing the json data; null will be returned. \n Exception message: " + ex.Message);
            }
            finally
            {
                if (parsed == null)
                {
                    callback?.Invoke(null);
                }
                else
                {
                    var spGdpr = JsonUnwrapper.UnwrapSpGdprConsentAndroid(parsed.gdpr);
                    customGdprConsent = spGdpr.consents;
                    callback?.Invoke(customGdprConsent);
                }
            }
        }
    }
}