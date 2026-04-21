---
name: Implement 
agent: agent
model: Claude Sonnet 4.6 (copilot)
---

- Implement the given plan step by step.
- Separate each step into a different atomic commit with a clear message following the conventional commit format.
- Provide a git commit message for each step that describes the change made in that commit
- Do not commit, the user commits the changes after you provide the commit message.

## Example

```
git add .
git commit -m "feat: Add endpoint for deleting all gods"
```