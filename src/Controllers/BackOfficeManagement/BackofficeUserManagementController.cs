using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Rsp.QuestionSetService.Controllers.BackOfficeManagement
{
    [ApiController]
    [Route("/umbraco/api/usermanagement")]
    public class BackofficeUserManagementController(
        IUserService userService,
        IUserGroupService userGroupService) : ControllerBase
    {
        [HttpPost("createUser")]
        public async Task<IActionResult> CreateUser(string email, string firstName, string lastName, string userGroup)
        {
            var adminUser = userService.GetUserById(-1);

            var userGroups = await userGroupService.GetAllAsync(0, 100);
            var editorGroup = userGroups.Items.Where(x => x.Name == "Editors").Select(x => x.Key).ToHashSet();

            var userModel = new UserCreateModel
            {
                Email = email,
                Kind = Umbraco.Cms.Core.Models.Membership.UserKind.Api,
                Name = firstName + " " + lastName,
                UserName = email,
                UserGroupKeys = editorGroup
            };

            var createdUser = await userService.CreateAsync(adminUser.Key, userModel, true);

            if (createdUser.Status == Umbraco.Cms.Core.Services.OperationStatus.UserOperationStatus.Success)
            {
                return Ok(createdUser.Result);
            }

            return BadRequest(createdUser);
        }

        [HttpPost("deleteUser")]
        public async Task<IActionResult> DeleteUser(string email)
        {
            var adminUser = userService.GetUserById(-1);
            var user = userService.GetByEmail(email);

            var deleteduser = await userService.DeleteAsync(adminUser.Key, user.Key);

            return Ok(deleteduser);
        }
    }
}