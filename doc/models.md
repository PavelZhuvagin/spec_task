# Arch controller layer

## 1. Шапка контроллера

- Порядок: namespace (с символом ;), using (удаляем лишние, сортируем по порядку, удаляем пробелы), xmldoc
- Наследоваание от ControllerBase

```csharp
namespace Ris.WebApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ris.BusinessLogic.Models.Attachment;
using Ris.BusinessLogic.Services.DataServices;

/// <summary>
/// Контроллер управления аватаром общих настроек системы.
/// </summary>
[ApiController]
public class SettingsAvatarController : ControllerBase
```

## 2. Атрибуты контроллера

- Порядок: ApiController, Authorize, Route, Produces (валидация входа), Consumes(валидация выхода)
- Route: с [controller] и $
- Consumes: на уровне контроллера, если тип ответа одинаковый для всех методов, ИНАЧЕ - на уровне метода (см. ниже)

```csharp
[ApiController]
[Authorize]
[Route($"api/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
```

## 3. DI

- Через абстракцию IService<T> (Interface Segregation, Dependency Inversion), private readonly field, через констрктор
- Модификаторы доступа ниже в примере
- Имя сервиса через camelCase

```csharp
private readonly ISettingsService settingsService;

/// <summary>
/// Initializes a new instance of the <see cref="SettingsController"/> class.
/// </summary>
/// <param name="settingsService">Сервис управления общими настройками системы.</param>
public SettingsController(ISettingsService settingsService)
{
    this.settingsService = settingsService;
}
```

## 4. Методы

- Проверить на RESTful архитектурный стиль

```csharp
```

## 5. Атрибуты метода

- Порядок: method + route, AuthorizePermission, Consumes, ProducesResponseType (декларативная документация)

```csharp
[HttpPut("request")]
[AuthorizePermission(PermissionObject.StudyPacs, PermissionOperation.Update)]
[Consumes("application/json")]
[ProducesResponseType(typeof(CreateUploadUrlResponse), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
```

## 6. xmldoc метода

- Добавить response code

```csharp
/// <summary>
/// Получить ссылку для загрузки аватара общих настроек системы.
/// </summary>
/// <returns>Ссылка для загрузки аватара.</returns>
/// <response code="200">Ссылка для загрузки успешно создана.</response>
/// <response code="400">Не удалось создать ссылку для загрузки.</response>
/// <response code="401">Пользователь не авторизован.</response>
```

## 7. Сигнатура метода

- Добавить везде CancellationToken
- Добавить Async в имена асинхронных методов
- Убрать virtual, где нет переопределения
- Прописать ВЕЗДЕ атрибуты FromHeader, FromRoute, FromQuery, FromBody для ЯВНОГО (повышения уровня читаемости) указания источника данных (+ подключает автоматическую структурную валидацию)
- Убрать вариативность ответов контроллера - оставить только Task<T> в виде ответов
- Имена параметров типа Guid должны иметь вид entityUid

```csharp
public async Task<GetSettingsResponse> GetAsync(Guid patientUid, CancellationToken ct) {}

public async Task GetAsync(CancellationToken ct) {}
```

## 8. Содержимое метода

- Убрать try-catch - все перехватывает middleware
- Убрать ModelState.IsValid (если встретится) - валидация вынесена в сервисы
- Убрать AutoMapper - маппинг выполняется в сервисах/хендлерах/утилитах
