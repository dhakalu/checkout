# ADR 1: Repository Strategy - Monorepo vs. Polyrepo

## Status
Accepted

## Context
As the project expands to 12 microservices, we need to decide on a repository structure. A common debate exists between the Monorepo (one repository for all code) and Polyrepo (one repository per service) approaches.

We have specifically considered the concerns raised in industry discussions (such as the [Phan.nz article](https://phan.nz)) regarding the scaling limitations of monorepos in .NET environments. These include:
*   **Tooling Strain:** Large solutions can degrade IDE performance and increase memory usage.
*   **CI/CD Bottlenecks:** Without proper isolation, a change in one service could trigger builds for all services, leading to long feedback loops.
*   **Dependency Tangling:** High risk of accidental coupling between services.

## Decision
We will use a **Monorepo** strategy.

The decision is driven by the current team size (1 developer) and the manageable number of services (12). At this scale, the administrative overhead of managing 12 separate repositories—including 12 sets of CI/CD pipelines, 12 sets of secret management, and complex cross-repo versioning—poses a higher risk to velocity than the technical constraints of a monorepo. 

We believe the "scale ceiling" is not an immediate threat, and the benefits of a unified workflow are more valuable at this stage.

## Consequences

### Positive (Pros)
*   **Simplified Onboarding & Local Dev:** A single `git clone` provides the entire system. Shared libraries can be updated and tested across all services in one go.
*   **Atomic Commits:** Changes spanning multiple services (e.g., a shared API contract change) can be committed, reviewed, and merged in a single Pull Request.
*   **Shared Infrastructure:** Easier to maintain consistent Dockerfiles, linting rules, and CI/CD templates.

### Negative (Cons)
*   **Pipeline Complexity:** We must implement path-based filtering (e.g., `on: push: paths: ['services/order-service/**']`) to ensure efficient CI/CD.
*   **Discipline Required:** Developers must be vigilant to avoid tight coupling between service projects just because they are co-located.
*   **IDE Performance:** We may eventually need to use "Solution Filters" (.slnf) or lightweight editors (VS Code) if the total project count causes IDE lag.

## Status
Accepted - May 2026
