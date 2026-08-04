# Test Levels

## Purpose

This document defines the different levels of testing used throughout the School Management System.

Its purpose is to clarify the role of each type of test and ensure that software quality is verified at the appropriate level throughout the development process.

Different test levels provide different kinds of confidence and should complement one another rather than replace each other.

---

# Testing Pyramid

The project follows a testing strategy that favors a balanced distribution of automated tests.

Whenever practical:

- Unit tests should be the most numerous.
- Integration tests should verify interactions between components.
- API tests should validate external behavior.
- End-to-End tests should verify complete business workflows.

Each testing level serves a different purpose and contributes to overall software quality.

---

# Unit Testing

Unit tests verify the behavior of individual components in isolation.

Their objective is to ensure that small pieces of logic behave correctly under different conditions.

Unit tests should be:

- Fast
- Independent
- Deterministic
- Easy to maintain

---

# Integration Testing

Integration tests verify that multiple components work correctly together.

They focus on interactions between application layers, services, databases, or external systems.

These tests provide confidence that independently tested components collaborate correctly.

---

# API Testing

API tests validate the application's public interfaces.

They verify that requests and responses behave according to the documented API contract.

Typical validation includes:

- Request handling
- Response structure
- Status codes
- Validation rules
- Error responses

---

# End-to-End Testing

End-to-End tests validate complete business scenarios from the user's perspective.

These tests provide confidence that the system functions correctly as an integrated application.

Because they are generally slower and more expensive to maintain, End-to-End tests should focus on the application's most critical workflows.

---

# Manual Testing

Manual testing remains valuable for scenarios that are difficult or impractical to automate.

Examples include:

- Exploratory testing
- User experience validation
- Visual verification
- Early feature validation

Manual testing complements automated testing but should not replace it where automation is practical.

---

# Selecting the Appropriate Test Level

Testing should occur at the lowest level capable of providing sufficient confidence.

Whenever possible:

- Validate business logic with Unit tests.
- Validate component interaction with Integration tests.
- Validate external behavior with API tests.
- Validate complete user workflows with End-to-End tests.

Choosing the appropriate level improves reliability while reducing maintenance costs.

---

# Relationship with Other Documents

This document defines the different testing levels used throughout the project.

Related quality practices are documented in:

- Quality Strategy
- Testing Strategy
- Test Data
- Quality Metrics