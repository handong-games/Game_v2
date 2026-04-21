using Game.Core.Define;

namespace Game.Core.Define
{
    public enum EDisplayAspect
    {
        [DisplayName("Auto")]
        Auto = 0,

        [DisplayName("4:3")]
        Ratio4x3 = 1,

        [DisplayName("16:10")]
        Ratio16x10 = 2,
        
        [DisplayName("16:9")]
        Ratio16x9 = 3,
        
        [DisplayName("21:9")]
        Ratio21x9 = 4
    }
}