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

The API uses environment variables for configuration. We use a dual approach to manage these variables:

### Local Development

1. **Create a .env file**: 
   - Copy `example.env` to `.env` (which is gitignored)
   - Fill in your actual API keys and configuration values

2. **Set secrets using .NET Secret Manager**:
   ```bash
   # Initialize user secrets (if not already done)
   dotnet user-secrets init --project shredle-api.csproj
   
   # Set secrets from your .env file
   dotnet user-secrets set "Supabase:Url" "your-supabase-url"
   dotnet user-secrets set "Supabase:Key" "your-supabase-key"
   dotnet user-secrets set "OpenAI:ApiKey" "your-openai-key"
   ```

3. **Environment Detection**: 
   - When running locally, set `ASPNETCORE_ENVIRONMENT=Development`
   - The app will automatically use fallback implementations if API keys are missing

### Production Deployment

For production (Heroku):

1. **Set environment variables in Heroku**:
   ```bash
   heroku config:set SUPABASE_URL=your-production-url
   heroku config:set SUPABASE_KEY=your-production-key
   heroku config:set OPENAI_API_KEY=your-production-key
   ```

2. **Heroku Detection**:
   - The app automatically detects it's running on Heroku
   - It uses the Heroku-provided PORT variable
   - Production-specific settings are applied

## Running the API

### Local Development

```bash
# Run in development mode
dotnet run

# Or use the .NET CLI watch mode for automatic reloading
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

## Technologies

- .NET 8.0
- Supabase (PostgreSQL)
- OpenAI API
- Heroku hosting

## License

[MIT License](LICENSE)
