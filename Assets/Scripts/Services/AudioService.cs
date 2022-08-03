using DG.Tweening;
using Solitaire.Models;
using System.Collections.Generic;
using UnityEngine;

namespace Solitaire.Services
{
    public class AudioService : IAudioService
    {
        readonly Dictionary<string, AudioClip> _audioMap;
        readonly Audio.Config _audioConfig;
        Transform _camTransform;
        AudioSource _music;
        Tweener _tweenFadeIn;
        Tweener _tweenFadeOut;

        float _volumeSfx = 1f;
        float _volumeMusic = 1f;

        const float FadeDuration = 2.0f;

        public AudioService(Audio.Config audioConfig)
        {
            _audioConfig = audioConfig;
            _audioMap = new Dictionary<string, AudioClip>(_audioConfig.AudioClips.Count);

            for (int i = 0; i < _audioConfig.AudioClips.Count; i++)
            {
                AudioClip clip = _audioConfig.AudioClips[i];
                _audioMap.Add(clip.name, clip);
            }
        }

        public void SetVolume(float volume)
        {
            _volumeSfx = volume;
            _volumeMusic = volume;
        }

        public void PlaySfx(string key, float volume)
        {
            // Try to get clip based on key
            if (!_audioMap.TryGetValue(key, out AudioClip clip))
            {
                Debug.LogWarning($"Couldn't find '{key}' AudioClip.");
                return;
            }

            // Cache camera transform
            if (_camTransform == null)
            {
                _camTransform = Camera.main.transform;
            }

            float vol = _volumeSfx * volume;

            if (vol <= 0f)
            {
                return;
            }

            // This is just a quick and dirty solution. It is very bad for performance
            // because this call creates and destroys an AudioSource each time.
            // A more professional way would be to use Audio Mixer, buses and pooling.
            AudioSource.PlayClipAtPoint(clip, _camTransform.position, vol);
        }

        public void PlayMusic(string key, float volume)
        {
            // Try to get clip based on key
            if (!_audioMap.TryGetValue(key, out AudioClip clip))
            {
                Debug.LogWarning($"Couldn't find '{key}' AudioClip.");
                return;
            }

            float vol = _volumeMusic * volume;

            if (vol <= 0f)
            {
                return;
            }

            // Create audio source if for the first time
            if (_music == null)
            {
                var go = new GameObject("Music");
                _music = go.AddComponent<AudioSource>();
                _music.spatialBlend = 0;
                _music.volume = 0;
                _music.loop = true;
                _music.volume = 0f;
                _music.clip = clip;
                _music.Play();
            }
            else
            {
                _music.clip = clip;
            }

            // Stop fade out
            _tweenFadeOut?.Pause();

            // Fade in music
            if (_tweenFadeIn == null)
            {
                _tweenFadeIn = _music.DOFade(vol, FadeDuration)
                    .SetEase(Ease.InQuad)
                    .SetAutoKill(false)
                    .OnRewind(() => _music.Play());
            }
            else
            {
                _tweenFadeIn.Restart();
            }
        }

        public void StopMusic()
        {
            // Handle error
            if (_music == null)
            {
                return;
            }

            // Stop fade in
            _tweenFadeIn?.Pause();

            // Fade out music
            if (_tweenFadeOut == null)
            {
                _tweenFadeOut = _music.DOFade(0f, FadeDuration)
                    .SetEase(Ease.OutQuad)
                    .SetAutoKill(false)
                    .OnComplete(() => _music.Stop());
            }
            else
            {
                _tweenFadeOut.Restart();
            }
        }
    }
}
