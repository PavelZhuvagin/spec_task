namespace TrainingCenter.BusinessLogic.Models.Leads;

using System.ComponentModel.DataAnnotations;
using TrainingCenter.Core.Enums;
using TrainingCenter.Core.Resources;

/// <summary>
/// Данные формы обратного звонка.
/// </summary>
public class CreateLeadRequest
{
    /// <summary>
    /// Обязательное имя клиента или наименование организации, до 100 символов.
    /// </summary>
    [Display(Name = nameof(Messages.NameField), ResourceType = typeof(Messages))]
    [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = nameof(Messages.RequiredField))]
    [StringLength(100, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = nameof(Messages.StringLength))]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Обязательный телефон в допустимом формате, до 32 символов.
    /// </summary>
    [Display(Name = nameof(Messages.PhoneField), ResourceType = typeof(Messages))]
    [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = nameof(Messages.RequiredField))]
    [Phone(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = nameof(Messages.InvalidPhone))]
    [StringLength(32, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = nameof(Messages.StringLength))]
    public string Phone { get; init; } = string.Empty;

    /// <summary>
    /// Обязательный корректный email, до 254 символов.
    /// </summary>
    [Display(Name = nameof(Messages.EmailField), ResourceType = typeof(Messages))]
    [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = nameof(Messages.RequiredField))]
    [EmailAddress(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = nameof(Messages.InvalidEmail))]
    [StringLength(254, ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = nameof(Messages.StringLength))]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Обязательный тип клиента из поддерживаемого перечисления.
    /// </summary>
    [Display(Name = nameof(Messages.ClientTypeField), ResourceType = typeof(Messages))]
    [Required(ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = nameof(Messages.RequiredField))]
    [EnumDataType(typeof(ClientType), ErrorMessageResourceType = typeof(Messages), ErrorMessageResourceName = nameof(Messages.InvalidClientType))]
    public ClientType? ClientType { get; init; }
}