# Система мониторинга и телеметрии бурового насоса

Программно-аппаратный комплекс для обработки телеметрических данных бурового насоса, расчета эксплуатационной наработки по режимам давления, выявления пропусков связи и недостоверных показаний, а также визуализации технологических параметров в реальном времени.

---

## 1. Архитектура и структура решения

Проект построен по принципам **Clean Architecture** на платформе **.NET 10** с веб-интерфейсом на базе HTML5 / JavaScript (Chart.js):

```text
├── backend/
│   ├── src/
│   │   ├── PumpDrillingTelemetryCore.Domain/        # Доменная модель, правила расчета режимов
│   │   ├── PumpDrillingTelemetryCore.Application/   # Команды MediatR, DTO, хранилище сессий
│   │   └── PumpDrillingTelemetryCore.Api/           # REST API контроллеры, middleware, валидаторы
│   ├── PumpDrillingCore.UnitTests/                  # Юнит-тесты расчетов и валидации
│   ├── PumpDrillingCore.FunctionalTests/            # Интеграционные тесты API с файлами 01-10
│   └── PumpDrillingTelemetryCore.sln
├── frontend_mrchem/
│   ├── main.html                                    # Главный экран мониторинга и графиков
│   ├── modal.html                                   # Модальное окно итоговых расчетов сессии
│   └── static/css/                                  # Стили интерфейса
├── test_file/                                       # Набор эталонных файлов (01..10, правила)

└── README.md                                        # Руководство
```

## 2. Инструкция по сборке и запуску

### 2.1. Предварительные требования
* [.NET 10 SDK](https://dotnet.microsoft.com/)
* Современный веб-браузер (Chrome, Edge, Firefox)
* HTTP-сервер для статики (Live Server в VS Code, `python -m http.server` или `npx serve`)

### 2.2. Запуск бэкенда
1. Откройте терминал в папке `backend/src/PumpDrillingTelemetryCore.Api`:
   ```bash
   dotnet restore
   dotnet run --launch-profile http
   ```
2. API будет доступен по адресу: `http://localhost:5112`
3. Документация Swagger/OpenAPI: `http://localhost:5112/swagger`

### 2.3. Запуск тестов
* **Unit-тесты:**
  ```bash
  dotnet test backend/PumpDrillingCore.UnitTests/PumpDrillingCore.UnitTests.csproj
  ```
* **Функциональные интеграционные тесты (требуется запущенный бэкенд на порту 5112):**
  ```bash
  dotnet test backend/PumpDrillingCore.FunctionalTests/PumpDrillingCore.FunctionalTests.csproj
  ```

### 2.4. Запуск фронтенда
Фронтенд состоит из статических файлов и модального окна в `iframe`. Запускать рекомендуется через локальный веб-сервер во избежание CORS/iframe restriction:
```bash
cd frontend_mrchem
python -m http.server 3000
```
Откройте в браузере: `http://localhost:3000/main.html`

---

## 3. Спецификация API

* `GET /api/pump/work-session` — Получить текущее состояние сессии (включая расчёт `summary` и точки `samples`).
* `POST /api/pump/work-session` — Передать JSON сессии в теле запроса.
* `POST /api/pump/work-session/import` — Загрузить `.json` файл через `multipart/form-data`.

При передаче некорректных файлов (например, `08_missing_rpm.json`, `09_wrong_type.json`, `10_duplicate_time.json`) сервер возвращает `400 Bad Request` со стандартным объектом `ProblemDetails` / `ValidationProblemDetails` и точным описанием нарушения структуры.
