using System;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Realms;
using HikePOS.Helpers;
namespace HikePOS.Models
{

    public class UserListDto : FullAuditedPassiveEntityDto
    {
        public UserListDto()
        {
            Roles = new ObservableCollection<UserListRoleDto>();
        }

        public string Name { get; set; }
        public string Surname { get; set; }

        [JsonIgnore]
        public string FullName
        {
            get
            {
                return Name.ToUppercaseFirstCharacter() + (string.IsNullOrEmpty(Surname) ? "" : " ") + Surname.ToUppercaseFirstCharacter();
            }
        }

        public string UserName { get; set; }

        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public Guid? ProfilePictureId { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public ObservableCollection<UserListRoleDto> Roles { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public DateTime CreationTime { get; set; }
        public string UserPin { get; set; }
        public ObservableCollection<UserListOutletDto> Outlets { get; set; }
        public ObservableCollection<string> GrantedPermissionNames { get; set; }

        public string DisplayName { get; set; }

        public string DisplayNameInCaps
        {
            get
            {
                if (DisplayName != null)
                    return DisplayName.ToUpper();
                else
                    return "Admin".ToUpper();
            }
        }

        //Ticket start:#33790 iPad :: Feature request :: User specific language.by rupesh
        public bool AllowOverrideLangugeSettingOverGeneralSetting { get; set; }
        string _language { get; set; }
        public string Language
        {
            get { return _language; }
            set
            {
                //Ticket start:#34015 iPad: hijari calendar is not working on iPad.by rupesh
                //if (value == "ar")
                //{
                //    _language = "ar-SY";
                //}
                //else
                //{
                _language = value;
                //}
                //Ticket end:#34015 .by rupesh

            }
        }

        public string TimeZone { get; set; }
        public string IANATimeZone { get; set; }
        //Ticket end:#33790 .by rupesh
        public decimal? MaximumDiscount { get; set; }

        public UserListDB ToModel()
        {
            UserListDB userList = new UserListDB
            {
                Id = Id,
                IsActive = IsActive,
                Name = Name,
                Surname = Surname,
                UserName = UserName,
                EmailAddress = EmailAddress,
                PhoneNumber = PhoneNumber,
                ProfilePictureId = ProfilePictureId,
                IsEmailConfirmed = IsEmailConfirmed,
                LastLoginTime = LastLoginTime,
                CreationTime = CreationTime,
                UserPin = UserPin,
                DisplayName = DisplayName,
                AllowOLSOverGeneralSetting = AllowOverrideLangugeSettingOverGeneralSetting,
                Language = Language,
                TimeZone = TimeZone,
                IANATimeZone = IANATimeZone,
                MaximumDiscount = MaximumDiscount
            };

            Roles?.ForEach(a => userList.Roles.Add(a.ToModel()));
            Outlets?.ForEach(a => userList.Outlets.Add(a.ToModel()));
            GrantedPermissionNames?.ForEach(a => userList.GrantedPermissionNames.Add(a));

            return userList;
        }

        public static UserListDto FromModel(UserListDB listDB)
        {
            if (listDB == null)
                return null;
            UserListDto userList = new UserListDto
            {
                Id = listDB.Id,
                IsActive = listDB.IsActive,
                Name = listDB.Name,
                Surname = listDB.Surname,
                UserName = listDB.UserName,
                EmailAddress = listDB.EmailAddress,
                PhoneNumber = listDB.PhoneNumber,
                ProfilePictureId = listDB.ProfilePictureId,
                IsEmailConfirmed = listDB.IsEmailConfirmed,
                LastLoginTime = listDB.LastLoginTime?.UtcDateTime,
                CreationTime = listDB.CreationTime.UtcDateTime,
                UserPin = listDB.UserPin,
                DisplayName = listDB.DisplayName,
                AllowOverrideLangugeSettingOverGeneralSetting = listDB.AllowOLSOverGeneralSetting,
                Language = listDB.Language,
                TimeZone = listDB.TimeZone,
                IANATimeZone = listDB.IANATimeZone,
                MaximumDiscount = listDB.MaximumDiscount
            };

            userList.Roles = new ObservableCollection<UserListRoleDto>(listDB.Roles.Select(a=>UserListRoleDto.FromModel(a)));
            userList.Outlets = new ObservableCollection<UserListOutletDto>(listDB.Outlets.Select(a => UserListOutletDto.FromModel(a)));
            userList.GrantedPermissionNames = new ObservableCollection<string>(listDB.GrantedPermissionNames);

            return userList;
        }
    }

    public partial class UserListOutletDto
    {
        public int OutletId { get; set; }
        public string OutletName { get; set; }
        public bool IsAssigned { get; set; }

        public UserListOutletDB ToModel()
        {
            UserListOutletDB userList = new UserListOutletDB
            {
                OutletId = OutletId,
                OutletName = OutletName,
                IsAssigned = IsAssigned,
            };

            return userList;
        }

        public static UserListOutletDto FromModel(UserListOutletDB listDB)
        {
            if (listDB == null)
                return null;
            UserListOutletDto userList = new UserListOutletDto
            {
                OutletId = listDB.OutletId,
                OutletName = listDB.OutletName,
                IsAssigned = listDB.IsAssigned,
            };
            return userList;
        }
    }

    public partial class UserListRoleDto 
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }

        public UserListRoleDB ToModel()
        {
            UserListRoleDB userList = new UserListRoleDB
            {
                RoleId = RoleId,
                RoleName = RoleName
            };

            return userList;
        }

        public static UserListRoleDto FromModel(UserListRoleDB listDB)
        {
            if (listDB == null)
                return null;
            UserListRoleDto userList = new UserListRoleDto
            {
                RoleId = listDB.RoleId,
                RoleName = listDB.RoleName
            };
            return userList;
        }
    }

    public partial class UserListOutletDB : IRealmObject
    {
        [PrimaryKey]
        public int OutletId { get; set; }
        public string OutletName { get; set; }
        public bool IsAssigned { get; set; }
    }

    public partial class UserListRoleDB : IRealmObject
    {
        [PrimaryKey]
        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }

    public class RoleRequestModel
    {
        public string permission { get; set; } = "";
    }

    public class UserClockActivityInputDto
    {
        public int Id { get; set; }
    }

    public partial class UserClockActivityDB : IRealmObject
    {
       
        public int Id { get; set; }

        [PrimaryKey]
        public int UserId { get; set; }

        public DateTimeOffset? InTime { get; set; }

        public DateTimeOffset? LastOutTime { get; set; }

        public DateTimeOffset? OutTime { get; set; }
    }

    public partial class UserClockActivityDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public DateTime? InTime { get; set; }

        public DateTime? DisplayInTime
        {

            get
            {
                if (InTime != null)
                    return InTime.Value.ToStoreTime();
                else
                    return InTime;
            }

        }
        public DateTime? LastOutTime { get; set; }

        public DateTime? DisplayLastOutTime
        {

            get
            {
                if (LastOutTime != null)
                    return LastOutTime.Value.ToStoreTime();
                else
                    return LastOutTime;
            }

        }

        public DateTime? OutTime { get; set; }

        public UserListDto User { get; set; }

        public UserClockActivityDB ToModel()
        {
            UserClockActivityDB userList = new UserClockActivityDB
            {
                Id = Id,
                UserId = UserId,
                InTime = InTime,
                LastOutTime = LastOutTime,
                OutTime = OutTime,
            };


            return userList;
        }

        public static UserClockActivityDto FromModel(UserClockActivityDB listDB)
        {
            if (listDB == null)
                return null;
            UserClockActivityDto userList = new UserClockActivityDto
            {
                Id = listDB.Id,
                UserId = listDB.UserId,
                InTime = listDB.InTime?.UtcDateTime,
                LastOutTime = listDB.LastOutTime?.UtcDateTime,
                OutTime = listDB.OutTime?.UtcDateTime,
            };

            return userList;
        }

        [JsonIgnore]
        public bool IsEnabled {

            get
            {
                return Settings.GrantedPermissionNames.Any(s => s == "Pages.Tenant.UsersYourCrew.Users.ClockInClockOut") || User.Id == Settings.CurrentUser.Id;
            }

        }

    }

    public partial class UserListDB : IRealmObject
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public Guid? ProfilePictureId { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public IList<UserListRoleDB> Roles { get; }
        public DateTimeOffset? LastLoginTime { get; set; }
        public DateTimeOffset CreationTime { get; set; }
        public string UserPin { get; set; }
        public IList<UserListOutletDB> Outlets { get; }
        public IList<string> GrantedPermissionNames { get; }
        public string DisplayName { get; set; }
        public bool AllowOLSOverGeneralSetting { get; set; }
        public string Language { get; set; }
        public string TimeZone { get; set; }
        public string IANATimeZone { get; set; }
        [PrimaryKey]
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public decimal? MaximumDiscount { get; set; }

    }

}