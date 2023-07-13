using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Energy.WeatherReadings.Models
{
    public enum WMOCodes
    {
        ClearSky = 0,

        MainlyClear = 1,
        PartlyCloudy = 2,
        Overcast = 3,

        Fog = 45,
        FreezingFog = 48,

        LightDrizzle = 51,
        ModerateDrizzle = 53,
        DenseDrizzle = 55,

        LightFreezingDrizzle = 56,
        DenseFreezingDrizzle = 57,

        SlightRain = 61,
        ModerateRain = 63,
        HeavyRain = 65,

        LightFreezingRain = 66,
        HeavyFreezingRain = 67,

        SlightSnowFall = 71,
        ModerateSnowFall = 73,
        HeavySnowFall = 75,

        SnowGrains = 77,

        SlightRainShowers = 80,
        ModerateRainShowers = 81,
        ViolentRainShowers = 82,

        SlightSnowShowers = 85,
        HeavySnowShowers = 86,

        SlightOrModerateThunderstorm = 95,

        ThunderstormWithSlightHail = 96,
        ThunderstormWithHeavyHail = 99
    }
}
