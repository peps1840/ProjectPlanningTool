﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using TeamBins.Common.ViewModels;
using TeamBins.Services;
using TeamBins6.Infrastrucutre.Services;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace TeamBins6.Controllers
{
    [Route("api/issue")]
    public class IssueApiController : Controller
    {

        ICommentManager commentManager;
        private readonly IProjectManager projectManager;
        private IIssueManager issueManager;
        //private IssueService issueService;
        IUserSessionHelper userSessionHelper;


        public IssueApiController(ICommentManager commentManager, IUserSessionHelper userSessionHelper, IProjectManager projectManager, IIssueManager issueManager) //: base(repositary)
        {
            this.issueManager = issueManager;
            this.projectManager = projectManager;
            this.userSessionHelper = userSessionHelper;
            this.commentManager = commentManager;
        }
      


        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ObjectResult Get(int id)
        {

            var issues= this.issueManager.GetIssuesGroupedByStatusGroup(25);

            return Ok(issues);
        }


        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}