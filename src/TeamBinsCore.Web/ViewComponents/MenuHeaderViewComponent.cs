﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeamBins.Common;
using TeamBins.Services;
using TeamBins6.Infrastrucutre.Extensions;
using TeamBins6.Infrastrucutre.Services;
using Microsoft.AspNetCore.Mvc;
using TeamBins.Common.ViewModels;

namespace TeamBins6.ViewComponents
{
    public class MenuHeaderViewComponent : ViewComponent
    {
        
        private readonly IUserAuthHelper userSessionHelper;
        private readonly IUserAccountManager userAccountManager;
        public MenuHeaderViewComponent(IUserAuthHelper userSessionHelper, IUserAccountManager userAccountManager)
        {
            this.userSessionHelper = userSessionHelper;
            this.userAccountManager = userAccountManager;
        }
       
        public async Task<IViewComponentResult> InvokeAsync(int id)
        {
            var vm = new UserMenuHeaderVM();

            if (userSessionHelper.UserId > 0)
            {
                var u = await userAccountManager.GetUser(userSessionHelper.UserId);
                vm.UserDisplayName = u.Name;
                vm.UserEmailAddress = u.EmailAddress;

                var teams = await userAccountManager.GetTeams(userSessionHelper.UserId);
                vm.Teams = teams.Select(x => new TeamDto {Id = x.Id, Name = x.Name}).ToList();
                if (u.DefaultTeamId != null)
                {
                    if (teams.Any(g => g.Id == u.DefaultTeamId))
                    {
                        vm.CurrentTeamName = teams.FirstOrDefault(g => g.Id == u.DefaultTeamId.Value).Name;
                    }
                    else
                    {
                        vm.CurrentTeamName = teams.FirstOrDefault().Name;
                    }
                }
                else
                {
                    vm.CurrentTeamName = teams.FirstOrDefault().Name;
                }
            }
            return View(vm);
        }
    }
}
