using System;

namespace Gemelo.Components.Cts.Code.Data.Users
{
    [Flags]
    public enum CtsUserState
    {
        None = 0x0000,
        SurveyCompleted = 0x0001,
        DemographySkipped = 0x0002
    }
}
