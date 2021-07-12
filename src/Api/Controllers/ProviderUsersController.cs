﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Bit.Core.Repositories;
using Microsoft.AspNetCore.Authorization;
using Bit.Core.Models.Api;
using Bit.Core.Exceptions;
using Bit.Core.Services;
using Bit.Core.Context;
using Bit.Core.Models.Business.Provider;

namespace Bit.Api.Controllers
{
    [Route("providers/{providerId:guid}/users")]
    [Authorize("Application")]
    public class ProviderUsersController : Controller
    {
        private readonly IProviderUserRepository _providerUserRepository;
        private readonly IProviderService _providerService;
        private readonly IUserService _userService;
        private readonly ICurrentContext _currentContext;

        public ProviderUsersController(
            IProviderUserRepository providerUserRepository,
            IProviderService providerService,
            IUserService userService,
            ICurrentContext currentContext)
        {
            _providerUserRepository = providerUserRepository;
            _providerService = providerService;
            _userService = userService;
            _currentContext = currentContext;
        }

        [HttpGet("{id:guid}")]
        public async Task<ProviderUserResponseModel> Get(Guid providerId, Guid id)
        {
            var providerUser = await _providerUserRepository.GetByIdAsync(id);
            if (providerUser == null || !_currentContext.ManageProviderUsers(providerUser.ProviderId))
            {
                throw new NotFoundException();
            }

            return new ProviderUserResponseModel(providerUser);
        }

        [HttpGet("")]
        public async Task<ListResponseModel<ProviderUserUserDetailsResponseModel>> Get(Guid providerId)
        {
            if (!_currentContext.ManageProviderUsers(providerId))
            {
                throw new NotFoundException();
            }

            var providerUsers = await _providerUserRepository.GetManyDetailsByProviderAsync(providerId);
            var responses = providerUsers.Select(o => new ProviderUserUserDetailsResponseModel(o));
            return new ListResponseModel<ProviderUserUserDetailsResponseModel>(responses);
        }

        [HttpPost("invite")]
        public async Task Invite(Guid providerId, [FromBody]ProviderUserInviteRequestModel model)
        {
            if (!_currentContext.ManageProviderUsers(providerId))
            {
                throw new NotFoundException();
            }

            var userId = _userService.GetProperUserId(User);
            await _providerService.InviteUserAsync(providerId, userId.Value, new ProviderUserInvite(model));
        }
        
        [HttpPost("reinvite")]
        public async Task<ListResponseModel<ProviderUserBulkResponseModel>> BulkReinvite(Guid providerId, [FromBody]ProviderUserBulkRequestModel model)
        {
            if (!_currentContext.ManageProviderUsers(providerId))
            {
                throw new NotFoundException();
            }

            var userId = _userService.GetProperUserId(User);
            var result = await _providerService.ResendInvitesAsync(providerId, userId.Value, model.Ids);
            return new ListResponseModel<ProviderUserBulkResponseModel>(
                result.Select(t => new ProviderUserBulkResponseModel(t.Item1.Id, t.Item2)));
        }
        
        [HttpPost("{id:guid}/reinvite")]
        public async Task Reinvite(Guid providerId, Guid id)
        {
            if (!_currentContext.ManageProviderUsers(providerId))
            {
                throw new NotFoundException();
            }

            var userId = _userService.GetProperUserId(User);
            await _providerService.ResendInvitesAsync(providerId, userId.Value, new [] { id });
        }

        [HttpPost("{id:guid}/accept")]
        public async Task Accept(Guid providerId, Guid id, [FromBody]ProviderUserAcceptRequestModel model)
        {
            var user = await _userService.GetUserByPrincipalAsync(User);
            if (user == null)
            {
                throw new UnauthorizedAccessException();
            }

            await _providerService.AcceptUserAsync(id, user, model.Token);
        }
        
        [HttpPost("{id:guid}/confirm")]
        public async Task Confirm(Guid providerId, Guid id, [FromBody]ProviderUserConfirmRequestModel model)
        {
            if (!_currentContext.ManageProviderUsers(providerId))
            {
                throw new NotFoundException();
            }

            var userId = _userService.GetProperUserId(User);
            await _providerService.ConfirmUsersAsync(providerId, new Dictionary<Guid, string> { [id] = model.Key }, userId.Value);
        }

        [HttpPost("confirm")]
        public async Task<ListResponseModel<ProviderUserBulkResponseModel>> BulkConfirm(Guid providerId,
            [FromBody]ProviderUserBulkConfirmRequestModel model)
        {
            if (!_currentContext.ManageProviderUsers(providerId))
            {
                throw new NotFoundException();
            }

            var userId = _userService.GetProperUserId(User);
            var results = await _providerService.ConfirmUsersAsync(providerId, model.ToDictionary(), userId.Value);

            return new ListResponseModel<ProviderUserBulkResponseModel>(results.Select(r =>
                new ProviderUserBulkResponseModel(r.Item1.Id, r.Item2)));
        }

        [HttpPost("public-keys")]
        public async Task<ListResponseModel<ProviderUserPublicKeyResponseModel>> UserPublicKeys(Guid providerId, [FromBody]ProviderUserBulkRequestModel model)
        {
            if (!_currentContext.ManageProviderUsers(providerId))
            {
                throw new NotFoundException();
            }

            var result = await _providerUserRepository.GetManyPublicKeysByProviderUserAsync(providerId, model.Ids);
            var responses = result.Select(r => new ProviderUserPublicKeyResponseModel(r.Id, r.PublicKey)).ToList();
            return new ListResponseModel<ProviderUserPublicKeyResponseModel>(responses);
        }

        [HttpPut("{id:guid}")]
        [HttpPost("{id:guid}")]
        public async Task Put(Guid providerId, Guid id, [FromBody]ProviderUserUpdateRequestModel model)
        {
            if (!_currentContext.ManageProviderUsers(providerId))
            {
                throw new NotFoundException();
            }

            var providerUser = await _providerUserRepository.GetByIdAsync(id);
            if (providerUser == null || providerUser.ProviderId != providerId)
            {
                throw new NotFoundException();
            }

            var userId = _userService.GetProperUserId(User);
            await _providerService.SaveUserAsync(model.ToProviderUser(providerUser), userId.Value);
        }

        [HttpDelete("{id:guid}")]
        [HttpPost("{id:guid}/delete")]
        public async Task Delete(Guid providerId, Guid id)
        {
            if (!_currentContext.ManageProviderUsers(providerId))
            {
                throw new NotFoundException();
            }

            var userId = _userService.GetProperUserId(User);
            await _providerService.DeleteUsersAsync(providerId, new [] { id }, userId.Value);
        }

        [HttpDelete("")]
        [HttpPost("delete")]
        public async Task<ListResponseModel<ProviderUserBulkResponseModel>> BulkDelete(Guid providerId, [FromBody]ProviderUserBulkRequestModel model)
        {
            if (!_currentContext.ManageProviderUsers(providerId))
            {
                throw new NotFoundException();
            }

            var userId = _userService.GetProperUserId(User);
            var result = await _providerService.DeleteUsersAsync(providerId, model.Ids, userId.Value);
            return new ListResponseModel<ProviderUserBulkResponseModel>(result.Select(r =>
                new ProviderUserBulkResponseModel(r.Item1.Id, r.Item2)));
        }
    }
}
