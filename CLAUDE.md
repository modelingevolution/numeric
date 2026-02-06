# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Critical Git Rules

**NEVER use `git rebase`** - Always use merge. No exceptions.

- git rebase - FORBIDDEN
- git pull --rebase - FORBIDDEN
- git pull (merge) - OK
- git merge - OK

## Build and Development Commands

```bash
# Build entire solution
dotnet build Sources/ModelingEvolution.Numeric.sln

# Run tests
dotnet test Sources/ModelingEvolution.Numeric.Tests/

# Run tests with detailed output
dotnet test Sources/ModelingEvolution.Numeric.Tests/ --logger:"console;verbosity=detailed"

# Create NuGet package
dotnet pack Sources/ModelingEvolution.Numeric/ModelingEvolution.Numeric.csproj --configuration Release
```

## Architecture

- **.NET 10.0** targeting generic math (`INumber<T>`)
- **XUnit + FluentAssertions** for testing
- NuGet package published to nuget.org and nuget.modelingevolution.com

## Project Structure

```
Sources/
  ModelingEvolution.Numeric/          # Library
    MovingAverage.cs                  # O(1) moving average with circular buffer
  ModelingEvolution.Numeric.Tests/    # Tests
    MovingAverageTests.cs
```

## Release Process

```bash
# Auto-increment patch version
./release.sh

# Minor version bump
./release.sh --minor -m "Added new feature"

# Specific version
./release.sh 1.2.0.0 -m "Release notes"

# Dry run
./release.sh --dry-run
```
