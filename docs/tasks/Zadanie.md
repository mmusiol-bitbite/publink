# **Product Engineer** Zadanie rekrutacyjne 

## Treść zadania 

Skarbnik dostał zapowiedź kontroli RIO. Mówi: «potrzebuję historii zmian na umowach, żeby pokazać kto i kiedy co ruszał». Masz AuditLog. Zbuduj API + front w .NET i React, które mu pomoże. Zakres definiujesz sam. To MVP na próbkę, nie produkcja, nie buduj więcej niż trzeba. W README wymień: 3 decyzje, których zadanie nie wymuszało a które podjąłeś, każdą uzasadnioną wartością dla skarbnika lub biznesu, nie elegancją techniczną. Napisz też, co świadomie odpuścił_ś i dlaczego. 

## Disclaimer: 

_Zadanie ma na celu ocenę umiejętności kandydata w procesie rekrutacji i nie będzie przez nas wykorzystywane do żadnego innego celu. Prosimy miej na uwadze, że to zadanie ma być próbką Twoich umiejętności analitycznych, zrozumienia procesów operacyjnych i kompetencji technicznych, a nie kompleksowym rozwiązaniem._ 

## Modele 

public enum Type { Added = 1, Deleted = 2, Modified = 3, } 

public enum EntityType { Unknown = 0, ContractHeaderEntity = 1, AnnexHeaderEntity = 2, AnnexChangeEntity = 3, FileEntity = 4, InvoiceEntity = 5, PaymentScheduleEntity = 6, ContractFundingEntity = 7 } 

"ConnectionStrings" : { "RekrutacjaDb" : "<REDACTED: credential supplied in the recruitment brief>" }, 

## Architektura i Modelowanie Domeny 

Za 6 miesięcy portfolio modułów rośnie o Podatki i Dotacje, audit przestaje pochodzić z jednej bazy SQL. Co byś zmienił w architekturze audit loga, KIEDY uruchamiasz zmianę, a czego celowo NIE robisz teraz i dlaczego? Spójność rozproszona bez ACID, kiedy aneks zapisany a harmonogram nie, to część odpowiedzi. Przedstaw w wygodnej formie, przeprowadzisz nas przez tok myślenia na rozmowie. 

## Instrukcja 

- Na opracowanie zadania masz 7 dni (jeśli z jakiegoś powodu potrzebujesz więcej czasu, daj znać). 

- Celem zadania jest poznanie Twojego podejścia do problemu, tego jak go postrzegasz. Kod który stworzysz jest istotny, ale istotniejsze jest podejście. 

- W razie pytań, jesteśmy dostępni pod mailem z rekrutacji. 

- Opis zadania jest dość powierzchowny, więc jeśli brakuje Ci jakiejś informacji, czuj się swobodnie w tworzeniu założeń (daj nam tylko o nich znać). 

- Rozwiązanie prześlij w dowolnej formie, na adres mailowy z rekrutacji. 

- Zadanie z Architektury i Modelowania zaprezentuj w wygodnej dla Ciebie formie, będziemy chcieli, żebyś na rozmowie przeprowadził nas przez Twój tok myślenia 

# **Powodzenia!** 
