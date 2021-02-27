using System;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using Android.Content;
using Ensure.Domain.Models;
using Newtonsoft.Json;

namespace Ensure.AndroidApp.Data
{
    public class UserService : BaseService
    {

        const string SharedPrefernceName = "EnsureSp";
        const string UserInfoSharedPreference = "UserInfo";

        private ISharedPreferences sharedPreferences;
        private EnsureApplication application;

        public UserService(Context context) : base(context)
        {
            application = (EnsureApplication)context.ApplicationContext;
            sharedPreferences = context.GetSharedPreferences(SharedPrefernceName, FileCreationMode.Private);
        }

        /// <summary>
        /// Tries to log user in and returns whether succeeded.
        /// </summary>
        /// <param name="userName">The username</param>
        /// <param name="password">The password</param>
        /// <returns>Whether logged in</returns>
        public async Task<ApiResponse> LogUserIn(string userName, string password)
        {
            using (var msg = await http.GetAsync($"/api/Account/login?username={userName}&password={password}"))
            {
                var res = JsonConvert.DeserializeObject<ApiResponse<ApiUserInfo>>(await msg.Content.ReadAsStringAsync());
                if (!res.IsError)
                {
                    application.UserInfo = res.Response;
                    SaveUserInfoToSp();
                }
                return res;
            }
        }

        public async Task<bool> RegiserUser(string userName, string email, string password,
            string pwdVertification, short target)
        {
            var model = new ApiSignupModel
            {
                Password = password,
                PasswordVertification = pwdVertification,
                UserName = userName,
                Email = email,
                DailyTarget = target
            };

            string data = JsonConvert.SerializeObject(model);
            var content = new StringContent(data, Encoding.UTF8, "application/json");
            var res = await http.PostAsync("/api/Account/Register", content);

            if (!res.IsSuccessStatusCode) return false;

            var info = JsonConvert.DeserializeObject<ApiUserInfo>(await res.Content.ReadAsStringAsync());
            application.UserInfo = info;
            SaveUserInfoToSp();
            return true;
        }

        public Task RequestPasswordResetEmail(string email)
        {
            return http.GetAsync($"/api/Account/ResetPassword?email={email}");
        }

        /// <summary>
        /// Loads the user info that is stored in the SharedPreferences
        /// </summary>
        public void LoadUserInfoFromSp()
        {
            var userInfoJson = sharedPreferences.GetString(UserInfoSharedPreference, String.Empty);
            if (!string.IsNullOrEmpty(userInfoJson))
            {
                application.UserInfo = JsonConvert.DeserializeObject<ApiUserInfo>(userInfoJson);
            }
        }

        /// <summary>
        /// Logs out the current user
        /// </summary>
        public void LogUserOut()
        {
            application.UserInfo = null;
            SaveUserInfoToSp();
        }

        /// <summary>
        /// Saves the current user info to the SharedPreferences
        /// </summary>
        public void SaveUserInfoToSp()
        {
            string json = JsonConvert.SerializeObject(application.UserInfo);
            sharedPreferences.Edit().PutString(UserInfoSharedPreference, json).Commit();
        }

        /// <summary>
        /// Refreshes the user's info and returns it.
        /// </summary>
        /// <returns>The user's updates info</returns>
        public async Task<ApiUserInfo> RefreshInfo()
        {
            if (!IsInternetConnectionAvailable()) return null; // can't refresh!
            var res = await http.GetAsync("/api/Account/GetInfo");
            if (!res.IsSuccessStatusCode)
            {
                HandleHttpError(res);
            }
            var info = JsonConvert.DeserializeObject<ApiUserInfo>(await res.Content.ReadAsStringAsync());
            info.JwtToken = application.UserInfo.JwtToken;
            application.UserInfo = info;
            SaveUserInfoToSp(); // save updated info locally
            return info;
        }

        public bool IsUserLoggedIn => UserInfo != null;
        public ApiUserInfo UserInfo => application.UserInfo;

        [Obsolete]
        /// <summary>
        /// Sets and syncs the user's daily target
        /// </summary>
        /// <param name="target">The new target</param>
        /// <returns>Whether update succeeded</returns>
        public async Task<bool> SetUserTarget(short target)
        {
            if (IsUserLoggedIn && IsInternetConnectionAvailable())
            {
                var res = await http.PostAsync($"/api/Account/SetTarget?target={target}");
                if (!res.IsSuccessStatusCode)
                {
                    HandleHttpError(res);
                    return false;
                }
                UserInfo.DailyTarget = target;
                SaveUserInfoToSp(); // save updates locally
                return true;
            }
            return false;
        }
    }
}
