using EmojiAvatar.DataModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.UI;

namespace EmojiAvatar {
    public class AvatarCreatorViewModel : INotifyPropertyChanged {
        private bool _isReady = false;
        private ObservableCollection<AvatarCreatorItemCollection> _groupedEmojis;
        private Emoji _selectedEmoji;
        private string _emojiSearchQuery;
        private List<EmojiSkinTone> _emojiSkinTones;
        private EmojiSkinTone _emojiCurrentSkinTone;

        private Color _gradientStartColor;
        private Color _gradientEndColor;
        private GradientDirection _gradientDirection;


        public bool IsReady { get { return _isReady; } private set { _isReady = value; OnPropertyChanged(); } }
        public ObservableCollection<AvatarCreatorItemCollection> GroupedEmojis { get { return _groupedEmojis; } set { _groupedEmojis = value; OnPropertyChanged(); } }
        public Emoji SelectedEmoji { get { return _selectedEmoji; } set { _selectedEmoji = value; OnPropertyChanged(); } }
        public string EmojiSearchQuery { get { return _emojiSearchQuery; } set { _emojiSearchQuery = value; OnPropertyChanged(); } }
        public List<EmojiSkinTone> EmojiSkinTones { get { return _emojiSkinTones; } set { _emojiSkinTones = value; OnPropertyChanged(); } }
        public EmojiSkinTone EmojiCurrentSkinTone { get { return _emojiCurrentSkinTone; } set { _emojiCurrentSkinTone = value; OnPropertyChanged(); } }

        public Color GradientStartColor { get { return _gradientStartColor; } set { _gradientStartColor = value; OnPropertyChanged(); } }
        public Color GradientEndColor { get { return _gradientEndColor; } set { _gradientEndColor = value; OnPropertyChanged(); } }
        public GradientDirection GradientDirection { get { return _gradientDirection; } set { _gradientDirection = value; OnPropertyChanged(); } }
        public List<GradientPreset> GradientPresets { get { return GradientPreset.Presets; } }

        public void Setup() {
            GradientStartColor = Colors.Yellow;
            GradientEndColor = Colors.Blue;

            EmojiSearchAction = new DelayedAction(() => SeachEmojiAndShow(EmojiSearchQuery), TimeSpan.FromSeconds(1));
            LoadEmojiList();

            PropertyChanged += AvatarCreatorViewModel_PropertyChanged;
        }

        private void AvatarCreatorViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(EmojiSearchQuery):
                    EmojiSearchAction.PrepareToExecute();
                    break;
                case nameof(EmojiCurrentSkinTone):
                    UpdateGroupedEmoji();
                    break;
            }
        }

        public void ApplyGradientPreset(GradientPreset preset) {
            GradientStartColor = preset.StartColor;
            GradientEndColor = preset.EndColor;
            GradientDirection = preset.Direction;
        }

        #region All about emoji

        EmojiWebResponse EmojiData;
        DelayedAction EmojiSearchAction;
        List<Emoji> EmojiWithSkinVariations = new List<Emoji>();

        private async void LoadEmojiList() {
            EmojiData = await Emoji.GetEmojisAsync();
            EmojiWithSkinVariations = EmojiData.Emoji.Where(e => e.SkinVariations != null).ToList();

            // Loading spritesheet
            bool isLoaded = await Emoji.LoadSpriteSheetAsync();
            if (isLoaded) {
                // Skin tones
                List<EmojiSkinTone> skinTones = new List<EmojiSkinTone> {
                    new EmojiSkinTone(String.Empty, "default", Color.FromArgb(255, 255, 200, 61))
                };
                EmojiCurrentSkinTone = skinTones.First();

                IEnumerable<Emoji> components = EmojiData.Emoji.Where(emoji => emoji.Category == "component");
                foreach (Emoji component in components) {
                    string name = String.Empty;
                    Color color = Color.FromArgb(0, 0, 0, 0);

                    switch (component.Unified) {
                        case "1F3FB":
                            name = "light";
                            color = Color.FromArgb(255, 249, 224, 192);
                            break;
                        case "1F3FC":
                            name = "medium-light";
                            color = Color.FromArgb(255, 225, 183, 147);
                            break;
                        case "1F3FD":
                            name = "medium";
                            color = Color.FromArgb(255, 197, 149, 105);
                            break;
                        case "1F3FE":
                            name = "medium-dark";
                            color = Color.FromArgb(255, 157, 104, 65);
                            break;
                        case "1F3FF":
                            name = "dark";
                            color = Color.FromArgb(255, 86, 66, 55);
                            break;
                    }

                    var skinToneName = EmojiData.SkinTones.Where(st => st.Key == name).FirstOrDefault();
                    if (!String.IsNullOrEmpty(skinToneName.Message)) name = skinToneName.Message;
                    skinTones.Add(new EmojiSkinTone(component.Unified, name, color));
                }
                EmojiSkinTones = skinTones;
                ShowGroupedEmojiList();
                GenerateRandomAvatar();
                IsReady = true;
            }
        }

        private void ShowGroupedEmojiList() {
            var grouped = EmojiData.Emoji.OrderBy(emoji => emoji.SortOrder)
                //.Where(emoji => emoji.Category != "component")
                .GroupBy(emoji => emoji.Category, (key, items) => {
                    var localized = EmojiData.Groups.Where(g => g.Key == key).FirstOrDefault();
                    if (localized != null) key = localized.Message.ToUpper();
                    return new AvatarCreatorItemCollection(key, items);
                });
            GroupedEmojis = new ObservableCollection<AvatarCreatorItemCollection>(grouped);
        }

        private void UpdateGroupedEmoji() {
            if (GroupedEmojis == null) return;
            foreach (var group in GroupedEmojis) {
                for (int i = 0; i < group.Items.Count; i++) {
                    Emoji emoji = group.Items[i] as Emoji;
                    var emojiWS = EmojiWithSkinVariations.Where(e => emoji.SortOrder == e.SortOrder).FirstOrDefault();
                    if (emojiWS == null) continue;
                    if (!String.IsNullOrEmpty(EmojiCurrentSkinTone.Hex)) {
                        string shex = EmojiCurrentSkinTone.Hex;
                        if (emojiWS.SkinVariations.ContainsKey(shex)) {
                            group.Items[i] = emojiWS.SkinVariations[EmojiCurrentSkinTone.Hex];
                        } else { 
                            foreach (var sve in emojiWS.SkinVariations) {
                                if (sve.Key == $"{shex}-{shex}") {
                                    group.Items[i] = sve.Value;
                                }
                            }
                        }
                    } else {
                        group.Items[i] = emojiWS;
                    }
                }
            }
        }

        private void SeachEmojiAndShow(string query) {
            if (String.IsNullOrEmpty(query)) {
                ShowGroupedEmojiList();
                if (!String.IsNullOrEmpty(EmojiCurrentSkinTone.Hex)) UpdateGroupedEmoji();
                return;
            }

            query = query.ToLower();

            List<Emoji> foundEmoji = new List<Emoji>();
            foreach (Emoji emoji in EmojiData.Emoji) {
                if (!String.IsNullOrEmpty(emoji.Name) && emoji.Name.ToLower().Contains(query)) {
                    foundEmoji.Add(TryGetEmojiWithSkin(emoji, EmojiCurrentSkinTone.Hex));
                    continue;
                } else if (emoji.ShortNames != null) {
                    var found = emoji.ShortNames.Where(s => s.Contains(query)).FirstOrDefault();
                    if (found != null) {
                        foundEmoji.Add(TryGetEmojiWithSkin(emoji, EmojiCurrentSkinTone.Hex));
                        continue;
                    }
                }
            }

            GroupedEmojis = new ObservableCollection<AvatarCreatorItemCollection>() { 
                new AvatarCreatorItemCollection("FOUND", foundEmoji)
            };
        }

        private Emoji TryGetEmojiWithSkin(Emoji emoji, string skinHex) {
            if (!String.IsNullOrEmpty(skinHex) && emoji.SkinVariations != null && emoji.SkinVariations.ContainsKey(skinHex)) 
                return emoji.SkinVariations[skinHex];
            return emoji;
        }

        #endregion

        public void GenerateRandomAvatar() {
            int seed = (int)(DateTime.Now - new DateTime(2022, 05, 25)).TotalSeconds;

            int gradientPresetIndex = new Random(seed).Next(0, GradientPresets.Count - 1);
            ApplyGradientPreset(GradientPresets[gradientPresetIndex]);

            int emojiIndex = new Random(seed).Next(0, EmojiData.Emoji.Count - 1);
            SelectedEmoji = TryGetEmojiWithSkin(EmojiData.Emoji[emojiIndex], EmojiCurrentSkinTone.Hex);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
