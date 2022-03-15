using GameDev.UI.RTS;

namespace GameDev.RTS
{
    public interface ISelectable
    {
        void OnSelect(Selector selector);

        void OnDeselect(Selector selector);

        void OnFocus(RtsUI ui);
    }
}
