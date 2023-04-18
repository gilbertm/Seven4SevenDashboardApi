using RAFFLE.WebApi.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace RAFFLE.WebApi.Domain.Catalog;

public class AppUser : AuditableEntity, IAggregateRoot
{
    // this application userid is mapped
    // to the main applicationuser
    public string? ApplicationUserId { get; private set; }
    public string? HomeAddress { get; private set; }
    public string? HomeCity { get; private set; }
    public string? HomeRegion { get; private set; }
    public string? HomeCountry { get; private set; }
    public string? Longitude { get; private set; }
    public string? Latitude { get; private set; }
    public bool? IsVerified { get; private set; }
    public VerificationStatus? AddressStatus { get; private set; }
    public VerificationStatus? DocumentsStatus { get; private set; }
    public VerificationStatus? RolePackageStatus { get; private set; }

    public string? RoleId { get; private set; }
    public string? RoleName { get; private set; }

    // manual propagation
    // this is from the application user
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string? Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? ImageUrl { get; private set; }

  
    public AppUser(string applicationUserId, string? homeAddress, string? homeCity, string? homeRegion, string? homeCountry, string? longitude, string? latitude, bool? isVerified, VerificationStatus? addressStatus, string? roleId, string? roleName, string? firstName, string? lastName, string? email, string? phoneNumber, string? imageUrl)
    {
        
        ApplicationUserId = applicationUserId;
        HomeAddress = homeAddress ?? default!;
        HomeCity = homeCity ?? default!;
        HomeRegion = homeRegion ?? default!;
        HomeCountry = homeCountry ?? default!;
        Longitude = longitude ?? default!;
        Latitude = latitude ?? default!;
        IsVerified = isVerified ?? false;
        AddressStatus = addressStatus ?? VerificationStatus.Initial;
        
        RoleId = roleId ?? default!;
        RoleName = roleName ?? default!;
        FirstName = firstName ?? default!;
        LastName = lastName ?? default!;
        Email = email ?? default!;
        PhoneNumber = phoneNumber ?? default!;
        ImageUrl = imageUrl ?? default!;
    }

    public AppUser Update(string applicationUserId, string? homeAddress, string? homeCity, string? homeRegion, string? homeCountry, string? longitude, string? latitude, bool? isVerified, VerificationStatus? addressStatus,  string? roleId, string? roleName, string? firstName, string? lastName, string? email, string? phoneNumber, string? imageUrl)
    {
        if (ApplicationUserId is not null && ApplicationUserId?.Equals(applicationUserId) is not true) ApplicationUserId = applicationUserId;
        if (homeAddress is not null && HomeAddress?.Equals(homeAddress) is not true) HomeAddress = homeAddress;
        if (homeCity is not null && HomeCity?.Equals(homeCity) is not true) HomeCity = homeCity;
        if (homeRegion is not null && HomeRegion?.Equals(homeRegion) is not true) HomeRegion = homeRegion;
        if (homeCountry is not null && HomeCountry?.Equals(homeCountry) is not true) HomeCountry = homeCountry;
        if (longitude is not null && Longitude?.Equals(longitude) is not true) Longitude = longitude;
        if (latitude is not null && Latitude?.Equals(latitude) is not true) Latitude = latitude;
        if (isVerified is not null && !IsVerified.Equals(isVerified)) IsVerified = isVerified;
        if (addressStatus is not null && !AddressStatus.Equals(addressStatus)) AddressStatus = addressStatus;
       
        if (roleId is not null && RoleId?.Equals(roleId) is not true) RoleId = roleId;
        if (roleName is not null && RoleName?.Equals(roleName) is not true) RoleName = roleName;
        if (firstName is not null && FirstName?.Equals(firstName) is not true) FirstName = firstName;
        if (lastName is not null && LastName?.Equals(lastName) is not true) LastName = lastName;
        if (email is not null && Email?.Equals(email) is not true) Email = email;
        if (phoneNumber is not null && PhoneNumber?.Equals(phoneNumber) is not true) PhoneNumber = phoneNumber;
        if (imageUrl is not null && ImageUrl?.Equals(imageUrl) is not true) ImageUrl = imageUrl;

        return this;
    }
}
