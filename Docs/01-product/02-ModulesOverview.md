# Product Modules

## Purpose

This document provides an overview of the functional modules that compose the School Management System product.

Its purpose is to describe the business capabilities offered by the platform without entering implementation or architectural details.

Each module represents a distinct functional area of the product and contributes to the overall value delivered to educational institutions.

---

# Module Lifecycle

Every product module progresses through one of the following stages:

| Status | Description |
|---------|-------------|
| Implemented | Available in the current product. |
| In Progress | Actively under development. |
| Planned | Approved for future development. |
| Future Consideration | Identified as a possible future capability but not yet committed. |

---

# Core Modules

## Students

**Purpose**

Manage student information throughout the educational lifecycle.

**Primary Users**

- Administration

**Status**

Implemented

---

## Courses

**Purpose**

Manage academic groups and organize students into courses.

**Primary Users**

- Administration

**Status**

Implemented

---

## Attendance

**Purpose**

Record, manage, and review daily student attendance.

Attendance is the first production-ready business module of the platform and serves as the foundation for validating the product.

**Primary Users**

- Teachers
- Administration

**Status**

In Progress

---

# Academic Modules

## Student Records

**Purpose**

Maintain the historical record of each student, including observations, disciplinary actions, achievements, and relevant administrative information.

**Primary Users**

- Teachers
- Administration

**Status**

Planned

---

## Teacher Management

**Purpose**

Manage teachers, assignments, schedules, and academic responsibilities.

**Primary Users**

- Administration

**Status**

Planned

---

## Scheduling

**Purpose**

Manage academic schedules, class timetables, and teacher availability.

**Primary Users**

- Administration
- Teachers

**Status**

Future Consideration

---

# Administrative Modules

## Parent Portal

**Purpose**

Provide parents with secure access to information related to their children.

**Primary Users**

- Parents

**Status**

Planned

---

## Billing

**Purpose**

Support tuition management, payment tracking, and financial administration.

**Primary Users**

- Administration

**Status**

Future Consideration

---

## Notifications

**Purpose**

Deliver relevant communications to teachers, parents, and administrators.

**Primary Users**

- Teachers
- Parents
- Administration

**Status**

Future Consideration

---

## Document Management

**Purpose**

Store and organize documents associated with students and school administration.

**Primary Users**

- Administration

**Status**

Future Consideration

---

## Reports & Analytics

**Purpose**

Provide operational and strategic insights through reports, dashboards, and historical data.

**Primary Users**

- Administration

**Status**

Future Consideration

---

# Module Relationships

The platform is designed around independent but collaborative business modules.

Modules may exchange information through well-defined interfaces while maintaining clear ownership of their respective business responsibilities.

The objective is to maximize modularity without sacrificing product consistency.

---

# Evolution

New modules should only be introduced when they:

- Support the product vision.
- Deliver measurable business value.
- Integrate naturally with the existing platform.
- Preserve simplicity and maintainability.
- Align with the Product Roadmap.

Modules should evolve incrementally rather than attempting to solve every business need from their first release.

---

# Relationship with Other Documents

This document provides a functional overview of the platform's modules.

It should be read together with:

- ProductRoadmap.md
- Vision.md
- Scope.md
- 03-modules/

The Product Roadmap defines when modules are expected to evolve.

The Modules documentation describes how each module is designed and implemented.

This document serves as the bridge between product planning and software architecture.