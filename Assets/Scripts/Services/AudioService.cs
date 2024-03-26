using System.Collections.Generic;
using DG.Tweening;
using Solitaire.Models;
using UnityEngine;

namespace Solitaire.Services
{
    public class AudioService : IAudioService
    {
        private const float FadeDuration = 2.0f;
        
        private readonly Dictionary<string, AudioClip> _audioMap;
        private Transform _camTransform;
        private AudioSource _music;
        private Tweener _tweenFadeIn;
        private Tweener _tweenFadeOut;
        private float _volumeMusic = 1f;
        private float _volumeSfx = 1f;

        public AudioService(Audio.Config audioConfig)
        {
            _audioMap = new Dictionary<string, AudioClip>(audioConfig.AudioClips.Count);

            for (var i = 0; i < audioConfig.AudioClips.Count; i++)
            {
                var clip = audioConfig.AudioClips[i];
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
            if (!_audioMap.TryGetValue(key, out var clip))
            {
                Debug.LogWarning($"Couldn't find '{key}' AudioClip.");
                return;
            }

            // Cache camera transform
            if (_camTransform == null)
                _camTransform = Camera.main.transform;

            var vol = _volumeSfx * volume;

            if (vol <= 0f)
                return;

            // This is just a quick and dirty solution. It is very bad for performance
            // because this call creates and destroys an AudioSource each time.
            // A more professional way would be to use Audio Mixer, buses and pooling.
            AudioSource.PlayClipAtPoint(clip, _camTransform.position, vol);
        }

        public void PlayMusic(string key, float volume)
        {
            // Try to get clip based on key
            if (!_audioMap.TryGetValue(key, out var clip))
            {
                Debug.LogWarning($"Couldn't find '{key}' AudioClip.");
                return;
            }

            var vol = _volumeMusic * volume;

            if (vol <= 0f)
                return;

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
                _tweenFadeIn = _music
                    .DOFade(vol, FadeDuration)
                    .SetEase(Ease.InQuad)
                    .SetAutoKill(false)
                    .OnRewind(() => _music.Play());
            else
                _tweenFadeIn.Restart();
        }

        public void StopMusic()
        {
            // Handle error
            if (_music == null)
                return;

            // Stop fade in
            _tweenFadeIn?.Pause();

            // Fade out music
            if (_tweenFadeOut == null)
                _tweenFadeOut = _music
                    .DOFade(0f, FadeDuration)
                    .SetEase(Ease.OutQuad)
                    .SetAutoKill(false)
                    .OnComplete(() => _music.Stop());
            else
                _tweenFadeOut.Restart();
        }
    }
}
