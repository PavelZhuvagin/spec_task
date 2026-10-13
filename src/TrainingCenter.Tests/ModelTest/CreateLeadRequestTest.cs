namespace TrainingCenter.Tests.ModelTest;

using System.ComponentModel.DataAnnotations;
using TrainingCenter.BusinessLogic.Models.Leads;
using TrainingCenter.Core.Enums;
using Xunit;

/// <summary>
/// Тест модели <see cref="CreateLeadRequest"/>.
/// </summary>
public class CreateLeadRequestTest
{
    /// <summary>
    /// Тест валидации модели <see cref="CreateLeadRequest"/>.
    /// Проверяет отклонение невалидных телефона, email и типа клиента.
    /// </summary>
    /// <param name="phone">Телефон тестовой заявки.</param>
    /// <param name="email">Email тестовой заявки.</param>
    /// <param name="clientType">Тип клиента либо отсутствующее значение.</param>
    [Theory]
    [InlineData("", "ivan@example.com", ClientType.Individual)]
    [InlineData("   ", "ivan@example.com", ClientType.Individual)]
    [InlineData("+79991234567", "invalid", ClientType.Individual)]
    [InlineData("+79991234567", "ivan@example.com", null)]
    [InlineData("+79991234567", "ivan@example.com", (ClientType)999)]
    public void ValidateInvalidDataTest(string phone, string email, ClientType? clientType)
    {
        var request = new CreateLeadRequest { Name = "Иван", Phone = phone, Email = email, ClientType = clientType };
        Assert.False(Validator.TryValidateObject(request, new ValidationContext(request), new List<ValidationResult>(), true));
    }
}