using Gemelo.Components.Cts.Code.Data.Users;

namespace Gemelo.Components.Cts.Code.Communication
{
    public class ChangeUserStateRequest
    {
        public int CtsUserID { get; set; }

        public CtsUserState AddStates { get; set; } = CtsUserState.None;

        public CtsUserState RemoveStates { get; set; } = CtsUserState.None;
    }
}
