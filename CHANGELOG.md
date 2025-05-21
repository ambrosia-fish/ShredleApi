# Changelog

All notable changes to the Guitar Solo Guesser API will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
### Added
- Added automatic environment detection for development/production modes
- Added `EnvironmentHelper` class to centralize environment-specific logic
- Created detailed development mode with fallback implementations
- Added verbose logging in development mode
- Environment now auto-detects Heroku vs local environments

## [0.3.0] - 2025-05-20
### Fixed
- Fixed critical database column casing issues with Supabase API:
  - Changed model JSON property names from lowercase/snake_case to PascalCase to match the actual database columns
  - Updated SupabaseService to use PascalCase in all REST API queries
  - Fixed Supabase URL in appsettings.json
  - Added extensive logging to help diagnose API issues
  - Made casing consistent across the entire application

## [0.2.0] - 2025-05-19
### Added
- Created Models for Solo and DailyGame
- Implemented Entity Framework Core
- Added DatabaseContext for data persistence
- Created SupabaseService for database interactions
- Added OpenAIService for generating hints and checking guesses
- Set up environment variables with secrets for API keys
- Implemented GameController with endpoints for gameplay
- Added DTOs for game state, guess, and solo responses
- Implemented Spotify authentication handling (frontend-only approach)
- Created CI/CD pipeline for Heroku deployment

### Changed
- Removed Spotify authentication from backend (now handled in frontend only)

### API Endpoints
- `GET /api/game/daily`: Retrieves the current daily solo with progressive reveals based on guess count
  - Query Parameter: `guessCount` (optional, default: 0)
  - Response: `GameStateResponse` with solo details revealed based on guess count
- `POST /api/game/guess`: Submit a guess for the current daily solo
  - Request Body: `GuessRequest` with `songGuess` string
  - Query Parameter: `previousGuessCount` (optional, default: 0)
  - Response: `GameStateResponse` with updated reveal state

## [0.1.0] - 2025-05-18
### Added
- Created project repository
- Added basic README
- Added CHANGELOG.md

## Solo Selection Branch Changes (Unreleased)
### Added
- Implemented AI-powered guess checking with OpenAI
- Added fallback mechanisms for guess checking when API is unavailable
- Added hardcoded solos for testing
- Improved database queries with better error handling
- Added set-daily-solo endpoint for manual override
