# Application Flows Cheatsheet - PL

| Metadata | Value |
| --- | --- |
| Last updated | 2026-06-22 |
| Owner | Publink Audit architecture / technical writing |
| Sources | `docs/README.md`, `docs/architecture/backend-object-catalog.md`, `docs/diagrams/sequence/checkpoint-state-duplicates.md`, `docs/api/rest-api.md`, backend projects |
| Confidence | High for implemented flows; Medium for legacy business semantics |
| Related | [English version](application-flows-cheatsheet-en.md), [Solution Walkthrough](solution-walkthrough.md), [Backend Object Catalog](../architecture/backend-object-catalog.md), [Audit Storage ERD](../diagrams/erd/audit-storage.md) |

Ten cheatsheet opisuje glowne przeplywy w aplikacji Publink Audit. Kazdy flow pokazuje krok po kroku, co dzieje sie w systemie, ktore komponenty biora udzial i jakie tabele/stany sa istotne.

Publink Audit nie jest klasycznym CRUD-em na legacy bazie. To audit explorer oparty o read model: dane przychodza z legacy SQL, sa zamieniane na canonical eventy, zapisywane w `audit_events`, projektowane do tabel pod search/timeline, a potem czytane przez API i frontend.

## 1. Flow Importu Z Legacy SQL

1. `Audit.Ingestion.Worker` uruchamia sie cyklicznie jako worker.
2. Worker czyta z `import_checkpoints` ostatni zaimportowany `SourceEventId`.
3. Na tej podstawie pyta legacy SQL o audit rows po ostatnim checkpointcie.
4. Kazdy legacy row trafia do modelu `LegacyAuditRecord`.
5. `LegacyAuditEventMapper` mapuje rekord na `AuditEntryImportedV1`.
6. Mapper generuje deterministyczne `EventId` z `Source` i `SourceEventId`.
7. `Audit.Ingestion.Worker` publikuje `AuditEntryImportedV1` na Service Bus.
8. Po publikacji worker zapisuje nowy checkpoint w `import_checkpoints`.
9. Jesli batch jest pusty, checkpoint zostaje bez zmian.
10. Po sweepie worker moze oznaczyc zrodlo jako zsynchronizowane.
11. Legacy row jest od tej chwili reprezentowany jako event do przetworzenia.

Znaczenie: import oddziela legacy SQL od reszty systemu. Query API i frontend nie czytaja legacy bazy bezposrednio.

## 2. Flow Processingu I Projekcji

1. `Audit.Processing.Worker` slucha endpointu `audit-projection`.
2. Service Bus dostarcza `AuditEntryImportedV1`.
3. `AuditEntryImportedConsumer` sprawdza, czy istnieje event o `(Source, SourceEventId)`.
4. Jesli istnieje, wynik to `Duplicate` i projekcje nie sa aktualizowane.
5. Jesli event jest nowy, worker zapisuje go do `audit_events`.
6. `audit_events` przechowuje canonical imported event w Publink Audit.
7. Worker aktualizuje `contract_timeline_items` wpisem historii zmian.
8. Worker aktualizuje `contract_search` aktualnymi polami wyszukiwania.
9. Worker aktualizuje `contract_search_aliases` wartosciami historycznymi.
10. Na koncu nastepuje commit przez unit of work.
11. Jesli zapis sie nie uda, MassTransit wykonuje retry.
12. Po wyczerpaniu retry wiadomosc moze trafic do DLQ.

Znaczenie: processing zamienia eventy w czytelny read model dla API, UI i eksportu.

## 3. Flow Obslugi Duplikatow

1. System zaklada at-least-once delivery, wiec wiadomosc moze przyjsc wiecej niz raz.
2. Duplikat moze pojawic sie przez retry, ponowna dostawe z brokera albo reimport.
3. Kluczem idempotencji jest `(Source, SourceEventId)`.
4. Processing najpierw probuje dopisac canonical event.
5. Jesli event juz istnieje, `CanonicalAuditEventPersister` zwraca `Duplicate`.
6. Worker nie dopisuje drugiego timeline itemu.
7. Worker nie aktualizuje ponownie `contract_search` ani aliasow.
8. Wiadomosc jest uznana za obsluzona.

Znaczenie: retry nie tworzy podwojnej historii zmian.

## 4. Flow Wyszukiwania Aktywnych Kontraktow

1. Uzytkownik wpisuje fraze w React SPA.
2. Frontend wywoluje `GET /api/v1/contracts/search`.
3. `Audit.Query.Api` waliduje request.
4. API nie pyta legacy SQL.
5. API czyta aktywny read model przez Dappera.
6. Search korzysta z `contract_search` i `contract_search_aliases`.
7. `contract_search` zawiera aktualne dane wyszukiwalne kontraktu.
8. `contract_search_aliases` zawiera wartosci historyczne.
9. API zwraca liste `ContractSearchResult`.
10. Frontend pokazuje wyniki, a po wyborze kontraktu przechodzi do timeline.

Znaczenie: search jest szybki, bo czyta dedykowana projekcje, nie legacy SQL ani pelna historie eventow.

## 5. Flow Timeline Kontraktu

1. Uzytkownik wybiera kontrakt.
2. Frontend wywoluje `GET /api/v1/contracts/{contractId}/audit-events`.
3. Request moze zawierac filtry: daty, actor, change type, entity type, limit i cursor.
4. API waliduje filtry.
5. Query API czyta z `contract_timeline_items`.
6. Timeline itemy sa juz projekcja, wiec API nie przelicza historii z legacy rows.
7. Odpowiedz zawiera event ID, sequence, date, correlation ID, change/entity kind, actor, field changes i data-quality issues.
8. API zwraca `TimelinePage`.
9. Jesli sa kolejne dane, odpowiedz zawiera `nextCursor`.
10. `nextCursor` jest kursorem paginacji API, nie checkpointem importu.

Znaczenie: timeline pokazuje historie zmian kontraktu w formie gotowej do przegladania, filtrowania i eksportu.

## 6. Flow Eksportu Audit History

1. Uzytkownik wybiera eksport historii kontraktu.
2. Frontend wywoluje `GET /api/v1/contracts/{contractId}/audit-events/export`.
3. Eksport przyjmuje filtry zgodne z timeline oraz `locale`, np. `pl` albo `en`.
4. API waliduje request.
5. `ContractAuditExportService` pobiera timeline z read modelu.
6. Jesli wynik ma ponad 10 000 eventow, API zwraca `413 exportTooLarge`.
7. Jesli limit jest OK, generowany jest ZIP.
8. ZIP zawiera `audit.csv`, `manifest.json` i `checksums.sha256`.
9. API dodaje header `X-Content-SHA256`.
10. Frontend pobiera paczke jako plik.

Znaczenie: export daje przenosny pakiet historii audytu. Nie jest to pelny legal evidence system, bo WORM, podpisy i trusted timestamps sa poza MVP.

## 7. Flow Manualnej Synchronizacji

1. Uzytkownik lub operator uruchamia synchronizacje.
2. Frontend/API wywoluje `POST /api/v1/synchronization/requests`.
3. Query API tworzy albo dolacza do lease'a w `legacy_synchronization_requests`.
4. Jesli sync juz trwa, API zwraca `joined = true`.
5. API wysyla `RequestLegacySynchronizationV1` na Service Bus.
6. API zwraca `202 Accepted`.
7. `Audit.Ingestion.Worker` konsumuje command z kolejki `legacy-synchronization`.
8. Worker wykonuje import do aktualnego punktu z legacy SQL.
9. Worker publikuje `AuditEntryImportedV1` dla nowych rows.
10. Po zakonczeniu oznacza request jako completed.
11. Status jest dostepny przez `GET /api/v1/synchronization/status`.

Znaczenie: manual sync pozwala wymusic dogonienie legacy source bez wywolywania workerow przez HTTP.

## 8. Flow Archiwizacji

1. `Audit.Archival.Worker` dziala cyklicznie.
2. Worker szuka kontraktow nieaktywnych dluzej niz skonfigurowany okres.
3. Dla kandydata tworzy albo aktualizuje `contract_archive_transfers`.
4. Transfer dostaje stan `Copying`.
5. Worker laduje dane z `contract_search`, `contract_search_aliases`, `contract_timeline_items` i `audit_events`.
6. Z tych danych budowany jest snapshot archiwalny.
7. Snapshot trafia do archive DB jako `archived_contracts`, `archived_contract_aliases`, `archived_timeline_items` i `archived_audit_events`.
8. Worker weryfikuje snapshot.
9. Po weryfikacji transfer przechodzi w `Verified`.
10. Przed usunieciem danych aktywnych worker robi serializable recheck.
11. Jesli kontrakt sie nie zmienil, aktywne rows sa usuwane i transfer przechodzi w `Archived`.
12. Jesli kontrakt zmienil sie w miedzyczasie, snapshot jest usuwany, a transfer wraca do `Active`.
13. Jesli wystapi blad, transfer dostaje `Failed` i `ErrorCode`.

Znaczenie: archiwizacja przenosi nieaktywne kontrakty z hot storage do archive storage bez rozproszonej transakcji ACID.

## 9. Flow Czytania Archiwum

1. Uzytkownik przechodzi do archive view.
2. Frontend wywoluje endpointy `/api/v1/archive/...`.
3. API uzywa archive read source zamiast active read source.
4. Search archiwalny czyta z `archived_contracts` i `archived_contract_aliases`.
5. Timeline archiwalny czyta z `archived_timeline_items`.
6. Historia eventow jest zachowana w `archived_audit_events`.
7. Export archiwalny korzysta z tych samych koncepcji co export aktywny.

Znaczenie: archiwum jest osobnym read modelem, nie tylko flaga w aktywnej tabeli.

## 10. Flow Health, Status I Operacyjny

1. `GET /health/live` sprawdza, czy proces dziala.
2. `GET /health/ready` sprawdza gotowosc zaleznosci.
3. `GET /api/v1/synchronization/status` czyta `import_checkpoints` i `legacy_synchronization_requests`.
4. Status pokazuje ostatni source event ID, czas synchronizacji i aktywny/ostatni manual sync.
5. Status odczytuje tez DLQ count dla `audit-projection`.
6. MassTransitowe `InboxStates`, `OutboxMessages` i `OutboxStates` sa tabela techniczna reliability, nie historia audytu.

Znaczenie: status/health pomaga operatorowi odroznic problem z importem, processingiem, brokerem, baza i archiwizacja.

## Podsumowanie

Glowne flow:

```text
legacy SQL row
  -> LegacyAuditRecord
  -> AuditEntryImportedV1
  -> Service Bus
  -> Audit.Processing.Worker
  -> audit_events
  -> contract_search / contract_search_aliases / contract_timeline_items
  -> Audit.Query.Api
  -> React SPA / export ZIP
```

Najwazniejsze zasady procesu:

1. Legacy SQL pozostaje zrodlem danych wejsciowych.
2. `Audit.Ingestion.Worker` odpowiada za polling i checkpointy.
3. `AuditEntryImportedV1` jest canonical eventem transportowym.
4. `audit_events` przechowuje canonical imported events w aktywnym store.
5. Search i timeline sa projekcjami, a nie bezposrednim odczytem legacy rows.
6. Duplikaty sa bezpieczne dzieki `(Source, SourceEventId)`.
7. Manual sync jest asynchronicznym commandem przez Service Bus.
8. Archiwizacja kopiuje, weryfikuje, recheckuje i dopiero potem usuwa hot rows.
9. Archive DB ma osobne snapshoty do search, timeline i exportu.
10. MassTransit inbox/outbox tables sa infrastruktura niezawodnosci, nie danymi audytowymi.
