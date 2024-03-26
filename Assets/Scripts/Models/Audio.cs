using System;
using System.Collections.Generic;
using UnityEngine;

namespace Solitaire.Models
{
    public static class Audio
    {
        public const string Music = "Music";
        public const string SfxShuffle = "Shuffle";
        public const string SfxDeal = "Deal";
        public const string SfxDraw = "Draw";
        public const string SfxHint = "Hint";
        public const string SfxClick = "Click";
        public const string SfxError = "Error";

        [Serializable]
        public class Config
        {
            public List<AudioClip> AudioClips;
        }
    }
}