# Entity Framework Migration Guide

## Prerequisites
1. Install EF Core tools globally:
   ```bash
   dotnet tool install --global dotnet-ef
   ```

## Environment Variables Setup

### For Heroku:
Set these config vars in Heroku dashboard or CLI:
```bash
# Option 1: DATABASE_URL (Heroku format)
DATABASE_URL=postgres://username:password@host:port/database

# Option 2: CONNECTION_STRING (standard format)
CONNECTION_STRING=Host=db.xxxxx.supabase.co;Database=postgres;Username=postgres;Password=your-password;SSL Mode=Require;Trust Server Certificate=true

# OpenAI API Key
OpenAI__ApiKey=your-openai-api-key
```

### For Local Development:
Use user-secrets:
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=db.xxxxx.supabase.co;Database=postgres;Username=postgres;Password=your-password"
dotnet user-secrets set "OpenAI:ApiKey" "your-openai-api-key"
```

## Creating Initial Migration

1. Navigate to the project directory:
   ```bash
   cd ShredleApi
   ```

2. Create the initial migration:
   ```bash
   dotnet ef migrations add InitialCreate
   ```

3. The migration will be created in the `Migrations` folder.

## Deploying to Production

The application is configured to automatically apply migrations on startup. When you deploy to Heroku, the migrations will run automatically.

If you need to apply migrations manually:
```bash
dotnet ef database update
```

## Database Schema

The migration will create two tables:

### games
- id (int, primary key, auto-increment)
- date (datetime, unique)
- solo_id (int, foreign key to solos)

### solos
- id (int, primary key, auto-increment)
- title (varchar(200))
- artist (varchar(200))
- spotify_id (varchar(100))
- start_time_clip1 (double)
- end_time_clip1 (double)
- start_time_clip2 (double)
- end_time_clip2 (double)
- start_time_clip3 (double)
- end_time_clip3 (double)
- start_time_clip4 (double)
- end_time_clip4 (double)
- guitarist (varchar(200))
- hint (varchar(500))

## Files to Delete

After successful migration, delete these deprecated files:
- Data/SupabaseRepository.cs

## API Changes

The Game ID has changed from string to int. Update your frontend to expect:
```json
{
  "id": 123,           // Changed from "game_20250522" to integer
  "date": "2025-05-22T00:00:00Z",
  "soloId": 456
}
```

## Testing

1. Test locally first with your Supabase connection
2. Deploy to a staging environment if available
3. Test all endpoints:
   - GET /api/game/daily
   - GET /api/solo/{id}
   - POST /api/guess
