using System.Collections.ObjectModel;
using Energy.App.Blazor.Client.Services.Api.Interfaces;
using Energy.App.Blazor.Shared;
using Energy.App.Blazor.Shared.Tariffs;

namespace Energy.App.Blazor.Client.StateContainers;

public class UserState
{
    private readonly IUserApi _userApi;
    private readonly IHouseholdApi _householdApi;

    public UserState(IUserApi userApi, IHouseholdApi householdApi)
    {
        _userApi = userApi;
        _householdApi = householdApi;
    }

    public async Task Load()
    {
        var userDetails = await _userApi.EnsureCreated();
        SetUserDetails(userDetails);

        if (UserDetails.HouseholdSetupComplete)
        {
            var householdDetails = await _householdApi.GetHouseholdForUser();
            await SetHouseholdDetails(householdDetails);

        }

    }

    public HouseholdDetails HouseholdDetails { get; private set; }
    public UserDetails UserDetails { get; private set; }

    public bool UserSet => UserDetails != null;
    public bool HouseholdSet => HouseholdDetails != null;

    public void SetUserDetails(UserDetails userDetails)
    {
        UserDetails = userDetails;

        NotifyUserSet?.Invoke(this, userDetails);
    }

    public event EventHandler<UserDetails> NotifyUserSet;

    public async Task SetHouseholdDetails(HouseholdDetails householdDetails)
    {
        HouseholdDetails = householdDetails;
        if (NotifyHouseholdSet != null)
        {
            await NotifyHouseholdSet.Invoke();
        }
    }

    public event Func<Task> NotifyHouseholdSet;



}