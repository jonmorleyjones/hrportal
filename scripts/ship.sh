#!/bin/bash
# Ship: Commit, create PR, merge, and return to main
# Usage: ./scripts/ship.sh "commit message"

set -e

COMMIT_MSG="$1"

if [ -z "$COMMIT_MSG" ]; then
  echo "Usage: ./scripts/ship.sh \"commit message\""
  exit 1
fi

# Check if there are changes to commit
if [ -z "$(git status --porcelain)" ]; then
  echo "No changes to commit"
  exit 1
fi

# Get current branch
BRANCH=$(git branch --show-current)

if [ "$BRANCH" = "main" ]; then
  echo "Error: Cannot ship from main branch. Create a feature branch first."
  exit 1
fi

echo "📦 Staging all changes..."
git add -A

echo "💾 Committing..."
git commit -m "$COMMIT_MSG

Co-Authored-By: Claude Opus 4.5 <noreply@anthropic.com>"

echo "🚀 Pushing to origin..."
git push -u origin "$BRANCH"

echo "📝 Creating PR..."
PR_URL=$(gh pr create --title "$COMMIT_MSG" --body "🤖 Generated with [Claude Code](https://claude.com/claude-code)" --fill 2>/dev/null || gh pr view --json url -q .url)

echo "🔀 Merging PR..."
gh pr merge --merge

echo "🔄 Switching to main and pulling latest..."
git checkout main
git pull

echo "✅ Done! Shipped to main."
