# C4 Context Diagram

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-21 |
| Owner | Publink Audit architecture |
| Sources | Code/config analysis |
| Confidence | High |
| Related | [Context Diagram](../../architecture/context-diagram.md) |

```mermaid
C4Context
    title Publink Audit context
    Person(user, "Treasurer / audit operator")
    System(system, "Publink Audit", "Contract audit history explorer")
    System_Ext(legacy, "Legacy SQL audit source")
    System_Ext(bus, "Azure Service Bus")
    System_Ext(sql, "MSSQL active/archive stores")
    Rel(user, system, "Searches, reviews, exports", "Browser")
    Rel(system, legacy, "Reads audit rows", "SQL")
    Rel(system, bus, "Publishes and consumes", "AMQP")
    Rel(system, sql, "Persists and queries", "TDS")
```