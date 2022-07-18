using UniRx;

namespace Solitaire.Models
{
    public class Options
    {
        public BoolReactiveProperty DrawThree { get; set; } = new BoolReactiveProperty(false);
        public BoolReactiveProperty SoundsEnabled { get; set; } = new BoolReactiveProperty(true);
    }
}
