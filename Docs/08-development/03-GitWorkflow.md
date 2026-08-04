# Git Workflow

## Purpose

This document defines the Git workflow used throughout the School Management System.

Its purpose is to ensure that source code changes are organized, traceable, and easy to review while supporting collaborative and incremental development.

---

# General Principles

The repository should always remain in a consistent and buildable state.

Every change should be easy to understand, review, and, if necessary, revert.

Version control should support collaboration rather than complicate it.

---

# Branching Strategy

Development should take place in dedicated branches whenever appropriate.

Each branch should focus on a single feature, bug fix, or improvement.

Branches should remain short-lived whenever possible.

---

# Commits

Each commit should represent a logical unit of work.

Commits should:

- Be focused on a single purpose.
- Keep the repository in a valid state.
- Avoid mixing unrelated changes.

Small, meaningful commits are preferred over large, infrequent ones.

---

# Commit Messages

Commit messages should clearly describe the purpose of the change.

They should explain **what** was changed rather than how it was implemented.

Messages should remain concise, descriptive, and consistent.

---

# Pull Requests

Changes should be reviewed before being merged into the main branch whenever collaboration is involved.

Pull requests should:

- Have a clear objective.
- Be limited in scope.
- Be easy to review.
- Include only completed work.

---

# Merging

Code should only be merged after it has been validated.

The main branch should always represent a stable version of the project.

---

# History

The Git history should remain clean and meaningful.

Repository history serves as documentation of the project's evolution and should be preserved whenever practical.

---

# Relationship with Other Documents

This document defines the project's version control workflow.

Related engineering practices are documented in:

- Development Workflow
- Code Review
- Project Conventions