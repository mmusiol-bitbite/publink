# Context Diagram

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-21 |
| Owner | Publink Audit architecture |
| Sources | Code/config analysis |
| Confidence | High |
| Related | [C4 Context](../diagrams/c4/context.md), [System Overview](system-overview.md) |

```mermaid
C4Context
    title Publink Audit context
    Person(user, "Treasurer / audit operator")
    Person(operator, "Developer / operator")
    System(system, "Publink Audit", "Contract audit history explorer")
    System_Ext(legacy, "Legacy SQL audit source")
    System_Ext(bus, "Azure Service Bus")
    System_Ext(sql, "MSSQL active/archive stores")
    Rel(user, system, "Searches, reviews, exports", "Browser")
    Rel(operator, system, "Runs and monitors", "Docker/health/logs")
    Rel(system, legacy, "Reads audit rows", "SQL")
    Rel(system, bus, "Publishes/consumes", "AMQP")
    Rel(system, sql, "Persists and queries", "TDS")
```

This diagram is the quickest way to read the system boundary. It shows that users interact only with Publink Audit, while the application integrates with legacy SQL for source audit rows, Service Bus for asynchronous work and MSSQL for its owned read/archive data.