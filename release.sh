#!/bin/bash

# ModelingEvolution.Numeric Release Script
# Usage: ./release.sh [--patch|--minor|--major|--version X.X.X.X]

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print colored output
print_info() { echo -e "${GREEN}[INFO]${NC} $1"; }
print_warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }
print_error() { echo -e "${RED}[ERROR]${NC} $1"; }

# Function to get the latest version from git tags
get_current_version() {
    local latest_tag=$(git tag -l "v*" | sort -V | tail -1)

    if [ -n "$latest_tag" ]; then
        echo "${latest_tag#v}"
    else
        local csproj_version=$(grep -oP '(?<=<AssemblyVersion>)[^<]+' Sources/ModelingEvolution.Numeric/ModelingEvolution.Numeric.csproj 2>/dev/null || echo "")
        if [ -n "$csproj_version" ]; then
            echo "$csproj_version"
        else
            echo "1.0.0.0"
        fi
    fi
}

# Function to calculate next version
calculate_next_version() {
    local current_version=$1
    local bump_type=$2

    IFS='.' read -ra VERSION_PARTS <<< "$current_version"
    local major="${VERSION_PARTS[0]:-1}"
    local minor="${VERSION_PARTS[1]:-0}"
    local patch="${VERSION_PARTS[2]:-0}"
    local build="${VERSION_PARTS[3]:-0}"

    case "$bump_type" in
        major)
            major=$((major + 1))
            minor=0
            patch=0
            build=0
            ;;
        minor)
            minor=$((minor + 1))
            patch=0
            build=$((build + 1))
            ;;
        patch)
            patch=$((patch + 1))
            build=$((build + 1))
            ;;
        *)
            print_error "Unknown bump type: $bump_type"
            exit 1
            ;;
    esac

    echo "$major.$minor.$patch.$build"
}

# Function to update version in csproj
update_csproj_version() {
    local version=$1
    local csproj_file="Sources/ModelingEvolution.Numeric/ModelingEvolution.Numeric.csproj"

    if [ ! -f "$csproj_file" ]; then
        print_error "Could not find $csproj_file"
        return 1
    fi

    print_info "Updating version in $csproj_file to $version"

    sed -i "s/<AssemblyVersion>.*<\/AssemblyVersion>/<AssemblyVersion>$version<\/AssemblyVersion>/" "$csproj_file"
    sed -i "s/<FileVersion>.*<\/FileVersion>/<FileVersion>$version<\/FileVersion>/" "$csproj_file"

    if grep -q "<Version>" "$csproj_file"; then
        sed -i "s/<Version>.*<\/Version>/<Version>$version<\/Version>/" "$csproj_file"
    else
        sed -i "/<AssemblyVersion>/a\    <Version>$version<\/Version>" "$csproj_file"
    fi
}

# Main script
main() {
    local bump_type="patch"
    local custom_version=""

    while [[ $# -gt 0 ]]; do
        case $1 in
            --patch)
                bump_type="patch"
                shift
                ;;
            --minor)
                bump_type="minor"
                shift
                ;;
            --major)
                bump_type="major"
                shift
                ;;
            --version)
                custom_version="$2"
                shift 2
                ;;
            --dry-run)
                DRY_RUN=1
                shift
                ;;
            -h|--help)
                echo "Usage: $0 [--patch|--minor|--major|--version X.X.X.X] [--dry-run]"
                echo ""
                echo "Options:"
                echo "  --patch    Increment patch version (default)"
                echo "  --minor    Increment minor version"
                echo "  --major    Increment major version"
                echo "  --version  Set a specific version"
                echo "  --dry-run  Show what would happen without executing"
                echo "  -h, --help Show this help message"
                exit 0
                ;;
            *)
                print_error "Unknown option: $1"
                echo "Use --help for usage information"
                exit 1
                ;;
        esac
    done

    if ! git rev-parse --git-dir > /dev/null 2>&1; then
        print_error "Not in a git repository"
        exit 1
    fi

    if ! git diff-index --quiet HEAD -- 2>/dev/null; then
        print_warn "You have uncommitted changes. Continue anyway? (y/N)"
        read -r response
        if [[ ! "$response" =~ ^[Yy]$ ]]; then
            print_info "Aborted"
            exit 0
        fi
    fi

    local current_version=$(get_current_version)
    print_info "Current version: $current_version"

    local next_version
    if [ -n "$custom_version" ]; then
        next_version="$custom_version"
        print_info "Using custom version: $next_version"
    else
        next_version=$(calculate_next_version "$current_version" "$bump_type")
        print_info "Next version ($bump_type bump): $next_version"
    fi

    if [ "${DRY_RUN:-0}" = "1" ]; then
        echo ""
        print_info "[DRY RUN] Would:"
        echo "  1. Update version in .csproj to $next_version"
        echo "  2. Commit the changes"
        echo "  3. Create tag: v$next_version"
        echo "  4. Push changes and tag to origin"
        exit 0
    fi

    echo ""
    print_warn "This will:"
    echo "  1. Update version in .csproj to $next_version"
    echo "  2. Commit the changes"
    echo "  3. Create tag: v$next_version"
    echo "  4. Push changes and tag to origin"
    echo ""
    print_warn "Continue? (y/N)"
    read -r response

    if [[ ! "$response" =~ ^[Yy]$ ]]; then
        print_info "Aborted"
        exit 0
    fi

    update_csproj_version "$next_version"

    print_info "Committing version update..."
    git add Sources/ModelingEvolution.Numeric/ModelingEvolution.Numeric.csproj
    git commit -m "Bump version to $next_version"

    local tag_name="v$next_version"
    print_info "Creating tag: $tag_name"
    git tag -a "$tag_name" -m "Release ModelingEvolution.Numeric v$next_version"

    print_info "Pushing to origin..."
    git push origin HEAD
    git push origin "$tag_name"

    print_info "Successfully created release $next_version"
    echo ""
    echo "Tag: $tag_name"
    echo "This will trigger the GitHub Actions workflow to:"
    echo "  - Publish to NuGet"
    echo ""
    echo "Monitor progress at: https://github.com/modelingevolution/numeric/actions"
}

main "$@"
