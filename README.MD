# Fintech Demo Backend - Portfolio Showcase

A production-ready, highly secure, and clean-architecture backend for a functional Fintech application built on **ASP.NET Core 9.0**, **Entity Framework Core**, **SQL Server**, and **xUnit**. 

This repository showcases advanced development practices including **Strategy Design Pattern** for transactions, **API Idempotency** controls, a thread-safe **Token Revocation System** for JWT authentication, a local **AI-Powered Financial Analyst** integration (Ollama), automated background maintenance workers, and a robust suite of integration tests featuring custom application factories.

---

## 🏗️ Architecture & Project Structure

The project follows a decoupled, layered architectural approach that separates HTTP delivery, domain logic, data persistence, and background maintenance.

```
📁 fintech-demo-backend
├── 📁 Fintech.Api                  # Main ASP.NET Core Web API Project
│   ├── 📁 Controllers             # HTTP Endpoints (inherits CustomControllerBase)
│   ├── 📁 DTOs                    # Data Transfer Objects (contracts for API requests/responses)
│   ├── 📁 Data                    # EF Core DbContext, migrations, and seed data
│   ├── 📁 Middleware              # Exception Handler & Token Blacklist middlewares
│   ├── 📁 Models                  # EF Core database entities (User, Account, Transaction, etc.)
│   ├── 📁 Services                # Business Logic (Auth, Account, Transaction, Idempotency, AI)
│   │   └── 📁 Transactions        # Strategy Pattern implementations for deposits/withdrawals/transfers
│   ├── 📁 Workers                 # Hosted background services (maintenance & token cleanup)
│   └── 📄 Program.cs              # API entrypoint, DI container, and middleware pipeline config
│
└── 📁 Fintech.Tests                # Integration and Unit Testing Project (xUnit)
    ├── 📄 CustomWebApplicationFactory.cs             # Replaces DB with InMemory context & seeds test data
    ├── 📄 CustomWebApplicationFactoryWithRateLimiter.cs # Simulates "Production" environment to test rate limiting
    ├── 📄 AuthTests.cs            # Tests token generation, validation, and logout revocation
    ├── 📄 AccountTests.cs         # Tests account CRUD, ownership authorization, and cascade deletes
    ├── 📄 TransactionTests.cs     # Tests strategies, balance calculations, idempotency, and concurrency
    └── 📄 SecurityTests.cs        # Tests API rate limiting controls
```

### Key Architectural Patterns
* **Strategy Pattern**: Implemented in [TransactionService.cs](Fintech.Api/Services/TransactionService.cs) to resolve transaction creation dynamically. Based on the `TransactionType` (Deposit, Withdrawal, or Transfer), the service executes the corresponding strategy class implementing [ITransactionStrategy.cs](Fintech.Api/Services/Transactions/ITransactionStrategy.cs). This makes the transaction engine highly extensible and easy to scale.
* **Dependency Injection (DI)**: Follows strict C# DI patterns. Strategies are registered as scoped services and collected into a dictionary keyed by transaction types at runtime.
* **Optimistic Concurrency Control (OCC)**: Prevents the classic "double spend" or lost update anomalies during concurrent account transactions using a SQL Server database rowversion mechanism.

---

## 🗄️ Database Models & Schema

The data model is built code-first using Entity Framework Core. You can view the configurations in [AppDbContext.cs](Fintech.Api/Data/AppDbContext.cs).

### 1. `User`
Represents client identities and access groups.
* **`Id`** (`Guid`, Primary Key)
* **`Username`** (`string`, max length 50, unique index, case-insensitive checks)
* **`Role`** (`UserRoles` Enum: `Admin` or `User`, stored in the database as a string)
* **`CreatedAtUtc`** (`DateTime`)

### 2. `Account`
Represents a user's financial wallet.
* **`Id`** (`Guid`, Primary Key, automatically initialized using `NEWSEQUENTIALID()`)
* **`Name`** (`string`, max length 100)
* **`IsActive`** (`bool`)
* **`UserId`** (`Guid`, Foreign Key referencing `User`)
* **`Balance`** (`decimal(18, 4)` to guarantee absolute precision for monetary operations)
* **`Currency`** (`string`, max length 3, e.g., "EUR")
* **`CreatedAtUtc`** (`DateTime`)
* **`RowVersion`** (`byte[]`, decorated with `[Timestamp]` for optimistic concurrency check)

### 3. `Transaction`
Represents a ledger entry of a balance change.
* **`Id`** (`Guid`, Primary Key)
* **`SourceAccountId`** (`Guid?`, Foreign Key referencing `Account`, nullable for Deposits)
* **`DestinationAccountId`** (`Guid?`, Foreign Key referencing `Account`, nullable for Withdrawals)
* **`UserId`** (`Guid`, Foreign Key referencing `User`)
* **`Description`** (`string`)
* **`Category`** (`string`)
* **`Amount`** (`decimal(18, 4)`)
* **`Type`** (`TransactionType` Enum: `Transfer`, `Withdrawal`, `Deposit`, stored in the DB as a string)
* **`CreatedAtUtc`** (`DateTime`)

### 4. `IdempotencyKey`
Saves API responses associated with unique idempotency keys.
* **`ResourceKey`** (`string`, Primary Key, max length 450)
* **`RequestPath`** (`string`, max length 250)
* **`ResponseStatusCode`** (`int`)
* **`ResponseBody`** (`string`)
* **`ExpiresAtUtc`** (`DateTime`)

---

## ⚡ Core Functionality

### 1. Strategy-Driven Transaction Processing
Rather than utilizing monolithic logic, transactions are routed via specific strategy classes:
* [DepositStrategy.cs](Fintech.Api/Services/Transactions/DepositStrategy.cs): Validates non-negative amounts, ensures the target account is active, adds money, and registers the transaction.
* [WithdrawalStrategy.cs](Fintech.Api/Services/Transactions/WithdrawalStrategy.cs): Checks ownership of the source account, ensures active status, validates sufficient funds, subtracts money, and records the transaction.
* [TransferStrategy.cs](Fintech.Api/Services/Transactions/TransferStrategy.cs): Validates the source account ownership, ensures both source and destination accounts are active, checks for sufficient funds, and atomically moves the balance.

### 2. Request Idempotency
To prevent accidental double-execution (e.g., repeating a charge due to client network retries), the `POST /api/Transaction/transactions` endpoint enforces an `Idempotency-Key` header.
* The system checks if the key has been processed for that specific endpoint within the past 24 hours.
* If a duplicate is found, the backend returns the cached HTTP response (status code and payload) instantly without running the transaction again.
* Expired keys are cleaned up automatically.

### 3. Cascading Deletions & Balance Re-evaluations
* Deleting an account executes a cascade purge that removes all related transactions.
* If any transaction involved a secondary active account (e.g., a transfer), the system automatically recalculates the balance of that secondary account using a ledger-level summary function ([AccountService.cs](Fintech.Api/Services/AccountService.cs#L161)) to preserve data integrity.
* *Note:* The balance recalculation logic evaluates transactions using signed directions based on accounts (`deposits + transfersIn - withdrawals - transfersOut`), illustrating the depth of transaction ledger management.

### 4. AI-Powered Financial Analyst (Local Ollama)
Exposes an AI chat interface utilizing a locally-hosted LLM (`qwen2.5-coder:3b` at `localhost:11434` via `OllamaSharp`):
* **Standard Chat**: Returns a complete financial response asynchronously with the system prompt `"You are a financial analyst"`.
* **Streaming Chat**: Utilizes **Server-Sent Events (SSE)** via `IAsyncEnumerable<string>` in [OllamaController.cs](Fintech.Api/Controllers/OllamaController.cs#L31) to flush tokens to the client in real-time.
* In non-development environments, a stub service ([OllamaStubService](Fintech.Api/Services/OllamaService.cs#L38)) gracefully takes over to avoid deployment dependencies.

### 5. Automated Background Workers
* [DatabaseCapWorker.cs](Fintech.Api/Workers/DatabaseCapWorker.cs): Runs every hour to enforce resource quotas suitable for public demo hosting (capping active database size to a maximum of 10 accounts and 100 transactions by automatically purging the oldest entries).
* [TokenCleanupWorker.cs](Fintech.Api/Workers/TokenCleanupWorker.cs): Runs every hour to purge expired tokens from the in-memory token blacklist structure.

---

## 🔒 Security Architecture

The application implements a defense-in-depth security approach to protect financial data and avoid common web application vulnerabilities.

* **Double-Layered Token Revocation (Logout Blacklist)**: 
  When a user logs out, their JWT is registered in a thread-safe, in-memory `HashSet<RevokedToken>` singleton. Token validation is checked:
  1. During the JWT validation event (`OnTokenValidated`) in the ASP.NET Core Authentication handler.
  2. Within a custom [TokenBlacklistMiddleware.cs](Fintech.Api/Middleware/TokenBlacklistMiddleware.cs) early in the request pipeline.
* **Strict Ownership Authorization Checks**: All controllers utilize a `GetCurrentUserId()` helper to enforce that a user can only query, modify, or delete accounts and transactions that they own, preventing Horizontal Privilege Escalation (Insecure Direct Object Reference - IDOR).
* **Rate Limiting**: In Production modes, a fixed-window rate limiter is activated for API controllers (`ApiLimiter`), restricting users to **20 requests per 20 minutes** to safeguard against brute-force authentication and denial-of-service (DoS) attempts.
* **Optimistic Concurrency Control**: Entity Framework Core captures `DbUpdateConcurrencyException` when a database write conflict is detected (using the `RowVersion` timestamp). This conflict is caught by custom middleware and mapped to a `409 Conflict` HTTP response, instructing the client to retry.
* **Kestrel Hardening & Security Headers**:
  - `options.AddServerHeader = false` strips the default `Server: Kestrel` header to limit framework disclosure.
  - HSTS (`UseHsts`) and HTTPS redirection (`UseHttpsRedirection`) are enforced.
* **Safe Secret Management (`dotnet user-secrets`)**: Sensitive configurations (such as JWT signing keys and DB connection strings) are stored outside the source tree in local developer profiles using the `.NET User Secrets Manager` tool. This ensures credentials are never committed to public version control systems.
* **Safe Error Boundaries**: A global [ExceptionHandlingMiddleware.cs](Fintech.Api/Middleware/ExceptionHandlingMiddleware.cs) traps unhandled exceptions, logs them securely using Serilog, and exposes sanitized client messages (e.g. mapping `KeyNotFoundException` to HTTP 404, `DbUpdateConcurrencyException` to HTTP 409, and general errors to a clean HTTP 500 without database schema leakages).

---

## 🧪 Testing Suite & Strategy

The project contains a comprehensive integration testing suite inside `Fintech.Tests`.

### Test Architecture
* **xUnit Test Collection**: Keeps database operations sequential and predictable.
* **`CustomWebApplicationFactory`**: A testing factory that overrides the database configuration to swap SQL Server with an EF Core InMemory Database (`TestDb`) and automatically triggers `EnsureDeleted()` / `EnsureCreated()` on startup to seed clean, predictable mock records (Alice, Bob, accounts, initial deposits, and transfers).
* **`CustomWebApplicationFactoryWithRateLimiter`**: An extension factory that overrides the host environment to `"Production"`. This triggers the rate limiting middleware setup, enabling the testing of rate limiting limits.

### Verified Test Cases
* **Authentication (`AuthTests.cs`)**:
  - Generates signed JWTs upon valid username logins.
  - Returns `401 Unauthorized` for unregistered users.
  - Enforces immediate JWT blacklist rejection upon logout.
* **Account Ledger (`AccountTests.cs`)**:
  - Verifies account retrieval and updates for owners.
  - Rejects foreign ownership access with `403 Forbidden`.
  - Verifies cascade account deletions and correct balance recalculations of linked counterparty accounts.
* **Transactions (`TransactionTests.cs`)**:
  - Validates balance subtraction/addition across deposits, withdrawals, and transfer strategies.
  - Blocks transfers and withdrawals when balances are insufficient or accounts are inactive.
  - Confirms idempotency blocks for duplicate request keys (replays the original response).
  - Uses `Moq` to force a `DbUpdateConcurrencyException` and verifies the middleware responds with `409 Conflict`.
* **Rate Limiting Security (`SecurityTests.cs`)**:
  - Simulates 100 quick requests and asserts that the rate limiter halts processing with a `429 Too Many Requests` on the next call.

---

## 🚀 Getting Started

### Prerequisites
* **SDK**: [.NET 9.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
* **Database**: LocalDB or SQL Server Express
* **AI (Optional)**: [Ollama](https://ollama.com/) with the model `qwen2.5-coder:3b` pulled:
  ```bash
  ollama pull qwen2.5-coder:3b
  ```

### Run the API
1. Restore dependencies:
   ```bash
   dotnet restore
   ```
2. Configure local Development Secrets using the **Secret Manager**:
   Initialize and set the local secrets (such as JWT configuration and DB Connection String) so they are kept safe and out of `appsettings.json`:
   ```bash
   cd Fintech.Api
   dotnet user-secrets init
   dotnet user-secrets set "JwtSettings:Secret" "YourSuperSecretJWTKeyHereThatIsAtLeast32BytesLong"
   dotnet user-secrets set "JwtSettings:Issuer" "FintechApi"
   dotnet user-secrets set "JwtSettings:Audience" "FintechClient"
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\\mssqllocaldb;Database=FintechDemoDb;Trusted_Connection=True;MultipleActiveResultSets=true"
   cd ..
   ```
3. Apply database migrations:
   ```bash
   cd Fintech.Api
   dotnet ef database update
   ```
4. Run the development server:
   ```bash
   dotnet run
   ```
4. Access API Documentation via Scalar at: `https://localhost:7196/scalar/v1` (Port may vary depending on `launchSettings.json`).

### Run the Tests
Execute the integration test suite via the CLI:
```bash
dotnet test
```