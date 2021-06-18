using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Text.Json;

namespace ConsentManagementProviderLib
{
    public class CMPiOSListenerHelper : MonoBehaviour
    {

#if UNITY_IOS && !UNITY_EDITOR_OSX
    [DllImport("__Internal")]
    private static extern void _setUnityCallback(string gameObjectName);
#endif

        private Action<SpGdprConsent> onCustomConsentsGDPRSuccessAction;

        private void Awake()
        {
            gameObject.name = "CMPiOSListenerHelper";
#if UNITY_IOS && !UNITY_EDITOR_OSX
        CmpDebugUtil.Log("Constructing CMPiOSListenerHelper game object...");
        DontDestroyOnLoad(this.gameObject);
        _setUnityCallback(gameObject.name);
#endif
        }

        internal void SetCustomConsentsGDPRSuccessAction(Action<SpGdprConsent> action)
        {
            this.onCustomConsentsGDPRSuccessAction = action;
        }

        void Callback(string message)
        {
            CmpDebugUtil.Log("IOS_CALLBACK_RECEIVED: " + message);
        }

        void OnConsentReady(string message)
        {
            CmpDebugUtil.Log("OnConsentReady IOS_CALLBACK_RECEIVED: " + message);
            //TODO: Deserialize JSON
            // JsonUtility.FromJson<>(message);
            ConsentMessenger.Broadcast<IOnConsentMessageReady>();
        }

        void OnConsentUIReady(string message)
        {
            CmpDebugUtil.Log("OnConsentUIReady IOS_CALLBACK_RECEIVED: " + message);
            ConsentMessenger.Broadcast<IOnConsentUIReadyEventHandler>(message);
        }

        void OnConsentAction(string message)
        {
            CmpDebugUtil.Log("OnConsentAction IOS_CALLBACK_RECEIVED: " + message);
            CONSENT_ACTION_TYPE actionType = (CONSENT_ACTION_TYPE) Convert.ToInt32(message);
            ConsentMessenger.Broadcast<IOnConsentActionEventHandler>(actionType);
        }

        void OnConsentUIFinished(string message)
        {
            CmpDebugUtil.Log("OnConsentUIFinished IOS_CALLBACK_RECEIVED: " + message);
            ConsentMessenger.Broadcast<IOnConsentUIFinishedEventHandler>();
        }

        void OnErrorCallback(string jsonError)
        {
            CmpDebugUtil.LogError("OnErrorCallback IOS_CALLBACK_RECEIVED: " + jsonError);
            Exception ex = new Exception(jsonError);
            ConsentMessenger.Broadcast<IOnConsentErrorEventHandler>(ex);
        }

        void OnCustomConsentGDPRCallback(string jsonSPGDPRConsent)
        {
            CmpDebugUtil.Log("OnCustomConsentGDPRCallback IOS_CALLBACK_RECEIVED: " + jsonSPGDPRConsent);
            try
            {
                SpGdprConsentWrapper parsed = JsonSerializer.Deserialize<SpGdprConsentWrapper>(jsonSPGDPRConsent);
                if (parsed == null)
                {
                    onCustomConsentsGDPRSuccessAction?.Invoke(null);
                }
                else
                {
                    SpGdprConsent unwrapped = JsonUnwrapper.UnwrapSpGdprConsent(parsed);
                    onCustomConsentsGDPRSuccessAction?.Invoke(unwrapped);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    "Something went wrong while parsing the json data; null will be returned. \n Exception message: " +
                    ex.Message);
                onCustomConsentsGDPRSuccessAction?.Invoke(null);
            }
        }
    }
}
