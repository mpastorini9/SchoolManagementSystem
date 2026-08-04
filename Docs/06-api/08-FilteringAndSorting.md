# Filtering and Sorting

## Purpose

This document defines the strategy for filtering and sorting collections exposed by the application's API.

Filtering and sorting allow consumers to retrieve only the information relevant to their current needs while maintaining predictable and consistent behavior across all endpoints.

These capabilities improve usability without changing the underlying business logic.

---

# Philosophy

Filtering and sorting exist to improve information discovery.

Clients should be able to locate relevant information efficiently without requesting unnecessary data.

The API should expose flexible querying capabilities while preserving consistency and simplicity.

---

# Responsibilities

Filtering and sorting are responsible for:

- Reducing unnecessary data transfer.
- Improving information discoverability.
- Supporting efficient client workflows.
- Providing predictable collection queries.
- Maintaining consistent behavior across endpoints.

Filtering and sorting should never modify business rules.

---

# Filtering

Filtering allows clients to retrieve subsets of a collection based on business-relevant criteria.

Examples include:

- Students in a specific course.
- Active teachers.
- Attendance records for a given date.
- Outstanding payments.

Filtering should represent business concepts rather than technical implementation details.

---

# Sorting

Sorting allows clients to define the order in which information is returned.

Sorting should improve readability and support common user workflows.

Typical examples include ordering by:

- Name.
- Date.
- Creation time.
- Status.

Sorting should remain deterministic and predictable.

---

# Consistency

Filtering and sorting should behave consistently throughout the API.

Consumers should not need to learn different query mechanisms for different resources.

The same concepts should use the same conventions across all endpoints.

---

# Composability

Filtering and sorting should work naturally together.

Clients should be able to combine supported filters and sorting options without introducing ambiguity.

Supported combinations should remain predictable and well-defined.

---

# Business-Oriented Queries

Filtering should expose business concepts rather than database implementation.

Consumers should request information using terms that make sense within the business domain.

The API should avoid exposing persistence or infrastructure concerns.

---

# Performance

Filtering should reduce unnecessary processing and data transfer whenever possible.

The API should return only the information required by the client.

Efficient queries improve scalability for both consumers and the server.

---

# Future Evolution

New filtering and sorting capabilities should extend existing behavior without breaking compatibility.

Additional query options should follow the same conventions already established throughout the API.

Consistency should be preserved as the API grows.

---

# Design Principles

Filtering and sorting follow these principles:

- Consistency.
- Predictability.
- Simplicity.
- Business-oriented design.
- Composability.
- Performance.

Consumers should be able to query information naturally and efficiently.

---

# Design Goals

The filtering and sorting strategy aims to provide:

- Flexible data retrieval.
- Consistent query behavior.
- Improved usability.
- Better performance.
- Reduced network usage.
- Long-term maintainability.

Query capabilities should simplify information retrieval without increasing complexity.

---

# Relationship with Other Documents

This document defines how collections can be queried through the API.

It should be read together with:

- Pagination.md
- RequestResponse.md
- Endpoints.md
- ErrorResponses.md

Together, these documents define how clients retrieve, navigate, and consume collections exposed by the API.