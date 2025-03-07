﻿using System;
using ConsentManagementProviderLib.EventHandlerInterface;
using ConsentManagementProviderLib.Observer;
using UnityEngine;

namespace ConsentManagementProviderLib
{
    public static class ConsentMessenger
    {
        public static void AddListener<T>(GameObject go) where T : IConsentEventHandler
        {
            BroadcastReceivers.RegisterBroadcastReceiver<T>(go);
        }

        public static void RemoveListener<T>(GameObject go) where T : IConsentEventHandler
        {
            BroadcastReceivers.UnregisterBroadcastReceiver<T>(go);
        }

        public static void Broadcast<T>(params object[] list) where T : IConsentEventHandler
        {
            CmpDebugUtil.LogWarning("T == " + typeof(T).Name);
            switch (typeof(T).Name)
            {
                case nameof(IOnConsentReady):
                    SpConsents consents = (SpConsents)list[0];
                    BroadcastEventDispatcher.Execute<IOnConsentReady>(null, (i, d) => i.OnConsentReady(consents));
                    break;
                case nameof(IOnConsentAction):
                    SpAction actionType = (SpAction)list[0];
                    BroadcastEventDispatcher.Execute<IOnConsentAction>(null, (i, d) => i.OnConsentAction(actionType));
                    break;
                case nameof(IOnConsentError):
                    Exception exception= (Exception)list[0];
                    BroadcastEventDispatcher.Execute<IOnConsentError>(null, (i, d) => i.OnConsentError(exception));
                    break;
                case nameof(IOnConsentUIReady):
                    BroadcastEventDispatcher.Execute<IOnConsentUIReady>(null, (i,d) => i.OnConsentUIReady());
                    break;
                case nameof(IOnConsentUIFinished):
                    BroadcastEventDispatcher.Execute<IOnConsentUIFinished>(null, (i,d) => i.OnConsentUIFinished());
                    break;
                case nameof(IOnConsentSpFinished):
                    SpConsents spConsents = (SpConsents)list[0];
                    BroadcastEventDispatcher.Execute<IOnConsentSpFinished>(null, (i,d) => i.OnConsentSpFinished(spConsents));
                    break;
            }
        }
    }
}