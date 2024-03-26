using UnityEngine;
using UnityEngine.Advertisements;
using Zenject;

namespace Solitaire.Services
{
    public class AdService : IInitializable, IUnityAdsInitializationListener, IAdService
    {
#if UNITY_ANDROID
        private const string GameId = "4848282";
        private const string PlacementId = "Banner_Android";
#elif UNITY_IOS
        private const string GameId = "4848283";
        private const string PlacementId = "Banner_iOS";
#else
        private const string GameId = "unsupported_platform";
        private const string PlacementId = "Banner";
#endif

        public void Initialize()
        {
            if (!Advertisement.isSupported)
                return;

            Advertisement.Initialize(GameId, true, this);
        }

        public void OnInitializationComplete()
        {
            ShowBanner();
        }

        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
            Debug.LogError($"{error}: {message}");
        }

        public void ShowBanner()
        {
            if (!Advertisement.isSupported)
                return;

            Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
            Advertisement.Banner.Show(PlacementId);
        }

        public void HideBanner()
        {
            if (!Advertisement.isSupported)
                return;

            Advertisement.Banner.Hide();
        }
    }
}
