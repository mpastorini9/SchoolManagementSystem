# Local Development

## Purpose

This document defines the general principles for setting up and maintaining a local development environment for the School Management System.

Its purpose is to ensure that contributors can build, run, and test the project consistently while minimizing environment-specific issues.

A consistent development environment improves collaboration, reduces onboarding time, and increases development reliability.

---

# Development Environment

Every contributor should work in a local environment capable of:

- Building the project.
- Running the application.
- Executing tests.
- Supporting the project's development workflow.

The development environment should remain as consistent as possible across all contributors.

---

# Required Tools

The project requires the tools and technologies defined by the current technology stack.

Contributors should install supported versions of the required SDKs, runtimes, development tools, and package managers before contributing to the project.

Technology-specific requirements are documented in the corresponding technical documentation.

---

# Project Configuration

Contributors should configure their local environment according to the project's documented conventions.

Environment-specific configuration should remain separate from the source code whenever possible.

Sensitive information, such as credentials or secrets, should never be committed to the repository.

---

# Keeping the Environment Updated

Development tools should remain reasonably up to date to ensure compatibility with the rest of the project.

When upgrading development tools, contributors should verify that the project continues to build and run correctly.

---

# Troubleshooting

Environment-specific issues should be documented whenever they are likely to affect other contributors.

Whenever possible, recurring setup problems should be solved through improvements to the project rather than relying on manual workarounds.

---

# Onboarding

New contributors should be able to prepare a working development environment by following the project's documentation.

The onboarding process should be kept as simple, repeatable, and predictable as possible.

---

# Relationship with Other Documents

This document defines the principles for preparing and maintaining a local development environment.

Related engineering practices are documented in:

- Development Workflow
- Dependency Management
- Documentation