namespace Solitaire.Services
{
    public interface IAudioService
    {
        void PlayMusic(string key, float volume);
        void PlaySfx(string key, float volume);
        void SetVolume(float volume);
        void StopMusic();
    }
}
