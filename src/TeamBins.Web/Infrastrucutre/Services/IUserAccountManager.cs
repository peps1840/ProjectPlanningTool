using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

using TeamBins.Common;
using TeamBins.Common.ViewModels;
using TeamBins.Infrastrucutre.Services;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using TeamBins.DataAccessCore;

using TeamBins.DataAccess;

namespace TeamBins.Services
{

    public interface IUserAccountManager
    {
        Task SaveNotificationSettings(UserEmailNotificationSettingsVM model);
        Task<DefaultIssueSettings> GetIssueSettingsForUser();
        Task<EditProfileVm> GetUserProfile();
        Task SetDefaultTeam(int userId, int teamId);
        Task<IEnumerable<TeamDto>> GetTeams(int userId);
        Task<UserAccountDto> GetUser(int id);
        //bool DoesAccountExist(string email);
        //LoggedInSessionInfo CreateUserAccount(UserAccountDto userAccount);

        //UserAccountDto GetUser(string email);
        //UserAccountDto GetUser(int id);

        //Task SaveLastLoginAsync(int userId);

        //ResetPaswordRequestDto ProcessPasswordRecovery(string email);

        //ResetPaswordRequestDto GetResetPaswordRequest(string id);

        //bool ResetPassword(string resetPasswordLink, string password);

        //// ResetPasswordVM 
        //EditProfileVm GetUserProfile();
        //UserEmailNotificationSettingsVM GetNotificationSettings();
        //DefaultIssueSettings GetIssueSettingsForUser();

        Task UpdateProfile(EditProfileVm model);
        //void UpdatePassword(ChangePasswordVM model);
        Task SaveDefaultProjectForTeam(DefaultIssueSettings defaultIssueSettings);
        Task<UserAccountDto> GetUser(string email);
        Task<LoggedInSessionInfo> CreateAccount(UserAccountDto userAccount);
        Task<UserEmailNotificationSettingsVM> GetNotificationSettings();
        Task UpdateLastLoginTime(int userId);
    }

    public class UserAccountManager : IUserAccountManager
    {
        private IEmailManager emailManager;
        private IProjectManager projectManager;
        private IUserRepository userRepository;
        private IUserAuthHelper userSessionHelper;
        private ITeamRepository teamRepository;
        public UserAccountManager(IUserRepository userRepository, IUserAuthHelper userSessionHelper, IProjectManager projectManager,ITeamRepository teamRepository,IEmailManager emailManager)
        {
            this.emailManager = emailManager;
            this.userRepository = userRepository;
            this.userSessionHelper = userSessionHelper;
            this.projectManager = projectManager;
            this.teamRepository = teamRepository;
        }

        public async Task SaveDefaultProjectForTeam(DefaultIssueSettings defaultIssueSettings)
        {
            defaultIssueSettings.TeamId = this.userSessionHelper.TeamId;
            defaultIssueSettings.UserId = this.userSessionHelper.UserId;
             await this.userRepository.SaveDefaultIssueSettings(defaultIssueSettings);
        }
        public async Task<EditProfileVm> GetUserProfile()
        {
            var vm = new EditProfileVm();
            var user = await this.userRepository.GetUser(this.userSessionHelper.UserId);
            if (user != null)
            {
                vm.Name = user.Name;
                vm.Email = user.EmailAddress;
            }
            return vm;
        }

      

        public async Task<DefaultIssueSettings> GetIssueSettingsForUser()
        {
            var vm = new DefaultIssueSettings();
            vm.Projects=this.projectManager.GetProjects(this.userSessionHelper.TeamId)
                    .Select(s => new SelectListItem {Value = s.Id.ToString(), Text = s.Name})
                    .ToList();


            var tm = this.teamRepository.GetTeamMember(this.userSessionHelper.TeamId, this.userSessionHelper.UserId);
            vm.SelectedProject = tm.DefaultProjectId;
            return await Task.FromResult(vm);
        }

        public async Task UpdateLastLoginTime(int userId)
        {
            await this.userRepository.UpdateLastLoginTime(userId);
        }
        public  async Task SetDefaultTeam(int userId, int teamId)
        {
            await this.userRepository.SetDefaultTeam(userId, teamId);
        }
        public async Task<UserAccountDto> GetUser(int id)
        {
            return await this.userRepository.GetUser(id);
        }
        public async Task<UserAccountDto> GetUser(string email)
        {
            return await this.userRepository.GetUser(email);
        }
        public async Task<IEnumerable<TeamDto>> GetTeams(int userId)
        {
            return await userRepository.GetTeams(userId);

        }

        public async Task<LoggedInSessionInfo> CreateAccount(UserAccountDto userAccount)
        {
            var userId = await userRepository.CreateAccount(userAccount);
            var teamName = userAccount.Name.Replace(" ", "") + " Team";
            var teamId = await Task.FromResult(teamRepository.SaveTeam(new TeamDto {CreatedById = userId, Name = teamName }));
            await this.userRepository.SetDefaultTeam(userId, teamId);

            await this.emailManager.SendAccountCreatedEmail(new UserDto
            {
                Name = userAccount.Name,
                EmailAddress = userAccount.EmailAddress
            });

            return new LoggedInSessionInfo() {TeamId = teamId, UserId = userId, UserDisplayName = userAccount.Name};
        }
        public async Task UpdateProfile(EditProfileVm model)
        {
            model.Id = this.userSessionHelper.UserId;
            await userRepository.SaveUserProfile(model);
        }

        public async Task<UserEmailNotificationSettingsVM> GetNotificationSettings()
        {
            return  new UserEmailNotificationSettingsVM
            {
                TeamId = userSessionHelper.TeamId,
                EmailSubscriptions = await userRepository.EmailSubscriptions(userSessionHelper.UserId,userSessionHelper.TeamId)
            };
        }
        public async Task SaveNotificationSettings(UserEmailNotificationSettingsVM model)
        {
            model.UserId = userSessionHelper.UserId;
            model.TeamId = userSessionHelper.TeamId;
            await userRepository.SaveNotificationSettings(model);

        }
    }

}