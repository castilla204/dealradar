# DealRadar — Detección Inteligente de Gangas en Segunda Mano

> Sistema multi-plataforma que detecta y ordena oportunidades de precio en Wallapop, Coches.net y otros marketplaces en tiempo real.

![Python](https://img.shields.io/badge/python-3.10+-3776AB?style=flat-square&logo=python&logoColor=white)
![Flutter](https://img.shields.io/badge/Flutter-02B5F5?style=flat-square&logo=flutter&logoColor=white)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-FF6600?style=flat-square&logo=rabbitmq&logoColor=white)
![MongoDB](https://img.shields.io/badge/MongoDB-47A248?style=flat-square&logo=mongodb&logoColor=white)

---

## ¿Qué es DealRadar?

DealRadar es un agregador inteligente de anuncios de segunda mano que analiza automáticamente precios y criterios de valoración para identificar gangas reales en varios marketplaces simultáneamente.

**Diseño escalable:** añadir una nueva plataforma = solo un nuevo scraper sin tocar el resto del sistema.

---

## Arquitectura

```
[Wallapop Scraper]  [Coches.net Scraper]  [MilAnuncios Scraper]  [...]
        │                   │                       │
        └───────────────────┴───────────────────────┘
                            │
                  [RabbitMQ Message Bus]
                            │
                  [Procesador de precios]
                            │
                  [MongoDB + API Python]
                            │
               [Flutter App (Android/iOS/Web)]
```

---

## Stack

| Capa | Tecnología |
|---|---|
| Frontend | Flutter (Android/iOS/Web) |
| Mensajería | RabbitMQ |
| Backend / Scraping | Python microservicios |
| Base de datos | MongoDB |
| Ingeniería inversa | APIs privadas de Wallapop y MilAnuncios |

---

## Autor

**Diego Castilla Abella** - [github.com/castilla204](https://github.com/castilla204)

<!-- meta:sync 1785961080 -->

<!-- meta:sync 1785961111 -->

<!-- meta:sync 1785962348 -->
