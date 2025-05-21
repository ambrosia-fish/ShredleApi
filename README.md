# Shredle API

Backend API for the Shredle guitar solo guessing game.

## Overview

The Shredle API provides endpoints for the Shredle guitar solo guessing game. It manages daily challenges, processes guesses, and provides hints for guitar solos.

## Features

- Daily game management
- Solo database integration with Supabase
- AI-powered hint generation with OpenAI
- Smart guess checking with fuzzy matching
- Development and production environment detection

## Environment Setup

The API uses a hierarchical approach to configuration with the following precedence:

1. Environment variables (highest priority, used on Heroku)
2. User secrets (for local development)
3. appsettings.json (fallback)

### Local Development Setup

We recommend using .NET Secret Manager for local development to keep sensitive data secure. There are two options:

#### Option 1: User Secrets (Recommended)

The most secure approach for local development:

```bash
# Initialize user secrets (if not already done)
dotnet user-secrets init --project shredle-api.csproj

# Set your secrets
dotnet user-secrets set "Supabase:Url" "your-supabase-url"
dotnet user-secrets set "Supabase:Key" "your-supabase-key"
dotnet user-secrets set "OpenAI:ApiKey" "your-openai-key"
```

To view your secrets:
```bash
dotnet user-secrets list --project shredle-api.csproj
```

#### Option 2: appsettings.json

Alternatively, you can edit `appsettings.json` directly, but this is less secure as you might accidentally commit sensitive data:

```json
{
  "Supabase": {
    "Url": "your-supabase-url",
    "Key": "your-supabase-key"
  },
  "OpenAI": {
    "ApiKey": "your-openai-key"
  }
}
```

#### Development Mode Features

When running in Development mode:
- More verbose logging is enabled
- Fallback implementations are used when API keys are missing
- Environment detection automatically adjusts behavior

### Production Deployment (Heroku)

In production, set environment variables directly in Heroku:

```bash
heroku config:set SUPABASE_URL=your-production-url
heroku config:set SUPABASE_KEY=your-production-key
heroku config:set OPENAI_API_KEY=your-production-key
```

The application automatically detects when it's running on Heroku and:
- Uses the Heroku-provided PORT variable
- Prioritizes environment variables over other configuration
- Applies production-specific logging and error handling

## Running the API

### Local Development

```bash
# Set ASPNETCORE_ENVIRONMENT for development mode
$env:ASPNETCORE_ENVIRONMENT="Development"  # PowerShell
export ASPNETCORE_ENVIRONMENT="Development"  # Bash/Zsh

# Run the app
dotnet run

# Or use watch mode for automatic reloading
dotnet watch run
```

### Production

```bash
# Deploy to Heroku
git push heroku main
```

## API Endpoints

- `GET /api/game/daily` - Get the current daily solo
- `POST /api/game/guess` - Submit a guess for the current solo
- More endpoints documented in Swagger at `/swagger` when running

## Configuration Hierarchy

The application loads configuration in the following order (higher overrides lower):

1. **Environment Variables**: 
   - Used primarily in production (Heroku)
   - Example: `SUPABASE_URL`, `SUPABASE_KEY`, `OPENAI_API_KEY`

2. **User Secrets**: 
   - Used in development for sensitive data
   - Stored securely outside the project directory
   - Access with `dotnet user-secrets list`

3. **appsettings.{Environment}.json**: 
   - Environment-specific settings
   - Example: `appsettings.Development.json`

4. **appsettings.json**: 
   - Base configuration (fallback)
   - Should contain only non-sensitive defaults

## Technologies

- .NET 8.0
- Supabase (PostgreSQL)
- OpenAI API
- Heroku hosting

## License

[MIT License](LICENSE)
