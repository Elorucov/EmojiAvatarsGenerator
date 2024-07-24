using System.Collections.Generic;
using Windows.UI;

namespace EmojiAvatar.DataModels {
    public enum GradientDirection {
        TopLeftToBottomRight, TopToBottom, TopRightToBottomLeft
    }

    public class GradientPreset {
        public Color StartColor { get; private set; }
        public Color EndColor { get; private set; }
        public GradientDirection Direction { get; set; }

        private GradientPreset(Color start, Color end, GradientDirection direction = GradientDirection.TopLeftToBottomRight) {
            StartColor = start;
            EndColor = end;
            Direction = direction;
        }

        public static List<GradientPreset> Presets = new List<GradientPreset> {
            new GradientPreset(Color.FromArgb(255,118,185,255), Color.FromArgb(255,118,185,255)),
            new GradientPreset(Color.FromArgb(255,132,222,134), Color.FromArgb(255,134,227,139)),
            new GradientPreset(Color.FromArgb(255,255,216,74), Color.FromArgb(255,118,243,170)),
            new GradientPreset(Color.FromArgb(255,255,212,72), Color.FromArgb(255,255,160,33), GradientDirection.TopToBottom),
            new GradientPreset(Color.FromArgb(255,255,128,183), Color.FromArgb(255,255,128,163), GradientDirection.TopToBottom),
            new GradientPreset(Color.FromArgb(255,255,209,76), Color.FromArgb(255,231,69,120)),
            new GradientPreset(Color.FromArgb(255,254,76,137), Color.FromArgb(255,52,152,255)),
            new GradientPreset(Color.FromArgb(255,87,88,110), Color.FromArgb(255,60,62,89), GradientDirection.TopToBottom),
            new GradientPreset(Color.FromArgb(255,168,151,244), Color.FromArgb(255,250,186,216)),
            new GradientPreset(Color.FromArgb(255,235,231,255), Color.FromArgb(255,189,174,250), GradientDirection.TopToBottom),
            new GradientPreset(Color.FromArgb(255,235,245,253), Color.FromArgb(255,235,245,253))
        };
    }
}
