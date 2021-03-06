﻿using System;
using Bit.Core.Utilities;
using Bit.Core.Models.Table;
using Bit.Core.Enums;
using Newtonsoft.Json;
using Bit.Core.Models.Data;
using System.ComponentModel.DataAnnotations;
using Bit.Core.Services;
using Bit.Core.Exceptions;

namespace Bit.Core.Models.Api
{
    public class SendRequestModel
    {
        public SendType Type { get; set; }
        [EncryptedString]
        [EncryptedStringLength(1000)]
        public string Name { get; set; }
        [EncryptedString]
        [EncryptedStringLength(1000)]
        public string Notes { get; set; }
        [Required]
        [EncryptedString]
        [EncryptedStringLength(1000)]
        public string Key { get; set; }
        [Range(1, int.MaxValue)]
        public int? MaxAccessCount { get; set; }
        public DateTime? ExpirationDate { get; set; }
        [Required]
        public DateTime? DeletionDate { get; set; }
        public SendFileModel File { get; set; }
        public SendTextModel Text { get; set; }
        [StringLength(1000)]
        public string Password { get; set; }
        [Required]
        public bool? Disabled { get; set; }

        public Send ToSend(Guid userId, ISendService sendService)
        {
            var send = new Send
            {
                Type = Type,
                UserId = (Guid?)userId
            };
            ToSend(send, sendService);
            return send;
        }

        public (Send, SendFileData) ToSend(Guid userId, string fileName, ISendService sendService)
        {
            var send = ToSendBase(new Send
            {
                Type = Type,
                UserId = (Guid?)userId
            }, sendService);
            var data = new SendFileData(this, fileName);
            return (send, data);
        }

        public Send ToSend(Send existingSend, ISendService sendService)
        {
            existingSend = ToSendBase(existingSend, sendService);
            switch (existingSend.Type)
            {
                case SendType.File:
                    var fileData = JsonConvert.DeserializeObject<SendFileData>(existingSend.Data);
                    fileData.Name = Name;
                    fileData.Notes = Notes;
                    existingSend.Data = JsonConvert.SerializeObject(fileData,
                        new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    break;
                case SendType.Text:
                    existingSend.Data = JsonConvert.SerializeObject(new SendTextData(this),
                        new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                    break;
                default:
                    throw new ArgumentException("Unsupported type: " + nameof(Type) + ".");
            }
            return existingSend;
        }

        public void ValidateCreation()
        {
            // Add 1 minute for a sane buffer and client clock float
            var nowPlus1Minute = DateTime.UtcNow.AddMinutes(1);
            if (ExpirationDate.HasValue && ExpirationDate.Value <= nowPlus1Minute)
            {
                throw new BadRequestException("You cannot create a send that is already expired. " +
                    "Adjust the expiration date and try again.");
            }
            if (DeletionDate.HasValue && DeletionDate.Value <= nowPlus1Minute)
            {
                throw new BadRequestException("You cannot create a send that is already deleted. " +
                    "Adjust the deletion date and try again.");
            }
        }

        private Send ToSendBase(Send existingSend, ISendService sendService)
        {
            existingSend.Key = Key;
            existingSend.ExpirationDate = ExpirationDate;
            existingSend.DeletionDate = DeletionDate.Value;
            existingSend.MaxAccessCount = MaxAccessCount;
            if (!string.IsNullOrWhiteSpace(Password))
            {
                existingSend.Password = sendService.HashPassword(Password);
            }
            existingSend.Disabled = Disabled.GetValueOrDefault();
            return existingSend;
        }
    }
}
