using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using BrConselhosWs.Models;

namespace BrConselhosWs.Controllers
{
    public class TokenController
    {
        public string GenerateToken(string reason, UserModels user)
        {
            byte[] _time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
            byte[] _key = Guid.Parse(user.SecurityStamp).ToByteArray();
            byte[] _Id = Encoding.Default.GetBytes(user.id.ToString());
            byte[] _reason = Encoding.Default.GetBytes(reason);
            byte[] data = new byte[_time.Length + _key.Length + _reason.Length + _Id.Length];

            System.Buffer.BlockCopy(_time, 0, data, 0, _time.Length);
            System.Buffer.BlockCopy(_key, 0, data, _time.Length, _key.Length);
            System.Buffer.BlockCopy(_reason, 0, data, _time.Length + _key.Length, _reason.Length);
            System.Buffer.BlockCopy(_Id, 0, data, _time.Length + _key.Length + _reason.Length, _Id.Length);

            return Convert.ToBase64String(data.ToArray());
        }

        public TokenModels ValidateToken(string reason, UserModels user, string token)
        {
            var result = new TokenModels();
            byte[] data = Convert.FromBase64String(token);
            byte[] _time = data.Take(8).ToArray();
            byte[] _key = data.Skip(8).Take(16).ToArray();
            byte[] _reason = data.Skip(24).Take(4).ToArray();
            byte[] _Id = data.Skip(28).ToArray();

            DateTime when = DateTime.FromBinary(BitConverter.ToInt64(_time, 0));
            if (when < DateTime.UtcNow.AddHours(-24))
            {
                result.Errors.Add(TokenValidationStatus.Expired);
            }

            Guid gKey = new Guid(_key);
            if (gKey.ToString() != user.SecurityStamp)
            {
                result.Errors.Add(TokenValidationStatus.WrongGuid);
            }

            if (reason != Encoding.Default.GetString(_reason))
            {
                result.Errors.Add(TokenValidationStatus.WrongPurpose);
            }

            if (user.id.ToString() != Encoding.Default.GetString(_Id))
            {
                result.Errors.Add(TokenValidationStatus.WrongUser);
            }

            return result;
        }
    }
}