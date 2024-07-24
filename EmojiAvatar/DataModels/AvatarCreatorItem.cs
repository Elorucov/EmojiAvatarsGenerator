using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;

namespace EmojiAvatar.DataModels {
    public enum RenderMode { InGridViewItem, InCanvas }

    public interface IAvatarCreatorItem {
        FrameworkElement Render(RenderMode mode);
    }

    public class AvatarCreatorItemCollection {
        public string Name { get; private set; }
        public ObservableCollection<IAvatarCreatorItem> Items { get; private set; }

        public AvatarCreatorItemCollection(string name, IEnumerable<IAvatarCreatorItem> items) {
            Name = name;
            Items = new ObservableCollection<IAvatarCreatorItem>(items);
        }
    }
}