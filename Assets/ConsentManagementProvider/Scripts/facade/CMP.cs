﻿using System;
using System.Collections.Generic;
using System.Linq;
using ConsentManagementProviderLib.Android;
using ConsentManagementProviderLib.iOS;
using ConsentManagementProviderLib.Observer;
using UnityEngine;

namespace ConsentManagementProviderLib
{
    public static class CMP
    {
        private static GameObject mainThreadBroadcastEventsExecutor;
        private static bool IsEditor => Application.platform == RuntimePlatform.LinuxEditor
                                        || Application.platform == RuntimePlatform.WindowsEditor
                                        || Application.platform == RuntimePlatform.OSXEditor;
        
        public static void Initialize(
            List<SpCampaign> spCampaigns, 
            int accountId,
            int propertyId,
            string propertyName, 
            MESSAGE_LANGUAGE language, 
            CAMPAIGN_ENV campaignsEnvironment,
            long messageTimeoutInSeconds = 3)
        {
            if(!IsSpCampaignsValid(spCampaigns))
            { 
                return;
            }
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                CreateBroadcastExecutorGO();
                //excluding ios14 campaign if any
                RemoveIos14SpCampaign(ref spCampaigns);
                if (!IsSpCampaignsValid(spCampaigns))
                {
                    return;
                }
                ConsentWrapperAndroid.Instance.InitializeLib(spCampaigns: spCampaigns,
                                                            accountId: accountId,
                                                            propertyId: propertyId,
                                                            propertyName: propertyName,
                                                            language: language,
                                                            campaignsEnvironment: campaignsEnvironment,
                                                            messageTimeoutMilliSeconds: messageTimeoutInSeconds * 1000);
            }
#elif UNITY_IOS && !UNITY_EDITOR_OSX
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                CreateBroadcastExecutorGO();
                ConsentWrapperIOS.Instance.InitializeLib(spCampaigns: spCampaigns,
                                                        accountId: accountId,
                                                        propertyId: propertyId,
                                                        propertyName: propertyName,
                                                        language: language,
                                                        campaignsEnvironment: campaignsEnvironment,
                                                        messageTimeoutInSeconds: messageTimeoutInSeconds);
            }
#endif
        }
        
        public static void LoadMessage(string authId = null)
        {
            if (IsEditor)
            {
                Debug.LogWarning("Emulating LoadMessage call... Sourcepoint CMP works only for real Android/iOS devices, not the Unity Editor.");
                return;
            }
            
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                ConsentWrapperAndroid.Instance.LoadMessage(authId: authId);
            }
#elif UNITY_IOS && !UNITY_EDITOR_OSX
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                ConsentWrapperIOS.Instance.LoadMessage(authId: authId);
            }
#endif
        }

        public static void LoadPrivacyManager(CAMPAIGN_TYPE campaignType, string pmId, PRIVACY_MANAGER_TAB tab)
        {
            if (IsEditor)
            {
                Debug.LogWarning($"Emulating LoadPrivacyManager call for {campaignType}... " +
                                 $"Sourcepoint CMP works only for real Android/iOS devices, not the Unity Editor.");
                return;
            }
            
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                ConsentWrapperAndroid.Instance.LoadPrivacyManager(campaignType: campaignType,
                                                                 pmId: pmId,
                                                                 tab: tab);
            }
#elif UNITY_IOS && !UNITY_EDITOR_OSX
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            { 
                if(campaignType == CAMPAIGN_TYPE.GDPR)
                {
                    ConsentWrapperIOS.Instance.LoadGDPRPrivacyManager(pmId: pmId, 
                                                                      tab: tab);
                }
                else if(campaignType == CAMPAIGN_TYPE.CCPA)
                {
                    ConsentWrapperIOS.Instance.LoadCCPAPrivacyManager(pmId: pmId,
                                                                      tab: tab);
                }
            }
#endif
        }

        public static void CustomConsentGDPR(string[] vendors, string[] categories, string[] legIntCategories, Action<GdprConsent> onSuccessDelegate)
        {
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                ConsentWrapperAndroid.Instance.CustomConsentGDPR(vendors: vendors,
                                                                categories: categories,
                                                                legIntCategories: legIntCategories,
                                                                onSuccessDelegate: onSuccessDelegate);
            }
#elif UNITY_IOS && !UNITY_EDITOR_OSX
            if(Application.platform == RuntimePlatform.IPhonePlayer)
            {
                ConsentWrapperIOS.Instance.CustomConsentGDPR(vendors: vendors,
                                                           categories: categories,
                                                           legIntCategories: legIntCategories,
                                                           onSuccessDelegate: onSuccessDelegate);
            }
#endif
        }

        public static SpConsents GetSpConsents()
        {
            SpConsents result = null;
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                result = ConsentWrapperAndroid.Instance.GetSpConsents();
            }
#elif UNITY_IOS && !UNITY_EDITOR_OSX
            if(Application.platform == RuntimePlatform.IPhonePlayer)
            {
                result = ConsentWrapperIOS.Instance.GetSpConsents();
            }
#endif
            return result;
        }

        public static GdprConsent GetCustomConsent()
        {
            GdprConsent result = null;
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                result = ConsentWrapperAndroid.Instance.GetCustomGdprConsent();
            }
#elif UNITY_IOS && !UNITY_EDITOR_OSX
            if(Application.platform == RuntimePlatform.IPhonePlayer)
            {
                result = ConsentWrapperIOS.Instance.GetCustomGdprConsent();
            }
#endif
            return result;
        }

        public static void Dispose()
        {
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                ConsentWrapperAndroid.Instance.Dispose();
            }
#elif UNITY_IOS && !UNITY_EDITOR_OSX
            if(Application.platform == RuntimePlatform.IPhonePlayer)
            {
                ConsentWrapperIOS.Instance.Dispose();
            }
#endif
        }
        
        #region private methods
        private static void CreateBroadcastExecutorGO()
        {
            if (mainThreadBroadcastEventsExecutor != null) return;
            mainThreadBroadcastEventsExecutor = new GameObject();
            mainThreadBroadcastEventsExecutor.AddComponent<BroadcastEventsExecutor>();
        }

        private static void RemoveIos14SpCampaign(ref List<SpCampaign> spCampaigns)
        {
            List<SpCampaign> ios14 = spCampaigns.Where(campaign => campaign.CampaignType == CAMPAIGN_TYPE.IOS14).ToList();
            if (ios14 != null && ios14.Count > 0)
            {
                Debug.LogWarning("ios14 campaign is not allowed in non-ios device! Skipping it...");
                foreach (SpCampaign ios in ios14)
                {
                    spCampaigns.Remove(ios);
                }
            }
        }

        private static bool IsSpCampaignsValid(List<SpCampaign> spCampaigns)
        {
            //check if there more than 0 campaign
            if (spCampaigns.Count == 0)
            {
                Debug.LogError("You should add at least one SpCampaign to use CMP! Aborting...");
                return false;
            }
            if (IsEditor)
            {
                Debug.LogWarning("ATTENTION! Sourcepoint CMP works only for real Android/iOS devices, not the Unity Editor.");
                return false;
            }
            
            return true;
        }
        #endregion
    }
}