Расчёт скидок и API обратного звонка на .NET 8. Нужен SDK 8.0.416+ в ветке 8.0. БД и Docker поднимать не нужно.

## Запуск

Выбрать `TrainingCenter.WebApi` запускаемым проектом

Swagger: http://localhost:5080/swagger. В нём есть выбор языка `ru` / `en`.

По умолчанию `Demo`: основной канал специально падает, резерв принимает письмо в память. Аккаунты для такого запуска не нужны

## Настройка реальной почты

Основной канал Яндекс, резерв Gmail. Серверы уже настроены в `TrainingCenter.WebApi/appsettings.json`.

В Яндексе разрешить IMAP и пароли приложений в настройках почтовых программ. В Google включить двухэтапную аутентификацию. Для обоих аккаунтов создать **пароль приложения**.

В Visual Studio: ПКМ на `TrainingCenter.WebApi` → **Управление секретами пользователя / Manage User Secrets**. В открывшийся `secrets.json` внести свои значения:

```json
{
  "Mail:Mode": "Live",
  "Mail:SimulatePrimaryFailure": false,
  "Mail:Primary:UserName": "yourlogin@yandex.ru",
  "Mail:Primary:FromAddress": "yourlogin@yandex.ru",
  "Mail:Primary:Password": "YANDEX_APP_PASSWORD",
  "Mail:Fallback:UserName": "yourlogin@gmail.com",
  "Mail:Fallback:FromAddress": "yourlogin@gmail.com",
  "Mail:Fallback:Password": "GOOGLE_APP_PASSWORD",
  "Mail:PrimaryRecipient": "yourlogin@yandex.ru",
  "Mail:LegalEntityRecipient": "yourlogin+corp@yandex.ru",
  "Mail:IndividualRecipient": "yourlogin+individual@yandex.ru"
}
```

`UserName` и `FromAddress` - отправитель, `Recipient` - получатель. Адреса с `+corp` и `+individual` ведут в тот же ящик Яндекса. Можно поставить один обычный адрес во всех трёх полях получателей.

Вместо User Secrets можно использовать env: ключи те же, но `:` заменить на `__`. Например, в PowerShell:

```powershell
$env:Mail__Mode = "Live"
$env:Mail__Primary__Password = "YANDEX_APP_PASSWORD"
$env:Mail__Fallback__Password = "GOOGLE_APP_PASSWORD"
```

Остальные поля аналогично: `Mail__Primary__UserName`, `Mail__PrimaryRecipient` и т.д. В Visual Studio env задаются в профиле запуска нужного проекта. Env перекрывает User Secrets. После смены настроек перезапустить приложение. Пароли в файлы решения не записываем.

## Общий флоу

В Swagger отправить `POST /api/leads`:

```json
{
  "name": "Тест",
  "phone": "+79991234567",
  "email": "client@example.com",
  "clientType": "Individual"
}
```

Для юрлица - `LegalEntity`. `email` в форме - контакт клиента, получатель берётся из конфига.

Флоу: валидация → сохранение заявки через репозиторий и UoW → основной SMTP → при сбое резерв → сохранение статуса → 200 с номером заявки.

Для проверки резерва поставить `Mail:SimulatePrimaryFailure = true`. Если оба канала откажут, заявка останется в памяти со статусом `Failed`, ответ будет 503. Невалидная форма получает 400. Автоматических повторов нет; при перезапуске заявки теряются.

## MailProbe

Проверяет отправку SMTP и получение IMAP. Использует те же User Secrets, что API. Нужны `Live` и настройки обоих аккаунтов.

В Visual Studio выбрать `TrainingCenter.MailProbe` запускаемым проектом, в свойствах запуска указать аргумент `Primary` или `Fallback`, затем Ctrl+F5.

Отправляет письмо **самому себе** и ищет его в INBOX по Message-Id. Имитацию сбоя игнорирует. Результат - в консоли; если письмо не найдено, проверить спам. Код выхода: 0 - успех, 1 - проверка не прошла, 2 - неверный аргумент или деморежим.

## Что сделано по заданию

| Пункт | Решение |
| --- | --- |
| Горящие группы | `CartService`: 20%, если до старта осталось от нуля до строго менее 5 суток. Уже начавшиеся курсы не считаются горящими. |
| Выпускник | При `IsGraduate = true` скидка 30%. |
| Скидки не складываются | Максимум для каждого курса, округление до копеек, затем сумма. Отдельный сервис, без эндпоинта. |
| Форма обратного звонка | `POST /api/leads`: имя, телефон, email, тип клиента. Основной и резервный почтовые каналы. |
| Отказоустойчивость | При сбое - резерв: юрлицо → `mail1@yandex.ru`, физлицо → `mail2@yandex.ru` по умолчанию. Адреса настраиваются. Успешный резерв → 200. |
| Архитектура | Контроллер, сервис, generic-репозиторий, UoW, EF Core InMemory и DI. Ввод-вывод - async/await. Транзакции и rollback InMemory не поддерживает. |
| Валидация | Обязательные поля, формат email и телефона, допустимый тип клиента. |
| Тесты | xUnit: скидки, основной и резервный каналы, валидация, UoW, локализация, TraceId и ошибки SMTP. Реальные письма тесты не отправляют. |

Запуск тестов из корня: `dotnet test TrainingCenter.sln`.