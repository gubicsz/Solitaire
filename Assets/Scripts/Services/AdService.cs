using UnityEngine.Advertisements;
using Zenject;

namespace Solitaire.Services
{
    public class AdService : IInitializable, IUnityAdsInitializationListener, IAdService
    {
#if UNITY_ANDROID
        string _gameId = "4848282";
        string _placementId = "Banner_Android";
#elif UNITY_IOS
        string _gameId = "4848283";
        string _placementId = "Banner_iOS";
#else
        string _gameId = "unsupported_platform";
        string _placementId = "Banner";
#endif

        public void Initialize()
        {
            if (!Advertisement.isSupported)
            {
                return;
            }

            Advertisement.Initialize(_gameId, true, this);
        }

        public void OnInitializationComplete()
        {
            ShowBanner();
        }

        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
            UnityEngine.Debug.LogError($"{error}: {message}");
        }

        public void ShowBanner()
        {
            if (!Advertisement.isSupported)
            {
                return;
            }

            Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
            Advertisement.Banner.Show(_placementId);
        }

        public void HideBanner()
        {
            if (!Advertisement.isSupported)
            {
                return;
            }

            Advertisement.Banner.Hide();
        }
    }
}
