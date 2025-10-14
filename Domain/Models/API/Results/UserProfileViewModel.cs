using DataAccess.Enums;
using DataAccess.Schemas.Auth;
using Newtonsoft.Json;

namespace Domain.Models.API.Results;

public class UserProfileViewModel
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("first_name")]
    public string FirstName { get; set; } = default!;

    [JsonProperty("last_name")]
    public string? LastName { get; set; }

    [JsonProperty("phone")]
    public string Phone { get; set; } = default!;

    [JsonProperty("email")]
    public string? Email { get; set; }

    [JsonProperty("date_of_birth")]
    public DateTime? DateOfBirth { get; set; }

    [JsonProperty("gender")]
    public Gender? Gender { get; set; }

    [JsonProperty("address")]
    public string? Address { get; set; }

    [JsonProperty("profile_photo_url")]
    public string? ProfilePhotoUrl { get; set; }

    [JsonProperty("status")]
    public EntityStatus Status { get; set; }

    [JsonProperty("created_date")]
    public DateTime CreatedDate { get; set; }

    [JsonProperty("updated_date")]
    public DateTime? UpdatedDate { get; set; }

    public UserProfileViewModel() { }

    public UserProfileViewModel(User user, string baseUrl)
    {
        Id = user.Id;
        FirstName = user.FirstName;
        LastName = user.LastName;
        Phone = user.Phone;
        Email = user.Email;
        DateOfBirth = user.DateOfBirth;
        Gender = user.Gender;
        Address = user.Address;
        ProfilePhotoUrl = !string.IsNullOrEmpty(user.ProfilePhotoPath) 
            ? $"{baseUrl}/{user.ProfilePhotoPath}" 
            : null;
        Status = user.Status;
        CreatedDate = user.CreatedDate;
        UpdatedDate = user.UpdatedDate;
    }
}